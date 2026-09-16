using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using VM.Server.API.Dtos;
using VM.Server.Domain.Errors;
using VM.Server.Service.Products;

namespace VM.Server.API.Tests;

public sealed class ProductsEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = TestApp.Create();
    private readonly HttpClient _client;

    public ProductsEndpointsTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task List_ReturnsTheSixSeedProducts()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().HaveCount(6);
        products!.Should().OnlyContain(p => p.Quantity == 10);
    }

    [Fact]
    public async Task Get_KnownId_ReturnsThatProduct()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        var target = products![0];

        var response = await _client.GetAsync($"/api/products/{target.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product!.Id.Should().Be(target.Id);
    }

    [Fact]
    public async Task Get_UnknownId_ReturnsProductNotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task Create_ValidProduct_Returns201AndIsListedAfterward()
    {
        var request = new CreateProductRequest("Iced Tea", 165, 8, "assets/products/iced-tea.svg");

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        created!.Name.Should().Be("Iced Tea");

        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        products.Should().Contain(p => p.Id == created.Id);
    }

    [Fact]
    public async Task Update_ExistingProduct_ReturnsTheUpdatedProduct()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        var target = products![0];
        var request = new UpdateProductRequest("Renamed", target.PriceCents, 3, target.ImageUrl);

        var response = await _client.PutAsJsonAsync($"/api/products/{target.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        updated!.Name.Should().Be("Renamed");
        updated.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsProductNotFound()
    {
        var request = new UpdateProductRequest("Anything", 100, 1, null);

        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task Delete_ExistingProduct_RemovesItFromSubsequentLists()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        var target = products![0];

        var response = await _client.DeleteAsync($"/api/products/{target.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var remaining = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        remaining.Should().NotContain(p => p.Id == target.Id);
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsProductNotFound()
    {
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task Reload_AfterADelete_RestoresTheOriginalCatalogue()
    {
        var original = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        await _client.DeleteAsync($"/api/products/{original![0].Id}");

        var reloadResponse = await _client.PostAsync("/api/products/reload", content: null);

        reloadResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterReload = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        afterReload.Should().BeEquivalentTo(original, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Create_WithBlankName_ReturnsInvalidProduct()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("   ", 100, 1, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.InvalidProduct);
    }

    [Fact]
    public async Task Create_WithPriceNotAMultipleOfFive_ReturnsInvalidPrice()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("New Drink", 103, 1, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.InvalidPrice);
    }

    [Fact]
    public async Task Create_WithQuantity16_ReturnsInvalidQuantity()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("New Drink", 300, 16, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.InvalidQuantity);
    }

    [Fact]
    public async Task Create_WithNameAlreadyUsed_ReturnsDuplicateProduct()
    {
        var existing = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");

        var response = await _client.PostAsJsonAsync(
            "/api/products", new CreateProductRequest(existing![0].Name, 995, 1, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.DuplicateProduct);
    }

    [Fact]
    public async Task Create_WithPriceAlreadyUsed_ReturnsDuplicatePrice()
    {
        var existing = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");

        var response = await _client.PostAsJsonAsync(
            "/api/products", new CreateProductRequest("Brand New", existing![0].PriceCents, 1, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.DuplicatePrice);
    }
}
