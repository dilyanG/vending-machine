using VM.Server.Domain.Entities;

namespace VM.Server.Service.Tests;

/// <summary>
/// Product/Slot are plain data carriers post-refactor (no factories, no
/// validation) - this is the one place tests build them, so a boundary-value
/// scenario doesn't need to repeat the object-initializer shape everywhere.
/// </summary>
internal static class TestProducts
{
    public static Product Create(string name, int priceCents, string? imageUrl = null) =>
        new() { Id = Guid.NewGuid(), Name = name, PriceCents = priceCents, ImageUrl = imageUrl };

    public static Product Restore(Guid id, string name, int priceCents, string? imageUrl = null) =>
        new() { Id = id, Name = name, PriceCents = priceCents, ImageUrl = imageUrl };

    public static Slot Slot(Product product, int quantity) => new() { Product = product, Quantity = quantity };
}
