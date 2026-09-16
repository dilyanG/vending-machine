using FluentAssertions;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Tests;

public class CoinInventoryTests
{
    [Fact]
    public void Add_CalledTwiceForSameDenomination_Accumulates()
    {
        var inventory = CoinInventory.Empty();

        inventory.Add(50);
        inventory.Add(50, 2);

        inventory.CountOf(50).Should().Be(3);
    }

    [Fact]
    public void Add_WithUnacceptedDenomination_ThrowsInvalidDenomination()
    {
        var inventory = CoinInventory.Empty();

        var act = () => inventory.Add(2);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidDenomination);
    }

    [Fact]
    public void Add_WithZeroOrNegativeCount_ThrowsArgumentOutOfRange()
    {
        var inventory = CoinInventory.Empty();

        var act = () => inventory.Add(50, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Remove_LastCoinOfADenomination_DropsTheKey()
    {
        var inventory = CoinInventory.Empty();
        inventory.Add(50);

        inventory.Remove(50);

        inventory.ToSnapshot().Should().NotContainKey(50);
        inventory.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Remove_MoreThanAvailable_ThrowsDomainException()
    {
        var inventory = CoinInventory.Empty();
        inventory.Add(50);

        var act = () => inventory.Remove(50, 2);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.ChangeUnavailable);
    }

    [Fact]
    public void TotalCents_AcrossMixedInventory_SumsCorrectly()
    {
        var inventory = CoinInventory.Empty();
        inventory.Add(200);
        inventory.Add(50, 3);
        inventory.Add(5, 4);

        inventory.TotalCents.Should().Be(200 + 150 + 20);
    }

    [Fact]
    public void AddAll_MergesAnotherInventory_WithoutMutatingIt()
    {
        var bank = CoinInventory.Empty();
        bank.Add(50, 2);
        var inserted = CoinInventory.Empty();
        inserted.Add(50);
        inserted.Add(5);

        bank.AddAll(inserted);

        bank.CountOf(50).Should().Be(3);
        bank.CountOf(5).Should().Be(1);
        inserted.CountOf(50).Should().Be(1);
        inserted.CountOf(5).Should().Be(1);
    }

    [Fact]
    public void From_SkipsZeroCounts()
    {
        var inventory = CoinInventory.From(new Dictionary<int, int> { [50] = 2, [10] = 0 });

        inventory.ToSnapshot().Should().NotContainKey(10);
        inventory.CountOf(50).Should().Be(2);
    }

    [Fact]
    public void ToSnapshot_ReturnsADefensiveCopy()
    {
        var inventory = CoinInventory.Empty();
        inventory.Add(50);

        var snapshot = inventory.ToSnapshot();
        inventory.Add(50);

        snapshot[50].Should().Be(1);
    }

    [Fact]
    public void Clear_RemovesAllCoins()
    {
        var inventory = CoinInventory.Empty();
        inventory.Add(50);
        inventory.Add(5, 3);

        inventory.Clear();

        inventory.IsEmpty.Should().BeTrue();
        inventory.TotalCents.Should().Be(0);
    }

    [Fact]
    public void Empty_HasNoCoinsAndZeroTotal()
    {
        var inventory = CoinInventory.Empty();

        inventory.IsEmpty.Should().BeTrue();
        inventory.TotalCents.Should().Be(0);
    }
}
