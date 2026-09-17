using Microsoft.Extensions.DependencyInjection;
using VM.Server.Repository.InMemory;
using VM.Server.Repository.MockExternalApi;
using VM.Server.Service.Abstractions;
using VM.Server.Service.Products;
using VM.Server.Service.State;
using VM.Server.Service.Vending;

namespace VM.Server.Repository;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVendingMachineBackend(this IServiceCollection services)
    {
        services.AddSingleton<IExternalCatalogSource>(_ => new FileExternalCatalogSource(
            Path.Combine(AppContext.BaseDirectory, "MockExternalApi", "catalogue.seed.json")));
        services.AddSingleton<ProductValidationService>();
        services.AddSingleton<MachineStateService>();
        services.AddSingleton<IVendingMachineStore, InMemoryVendingMachineStore>();
        services.AddSingleton<IChangeCalculator, ChangeCalculationService>();
        services.AddSingleton<ProductService>();
        services.AddSingleton<VendingService>();

        return services;
    }
}
