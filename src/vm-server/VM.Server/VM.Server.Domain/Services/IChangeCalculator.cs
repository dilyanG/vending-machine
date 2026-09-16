namespace VM.Server.Domain.Services;

public interface IChangeCalculator
{
    ChangeResult Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins);
}
