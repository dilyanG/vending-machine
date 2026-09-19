using VM.Server.Service.ServiceModels;

namespace VM.Server.Service.Abstractions;

public interface IChangeCalculator
{
    ChangeResultServiceModel Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins);
}
