using VM.Server.Service.ServiceModels;
using VM.Server.Domain.Entities;
using VM.Server.Service.Abstractions;

namespace VM.Server.API.Endpoints;

public static class ExternalEndpoints
{
    public static IEndpointRouteBuilder MapExternalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external");

        group.MapGet("/catalog", GetCatalogAsync)
            .Produces<IReadOnlyList<ExternalProductServiceModel>>();

        return endpoints;
    }

    private static async Task<IResult> GetCatalogAsync(
        IExternalCatalogSource catalogSource, CancellationToken cancellationToken)
    {
        var products = await catalogSource.GetCatalogueAsync(cancellationToken);
        var response = new List<ExternalProductServiceModel>(products.Count);
        foreach (var product in products)
        {
            response.Add(ToExternalProductServiceModel(product));
        }

        return Results.Ok(response);
    }

    private static ExternalProductServiceModel ToExternalProductServiceModel(Product product) =>
        new(product.Id, product.Name, product.PriceCents, product.ImageUrl);
}
