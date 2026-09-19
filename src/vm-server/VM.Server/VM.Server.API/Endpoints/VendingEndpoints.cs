using VM.Server.API.DTO;
using VM.Server.Service.ServiceModels;
using VM.Server.Service.Implementations;

namespace VM.Server.API.Endpoints;

public static class VendingEndpoints
{
    public static IEndpointRouteBuilder MapVendingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/vending");

        group.MapGet("/denominations", GetDenominationsAsync)
            .Produces<IReadOnlyList<int>>();

        group.MapGet("/session", GetSessionAsync)
            .Produces<SessionServiceModel>();

        group.MapPost("/coins", InsertCoinAsync)
            .Produces<SessionServiceModel>()
            .Produces<ErrorResponseDTO>(StatusCodes.Status400BadRequest);

        group.MapPost("/purchase", PurchaseAsync)
            .Produces<PurchaseResultServiceModel>()
            .Produces<ErrorResponseDTO>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDTO>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDTO>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/reset", ResetAsync)
            .Produces<ReturnedCoinsServiceModel>();

        return endpoints;
    }

    private static async Task<IResult> GetDenominationsAsync(VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.GetDenominationsAsync(cancellationToken));

    private static async Task<IResult> GetSessionAsync(VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.GetSessionAsync(cancellationToken));

    private static async Task<IResult> InsertCoinAsync(
        InsertCoinRequestDTO request, VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.InsertCoinAsync(request.DenominationCents, cancellationToken));

    private static async Task<IResult> PurchaseAsync(
        PurchaseRequestDTO request, VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.PurchaseAsync(request.ProductId, cancellationToken));

    private static async Task<IResult> ResetAsync(VendingService vending, CancellationToken cancellationToken) =>
        Results.Ok(await vending.ResetAsync(cancellationToken));
}
