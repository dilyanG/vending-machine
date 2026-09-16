using FluentAssertions;
using VM.Server.Domain.Entities;
using VM.Server.Domain.Errors;

namespace VM.Server.Domain.Tests;

public class ProductTests
{
    private const string ValidName = "Espresso";
    private const int ValidPriceCents = 145;

    [Theory]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(3, false)]
    [InlineData(5, true)]
    [InlineData(145, true)]
    public void Create_WithPriceBoundary_AcceptsOrRejects(int priceCents, bool shouldSucceed)
    {
        var act = () => Product.Create(ValidName, priceCents);

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
        var act = () => Product.Create(name!, ValidPriceCents);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidProduct);
    }

    [Fact]
    public void Create_WithSurroundingWhitespaceInName_TrimsName()
    {
        var product = Product.Create("  Espresso  ", ValidPriceCents);

        product.Name.Should().Be("Espresso");
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var product = Product.Create(ValidName, ValidPriceCents);

        product.Rename("Latte");

        product.Name.Should().Be("Latte");
    }

    [Fact]
    public void Rename_WithBlankName_ThrowsInvalidProductAndLeavesNameUnchanged()
    {
        var product = Product.Create(ValidName, ValidPriceCents);

        var act = () => product.Rename("   ");

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidProduct);
        product.Name.Should().Be(ValidName);
    }

    [Fact]
    public void ChangePrice_WithInvalidPrice_ThrowsInvalidPrice()
    {
        var product = Product.Create(ValidName, ValidPriceCents);

        var act = () => product.ChangePrice(7);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidPrice);
    }

    [Fact]
    public void Restore_WithKnownId_KeepsThatId()
    {
        var id = Guid.NewGuid();

        var product = Product.Restore(id, ValidName, ValidPriceCents);

        product.Id.Should().Be(id);
    }
}
