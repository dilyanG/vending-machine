using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Entities;

public sealed class Slot
{
    public Product Product { get; private set; }

    public int Quantity { get; private set; }

    public Guid ProductId => Product.Id;

    public bool IsAvailable => Quantity > 0;

    private Slot(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }

    public static Slot Create(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        ValidateQuantity(quantity);
        return new Slot(product, quantity);
    }

    public void Dispense()
    {
        if (Quantity == 0)
        {
            throw new DomainException(ErrorCodes.OutOfStock, $"Product '{Product.Name}' is out of stock.");
        }

        Quantity--;
    }

    public void Restock(int quantity)
    {
        ValidateQuantity(quantity);
        Quantity = quantity;
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity < 0 || quantity > CoinDenominations.MaxQuantityPerProduct)
        {
            throw new DomainException(
                ErrorCodes.InvalidQuantity,
                $"Slot quantity must be between 0 and {CoinDenominations.MaxQuantityPerProduct}.",
                new Dictionary<string, object> { ["quantity"] = quantity });
        }
    }
}
