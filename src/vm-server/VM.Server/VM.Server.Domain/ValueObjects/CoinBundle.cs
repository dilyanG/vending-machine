using VM.Server.Domain.Errors;

namespace VM.Server.Domain.ValueObjects;

public sealed class CoinBundle : IEquatable<CoinBundle>
{
    private readonly Dictionary<int, int> _counts;

    private CoinBundle(Dictionary<int, int> counts) => _counts = counts;

    public static CoinBundle Empty { get; } = new([]);

    public IReadOnlyDictionary<int, int> Counts => _counts;

    public bool IsEmpty => _counts.Count == 0;

    public int TotalCents => _counts.Sum(coin => coin.Key * coin.Value);

    public CoinBundle Add(int denominationCents, int count = 1)
    {
        ValidateDenomination(denominationCents);
        ValidateCount(count);

        var next = new Dictionary<int, int>(_counts);
        next[denominationCents] = next.GetValueOrDefault(denominationCents) + count;
        return new CoinBundle(next);
    }

    public CoinBundle Remove(int denominationCents, int count = 1)
    {
        if (!TryRemove(denominationCents, count, out var result))
        {
            var available = _counts.GetValueOrDefault(denominationCents);
            throw new InvalidOperationException(
                $"Cannot remove {count} coin(s) of {denominationCents}c: only {available} available.");
        }

        return result;
    }

    public bool TryRemove(int denominationCents, int count, out CoinBundle result)
    {
        ValidateDenomination(denominationCents);
        ValidateCount(count);

        var available = _counts.GetValueOrDefault(denominationCents);
        if (available < count)
        {
            result = this;
            return false;
        }

        var next = new Dictionary<int, int>(_counts);
        var remaining = available - count;
        if (remaining == 0)
        {
            next.Remove(denominationCents);
        }
        else
        {
            next[denominationCents] = remaining;
        }

        result = new CoinBundle(next);
        return true;
    }

    public CoinBundle Combine(CoinBundle other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var next = new Dictionary<int, int>(_counts);
        foreach (var (denomination, count) in other._counts)
        {
            next[denomination] = next.GetValueOrDefault(denomination) + count;
        }

        return new CoinBundle(next);
    }

    public bool Equals(CoinBundle? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (_counts.Count != other._counts.Count)
        {
            return false;
        }

        foreach (var (denomination, count) in _counts)
        {
            if (!other._counts.TryGetValue(denomination, out var otherCount) || otherCount != count)
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as CoinBundle);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var (denomination, count) in _counts.OrderBy(coin => coin.Key))
        {
            hash.Add(denomination);
            hash.Add(count);
        }

        return hash.ToHashCode();
    }

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
