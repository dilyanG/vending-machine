using VM.Server.Domain;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;

namespace VM.Server.Service.Implementations;

/// <summary>
/// The one place every product/slot rule lives: name, price, quantity and the
/// cross-slot uniqueness rules. No other service re-implements any of these
/// checks - they call this one.
/// </summary>
public sealed class ProductValidationService
{
    public string ValidateName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new DomainException(ErrorCodes.InvalidProduct, "Product name must not be empty.");
        }

        return trimmed;
    }

    public void ValidatePrice(int priceCents)
    {
        if (priceCents <= 0 || priceCents % 5 != 0)
        {
            throw new DomainException(
                ErrorCodes.InvalidPrice,
                "Product price must be a positive multiple of 5 cents.",
                new Dictionary<string, object> { ["priceCents"] = priceCents });
        }
    }

    public void ValidateQuantity(int quantity)
    {
        if (quantity < 0 || quantity > CoinDenominations.MaxQuantityPerProduct)
        {
            throw new DomainException(
                ErrorCodes.InvalidQuantity,
                $"Slot quantity must be between 0 and {CoinDenominations.MaxQuantityPerProduct}.",
                new Dictionary<string, object> { ["quantity"] = quantity });
        }
    }

    public void EnsureUnique(
        IEnumerable<Slot> existingSlots, Guid candidateId, string candidateName, int candidatePriceCents, Guid? excludingProductId)
    {
        foreach (var slot in existingSlots)
        {
            if (excludingProductId is not null && slot.ProductId == excludingProductId)
            {
                continue;
            }

            if (slot.ProductId == candidateId)
            {
                throw new DomainException(
                    ErrorCodes.DuplicateProduct,
                    $"A product with id '{candidateId}' already occupies a slot.",
                    new Dictionary<string, object> { ["productId"] = candidateId });
            }

            if (string.Equals(slot.Product.Name, candidateName, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(
                    ErrorCodes.DuplicateProduct,
                    $"A product named '{candidateName}' already exists.",
                    new Dictionary<string, object> { ["name"] = candidateName });
            }

            if (slot.Product.PriceCents == candidatePriceCents)
            {
                throw new DomainException(
                    ErrorCodes.DuplicatePrice,
                    $"Price {candidatePriceCents} cents is already used by another product.",
                    new Dictionary<string, object> { ["priceCents"] = candidatePriceCents });
            }
        }
    }
}
