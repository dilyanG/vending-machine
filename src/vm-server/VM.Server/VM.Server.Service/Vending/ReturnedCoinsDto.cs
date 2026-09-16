namespace VM.Server.Service.Vending;

public sealed record ReturnedCoinsDto(IReadOnlyList<CoinCountDto> ReturnedCoins, int ReturnedTotalCents);
