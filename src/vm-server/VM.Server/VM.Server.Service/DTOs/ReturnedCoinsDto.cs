namespace VM.Server.Service.DTOs;

public sealed record ReturnedCoinsDto(IReadOnlyList<CoinCountDto> ReturnedCoins, int ReturnedTotalCents);
