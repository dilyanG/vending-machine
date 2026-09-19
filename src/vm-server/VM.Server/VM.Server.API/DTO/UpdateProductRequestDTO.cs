namespace VM.Server.API.DTO;

public sealed record UpdateProductRequestDTO(string Name, int PriceCents, int Quantity, string? ImageUrl);
