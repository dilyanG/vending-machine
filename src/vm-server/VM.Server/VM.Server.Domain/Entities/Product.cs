using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int PriceCents { get; private set; }

    public string? ImageUrl { get; private set; }

    private Product(Guid id, string name, int priceCents, string? imageUrl)
    {
        Id = id;
        Name = name;
        PriceCents = priceCents;
        ImageUrl = imageUrl;
    }

    public static Product Create(string name, int priceCents, string? imageUrl = null)
    {
        var validatedName = ValidateName(name);
        ValidatePrice(priceCents);
        return new Product(Guid.NewGuid(), validatedName, priceCents, imageUrl);
    }

    public static Product Restore(Guid id, string name, int priceCents, string? imageUrl = null)
    {
        var validatedName = ValidateName(name);
        ValidatePrice(priceCents);
        return new Product(id, validatedName, priceCents, imageUrl);
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void ChangePrice(int priceCents)
    {
        ValidatePrice(priceCents);
        PriceCents = priceCents;
    }

    private static string ValidateName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new DomainException(ErrorCodes.InvalidProduct, "Product name must not be empty.");
        }

        return trimmed;
    }

    private static void ValidatePrice(int priceCents)
    {
        if (priceCents <= 0 || priceCents % 5 != 0)
        {
            throw new DomainException(
                ErrorCodes.InvalidPrice,
                "Product price must be a positive multiple of 5 cents.",
                new Dictionary<string, object> { ["priceCents"] = priceCents });
        }
    }
}
