namespace VM.Server.Service.ServiceModels;

public sealed record ProductServiceModel(Guid Id, string Name, int PriceCents, int Quantity, string? ImageUrl);
