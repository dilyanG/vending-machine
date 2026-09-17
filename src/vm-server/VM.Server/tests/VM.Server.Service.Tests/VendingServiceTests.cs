using FluentAssertions;
using Microsoft.Extensions.Options;
using VM.Server.Domain;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;
using VM.Server.Repository.InMemory;
using VM.Server.Service.Abstractions;
using VM.Server.Service.Products;
using VM.Server.Service.State;
using VM.Server.Service.Vending;

namespace VM.Server.Service.Tests;

public class VendingServiceTests
{
    private static (VendingService Service, IVendingMachineStore Store) CreateService(
        Product product,
        IReadOnlyDictionary<int, int> initialBank,
        IChangeCalculator? changeCalculator = null,
        int initialQuantityPerSlot = 10)
    {
        var source = new FakeExternalCatalogSource([product]);
        var machineState = new MachineStateService(source, new ProductValidationService());
        var store = new InMemoryVendingMachineStore(
            machineState,
            Options.Create(new VendingMachineOptions
            {
                InitialQuantityPerSlot = initialQuantityPerSlot,
                CoinBank = new Dictionary<int, int>(initialBank),
            }));
        var service = new VendingService(store, changeCalculator ?? new ChangeCalculationService());
        return (service, store);
    }

    private static Task<MachineSnapshot> SnapshotAsync(IVendingMachineStore store, Guid productId) =>
        store.AccessAsync(machine => new MachineSnapshot(
            machine.Slots[productId].Quantity,
            new Dictionary<int, int>(machine.Bank.Counts),
            new Dictionary<int, int>(machine.InsertedCoins.Counts)));

    [Fact]
    public async Task GetDenominationsAsync_ReturnsExactlyTheSixAcceptedValuesAscending()
    {
        var (service, _) = CreateService(TestProducts.Create("Espresso", 145), new Dictionary<int, int>());

        var denominations = await service.GetDenominationsAsync();

        denominations.Should().Equal(5, 10, 20, 50, 100, 200);
    }

