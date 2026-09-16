using VM.Server.Domain;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Services;
using VM.Server.Service.Abstractions;
using VM.Server.Service.Products;

namespace VM.Server.Service.Vending;

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
                machine.InsertCoin(denominationCents);
                return ToSessionDto(machine);
            },
            cancellationToken);

    public Task<PurchaseResultDto> PurchaseAsync(Guid productId, CancellationToken cancellationToken = default) =>
        store.AccessAsync(machine => ToPurchaseResultDto(machine.Purchase(productId, changeCalculator)), cancellationToken);

    public Task<ReturnedCoinsDto> ResetAsync(CancellationToken cancellationToken = default) =>
        store.AccessAsync(
            machine =>
            {
                var totalBeforeReset = machine.InsertedTotalCents;
                var returnedCoins = machine.ReturnInsertedCoins();
                return new ReturnedCoinsDto(ToSortedCoinList(returnedCoins), totalBeforeReset);
            },
            cancellationToken);

    private static SessionDto ToSessionDto(VendingMachine machine) =>
        new(ToSortedCoinList(machine.InsertedCoins.ToSnapshot()), machine.InsertedTotalCents);

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
