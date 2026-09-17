namespace VM.Server.Service.Vending;

public interface IChangeCalculator
{
    ChangeResult Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins);
}
