using VM.Server.API.Dtos;
using VM.Server.Service.Products;

namespace VM.Server.API.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products");

        group.MapGet("", ListAsync)
            .Produces<IReadOnlyList<ProductDto>>();

        group.MapGet("/{id:guid}", GetAsync)
            .Produces<ProductDto>()
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound);

        group.MapPost("", CreateAsync)
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateAsync)
            .Produces<ProductDto>()
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDto>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound);

        group.MapPost("/reload", ReloadAsync)
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static async Task<IResult> ListAsync(ProductService products, CancellationToken cancellationToken) =>
        Results.Ok(await products.ListAsync(cancellationToken));

    private static async Task<IResult> GetAsync(Guid id, ProductService products, CancellationToken cancellationToken) =>
        Results.Ok(await products.GetAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request, ProductService products, CancellationToken cancellationToken)
    {
        var created = await products.CreateAsync(request.Name, request.PriceCents, request.Quantity, request.ImageUrl, cancellationToken);
        return Results.Created($"/api/products/{created.Id}", created);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id, UpdateProductRequest request, ProductService products, CancellationToken cancellationToken)
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
