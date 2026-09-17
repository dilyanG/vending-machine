using VM.Server.Service.Vending;

namespace VM.Server.Service.Tests;

internal sealed class FakeChangeCalculator : IChangeCalculator
{
    public static FakeChangeCalculator AlwaysFails() => new();

    public ChangeResult Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins) => ChangeResult.NotPossible();
}
