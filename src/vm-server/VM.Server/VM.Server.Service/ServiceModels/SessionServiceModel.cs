namespace VM.Server.Service.ServiceModels;

public sealed record SessionServiceModel(IReadOnlyList<CoinCountServiceModel> InsertedCoins, int InsertedTotalCents);
