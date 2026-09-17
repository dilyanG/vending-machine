using VM.Server.Service.Products;

namespace VM.Server.Service.Vending;

public sealed record PurchaseResultDto(
    ProductDto Product,
    int PaidCents,
    int PriceCents,
    int ChangeCents,
    IReadOnlyList<CoinCountDto> ChangeCoins);
