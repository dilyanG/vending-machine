using FluentAssertions;
using VM.Server.Domain.Errors;
using VM.Server.Domain.ValueObjects;

namespace VM.Server.Domain.Tests;

public class CoinBundleTests
{
    [Fact]
    public void Add_CalledTwiceForSameDenomination_Accumulates()
    {
        var bundle = CoinBundle.Empty.Add(50).Add(50, 2);

        bundle.Counts[50].Should().Be(3);
    }

    [Fact]
    public void Add_WithUnacceptedDenomination_ThrowsInvalidDenomination()
    {
        var act = () => CoinBundle.Empty.Add(2);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidDenomination);
    }

    [Fact]
    public void Add_WithZeroOrNegativeCount_ThrowsArgumentOutOfRange()
    {
        var act = () => CoinBundle.Empty.Add(50, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Add_DoesNotMutateTheOriginalBundle()
    {
        var original = CoinBundle.Empty.Add(50);

        original.Add(50);

        original.Counts[50].Should().Be(1);
    }

    [Fact]
    public void Remove_LastCoinOfADenomination_DropsTheKey()
    {
        var bundle = CoinBundle.Empty.Add(50).Remove(50);

        bundle.Counts.Should().NotContainKey(50);
        bundle.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Remove_MoreThanAvailable_ThrowsInvalidOperation()
    {
        var bundle = CoinBundle.Empty.Add(50);

        var act = () => bundle.Remove(50, 2);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Remove_DoesNotMutateTheOriginalBundle()
    {
        var original = CoinBundle.Empty.Add(50, 2);

        original.Remove(50);

        original.Counts[50].Should().Be(2);
    }

    [Fact]
    public void TryRemove_MoreThanAvailable_ReturnsFalseAndUnchangedBundle()
    {
        var bundle = CoinBundle.Empty.Add(50);

        var succeeded = bundle.TryRemove(50, 2, out var result);

        succeeded.Should().BeFalse();
        result.Should().Be(bundle);
    }

    [Fact]
    public void TryRemove_WithAvailableCoins_ReturnsTrueAndReducedBundle()
    {
        var bundle = CoinBundle.Empty.Add(50, 3);

        var succeeded = bundle.TryRemove(50, 2, out var result);

        succeeded.Should().BeTrue();
        result.Counts[50].Should().Be(1);
    }

    [Fact]
    public void TotalCents_AcrossMixedBundle_SumsCorrectly()
    {
        var bundle = CoinBundle.Empty.Add(200).Add(50, 3).Add(5, 4);

        bundle.TotalCents.Should().Be(200 + 150 + 20);
    }

    [Fact]
    public void Combine_TwoBundles_SumsCounts()
    {
        var a = CoinBundle.Empty.Add(50, 2);
        var b = CoinBundle.Empty.Add(50).Add(5);

        var combined = a.Combine(b);

        combined.Counts[50].Should().Be(3);
        combined.Counts[5].Should().Be(1);
    }

    [Fact]
    public void Equals_TwoIndependentlyBuiltIdenticalBundles_AreEqual()
    {
        var a = CoinBundle.Empty.Add(50).Add(5, 2);
        var b = CoinBundle.Empty.Add(5, 2).Add(50);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_BundlesWithDifferentCounts_AreNotEqual()
    {
        var a = CoinBundle.Empty.Add(50);
        var b = CoinBundle.Empty.Add(50, 2);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Empty_HasNoCoinsAndZeroTotal()
    {
        CoinBundle.Empty.IsEmpty.Should().BeTrue();
        CoinBundle.Empty.TotalCents.Should().Be(0);
    }
}
