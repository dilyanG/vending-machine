namespace VM.Server.Service.Products;

public sealed record ProductDto(Guid Id, string Name, int PriceCents, int Quantity, string? ImageUrl);
