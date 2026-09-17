using VM.Server.Domain;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;
using VM.Server.Service.Abstractions;
using VM.Server.Service.Products;

namespace VM.Server.Service.Vending;

/// <summary>
/// Owns the vending state transitions that used to live on the aggregate:
/// insert coin, purchase, reset. <see cref="Purchase"/> preserves the
/// compute-then-commit ordering exactly - find slot, check stock, check
/// funds, compute change, and only once every check has passed does it
/// mutate anything. There is no rollback code anywhere because a failed
/// purchase never touches state in the first place.
/// </summary>
public sealed class VendingService(IVendingMachineStore store, IChangeCalculator changeCalculator)
{
    public Task<IReadOnlyList<int>> GetDenominationsAsync(CancellationToken cancellationToken = default)
    {
        var denominations = new List<int>(CoinDenominations.Accepted);
        denominations.Sort();
        return Task.FromResult<IReadOnlyList<int>>(denominations);
    }

    public Task<SessionDto> GetSessionAsync(CancellationToken cancellationToken = default) =>
        store.AccessAsync(ToSessionDto, cancellationToken);

    public Task<SessionDto> InsertCoinAsync(int denominationCents, CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine =>
            {
                InsertCoin(machine, denominationCents);
                return ToSessionDto(machine);
            },
            cancellationToken);

    public Task<PurchaseResultDto> PurchaseAsync(Guid productId, CancellationToken cancellationToken = default) =>
        store.AccessAsync(machine => ToPurchaseResultDto(Purchase(machine, productId)), cancellationToken);

    public Task<ReturnedCoinsDto> ResetAsync(CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine =>
            {
                var totalBeforeReset = machine.InsertedTotalCents;
                var returnedCoins = ReturnInsertedCoins(machine);
                return new ReturnedCoinsDto(ToSortedCoinList(returnedCoins), totalBeforeReset);
            },
            cancellationToken);

    private static void InsertCoin(VendingMachine machine, int denominationCents)
    {
        EnsureAcceptedDenomination(denominationCents);
        machine.InsertedCoins.Counts[denominationCents] = machine.InsertedCoins.Counts.GetValueOrDefault(denominationCents) + 1;
    }

    private PurchaseResult Purchase(VendingMachine machine, Guid productId)
    {
        // find slot -> check stock -> check funds -> compute change ->
        // only if all four succeed, mutate. Nothing below this comment may
        // run before every check above it has passed.
        if (!machine.Slots.TryGetValue(productId, out var slot))
        {
            throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{productId}' exists.");
        }

        if (!slot.IsAvailable)
        {
            throw new DomainException(ErrorCodes.OutOfStock, $"Product '{slot.Product.Name}' is out of stock.");
        }

        var priceCents = slot.Product.PriceCents;
        var insertedTotalCents = machine.InsertedCoins.TotalCents;
        if (insertedTotalCents < priceCents)
        {
            throw new DomainException(
                ErrorCodes.InsufficientFunds,
                "Inserted amount is less than the product price.",
                new Dictionary<string, object> { ["priceCents"] = priceCents, ["insertedCents"] = insertedTotalCents });
        }

        var changeAmountCents = insertedTotalCents - priceCents;
        var availableCoins = MergeCounts(machine.Bank.Counts, machine.InsertedCoins.Counts);
        var changeResult = changeCalculator.Calculate(changeAmountCents, availableCoins);

        if (!changeResult.IsSuccess)
        {
            throw new DomainException(
                ErrorCodes.ChangeUnavailable,
                "The machine cannot give exact change for this purchase.",
                new Dictionary<string, object> { ["shortfallCents"] = changeAmountCents });
        }

        // Every check has passed - commit.
        var paidCents = insertedTotalCents;

        foreach (var (denomination, count) in machine.InsertedCoins.Counts)
        {
            machine.Bank.Counts[denomination] = machine.Bank.Counts.GetValueOrDefault(denomination) + count;
        }

        foreach (var (denomination, count) in changeResult.Coins)
        {
            var remaining = machine.Bank.Counts[denomination] - count;
            if (remaining == 0)
            {
                machine.Bank.Counts.Remove(denomination);
            }
            else
            {
                machine.Bank.Counts[denomination] = remaining;
            }
        }

        slot.Quantity--;
        machine.InsertedCoins.Counts.Clear();

        return new PurchaseResult(slot, paidCents, priceCents, changeResult.Coins);
    }

    private static IReadOnlyDictionary<int, int> ReturnInsertedCoins(VendingMachine machine)
    {
        var returned = new Dictionary<int, int>(machine.InsertedCoins.Counts);
        machine.InsertedCoins.Counts.Clear();
        return returned;
    }

    private static void EnsureAcceptedDenomination(int denominationCents)
    {
        if (!CoinDenominations.IsAccepted(denominationCents))
        {
            throw new DomainException(
                ErrorCodes.InvalidDenomination,
                $"{denominationCents} cents is not an accepted coin denomination.",
                new Dictionary<string, object> { ["denominationCents"] = denominationCents });
        }
    }

    private static IReadOnlyDictionary<int, int> MergeCounts(
        IReadOnlyDictionary<int, int> first, IReadOnlyDictionary<int, int> second)
    {
        var merged = new Dictionary<int, int>(first);
        foreach (var (denomination, count) in second)
        {
            merged[denomination] = merged.GetValueOrDefault(denomination) + count;
        }

        return merged;
    }

    private static SessionDto ToSessionDto(VendingMachine machine) =>
        new(ToSortedCoinList(machine.InsertedCoins.Counts), machine.InsertedTotalCents);

    private static PurchaseResultDto ToPurchaseResultDto(PurchaseResult result) =>
        new(
            ToProductDto(result.Slot),
            result.PaidCents,
            result.PriceCents,
            result.ChangeCents,
            ToSortedCoinList(result.ChangeCoins));

    private static ProductDto ToProductDto(Slot slot) =>
        new(slot.Product.Id, slot.Product.Name, slot.Product.PriceCents, slot.Quantity, slot.Product.ImageUrl);

    private static IReadOnlyList<CoinCountDto> ToSortedCoinList(IReadOnlyDictionary<int, int> coins)
    {
        var list = new List<CoinCountDto>(coins.Count);
        foreach (var (denomination, count) in coins)
        {
            list.Add(new CoinCountDto(denomination, count));
        }

        list.Sort((a, b) => b.DenominationCents.CompareTo(a.DenominationCents));
        return list;
    }
}
