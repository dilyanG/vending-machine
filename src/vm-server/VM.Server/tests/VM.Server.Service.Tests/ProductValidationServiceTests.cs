using FluentAssertions;
using VM.Server.Domain.Errors;
using VM.Server.Service.Products;

namespace VM.Server.Service.Tests;

public class ProductValidationServiceTests
{
    private readonly ProductValidationService _validation = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(3, false)]
    [InlineData(5, true)]
    [InlineData(145, true)]
    public void ValidatePrice_WithBoundary_AcceptsOrRejects(int priceCents, bool shouldSucceed)
    {
        var act = () => _validation.ValidatePrice(priceCents);

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
    public void ValidateName_WithBlankName_ThrowsInvalidProduct(string? name)
    {
        var act = () => _validation.ValidateName(name);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.InvalidProduct);
    }

    [Fact]
    public void ValidateName_WithSurroundingWhitespace_TrimsName() =>
        _validation.ValidateName("  Espresso  ").Should().Be("Espresso");

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void ValidateQuantity_WithBoundary_AcceptsOrRejects(int quantity, bool shouldSucceed)
    {
        var act = () => _validation.ValidateQuantity(quantity);

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
    public void EnsureUnique_WithDuplicateId_ThrowsDuplicateProduct()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var existing = new[] { TestProducts.Slot(espresso, 5) };

        var act = () => _validation.EnsureUnique(existing, espresso.Id, "Latte", 195, excludingProductId: null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.DuplicateProduct);
    }

    [Fact]
    public void EnsureUnique_WithDuplicateNameCaseInsensitive_ThrowsDuplicateProduct()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var existing = new[] { TestProducts.Slot(espresso, 5) };

        var act = () => _validation.EnsureUnique(existing, Guid.NewGuid(), "ESPRESSO", 195, excludingProductId: null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.DuplicateProduct);
    }

    [Fact]
    public void EnsureUnique_WithDuplicatePrice_ThrowsDuplicatePrice()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var existing = new[] { TestProducts.Slot(espresso, 5) };

        var act = () => _validation.EnsureUnique(existing, Guid.NewGuid(), "Latte", 145, excludingProductId: null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.DuplicatePrice);
    }

    [Fact]
    public void EnsureUnique_ExcludingTheCandidatesOwnId_AllowsItsOwnNameAndPrice()
    {
        var espresso = TestProducts.Create("Espresso", 145);
        var existing = new[] { TestProducts.Slot(espresso, 5) };

        var act = () => _validation.EnsureUnique(existing, espresso.Id, "Espresso", 145, excludingProductId: espresso.Id);

        act.Should().NotThrow();
    }
}
