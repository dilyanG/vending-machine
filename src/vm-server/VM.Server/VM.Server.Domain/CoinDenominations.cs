namespace VM.Server.Domain;

public static class CoinDenominations
{
    public const int MaxQuantityPerProduct = 15;

    public static IReadOnlyList<int> Accepted { get; } = [200, 100, 50, 20, 10, 5];

    public static bool IsAccepted(int denominationCents) => Accepted.Contains(denominationCents);
}
