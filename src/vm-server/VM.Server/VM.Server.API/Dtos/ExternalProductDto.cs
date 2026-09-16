namespace VM.Server.API.Dtos;

public sealed record ExternalProductDto(Guid Id, string Name, int PriceCents, string? ImageUrl);
