using FluentAssertions;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Tests;

public class VendingMachineTests
{
    private static Product Espresso(int priceCents = 145) => Product.Create("Espresso", priceCents);

    private static VendingMachine LoadSingleProductMachine(
        int priceCents,
        int quantity,
        IReadOnlyDictionary<int, int> initialBank,
        out Product product)
    {
        product = Espresso(priceCents);
        return VendingMachine.Load([product], initialBank, quantity);
    }

    [Fact]
    public void Load_CreatesOneSlotPerCatalogueProduct_AtGivenQuantity()
    {
        var products = new[] { Product.Create("Espresso", 145), Product.Create("Latte", 195) };

        var machine = VendingMachine.Load(products, new Dictionary<int, int>(), 7);

        machine.Slots.Should().HaveCount(2);
        machine.Slots.Should().OnlyContain(slot => slot.Quantity == 7);
        machine.Slots.Select(slot => slot.ProductId).Should().BeEquivalentTo(products.Select(p => p.Id));
    }

    [Fact]
    public void Load_WithDuplicatePrices_ThrowsDuplicatePrice()
    {
        var products = new[] { Product.Create("Espresso", 145), Product.Create("Latte", 145) };

        var act = () => VendingMachine.Load(products, new Dictionary<int, int>(), 5);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.DuplicatePrice);
    }

    [Fact]
    public void Load_WithDuplicateIds_ThrowsDuplicateProduct()
    {
        var espresso = Product.Create("Espresso", 145);
        var relabelled = Product.Restore(espresso.Id, "Latte", 195);

        var act = () => VendingMachine.Load([espresso, relabelled], new Dictionary<int, int>(), 5);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.DuplicateProduct);
    }

    [Fact]
    public void Purchase_HappyPath_DecrementsSlotClearsSessionAndMovesCoinsIntoBank()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int> { [50] = 2, [5] = 2 }, out var product);
        machine.InsertCoin(200);
        var calculator = FakeChangeCalculator.AlwaysSucceedsWith(new Dictionary<int, int> { [50] = 1, [5] = 1 });

        var result = machine.Purchase(product.Id, calculator);

        result.Slot.Quantity.Should().Be(4);
        result.PaidCents.Should().Be(200);
        result.PriceCents.Should().Be(145);
        result.ChangeCents.Should().Be(55);
        machine.InsertedTotalCents.Should().Be(0);
        machine.InsertedCoins.IsEmpty.Should().BeTrue();
        machine.Bank.ToSnapshot().Should().BeEquivalentTo(new Dictionary<int, int> { [200] = 1, [50] = 1, [5] = 1 });
    }

    [Fact]
    public void Purchase_WithExactMoney_ChangeIsEmptyAndStillSucceeds()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int>(), out var product);
        machine.InsertCoin(100);
        machine.InsertCoin(20);
        machine.InsertCoin(20);
        machine.InsertCoin(5);
        var calculator = FakeChangeCalculator.AlwaysSucceedsWith(new Dictionary<int, int>());

        var result = machine.Purchase(product.Id, calculator);

        result.ChangeCoins.Should().BeEmpty();
        result.ChangeCents.Should().Be(0);
        result.Slot.Quantity.Should().Be(4);
    }

    [Fact]
    public void Purchase_ChangeDrawnFromCustomersOwnInsertedCoins_Succeeds()
    {
        // Bank starts empty; the only 50c coin available for change is the one the customer just inserted.
        var machine = LoadSingleProductMachine(200, 5, new Dictionary<int, int>(), out var product);
        machine.InsertCoin(200);
        machine.InsertCoin(50);
        var calculator = FakeChangeCalculator.AlwaysSucceedsWith(new Dictionary<int, int> { [50] = 1 });

        var result = machine.Purchase(product.Id, calculator);

        result.ChangeCents.Should().Be(50);
        machine.Bank.ToSnapshot().Should().BeEquivalentTo(new Dictionary<int, int> { [200] = 1 });
    }

    [Fact]
    public void Purchase_OffersBankPlusInsertedCoinsAsAvailableChangeToTheCalculator()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int> { [50] = 3, [5] = 2 }, out var product);
        machine.InsertCoin(200);
        var calculator = FakeChangeCalculator.AlwaysSucceedsWith(new Dictionary<int, int> { [50] = 1, [5] = 1 });

        machine.Purchase(product.Id, calculator);

        calculator.LastAmountCents.Should().Be(55);
        calculator.LastAvailableCoins.Should().BeEquivalentTo(new Dictionary<int, int> { [50] = 3, [5] = 2, [200] = 1 });
    }

    [Fact]
    public void Purchase_WhenProductNotFound_ThrowsAndLeavesStateUnchanged()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int> { [50] = 2 }, out var product);
        machine.InsertCoin(200);
        var bankBefore = machine.Bank.ToSnapshot();
        var insertedBefore = machine.InsertedCoins.ToSnapshot();
        var quantityBefore = machine.FindSlot(product.Id)!.Quantity;

        var act = () => machine.Purchase(Guid.NewGuid(), FakeChangeCalculator.AlwaysFails());

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.ProductNotFound);
        machine.FindSlot(product.Id)!.Quantity.Should().Be(quantityBefore);
        machine.Bank.ToSnapshot().Should().BeEquivalentTo(bankBefore);
        machine.InsertedCoins.ToSnapshot().Should().BeEquivalentTo(insertedBefore);
    }

    [Fact]
    public void Purchase_WhenOutOfStock_ThrowsAndLeavesStateUnchanged()
    {
        var machine = LoadSingleProductMachine(145, 0, new Dictionary<int, int> { [50] = 2 }, out var product);
        machine.InsertCoin(200);
        var bankBefore = machine.Bank.ToSnapshot();
        var insertedBefore = machine.InsertedCoins.ToSnapshot();

        var act = () => machine.Purchase(product.Id, FakeChangeCalculator.AlwaysFails());

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.OutOfStock);
        machine.FindSlot(product.Id)!.Quantity.Should().Be(0);
        machine.Bank.ToSnapshot().Should().BeEquivalentTo(bankBefore);
        machine.InsertedCoins.ToSnapshot().Should().BeEquivalentTo(insertedBefore);
    }

    [Fact]
    public void Purchase_WhenInsufficientFunds_ThrowsAndLeavesStateUnchanged()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int> { [50] = 2 }, out var product);
        machine.InsertCoin(100);
        var bankBefore = machine.Bank.ToSnapshot();
        var insertedBefore = machine.InsertedCoins.ToSnapshot();
        var quantityBefore = machine.FindSlot(product.Id)!.Quantity;

        var act = () => machine.Purchase(product.Id, FakeChangeCalculator.AlwaysFails());

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InsufficientFunds);
        machine.FindSlot(product.Id)!.Quantity.Should().Be(quantityBefore);
        machine.Bank.ToSnapshot().Should().BeEquivalentTo(bankBefore);
        machine.InsertedCoins.ToSnapshot().Should().BeEquivalentTo(insertedBefore);
    }

    [Fact]
    public void Purchase_WhenChangeUnavailable_ThrowsAndLeavesSlotBankAndSessionUnchanged()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int> { [50] = 2 }, out var product);
        machine.InsertCoin(200);
        var quantityBefore = machine.FindSlot(product.Id)!.Quantity;
        var bankBefore = machine.Bank.ToSnapshot();
        var insertedBefore = machine.InsertedCoins.ToSnapshot();

        var act = () => machine.Purchase(product.Id, FakeChangeCalculator.AlwaysFails());

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.ChangeUnavailable);
        machine.FindSlot(product.Id)!.Quantity.Should().Be(quantityBefore);
        machine.Bank.ToSnapshot().Should().BeEquivalentTo(bankBefore);
        machine.InsertedCoins.ToSnapshot().Should().BeEquivalentTo(insertedBefore);
        machine.InsertedTotalCents.Should().Be(200);
    }

    [Theory]
    [InlineData(150, 200, 50, 1)]
    [InlineData(200, 200, 0, 0)]
    public void Purchase_OnSuccess_PaidCentsEqualsPriceCentsPlusChangeCents(
        int priceCents, int insertedCents, int changeDenomination, int changeCount)
    {
        var changeCoins = changeCount == 0
            ? new Dictionary<int, int>()
            : new Dictionary<int, int> { [changeDenomination] = changeCount };
        var machine = LoadSingleProductMachine(priceCents, 5, new Dictionary<int, int> { [50] = 5, [5] = 5 }, out var product);
        foreach (var coin in DenominateInsert(insertedCents))
        {
            machine.InsertCoin(coin);
        }

        var result = machine.Purchase(product.Id, FakeChangeCalculator.AlwaysSucceedsWith(changeCoins));

        result.PaidCents.Should().Be(result.PriceCents + result.ChangeCents);
    }

    [Fact]
    public void InsertCoin_WithUnacceptedDenomination_ThrowsInvalidDenomination()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int>(), out _);

        var act = () => machine.InsertCoin(2);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidDenomination);
    }

    [Fact]
    public void ReturnInsertedCoins_ReturnsSameDenominationsAndClearsSessionLeavingBankAndSlotsUntouched()
    {
        var machine = LoadSingleProductMachine(145, 5, new Dictionary<int, int> { [50] = 2 }, out var product);
        machine.InsertCoin(200);
        machine.InsertCoin(50);
        var bankBefore = machine.Bank.ToSnapshot();
        var quantityBefore = machine.FindSlot(product.Id)!.Quantity;

        var returned = machine.ReturnInsertedCoins();

        returned.Should().BeEquivalentTo(new Dictionary<int, int> { [200] = 1, [50] = 1 });
        machine.InsertedCoins.IsEmpty.Should().BeTrue();
        machine.InsertedTotalCents.Should().Be(0);
        machine.Bank.ToSnapshot().Should().BeEquivalentTo(bankBefore);
        machine.FindSlot(product.Id)!.Quantity.Should().Be(quantityBefore);
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
}
