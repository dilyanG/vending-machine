namespace VM.Server.Service.ServiceModels;

public sealed record ReturnedCoinsServiceModel(IReadOnlyList<CoinCountServiceModel> ReturnedCoins, int ReturnedTotalCents);
