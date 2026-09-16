namespace VM.Server.Domain.Services;

public sealed record ChangeResult
{
    public bool IsSuccess { get; private init; }

    public IReadOnlyDictionary<int, int> Coins { get; private init; } = new Dictionary<int, int>();

    public static ChangeResult Success(IReadOnlyDictionary<int, int> coins) => new() { IsSuccess = true, Coins = coins };

    public static ChangeResult Failure() => new() { IsSuccess = false };
}
