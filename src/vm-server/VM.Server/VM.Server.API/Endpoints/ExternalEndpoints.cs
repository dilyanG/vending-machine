using VM.Server.API.Dtos;
using VM.Server.Domain.Entities;
using VM.Server.Service.Abstractions;

namespace VM.Server.API.Endpoints;

public static class ExternalEndpoints
{
    public static IEndpointRouteBuilder MapExternalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external");

        group.MapGet("/catalog", GetCatalogAsync)
            .Produces<IReadOnlyList<ExternalProductDto>>();

        return endpoints;
    }

    private static async Task<IResult> GetCatalogAsync(
        IExternalCatalogSource catalogSource, CancellationToken cancellationToken)
    {
        var products = await catalogSource.GetCatalogueAsync(cancellationToken);
        var response = new List<ExternalProductDto>(products.Count);
        foreach (var product in products)
        {
            response.Add(ToDto(product));
        }

        return Results.Ok(response);
    }

    private static ExternalProductDto ToDto(Product product) =>
        new(product.Id, product.Name, product.PriceCents, product.ImageUrl);
}
