namespace VM.Server.API.Dtos;

public sealed record CreateProductRequest(string Name, int PriceCents, int Quantity, string? ImageUrl);
