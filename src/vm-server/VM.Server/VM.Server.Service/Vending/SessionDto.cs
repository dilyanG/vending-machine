namespace VM.Server.Service.Vending;

public sealed record SessionDto(IReadOnlyList<CoinCountDto> InsertedCoins, int InsertedTotalCents);
