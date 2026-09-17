using VM.Server.Service.Abstractions;
using VM.Server.Service.DTOs;

namespace VM.Server.Service.Tests;

internal sealed class FakeChangeCalculator : IChangeCalculator
{
    public static FakeChangeCalculator AlwaysFails() => new();

    public ChangeResultDto Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins) => ChangeResultDto.NotPossible();
}
