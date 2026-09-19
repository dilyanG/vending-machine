using VM.Server.Domain;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;
using VM.Server.Service.Abstractions;

namespace VM.Server.Service.Implementations;

/// <summary>
/// What "loading a machine" means: pulling the catalogue from the external
/// source, assigning every slot the same configured starting quantity, and
/// seeding the coin bank - plus the invariants that must hold across the
/// whole collection while it's being built (uniqueness) or configured
/// (accepted denominations only).
/// </summary>
public sealed class MachineStateService(IExternalCatalogSource catalogSource, ProductValidationService productValidation)
{
    public async Task<VendingMachine> LoadMachineAsync(
        IReadOnlyDictionary<int, int> initialBank, int initialQuantityPerSlot, CancellationToken cancellationToken = default)
    {
        var catalogue = await catalogSource.GetCatalogueAsync(cancellationToken);

        productValidation.ValidateQuantity(initialQuantityPerSlot);

        var slots = new Dictionary<Guid, Slot>();
        foreach (var product in catalogue)
        {
            productValidation.EnsureUnique(slots.Values, product.Id, product.Name, product.PriceCents, excludingProductId: null);
            slots[product.Id] = new Slot { Product = product, Quantity = initialQuantityPerSlot };
        }

        return new VendingMachine
        {
            Slots = slots,
            Bank = new CoinInventory { Counts = BuildBankCounts(initialBank) },
            InsertedCoins = new CoinInventory(),
        };
    }

    private static Dictionary<int, int> BuildBankCounts(IReadOnlyDictionary<int, int> initialBank)
    {
        var counts = new Dictionary<int, int>();
        foreach (var (denomination, count) in initialBank)
        {
            if (count == 0)
            {
                continue;
            }

            if (!CoinDenominations.IsAccepted(denomination))
            {
                throw new DomainException(
                    ErrorCodes.InvalidDenomination,
                    $"{denomination} cents is not an accepted coin denomination.",
                    new Dictionary<string, object> { ["denominationCents"] = denomination });
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBank), count, "Coin count must be positive.");
            }

            counts[denomination] = count;
        }

        return counts;
    }
}
