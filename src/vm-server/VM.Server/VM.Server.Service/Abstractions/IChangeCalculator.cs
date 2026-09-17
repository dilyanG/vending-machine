using VM.Server.Service.DTOs;

namespace VM.Server.Service.Abstractions;

public interface IChangeCalculator
{
    ChangeResultDto Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins);
}
