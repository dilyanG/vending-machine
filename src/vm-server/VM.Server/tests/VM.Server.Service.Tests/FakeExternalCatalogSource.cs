using VM.Server.Domain.Entities;
using VM.Server.Service.Abstractions;

namespace VM.Server.Service.Tests;

internal sealed class FakeExternalCatalogSource(IReadOnlyList<Product> catalogue, TimeSpan? delay = null) : IExternalCatalogSource
{
    private int _callCount;

    public int CallCount => _callCount;

    public async Task<IReadOnlyList<Product>> GetCatalogueAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _callCount);

        if (delay is { } actualDelay)
        {
            await Task.Delay(actualDelay, cancellationToken);
        }

        return catalogue;
    }
}
