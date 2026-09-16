using VM.Server.Domain.Entities;

namespace VM.Server.Service.Abstractions;

public interface IExternalCatalogSource
{
    Task<IReadOnlyList<Product>> GetCatalogueAsync(CancellationToken cancellationToken = default);
}
