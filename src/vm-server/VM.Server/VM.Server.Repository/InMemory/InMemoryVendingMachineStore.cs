using Microsoft.Extensions.Options;
using VM.Server.Domain.Entities;
using VM.Server.Service.Abstractions;

namespace VM.Server.Repository.InMemory;

public sealed class InMemoryVendingMachineStore(
    IExternalCatalogSource catalogSource, IOptions<VendingMachineOptions> options) : IVendingMachineStore
{
    private readonly VendingMachineOptions _options = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private VendingMachine? _machine;

    public async Task<TResult> AccessAsync<TResult>(
        Func<VendingMachine, TResult> operation, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var machine = await EnsureLoadedAsync(cancellationToken);
            return operation(machine);
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task ExecuteAsync(Action<VendingMachine> operation, CancellationToken cancellationToken = default) =>
        AccessAsync(
            machine =>
            {
                operation(machine);
                return true;
            },
            cancellationToken);

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _machine = await LoadMachineAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<VendingMachine> EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        _machine ??= await LoadMachineAsync(cancellationToken);
        return _machine;
    }

    private async Task<VendingMachine> LoadMachineAsync(CancellationToken cancellationToken)
    {
        var catalogue = await catalogSource.GetCatalogueAsync(cancellationToken);
        return VendingMachine.Load(catalogue, _options.CoinBank, _options.InitialQuantityPerSlot);
    }
}
