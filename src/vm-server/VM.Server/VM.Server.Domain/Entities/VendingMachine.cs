using VM.Server.Domain.Errors;
using VM.Server.Domain.Services;

namespace VM.Server.Domain.Entities;

public sealed class VendingMachine
{
    private readonly Dictionary<Guid, Slot> _slots;

    private VendingMachine(Dictionary<Guid, Slot> slots, CoinInventory bank)
    {
        _slots = slots;
        Bank = bank;
        InsertedCoins = CoinInventory.Empty();
    }

    public IReadOnlyCollection<Slot> Slots => _slots.Values;

    public CoinInventory Bank { get; }

    public CoinInventory InsertedCoins { get; }

    public int InsertedTotalCents => InsertedCoins.TotalCents;

    public static VendingMachine Load(
        IEnumerable<Product> catalogue,
        IReadOnlyDictionary<int, int> initialBank,
        int initialQuantityPerSlot)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(initialBank);

        var slots = new Dictionary<Guid, Slot>();
        foreach (var product in catalogue)
        {
            EnsureSlotInvariants(slots.Values, product, excludingProductId: null);
            var slot = Slot.Create(product, initialQuantityPerSlot);
            slots[slot.ProductId] = slot;
        }

        return new VendingMachine(slots, CoinInventory.From(initialBank));
    }

    public Slot? FindSlot(Guid productId) => _slots.GetValueOrDefault(productId);

    public void AddSlot(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        EnsureSlotInvariants(_slots.Values, product, excludingProductId: null);

        var slot = Slot.Create(product, quantity);
        _slots[slot.ProductId] = slot;
    }

    public void RemoveSlot(Guid productId)
    {
        if (!_slots.Remove(productId))
        {
            throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{productId}' exists.");
        }
    }

    public void UpdateSlot(Guid productId, Product updatedProduct, int quantity)
    {
        ArgumentNullException.ThrowIfNull(updatedProduct);
        if (!_slots.ContainsKey(productId))
        {
            throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{productId}' exists.");
        }

        EnsureSlotInvariants(_slots.Values, updatedProduct, excludingProductId: productId);

        _slots.Remove(productId);
        var slot = Slot.Create(updatedProduct, quantity);
        _slots[slot.ProductId] = slot;
    }

    public void InsertCoin(int denominationCents) => InsertedCoins.Add(denominationCents);

    public PurchaseResult Purchase(Guid productId, IChangeCalculator changeCalculator)
    {
        ArgumentNullException.ThrowIfNull(changeCalculator);

        var slot = FindSlot(productId)
            ?? throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{productId}' exists.");

        if (!slot.IsAvailable)
        {
            throw new DomainException(ErrorCodes.OutOfStock, $"Product '{slot.Product.Name}' is out of stock.");
        }

        var priceCents = slot.Product.PriceCents;
        if (InsertedTotalCents < priceCents)
        {
            throw new DomainException(
                ErrorCodes.InsufficientFunds,
                "Inserted amount is less than the product price.",
                new Dictionary<string, object> { ["priceCents"] = priceCents, ["insertedCents"] = InsertedTotalCents });
        }

        var changeAmountCents = InsertedTotalCents - priceCents;
        var availableCoins = MergeSnapshots(Bank.ToSnapshot(), InsertedCoins.ToSnapshot());
        var changeResult = changeCalculator.Calculate(changeAmountCents, availableCoins);

        if (!changeResult.IsSuccess)
        {
            throw new DomainException(
                ErrorCodes.ChangeUnavailable,
                "The machine cannot give exact change for this purchase.",
                new Dictionary<string, object> { ["shortfallCents"] = changeAmountCents });
        }

        var paidCents = InsertedTotalCents;

        Bank.AddAll(InsertedCoins);
        foreach (var (denomination, count) in changeResult.Coins)
        {
            Bank.Remove(denomination, count);
        }

        slot.Dispense();
        InsertedCoins.Clear();

        return new PurchaseResult(slot, paidCents, priceCents, changeResult.Coins);
    }

    public IReadOnlyDictionary<int, int> ReturnInsertedCoins()
    {
        var returned = InsertedCoins.ToSnapshot();
        InsertedCoins.Clear();
        return returned;
    }

    private static void EnsureSlotInvariants(IEnumerable<Slot> existingSlots, Product candidate, Guid? excludingProductId)
    {
        foreach (var slot in existingSlots)
        {
            if (excludingProductId is not null && slot.ProductId == excludingProductId)
            {
                continue;
            }

            if (slot.ProductId == candidate.Id)
            {
                throw new DomainException(
                    ErrorCodes.DuplicateProduct,
                    $"A product with id '{candidate.Id}' already occupies a slot.",
                    new Dictionary<string, object> { ["productId"] = candidate.Id });
            }

            if (slot.Product.PriceCents == candidate.PriceCents)
            {
                throw new DomainException(
                    ErrorCodes.DuplicatePrice,
                    $"Price {candidate.PriceCents} cents is already used by another product.",
                    new Dictionary<string, object> { ["priceCents"] = candidate.PriceCents });
            }
        }
    }

    private static IReadOnlyDictionary<int, int> MergeSnapshots(
        IReadOnlyDictionary<int, int> first, IReadOnlyDictionary<int, int> second)
    {
        var merged = new Dictionary<int, int>(first);
        foreach (var (denomination, count) in second)
        {
            merged[denomination] = merged.GetValueOrDefault(denomination) + count;
        }

        return merged;
    }
}
