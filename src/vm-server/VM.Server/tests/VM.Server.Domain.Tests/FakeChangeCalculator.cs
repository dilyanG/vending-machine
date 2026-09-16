using VM.Server.Domain.Services;

namespace VM.Server.Domain.Tests;

internal sealed class FakeChangeCalculator : IChangeCalculator
{
    public static FakeChangeCalculator AlwaysFails() => new();

    public ChangeResult Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins) => ChangeResult.NotPossible();
}
