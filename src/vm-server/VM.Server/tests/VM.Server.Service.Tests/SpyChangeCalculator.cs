using VM.Server.Service.Vending;

namespace VM.Server.Service.Tests;

internal sealed class SpyChangeCalculator(IChangeCalculator inner) : IChangeCalculator
{
    public int? LastAmountCents { get; private set; }

    public IReadOnlyDictionary<int, int>? LastAvailableCoins { get; private set; }

    public ChangeResult Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins)
    {
        LastAmountCents = amountCents;
        LastAvailableCoins = availableCoins;
        return inner.Calculate(amountCents, availableCoins);
    }
}
