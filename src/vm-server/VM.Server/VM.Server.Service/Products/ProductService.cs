using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;
using VM.Server.Service.Abstractions;

namespace VM.Server.Service.Products;

public sealed class ProductService(IVendingMachineStore store)
{
    public Task<IReadOnlyList<ProductDto>> ListAsync(CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine => (IReadOnlyList<ProductDto>)machine.Slots.Select(ToDto).ToList(),
            cancellationToken);

    public Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        store.AccessAsync(machine => ToDto(FindSlotOrThrow(machine, id)), cancellationToken);

    public Task<ProductDto> CreateAsync(
        string name, int priceCents, int quantity, string? imageUrl, CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine =>
            {
                var product = Product.Create(name, priceCents, imageUrl);
                machine.AddSlot(product, quantity);
                return ToDto(machine.FindSlot(product.Id)!);
            },
            cancellationToken);

    public Task<ProductDto> UpdateAsync(
        Guid id, string name, int priceCents, int quantity, string? imageUrl, CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine =>
            {
                var updatedProduct = Product.Restore(id, name, priceCents, imageUrl);
                machine.UpdateSlot(id, updatedProduct, quantity);
                return ToDto(machine.FindSlot(id)!);
            },
            cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        store.ExecuteAsync(machine => machine.RemoveSlot(id), cancellationToken);

    public Task ReloadAsync(CancellationToken cancellationToken = default) => store.ReloadAsync(cancellationToken);

    private static Slot FindSlotOrThrow(VendingMachine machine, Guid id) =>
        machine.FindSlot(id) ?? throw new DomainException(ErrorCodes.ProductNotFound, $"No product with id '{id}' exists.");

    private static ProductDto ToDto(Slot slot) =>
        new(slot.Product.Id, slot.Product.Name, slot.Product.PriceCents, slot.Quantity, slot.Product.ImageUrl);
}
