using FluentAssertions;
using VM.Server.Domain;

namespace VM.Server.Service.Tests;

public class CoinDenominationsTests
{
    [Theory]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(20, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(200, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(25, false)]
    [InlineData(500, false)]
    public void IsAccepted_WithDenomination_ReturnsExpected(int denominationCents, bool expected) =>
        CoinDenominations.IsAccepted(denominationCents).Should().Be(expected);

    [Fact]
    public void MaxQuantityPerProduct_Is15() => CoinDenominations.MaxQuantityPerProduct.Should().Be(15);

    [Fact]
    public void Accepted_ListsAllSixDenominations() =>
        CoinDenominations.Accepted.Should().BeEquivalentTo([5, 10, 20, 50, 100, 200]);
}
