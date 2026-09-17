using VM.Server.Domain;

namespace VM.Server.Service.Vending;

/// <summary>
/// Bounded coin-change by dynamic programming: the bank holds a limited count
/// of each denomination, so a greedy pass over largest-first can report
/// failure when a solution actually exists.
///
/// Example: making 60c from a bank of one 50c coin and three 20c coins.
/// Greedy takes the 50c first, leaving 10c - but the bank has no 10c or 5c
/// coins, so greedy reports failure. The correct answer is three 20c coins.
/// Greedy is only optimal for these denominations given an unlimited supply
/// of each; a real machine's bank is finite, so it must search instead.
/// </summary>
public sealed class ChangeCalculationService : IChangeCalculator
{
    private const int Unreachable = int.MaxValue;

    public ChangeResult Calculate(int amountCents, IReadOnlyDictionary<int, int> availableCoins)
    {
        ArgumentNullException.ThrowIfNull(availableCoins);
        if (amountCents < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountCents), amountCents, "Amount cannot be negative.");
        }

        if (amountCents == 0)
        {
            return ChangeResult.Made(new Dictionary<int, int>());
        }

        // Ascending order matters: it makes the largest denomination the last
        // one decided during the DP fill, at which point the table already
        // knows the true optimal cost of every smaller-denomination
        // remainder. That lets the tie-break below prefer more of the
        // largest denomination first (then the next-largest, and so on)
        // without ever sacrificing the minimum coin count.
        var denominations = CoinDenominations.Accepted.ToArray();
        Array.Sort(denominations);

        var available = new int[denominations.Length];
        for (var i = 0; i < denominations.Length; i++)
        {
            available[i] = availableCoins.GetValueOrDefault(denominations[i]);
        }

        var layerCount = denominations.Length;

        // dp[i, a]: minimum coins to make amount a using only denominations[0..i-1].
        var dp = new int[layerCount + 1, amountCents + 1];
        var chosenCount = new int[layerCount + 1, amountCents + 1];

        for (var a = 1; a <= amountCents; a++)
        {
            dp[0, a] = Unreachable;
        }

        for (var i = 1; i <= layerCount; i++)
        {
            var denomination = denominations[i - 1];
            var maxAvailable = available[i - 1];

            for (var a = 0; a <= amountCents; a++)
            {
                var maxCoinsOfThisDenomination = Math.Min(maxAvailable, a / denomination);

                var best = Unreachable;
                var bestK = 0;

                for (var k = 0; k <= maxCoinsOfThisDenomination; k++)
                {
                    var remainder = a - (k * denomination);
                    var remainderCost = dp[i - 1, remainder];
                    if (remainderCost == Unreachable)
                    {
                        continue;
                    }

                    var candidate = remainderCost + k;

                    // <= (not <) on a genuine tie keeps overwriting as k grows,
                    // so the loop always ends up on the largest k that still
                    // achieves the minimum - i.e. as many of this denomination
                    // as possible.
                    if (candidate <= best)
                    {
                        best = candidate;
                        bestK = k;
                    }
                }

                dp[i, a] = best;
                chosenCount[i, a] = bestK;
            }
        }

        if (dp[layerCount, amountCents] == Unreachable)
        {
            return ChangeResult.NotPossible();
        }

        var coins = new Dictionary<int, int>();
        var remainingAmount = amountCents;
        for (var i = layerCount; i >= 1; i--)
        {
            var denomination = denominations[i - 1];
            var k = chosenCount[i, remainingAmount];
            if (k > 0)
            {
                coins[denomination] = k;
            }

            remainingAmount -= k * denomination;
        }

        return ChangeResult.Made(coins);
    }
}
