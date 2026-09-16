using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using VM.Server.Repository.InMemory;
using VM.Server.Repository.MockExternalApi;
using VM.Server.Service.Products;

namespace VM.Server.Service.Tests;

public class CatalogueFileImmutabilityTests
{
    private static readonly string SeedFilePath =
        Path.Combine(AppContext.BaseDirectory, "MockExternalApi", "catalogue.seed.json");

    [Fact]
    public async Task ProductCrud_NeverWritesBackToTheExternalCatalogueFile()
    {
        File.Exists(SeedFilePath).Should().BeTrue("the seed file must ship next to the test binary");
        var hashBefore = await HashFileAsync(SeedFilePath);

        var catalogSource = new FileExternalCatalogSource(SeedFilePath);
        var store = new InMemoryVendingMachineStore(
            catalogSource, Options.Create(new VendingMachineOptions { InitialQuantityPerSlot = 10 }));
        var service = new ProductService(store);

        var original = await service.ListAsync();
        original.Should().HaveCount(6);

        var created = await service.CreateAsync("Iced Tea", 165, 8, "assets/products/iced-tea.svg");
        await service.UpdateAsync(created.Id, "Iced Tea Lemon", 165, 6, "assets/products/iced-tea-lemon.svg");
        await service.DeleteAsync(original[0].Id);

        var hashAfter = await HashFileAsync(SeedFilePath);
        hashAfter.Should().BeEquivalentTo(hashBefore, "CRUD must never write back to the external catalogue file");

        await service.ReloadAsync();
        var reloaded = await service.ListAsync();
        reloaded.Select(p => p.Id).Should().BeEquivalentTo(original.Select(p => p.Id));
    }

    private static async Task<byte[]> HashFileAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        return await SHA256.HashDataAsync(stream);
    }
}
