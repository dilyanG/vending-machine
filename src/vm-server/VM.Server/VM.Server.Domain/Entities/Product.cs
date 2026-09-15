using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int PriceCents { get; private set; }

    public int Quantity { get; private set; }

    public string? ImageUrl { get; private set; }

    private Product(Guid id, string name, int priceCents, int quantity, string? imageUrl)
    {
        Id = id;
        Name = name;
        PriceCents = priceCents;
        Quantity = quantity;
        ImageUrl = imageUrl;
    }

    public static Product Create(string name, int priceCents, int quantity, string? imageUrl = null)
    {
        var validatedName = ValidateName(name);
        ValidatePrice(priceCents);
        ValidateQuantity(quantity);
        return new Product(Guid.NewGuid(), validatedName, priceCents, quantity, imageUrl);
    }

    public static Product Restore(Guid id, string name, int priceCents, int quantity, string? imageUrl = null)
    {
        var validatedName = ValidateName(name);
        ValidatePrice(priceCents);
        ValidateQuantity(quantity);
        return new Product(id, validatedName, priceCents, quantity, imageUrl);
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void ChangePrice(int priceCents)
    {
        ValidatePrice(priceCents);
        PriceCents = priceCents;
    }

    public void SetQuantity(int quantity)
    {
        ValidateQuantity(quantity);
        Quantity = quantity;
    }

    public void DecrementStock()
    {
        if (Quantity == 0)
        {
            throw new DomainException(ErrorCodes.OutOfStock, $"Product '{Name}' is out of stock.");
        }

        Quantity--;
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

    private static void ValidateQuantity(int quantity)
    {
        if (quantity < 0 || quantity > CoinDenominations.MaxQuantityPerProduct)
        {
            throw new DomainException(
                ErrorCodes.InvalidQuantity,
                $"Product quantity must be between 0 and {CoinDenominations.MaxQuantityPerProduct}.",
                new Dictionary<string, object> { ["quantity"] = quantity });
        }
    }
}
