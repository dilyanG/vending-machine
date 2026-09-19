using VM.Server.Service.Abstractions;
using VM.Server.Service.ServiceModels;

namespace VM.Server.Service.Tests;

internal sealed class FakeChangeCalculator : IChangeCalculator
{
    public static FakeChangeCalculator AlwaysFails() => new();

    public ChangeResultServiceModel Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins) => ChangeResultServiceModel.NotPossible();
}
