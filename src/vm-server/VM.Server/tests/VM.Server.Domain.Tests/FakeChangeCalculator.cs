using VM.Server.Domain.Services;

namespace VM.Server.Domain.Tests;

internal sealed class FakeChangeCalculator(Func<int, IReadOnlyDictionary<int, int>, ChangeResult> onCalculate) : IChangeCalculator
{
    public int? LastAmountCents { get; private set; }

    public IReadOnlyDictionary<int, int>? LastAvailableCoins { get; private set; }

    public static FakeChangeCalculator AlwaysSucceedsWith(IReadOnlyDictionary<int, int> coins) =>
        new((_, _) => ChangeResult.Made(coins));

    public static FakeChangeCalculator AlwaysFails() => new((_, _) => ChangeResult.NotPossible());

    public ChangeResult Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins)
    {
        LastAmountCents = amountCents;
        LastAvailableCoins = availableCoins;
        return onCalculate(amountCents, availableCoins);
    }
}
