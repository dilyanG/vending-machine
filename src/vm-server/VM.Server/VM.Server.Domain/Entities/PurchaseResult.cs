namespace VM.Server.Domain.Entities;

public sealed record PurchaseResult(Slot Slot, int PaidCents, int PriceCents, IReadOnlyDictionary<int, int> ChangeCoins)
{
    public int ChangeCents => ChangeCoins.Sum(coin => coin.Key * coin.Value);
}
