namespace VM.Server.Service.DTOs;

public sealed record SessionDto(IReadOnlyList<CoinCountDto> InsertedCoins, int InsertedTotalCents);
