using FluentAssertions;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Tests;

public class SlotTests
{
    private static Product ValidProduct() => Product.Create("Espresso", 145);

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void Create_WithQuantityBoundary_AcceptsOrRejects(int quantity, bool shouldSucceed)
    {
        var act = () => Slot.Create(ValidProduct(), quantity);

        if (shouldSucceed)
        {
            act.Should().NotThrow();
        }
        else
        {
            act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidQuantity);
        }
    }

    [Fact]
    public void Dispense_WhenQuantityIsOne_SetsQuantityToZero()
    {
        var slot = Slot.Create(ValidProduct(), 1);

        slot.Dispense();

        slot.Quantity.Should().Be(0);
    }

    [Fact]
    public void Dispense_WhenQuantityIsZero_ThrowsOutOfStock()
    {
        var slot = Slot.Create(ValidProduct(), 0);

        var act = slot.Dispense;

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.OutOfStock);
    }

    [Fact]
    public void Restock_WithOutOfRangeValue_ThrowsInvalidQuantity()
    {
        var slot = Slot.Create(ValidProduct(), 5);

        var act = () => slot.Restock(16);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidQuantity);
    }

    [Fact]
    public void Restock_WithValidValue_UpdatesQuantity()
    {
        var slot = Slot.Create(ValidProduct(), 5);

        slot.Restock(10);

        slot.Quantity.Should().Be(10);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void IsAvailable_ReflectsQuantity(int quantity, bool expected)
    {
        var slot = Slot.Create(ValidProduct(), quantity);

        slot.IsAvailable.Should().Be(expected);
    }

    [Fact]
    public void ProductId_MatchesTheUnderlyingProductId()
    {
        var product = ValidProduct();
        var slot = Slot.Create(product, 5);

        slot.ProductId.Should().Be(product.Id);
    }
}
