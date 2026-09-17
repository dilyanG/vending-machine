using FluentAssertions;
using Microsoft.Extensions.Options;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;
using VM.Server.Repository.InMemory;
using VM.Server.Service.Products;
using VM.Server.Service.State;

namespace VM.Server.Service.Tests;

public class ProductServiceTests
{
    private static ProductService CreateService(out FakeExternalCatalogSource source, params Product[] catalogue)
    {
        source = new FakeExternalCatalogSource(catalogue);
        var validation = new ProductValidationService();
        var machineState = new MachineStateService(source, validation);
        var store = new InMemoryVendingMachineStore(machineState, Options.Create(new VendingMachineOptions { InitialQuantityPerSlot = 10 }));
        return new ProductService(store, validation);
    }

    [Fact]
    public async Task ListAsync_ReturnsProductPlusSlotQuantity()
    {
        var espresso = TestProducts.Create("Espresso", 145, "assets/espresso.svg");
        var service = CreateService(out _, espresso);

        var products = await service.ListAsync();

        products.Should().ContainSingle(p =>
            p.Id == espresso.Id && p.Name == "Espresso" && p.PriceCents == 145 && p.Quantity == 10
            && p.ImageUrl == "assets/espresso.svg");
    }

    [Fact]
    public async Task GetAsync_WithUnknownId_ThrowsProductNotFound()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.GetAsync(Guid.NewGuid());

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task CreateAsync_IsReflectedInSubsequentListAsync()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var created = await service.CreateAsync("Latte", 195, 5, "assets/latte.svg");
        var products = await service.ListAsync();

        products.Should().Contain(p => p.Id == created.Id && p.Name == "Latte" && p.Quantity == 5);
    }

    [Fact]
    public async Task UpdateAsync_IsReflectedInSubsequentGetAsync()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var service = CreateService(out _, espresso);

        await service.UpdateAsync(espresso.Id, "Double Espresso", 165, 3, "assets/double-espresso.svg");
        var product = await service.GetAsync(espresso.Id);

        product.Name.Should().Be("Double Espresso");
        product.PriceCents.Should().Be(165);
        product.Quantity.Should().Be(3);
        product.ImageUrl.Should().Be("assets/double-espresso.svg");
    }

    [Fact]
    public async Task DeleteAsync_IsReflectedInSubsequentListAsync()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var service = CreateService(out _, espresso);

        await service.DeleteAsync(espresso.Id);
        var products = await service.ListAsync();

        products.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_ThrowsProductNotFound()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.DeleteAsync(Guid.NewGuid());

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateNameCaseInsensitive_ThrowsDuplicateProduct()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.CreateAsync("ESPRESSO", 195, 5, null);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.DuplicateProduct);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicatePrice_ThrowsDuplicatePrice()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.CreateAsync("Latte", 145, 5, null);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.DuplicatePrice);
    }

    [Fact]
    public async Task CreateAsync_WithQuantity16_ThrowsInvalidQuantity()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.CreateAsync("Latte", 195, 16, null);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.InvalidQuantity);
    }

    [Fact]
    public async Task CreateAsync_WithPrice3_ThrowsInvalidPrice()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.CreateAsync("Latte", 3, 5, null);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.InvalidPrice);
    }

    [Fact]
    public async Task CreateAsync_WithBlankName_ThrowsInvalidProduct()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.CreateAsync("   ", 195, 5, null);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.InvalidProduct);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ThrowsProductNotFound()
    {
        var service = CreateService(out _, TestProducts.Create("Espresso", 145));

        var act = () => service.UpdateAsync(Guid.NewGuid(), "Anything", 100, 1, null);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task ReloadAsync_DiscardsInMemoryEditsAndRestoresTheCatalogue()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var latte = TestProducts.Create("Latte", 195);
        var service = CreateService(out _, espresso, latte);

        await service.DeleteAsync(espresso.Id);
        await service.CreateAsync("Mocha", 175, 5, null);
        await service.ReloadAsync();
        var products = await service.ListAsync();

        products.Select(p => p.Id).Should().BeEquivalentTo([espresso.Id, latte.Id]);
    }
}
