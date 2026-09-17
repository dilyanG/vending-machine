namespace VM.Server.API.Dtos;

public sealed record UpdateProductRequest(string Name, int PriceCents, int Quantity, string? ImageUrl);