    [Fact]
    public async Task InsertCoinAsync_WithUnacceptedDenomination_ThrowsInvalidDenomination()
    {
        var (service, _) = CreateService(TestProducts.Create("Espresso", 145), new Dictionary<int, int>());

        var act = () => service.InsertCoinAsync(2);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.InvalidDenomination);
    }

    [Fact]
    public async Task InsertCoinAsync_CalledTwiceForSameDenomination_AccumulatesInTheSession()
    {
        var (service, _) = CreateService(TestProducts.Create("Espresso", 145), new Dictionary<int, int>());

        await service.InsertCoinAsync(50);
        var session = await service.InsertCoinAsync(50);

        session.InsertedTotalCents.Should().Be(100);
    }

    [Fact]
    public async Task PurchaseAsync_OnSuccess_MapsAConsistentDto()
    {
        var product = TestProducts.Create("Espresso", 145);
        var (service, _) = CreateService(product, new Dictionary<int, int> { [50] = 2, [5] = 2 });
        await service.InsertCoinAsync(200);

        var result = await service.PurchaseAsync(product.Id);

        result.Product.Id.Should().Be(product.Id);
        result.Product.Quantity.Should().Be(9);
        result.PaidCents.Should().Be(200);
        result.PriceCents.Should().Be(145);
        result.ChangeCents.Should().Be(55);
        result.ChangeCoins.Sum(coin => coin.DenominationCents * coin.Count).Should().Be(result.ChangeCents);
        result.PaidCents.Should().Be(result.PriceCents + result.ChangeCents);
    }

    [Fact]
    public async Task PurchaseAsync_WithExactMoney_ChangeIsEmptyAndStillSucceeds()
    {
        var product = TestProducts.Create("Espresso", 145);
        var (service, _) = CreateService(product, new Dictionary<int, int>());
        await service.InsertCoinAsync(100);
        await service.InsertCoinAsync(20);
        await service.InsertCoinAsync(20);
        await service.InsertCoinAsync(5);

        var result = await service.PurchaseAsync(product.Id);

        result.ChangeCoins.Should().BeEmpty();
        result.ChangeCents.Should().Be(0);
        result.Product.Quantity.Should().Be(9);
    }

    [Fact]
    public async Task PurchaseAsync_ChangeDrawnFromCustomersOwnInsertedCoins_Succeeds()
    {
        // Bank starts empty; the only 50c coin available for change is the one the customer just inserted.
        var product = TestProducts.Create("Water", 200);
        var (service, store) = CreateService(product, new Dictionary<int, int>());
        await service.InsertCoinAsync(200);
        await service.InsertCoinAsync(50);

        var result = await service.PurchaseAsync(product.Id);

        result.ChangeCents.Should().Be(50);
        var bank = await store.AccessAsync(machine => new Dictionary<int, int>(machine.Bank.Counts));
        bank.Should().BeEquivalentTo(new Dictionary<int, int> { [200] = 1 });
    }

    [Fact]
    public async Task PurchaseAsync_OffersBankPlusInsertedCoinsAsAvailableChangeToTheCalculator()
    {
        var product = TestProducts.Create("Espresso", 145);
        var spy = new SpyChangeCalculator(new ChangeCalculationService());
        var (service, _) = CreateService(product, new Dictionary<int, int> { [50] = 3, [5] = 2 }, spy);
        await service.InsertCoinAsync(200);

        await service.PurchaseAsync(product.Id);

        spy.LastAmountCents.Should().Be(55);
        spy.LastAvailableCoins.Should().BeEquivalentTo(new Dictionary<int, int> { [50] = 3, [5] = 2, [200] = 1 });
    }

    [Fact]
    public async Task PurchaseAsync_UsesTheRealChangeCalculationService_ReturnsCorrectCoinsForAKnownBank()
    {
        // 60c change from a bank of one 50c coin and three 20c coins is the
        // canonical greedy counterexample (ChangeCalculationServiceTests) -
        // this proves the real calculator is wired in, not a stub that would
        // happily return any shape.
        var product = TestProducts.Create("Water", 140);
        var (service, _) = CreateService(product, new Dictionary<int, int> { [50] = 1, [20] = 3 });
        await service.InsertCoinAsync(200);

        var result = await service.PurchaseAsync(product.Id);

        result.ChangeCoins.Should().Equal(new CoinCountDto(20, 3));
    }

    [Fact]
    public async Task PurchaseAsync_ChangeCoins_AreSortedByDenominationDescending()
    {
        var product = TestProducts.Create("Snack", 65);
        var (service, _) = CreateService(product, new Dictionary<int, int> { [20] = 1, [10] = 1, [5] = 1 });
        await service.InsertCoinAsync(100);

        var result = await service.PurchaseAsync(product.Id);

        result.ChangeCoins.Should().HaveCountGreaterThan(1);
        result.ChangeCoins.Should().BeInDescendingOrder(coin => coin.DenominationCents);
    }

    [Fact]
    public async Task PurchaseAsync_WhenProductNotFound_ThrowsAndLeavesExistingSlotBankAndSessionUnchanged()
    {
        var product = TestProducts.Create("Espresso", 145);
        var (service, store) = CreateService(product, new Dictionary<int, int> { [50] = 2 });
        await service.InsertCoinAsync(200);
        var before = await SnapshotAsync(store, product.Id);

        var act = () => service.PurchaseAsync(Guid.NewGuid());

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.ProductNotFound);
        var after = await SnapshotAsync(store, product.Id);
        after.Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task PurchaseAsync_WhenOutOfStock_ThrowsAndLeavesSlotBankAndSessionUnchanged()
    {
        var product = TestProducts.Create("Espresso", 145);
        var (service, store) = CreateService(product, new Dictionary<int, int> { [50] = 2 }, initialQuantityPerSlot: 0);
        await service.InsertCoinAsync(200);
        var before = await SnapshotAsync(store, product.Id);

        var act = () => service.PurchaseAsync(product.Id);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.OutOfStock);
        var after = await SnapshotAsync(store, product.Id);
        after.Should().BeEquivalentTo(before);
        after.Quantity.Should().Be(0);
    }

    [Fact]
    public async Task PurchaseAsync_WhenInsufficientFunds_ThrowsAndLeavesSlotBankAndSessionUnchanged()
    {
        var product = TestProducts.Create("Espresso", 145);
        var (service, store) = CreateService(product, new Dictionary<int, int> { [50] = 2 });
        await service.InsertCoinAsync(100);
        var before = await SnapshotAsync(store, product.Id);

        var act = () => service.PurchaseAsync(product.Id);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.InsufficientFunds);
        var after = await SnapshotAsync(store, product.Id);
        after.Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task PurchaseAsync_WhenChangeUnavailable_ThrowsAndLeavesSlotBankAndSessionUnchanged()
    {
        // This is the atomicity proof: a refused purchase - for whichever
        // reason - must be a pure no-op. Snapshot slot quantity, bank and
        // session before the attempt and compare after.
        var product = TestProducts.Create("Espresso", 145);
        var (service, store) = CreateService(product, new Dictionary<int, int> { [50] = 2 }, FakeChangeCalculator.AlwaysFails());
        await service.InsertCoinAsync(200);
        var before = await SnapshotAsync(store, product.Id);

        var act = () => service.PurchaseAsync(product.Id);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.ChangeUnavailable);
        var after = await SnapshotAsync(store, product.Id);
        after.Should().BeEquivalentTo(before);
        (await service.GetSessionAsync()).InsertedTotalCents.Should().Be(200);
    }

    [Theory]
    [InlineData(150, 200)]
    [InlineData(200, 200)]
    [InlineData(145, 200)]
    public async Task PurchaseAsync_OnSuccess_PaidCentsEqualsPriceCentsPlusChangeCents(int priceCents, int insertedCents)
    {
        var product = TestProducts.Create("Espresso", priceCents);
        var (service, _) = CreateService(product, new Dictionary<int, int> { [50] = 5, [5] = 5 });
        foreach (var coin in DenominateInsert(insertedCents))
        {
            await service.InsertCoinAsync(coin);
        }

        var result = await service.PurchaseAsync(product.Id);

        result.PaidCents.Should().Be(result.PriceCents + result.ChangeCents);
    }

    [Fact]
    public async Task ResetAsync_ReturnsTheSameDenominationsInserted_SortedDescending()
    {
        var product = TestProducts.Create("Espresso", 145);
        var (service, _) = CreateService(product, new Dictionary<int, int>());
        await service.InsertCoinAsync(5);
        await service.InsertCoinAsync(200);
        await service.InsertCoinAsync(50);
        await service.InsertCoinAsync(50);

        var result = await service.ResetAsync();

        result.ReturnedTotalCents.Should().Be(305);
        result.ReturnedCoins.Should().Equal(new CoinCountDto(200, 1), new CoinCountDto(50, 2), new CoinCountDto(5, 1));

        var sessionAfterReset = await service.GetSessionAsync();
        sessionAfterReset.InsertedTotalCents.Should().Be(0);
        sessionAfterReset.InsertedCoins.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetAsync_NeverTouchesTheBankOrSlots()
    {
        var product = TestProducts.Create("Espresso", 145);
        var (service, store) = CreateService(product, new Dictionary<int, int> { [50] = 2 });
        await service.InsertCoinAsync(200);
        var before = await SnapshotAsync(store, product.Id);

        await service.ResetAsync();

        var after = await store.AccessAsync(machine => new Dictionary<int, int>(machine.Bank.Counts));
        after.Should().BeEquivalentTo(before.Bank);
        var quantityAfter = await store.AccessAsync(machine => machine.Slots[product.Id].Quantity);
        quantityAfter.Should().Be(before.Quantity);
    }

    private static IEnumerable<int> DenominateInsert(int totalCents)
    {
        var remaining = totalCents;
        foreach (var denomination in CoinDenominations.Accepted)
        {
            while (remaining >= denomination)
            {
                yield return denomination;
                remaining -= denomination;
            }
        }
    }

    private sealed record MachineSnapshot(int Quantity, Dictionary<int, int> Bank, Dictionary<int, int> Inserted);
}
