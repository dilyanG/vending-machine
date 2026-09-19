namespace VM.Server.Service.ServiceModels;

public sealed record ChangeResultServiceModel
{
    public bool IsSuccess { get; private init; }

    public IReadOnlyDictionary<int, int> Coins { get; private init; } = new Dictionary<int, int>();

    public int TotalCents => Coins.Sum(coin => coin.Key * coin.Value);

    public static ChangeResultServiceModel Made(IReadOnlyDictionary<int, int> coins) => new() { IsSuccess = true, Coins = coins };

    public static ChangeResultServiceModel NotPossible() => new() { IsSuccess = false };
}
