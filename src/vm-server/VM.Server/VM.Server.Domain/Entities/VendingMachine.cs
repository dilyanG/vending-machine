namespace VM.Server.Domain.Entities;

public sealed class VendingMachine
{
    public Dictionary<Guid, Slot> Slots { get; internal set; } = [];

    public CoinInventory Bank { get; internal set; } = null!;

    public CoinInventory InsertedCoins { get; internal set; } = new();

    public int InsertedTotalCents => InsertedCoins.TotalCents;
}
