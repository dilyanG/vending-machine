namespace VM.Server.Service.ServiceModels;

public sealed record PurchaseResultServiceModel(
    ProductServiceModel Product,
    int PaidCents,
    int PriceCents,
    int ChangeCents,
    IReadOnlyList<CoinCountServiceModel> ChangeCoins);
