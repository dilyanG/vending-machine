namespace VM.Server.Service.DTOs;

public sealed record PurchaseResultDto(
    ProductDto Product,
    int PaidCents,
    int PriceCents,
    int ChangeCents,
    IReadOnlyList<CoinCountDto> ChangeCoins);
