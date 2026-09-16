using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Entities;

public sealed class CoinInventory
{
    private readonly Dictionary<int, int> _counts;

    private CoinInventory(Dictionary<int, int> counts) => _counts = counts;

    public static CoinInventory Empty() => new([]);

    public static CoinInventory From(IReadOnlyDictionary<int, int> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);

        var validated = new Dictionary<int, int>();
        foreach (var (denomination, count) in counts)
        {
            if (count == 0)
            {
                continue;
            }

            ValidateDenomination(denomination);
            ValidateCount(count);
            validated[denomination] = count;
        }

        return new CoinInventory(validated);
    }

    public bool IsEmpty => _counts.Count == 0;

    public int TotalCents => _counts.Sum(coin => coin.Key * coin.Value);

    public int CountOf(int denominationCents) => _counts.GetValueOrDefault(denominationCents);

    public void Add(int denominationCents, int count = 1)
    {
        ValidateDenomination(denominationCents);
        ValidateCount(count);

        _counts[denominationCents] = _counts.GetValueOrDefault(denominationCents) + count;
    }

    public void AddAll(CoinInventory other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var (denomination, count) in other._counts)
        {
            _counts[denomination] = _counts.GetValueOrDefault(denomination) + count;
        }
    }

    public void Remove(int denominationCents, int count = 1)
    {
        ValidateDenomination(denominationCents);
        ValidateCount(count);

        var available = _counts.GetValueOrDefault(denominationCents);
        if (available < count)
        {
            throw new DomainException(
                ErrorCodes.ChangeUnavailable,
                $"Cannot remove {count} coin(s) of {denominationCents}c: only {available} available.",
                new Dictionary<string, object>
                {
                    ["denominationCents"] = denominationCents,
                    ["requested"] = count,
                    ["available"] = available,
                });
        }

        var remaining = available - count;
        if (remaining == 0)
        {
            _counts.Remove(denominationCents);
        }
        else
        {
            _counts[denominationCents] = remaining;
        }
    }

    public IReadOnlyDictionary<int, int> ToSnapshot() => new Dictionary<int, int>(_counts);

    public void Clear() => _counts.Clear();

    private static void ValidateDenomination(int denominationCents)
    {
        if (!CoinDenominations.IsAccepted(denominationCents))
        {
            throw new DomainException(
                ErrorCodes.InvalidDenomination,
                $"{denominationCents} cents is not an accepted coin denomination.",
                new Dictionary<string, object> { ["denominationCents"] = denominationCents });
        }
    }

    private static void ValidateCount(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Coin count must be positive.");
        }
    }
}
