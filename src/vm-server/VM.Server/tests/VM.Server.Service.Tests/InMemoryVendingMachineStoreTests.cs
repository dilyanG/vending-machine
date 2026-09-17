using FluentAssertions;
using Microsoft.Extensions.Options;
using VM.Server.Repository.InMemory;
using VM.Server.Service.Implementations;

namespace VM.Server.Service.Tests;

public class InMemoryVendingMachineStoreTests
{
    private static InMemoryVendingMachineStore CreateStore(FakeExternalCatalogSource source, int initialQuantityPerSlot = 10) =>
        new(
            new MachineStateService(source, new ProductValidationService()),
            Options.Create(new VendingMachineOptions { InitialQuantityPerSlot = initialQuantityPerSlot }));

    [Fact]
    public async Task AccessAsync_CalledConcurrentlyBeforeFirstLoad_ReadsTheCatalogueExactlyOnce()
    {
        var products = new[] { TestProducts.Create("Espresso", 145), TestProducts.Create("Latte", 195) };
        var source = new FakeExternalCatalogSource(products, TimeSpan.FromMilliseconds(50));
        var store = CreateStore(source);

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => store.AccessAsync(machine => machine.Slots.Count));
        var results = await Task.WhenAll(tasks);

        source.CallCount.Should().Be(1);
        results.Should().OnlyContain(count => count == 2);
    }

    [Fact]
    public async Task AccessAsync_MutatesTheSameAggregateAcrossCalls()
    {
        var product = TestProducts.Create("Espresso", 145);
        var source = new FakeExternalCatalogSource([product]);
        var store = CreateStore(source);

        await store.ExecuteAsync(machine => machine.InsertedCoins.Counts[50] = 1);
        var totalAfterSecondCall = await store.AccessAsync(machine => machine.InsertedTotalCents);

        totalAfterSecondCall.Should().Be(50);
    }

    [Fact]
    public async Task ReloadAsync_RebuildsTheAggregateFromTheCatalogueAgain()
    {
        var product = TestProducts.Create("Espresso", 145);
        var source = new FakeExternalCatalogSource([product]);
        var store = CreateStore(source);
        await store.ExecuteAsync(machine => machine.InsertedCoins.Counts[50] = 1);

        await store.ReloadAsync();

        var totalAfterReload = await store.AccessAsync(machine => machine.InsertedTotalCents);
        totalAfterReload.Should().Be(0);
        source.CallCount.Should().Be(2);
    }
}
