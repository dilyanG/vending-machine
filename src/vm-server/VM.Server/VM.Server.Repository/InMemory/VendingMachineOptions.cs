namespace VM.Server.Repository.InMemory;

public sealed class VendingMachineOptions
{
    public const string SectionName = "VendingMachine";

    public Dictionary<int, int> CoinBank { get; set; } = [];

    public int InitialQuantityPerSlot { get; set; } = 10;
}
