using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;
using VM.Server.Service.Abstractions;
using VM.Server.Service.ServiceModels;

namespace VM.Server.Service.Implementations;

public sealed class ProductService(IVendingMachineStore store, ProductValidationService validation)
{
    public Task<IReadOnlyList<ProductServiceModel>> ListAsync(CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine => (IReadOnlyList<ProductServiceModel>)machine.Slots.Values.Select(ToProductServiceModel).ToList(),
            cancellationToken);

    public Task<ProductServiceModel> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        store.AccessAsync(machine => ToProductServiceModel(FindSlotOrThrow(machine, id)), cancellationToken);

    public Task<ProductServiceModel> CreateAsync(
        string name, int priceCents, int quantity, string? imageUrl, CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine =>
            {
                var validatedName = validation.ValidateName(name);
                validation.ValidatePrice(priceCents);

                var product = new Product { Id = Guid.NewGuid(), Name = validatedName, PriceCents = priceCents, ImageUrl = imageUrl };
                validation.EnsureUnique(machine.Slots.Values, product.Id, product.Name, product.PriceCents, excludingProductId: null);
                validation.ValidateQuantity(quantity);

                var slot = new Slot { Product = product, Quantity = quantity };
                machine.Slots[product.Id] = slot;
                return ToProductServiceModel(slot);
            },
            cancellationToken);

    public Task<ProductServiceModel> UpdateAsync(
        Guid id, string name, int priceCents, int quantity, string? imageUrl, CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine =>
            {
                var validatedName = validation.ValidateName(name);
                validation.ValidatePrice(priceCents);

                if (!machine.Slots.ContainsKey(id))
                {
                    throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{id}' exists.");
                }

                validation.EnsureUnique(machine.Slots.Values, id, validatedName, priceCents, excludingProductId: id);
                validation.ValidateQuantity(quantity);

                var updatedProduct = new Product { Id = id, Name = validatedName, PriceCents = priceCents, ImageUrl = imageUrl };
                var slot = new Slot { Product = updatedProduct, Quantity = quantity };
                machine.Slots[id] = slot;
                return ToProductServiceModel(slot);
            },
            cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        store.ExecuteAsync(
            machine =>
            {
                if (!machine.Slots.Remove(id))
                {
                    throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{id}' exists.");
                }
            },
            cancellationToken);

    public Task ReloadAsync(CancellationToken cancellationToken = default) => store.ReloadAsync(cancellationToken);

    private static Slot FindSlotOrThrow(VendingMachine machine, Guid id) =>
        machine.Slots.GetValueOrDefault(id)
            ?? throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{id}' exists.");

    private static ProductServiceModel ToProductServiceModel(Slot slot) =>
        new(slot.Product.Id, slot.Product.Name, slot.Product.PriceCents, slot.Quantity, slot.Product.ImageUrl);
}
