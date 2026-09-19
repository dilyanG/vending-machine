namespace VM.Server.Domain.Entities;

public sealed class Slot
{
    public Product Product { get; internal set; } = null!;

    public int Quantity { get; internal set; }

    public Guid ProductId => Product.Id;

    public bool IsAvailable => Quantity > 0;
}
