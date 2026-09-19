using VM.Server.API.DTO;
using VM.Server.Service.ServiceModels;
using VM.Server.Service.Implementations;

namespace VM.Server.API.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products");

        group.MapGet("", ListAsync)
            .Produces<IReadOnlyList<ProductServiceModel>>();

        group.MapGet("/{id:guid}", GetAsync)
            .Produces<ProductServiceModel>()
            .Produces<ErrorResponseDTO>(StatusCodes.Status404NotFound);

        group.MapPost("", CreateAsync)
            .Produces<ProductServiceModel>(StatusCodes.Status201Created)
            .Produces<ErrorResponseDTO>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDTO>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateAsync)
            .Produces<ProductServiceModel>()
            .Produces<ErrorResponseDTO>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDTO>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDTO>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponseDTO>(StatusCodes.Status404NotFound);

        group.MapPost("/reload", ReloadAsync)
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static async Task<IResult> ListAsync(ProductService products, CancellationToken cancellationToken) =>
        Results.Ok(await products.ListAsync(cancellationToken));

    private static async Task<IResult> GetAsync(Guid id, ProductService products, CancellationToken cancellationToken) =>
        Results.Ok(await products.GetAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(
        CreateProductRequestDTO request, ProductService products, CancellationToken cancellationToken)
    {
        var created = await products.CreateAsync(request.Name, request.PriceCents, request.Quantity, request.ImageUrl, cancellationToken);
        return Results.Created($"/api/products/{created.Id}", created);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id, UpdateProductRequestDTO request, ProductService products, CancellationToken cancellationToken)
    {
        var updated = await products.UpdateAsync(id, request.Name, request.PriceCents, request.Quantity, request.ImageUrl, cancellationToken);
        return Results.Ok(updated);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ProductService products, CancellationToken cancellationToken)
    {
        await products.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ReloadAsync(ProductService products, CancellationToken cancellationToken)
    {
        await products.ReloadAsync(cancellationToken);
        return Results.NoContent();
    }
}
