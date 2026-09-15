using FluentAssertions;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Tests;

public class ProductTests
{
    private const string ValidName = "Espresso";
    private const int ValidPriceCents = 145;
    private const int ValidQuantity = 5;

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void Create_WithQuantityBoundary_AcceptsOrRejects(int quantity, bool shouldSucceed)
    {
        var act = () => Product.Create(ValidName, ValidPriceCents, quantity);

        if (shouldSucceed)
        {
            act.Should().NotThrow();
        }
        else
        {
            act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidQuantity);
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(3, false)]
    [InlineData(5, true)]
    [InlineData(145, true)]
    public void Create_WithPriceBoundary_AcceptsOrRejects(int priceCents, bool shouldSucceed)
    {
        var act = () => Product.Create(ValidName, priceCents, ValidQuantity);

        if (shouldSucceed)
        {
            act.Should().NotThrow();
        }
        else
        {
            act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidPrice);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ThrowsInvalidProduct(string? name)
    {
        var act = () => Product.Create(name!, ValidPriceCents, ValidQuantity);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidProduct);
    }

    [Fact]
    public void Create_WithSurroundingWhitespaceInName_TrimsName()
    {
        var product = Product.Create("  Espresso  ", ValidPriceCents, ValidQuantity);

        product.Name.Should().Be("Espresso");
    }

    [Fact]
    public void DecrementStock_WhenQuantityIsOne_SetsQuantityToZero()
    {
        var product = Product.Create(ValidName, ValidPriceCents, 1);

        product.DecrementStock();

        product.Quantity.Should().Be(0);
    }

    [Fact]
    public void DecrementStock_WhenQuantityIsZero_ThrowsOutOfStock()
    {
        var product = Product.Create(ValidName, ValidPriceCents, 0);

        var act = product.DecrementStock;

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.OutOfStock);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var product = Product.Create(ValidName, ValidPriceCents, ValidQuantity);

        product.Rename("Latte");

        product.Name.Should().Be("Latte");
    }

    [Fact]
    public void Rename_WithBlankName_ThrowsInvalidProductAndLeavesNameUnchanged()
    {
        var product = Product.Create(ValidName, ValidPriceCents, ValidQuantity);

        var act = () => product.Rename("   ");

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidProduct);
        product.Name.Should().Be(ValidName);
    }

    [Fact]
    public void ChangePrice_WithInvalidPrice_ThrowsInvalidPrice()
    {
        var product = Product.Create(ValidName, ValidPriceCents, ValidQuantity);

        var act = () => product.ChangePrice(7);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidPrice);
    }

    [Fact]
    public void SetQuantity_WithOutOfRangeValue_ThrowsInvalidQuantity()
    {
        var product = Product.Create(ValidName, ValidPriceCents, ValidQuantity);

        var act = () => product.SetQuantity(16);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidQuantity);
    }

    [Fact]
    public void Restore_WithKnownId_KeepsThatId()
    {
        var id = Guid.NewGuid();

        var product = Product.Restore(id, ValidName, ValidPriceCents, ValidQuantity);

        product.Id.Should().Be(id);
    }
}
