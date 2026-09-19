using System.Diagnostics;
using FluentAssertions;
using VM.Server.Service.Implementations;

namespace VM.Server.Service.Tests;

public class ChangeCalculationServiceTests
{
    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0, 0, true)] // amount 0 -> success, empty set
    [InlineData(50, 0, 0, 1, 0, 0, 0, true)] // exact single coin
    [InlineData(35, 0, 0, 0, 1, 1, 0, false)] // genuinely impossible: 20+10 can't make 35
    [InlineData(100, 0, 0, 0, 0, 0, 0, false)] // empty bank, non-zero amount
    [InlineData(100, 0, 0, 1, 0, 0, 0, false)] // bank total (50c) smaller than the amount
    public void Calculate_WithBoundaryBank_SucceedsOrNotAsExpected(
        int amountCents, int count200, int count100, int count50, int count20, int count10, int count5, bool expectedSuccess)
    {
        var bank = new Dictionary<int, int>
        {
            [200] = count200,
            [100] = count100,
            [50] = count50,
            [20] = count20,
            [10] = count10,
            [5] = count5,
        };

        var result = new ChangeCalculationService().Calculate(amountCents, bank);

        result.IsSuccess.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            result.TotalCents.Should().Be(amountCents);
        }
        else
        {
            result.Coins.Should().BeEmpty();
        }
    }

    [Fact]
    public void Calculate_WhenGreedyWouldStrand_FindsTheCorrectCombination()
    {
        // 60c from one 50c coin and three 20c coins. Greedy takes the 50c
        // first, leaving 10c the bank cannot pay (no 10c/5c coins) and
        // reports failure. The correct answer is three 20c coins.
        var bank = new Dictionary<int, int> { [50] = 1, [20] = 3 };

        var result = new ChangeCalculationService().Calculate(60, bank);

        result.IsSuccess.Should().BeTrue();
        result.Coins.Should().BeEquivalentTo(new Dictionary<int, int> { [20] = 3 });
    }

    [Fact]
    public void Calculate_WithMultipleWaysToPay_ReturnsTheMinimalCoinCount()
    {
        var bank = new Dictionary<int, int> { [20] = 2, [10] = 4, [5] = 8 };

        var result = new ChangeCalculationService().Calculate(40, bank);

        result.IsSuccess.Should().BeTrue();
        result.Coins.Should().BeEquivalentTo(new Dictionary<int, int> { [20] = 2 });
    }

    [Fact]
    public void Calculate_WhenBankContainsAnUnacceptedDenomination_NeverReturnsIt()
    {
        var bank = new Dictionary<int, int> { [1] = 1000, [5] = 20 };

        var result = new ChangeCalculationService().Calculate(15, bank);

        result.IsSuccess.Should().BeTrue();
        result.Coins.Should().NotContainKey(1);
        result.Coins.Should().BeEquivalentTo(new Dictionary<int, int> { [5] = 3 });
    }

    [Fact]
    public void Calculate_WithNegativeAmount_ThrowsArgumentOutOfRange()
    {
        var act = () => new ChangeCalculationService().Calculate(-5, new Dictionary<int, int>());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Calculate_DoesNotMutateTheAvailableCoinsDictionary()
    {
        var bank = new Dictionary<int, int> { [50] = 2, [20] = 1, [5] = 4 };
        var bankBefore = new Dictionary<int, int>(bank);

        new ChangeCalculationService().Calculate(75, bank);

        bank.Should().BeEquivalentTo(bankBefore);
    }

    [Fact]
    public void Calculate_CalledRepeatedlyWithSameInputs_ReturnsTheSameCoinsEveryTime()
    {
        var bank = new Dictionary<int, int> { [50] = 1, [20] = 3, [10] = 2, [5] = 4 };
        var calculator = new ChangeCalculationService();

        var first = calculator.Calculate(60, bank);

        for (var i = 0; i < 10; i++)
        {
            var result = calculator.Calculate(60, bank);
            result.Coins.Should().BeEquivalentTo(first.Coins);
        }
    }

    [Fact]
    public void Calculate_500CentsFrom200MixedCoins_CompletesInUnder50Milliseconds()
    {
        var bank = new Dictionary<int, int> { [200] = 40, [100] = 40, [50] = 40, [20] = 40, [10] = 20, [5] = 20 };
        var calculator = new ChangeCalculationService();

        var stopwatch = Stopwatch.StartNew();
        var result = calculator.Calculate(500, bank);
        stopwatch.Stop();

        result.IsSuccess.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    [Fact]
    public void Calculate_ForARangeOfAmounts_OnSuccessCoinsSumExactlyAndRespectAvailability()
    {
        var bank = new Dictionary<int, int> { [200] = 2, [100] = 3, [50] = 4, [20] = 5, [10] = 6, [5] = 7 };
        var calculator = new ChangeCalculationService();

        for (var amountCents = 0; amountCents <= 300; amountCents += 5)
        {
            var result = calculator.Calculate(amountCents, bank);
            if (!result.IsSuccess)
            {
                continue;
            }

            result.TotalCents.Should().Be(amountCents, $"amount {amountCents} should be paid exactly");
            foreach (var (denomination, count) in result.Coins)
            {
                count.Should().BeLessThanOrEqualTo(
                    bank.GetValueOrDefault(denomination), $"denomination {denomination} should not exceed availability");
            }
        }
    }
}
