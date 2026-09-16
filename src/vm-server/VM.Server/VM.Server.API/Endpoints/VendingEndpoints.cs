using VM.Server.API.Dtos;
using VM.Server.Service.Vending;

namespace VM.Server.API.Endpoints;

public static class VendingEndpoints
{
    public static IEndpointRouteBuilder MapVendingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/vending");

        group.MapGet("/denominations", GetDenominationsAsync)
            .Produces<IReadOnlyList<int>>();

        group.MapGet("/session", GetSessionAsync)
            .Produces<SessionDto>();

        group.MapPost("/coins", InsertCoinAsync)
            .Produces<SessionDto>()
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);

        group.MapPost("/purchase", PurchaseAsync)
            .Produces<PurchaseResultDto>()
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDto>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/reset", ResetAsync)
            .Produces<ReturnedCoinsDto>();

        return endpoints;
    }

    private static async Task<IResult> GetDenominationsAsync(VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.GetDenominationsAsync(cancellationToken));

    private static async Task<IResult> GetSessionAsync(VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.GetSessionAsync(cancellationToken));

    private static async Task<IResult> InsertCoinAsync(
        InsertCoinRequest request, VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.InsertCoinAsync(request.DenominationCents, cancellationToken));

    private static async Task<IResult> PurchaseAsync(
        PurchaseRequest request, VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.PurchaseAsync(request.ProductId, cancellationToken));

    private static async Task<IResult> ResetAsync(VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.ResetAsync(cancellationToken));
}
