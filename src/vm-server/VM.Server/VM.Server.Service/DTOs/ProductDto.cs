namespace VM.Server.Service.DTOs;

public sealed record ProductDto(Guid Id, string Name, int PriceCents, int Quantity, string? ImageUrl);
