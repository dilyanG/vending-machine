namespace VM.Server.Service.DTOs;

public sealed record ExternalProductDto(Guid Id, string Name, int PriceCents, string? ImageUrl);
