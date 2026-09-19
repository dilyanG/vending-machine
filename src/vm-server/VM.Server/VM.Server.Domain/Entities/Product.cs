namespace VM.Server.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; init; }

    public string Name { get; internal set; } = string.Empty;

    public int PriceCents { get; internal set; }

    public string? ImageUrl { get; internal set; }
}
