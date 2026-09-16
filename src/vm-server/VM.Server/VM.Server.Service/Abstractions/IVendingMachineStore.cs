using VM.Server.Domain.Entities;

namespace VM.Server.Service.Abstractions;

public interface IVendingMachineStore
{
    Task<TResult> AccessAsync<TResult>(Func<VendingMachine, TResult> operation, CancellationToken cancellationToken = default);

    Task ExecuteAsync(Action<VendingMachine> operation, CancellationToken cancellationToken = default);

    Task ReloadAsync(CancellationToken cancellationToken = default);
}
