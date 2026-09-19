namespace VM.Server.API.DTO;

public sealed record CreateProductRequestDTO(string Name, int PriceCents, int Quantity, string? ImageUrl);
