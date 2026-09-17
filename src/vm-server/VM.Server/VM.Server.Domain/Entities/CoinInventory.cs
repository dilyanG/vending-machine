namespace VM.Server.Domain.Entities;

public sealed class CoinInventory
{
    public Dictionary<int, int> Counts { get; internal set; } = [];

    public bool IsEmpty => Counts.Count == 0;

    public int TotalCents => Counts.Sum(coin => coin.Key * coin.Value);
}
