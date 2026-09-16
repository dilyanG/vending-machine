namespace VM.Server.Domain.Services;

public sealed record ChangeResult
{
    public bool IsSuccess { get; private init; }

    public IReadOnlyDictionary<int, int> Coins { get; private init; } = new Dictionary<int, int>();

    public int TotalCents => Coins.Sum(coin => coin.Key * coin.Value);

    public static ChangeResult Made(IReadOnlyDictionary<int, int> coins) => new() { IsSuccess = true, Coins = coins };

    public static ChangeResult NotPossible() => new() { IsSuccess = false };
}
