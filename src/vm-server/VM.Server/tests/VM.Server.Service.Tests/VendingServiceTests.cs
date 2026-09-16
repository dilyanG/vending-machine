using FluentAssertions;
using Microsoft.Extensions.Options;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;
using VM.Server.Domain.Services;
using VM.Server.Repository.InMemory;
using VM.Server.Service.Vending;

namespace VM.Server.Service.Tests;

public class VendingServiceTests
{
    // Business rules (OUT_OF_STOCK, INSUFFICIENT_FUNDS, CHANGE_UNAVAILABLE, the
    // atomicity of a failed purchase) are already covered on the aggregate in
    // VM.Server.Domain.Tests.VendingMachineTests - not duplicated here.
    private static VendingService CreateService(
        Product product, IReadOnlyDictionary<int, int> initialBank, int initialQuantityPerSlot = 10)
    {
        var source = new FakeExternalCatalogSource([product]);
        var store = new InMemoryVendingMachineStore(
            source,
            Options.Create(new VendingMachineOptions
            {
                InitialQuantityPerSlot = initialQuantityPerSlot,
                CoinBank = new Dictionary<int, int>(initialBank),
            }));
        return new VendingService(store, new BoundedChangeCalculator());
    }

    [Fact]
    public async Task GetDenominationsAsync_ReturnsExactlyTheSixAcceptedValuesAscending()
    {
        var service = CreateService(Product.Create("Espresso", 145), new Dictionary<int, int>());

        var denominations = await service.GetDenominationsAsync();

        denominations.Should().Equal(5, 10, 20, 50, 100, 200);
    }

    [Fact]
    public async Task PurchaseAsync_OnSuccess_MapsAConsistentDto()
    {
        var product = Product.Create("Espresso", 145);
        var service = CreateService(product, new Dictionary<int, int> { [50] = 2, [5] = 2 });
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
    public async Task PurchaseAsync_UsesTheRealBoundedChangeCalculator_ReturnsCorrectCoinsForAKnownBank()
    {
        // 60c change from a bank of one 50c coin and three 20c coins is the
        // canonical greedy counterexample (BoundedChangeCalculatorTests) -
        // this proves the real calculator is wired in, not a stub that would
        // happily return any shape.
        var product = Product.Create("Water", 140);
        var service = CreateService(product, new Dictionary<int, int> { [50] = 1, [20] = 3 });
        await service.InsertCoinAsync(200);

        var result = await service.PurchaseAsync(product.Id);

        result.ChangeCoins.Should().Equal(new CoinCountDto(20, 3));
    }

    [Fact]
    public async Task PurchaseAsync_ChangeCoins_AreSortedByDenominationDescending()
    {
        var product = Product.Create("Snack", 65);
        var service = CreateService(product, new Dictionary<int, int> { [20] = 1, [10] = 1, [5] = 1 });
        await service.InsertCoinAsync(100);

        var result = await service.PurchaseAsync(product.Id);

        result.ChangeCoins.Should().HaveCountGreaterThan(1);
        result.ChangeCoins.Should().BeInDescendingOrder(coin => coin.DenominationCents);
    }

    [Fact]
    public async Task PurchaseAsync_WhenAggregateThrows_SurfacesTheDomainExceptionUnchanged()
    {
        var product = Product.Create("Espresso", 145);
        var service = CreateService(product, new Dictionary<int, int>());

        var act = () => service.PurchaseAsync(Guid.NewGuid());

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task ResetAsync_ReturnsTheSameDenominationsInserted_SortedDescending()
    {
        var product = Product.Create("Espresso", 145);
        var service = CreateService(product, new Dictionary<int, int>());
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
}
