using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VM.Server.API.Tests;

public sealed class ExternalCatalogEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = TestApp.Create();
    private readonly HttpClient _client;

    public ExternalCatalogEndpointTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetCatalog_ReturnsSixProductsWithNoQuantityField()
    {
        var response = await _client.GetAsync("/api/external/catalog");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        document.RootElement.GetArrayLength().Should().Be(6);
        foreach (var product in document.RootElement.EnumerateArray())
        {
            product.TryGetProperty("priceCents", out _).Should().BeTrue();
            product.TryGetProperty("name", out _).Should().BeTrue();
            product.TryGetProperty("quantity", out _).Should().BeFalse("the external catalog carries no stock levels");
        }
    }
}
