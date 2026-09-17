namespace VM.Server.Service.DTOs;

public sealed record ChangeResultDto
{
    public bool IsSuccess { get; private init; }

    public IReadOnlyDictionary<int, int> Coins { get; private init; } = new Dictionary<int, int>();

    public int TotalCents => Coins.Sum(coin => coin.Key * coin.Value);

    public static ChangeResultDto Made(IReadOnlyDictionary<int, int> coins) => new() { IsSuccess = true, Coins = coins };

    public static ChangeResultDto NotPossible() => new() { IsSuccess = false };
}
