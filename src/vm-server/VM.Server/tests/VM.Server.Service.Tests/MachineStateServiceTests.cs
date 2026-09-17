using FluentAssertions;
using VM.Server.Domain.Errors;
using VM.Server.Service.Products;
using VM.Server.Service.State;

namespace VM.Server.Service.Tests;

public class MachineStateServiceTests
{
    private static MachineStateService CreateService(params VM.Server.Domain.Entities.Product[] catalogue) =>
        new(new FakeExternalCatalogSource(catalogue), new ProductValidationService());

    [Fact]
    public async Task LoadMachineAsync_CreatesOneSlotPerCatalogueProduct_AtGivenQuantity()
    {
        var products = new[] { TestProducts.Create("Espresso", 145), TestProducts.Create("Latte", 195) };
        var service = CreateService(products);

        var machine = await service.LoadMachineAsync(new Dictionary<int, int>(), 7);

        machine.Slots.Should().HaveCount(2);
        machine.Slots.Values.Should().OnlyContain(slot => slot.Quantity == 7);
        machine.Slots.Keys.Should().BeEquivalentTo(products.Select(p => p.Id));
    }

    [Fact]
    public async Task LoadMachineAsync_SeedsTheBankFromConfigurationSkippingZeroCounts()
    {
        var service = CreateService(TestProducts.Create("Espresso", 145));

        var machine = await service.LoadMachineAsync(new Dictionary<int, int> { [50] = 2, [10] = 0 }, 5);

        machine.Bank.Counts.Should().NotContainKey(10);
        machine.Bank.Counts[50].Should().Be(2);
    }

    [Fact]
    public async Task LoadMachineAsync_WithDuplicatePrices_ThrowsDuplicatePrice()
    {
        var service = CreateService(TestProducts.Create("Espresso", 145), TestProducts.Create("Latte", 145));

        var act = () => service.LoadMachineAsync(new Dictionary<int, int>(), 5);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.DuplicatePrice);
    }

    [Fact]
    public async Task LoadMachineAsync_WithDuplicateNamesCaseInsensitive_ThrowsDuplicateProduct()
    {
        var service = CreateService(TestProducts.Create("Espresso", 145), TestProducts.Create("ESPRESSO", 195));

        var act = () => service.LoadMachineAsync(new Dictionary<int, int>(), 5);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.DuplicateProduct);
    }

    [Fact]
    public async Task LoadMachineAsync_WithDuplicateIds_ThrowsDuplicateProduct()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var relabelled = TestProducts.Restore(espresso.Id, "Latte", 195);
        var service = CreateService(espresso, relabelled);

        var act = () => service.LoadMachineAsync(new Dictionary<int, int>(), 5);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.DuplicateProduct);
    }

    [Fact]
    public async Task LoadMachineAsync_WithUnacceptedBankDenomination_ThrowsInvalidDenomination()
    {
        var service = CreateService(TestProducts.Create("Espresso", 145));

        var act = () => service.LoadMachineAsync(new Dictionary<int, int> { [2] = 10 }, 5);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.InvalidDenomination);
    }

    [Fact]
    public async Task LoadMachineAsync_WithOutOfRangeInitialQuantity_ThrowsInvalidQuantity()
    {
        var service = CreateService(TestProducts.Create("Espresso", 145));

        var act = () => service.LoadMachineAsync(new Dictionary<int, int>(), 16);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.InvalidQuantity);
    }
}
