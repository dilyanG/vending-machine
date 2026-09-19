namespace VM.Server.Service.ServiceModels;

public sealed record ExternalProductServiceModel(Guid Id, string Name, int PriceCents, string? ImageUrl);
