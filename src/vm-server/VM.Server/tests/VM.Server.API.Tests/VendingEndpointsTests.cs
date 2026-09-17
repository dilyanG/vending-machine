using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using VM.Server.API.Dtos;
using VM.Server.Domain.Errors;
using VM.Server.Service.Products;
using VM.Server.Service.Vending;

namespace VM.Server.API.Tests;

public sealed class VendingEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = TestApp.Create();
    private readonly HttpClient _client;

    public VendingEndpointsTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetDenominations_ReturnsTheSixAcceptedValues()
    {
        var response = await _client.GetAsync("/api/vending/denominations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var denominations = await response.Content.ReadFromJsonAsync<List<int>>();
        denominations.Should().Equal(5, 10, 20, 50, 100, 200);
    }

    [Fact]
    public async Task GetSession_Initially_IsEmpty()
    {
        var response = await _client.GetAsync("/api/vending/session");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<SessionDto>();
        session!.InsertedTotalCents.Should().Be(0);
        session.InsertedCoins.Should().BeEmpty();
    }

    [Fact]
    public async Task InsertCoin_UpdatesTheSessionTotal()
    {
        var response = await _client.PostAsJsonAsync("/api/vending/coins", new InsertCoinRequest(50));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<SessionDto>();
        session!.InsertedTotalCents.Should().Be(50);
    }

    [Fact]
    public async Task InsertCoin_UnacceptedDenomination_ReturnsInvalidDenomination()
    {
        var response = await _client.PostAsJsonAsync("/api/vending/coins", new InsertCoinRequest(2));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.InvalidDenomination);
    }

    [Fact]
    public async Task Purchase_HappyPath_ReturnsCorrectChangeAndDecrementsQuantity()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        var target = products!.First(p => p.PriceCents == 85); // Water, 85c
        var quantityBefore = target.Quantity;

        await _client.PostAsJsonAsync("/api/vending/coins", new InsertCoinRequest(100));

        var response = await _client.PostAsJsonAsync("/api/vending/purchase", new PurchaseRequest(target.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PurchaseResultDto>();
        result!.PaidCents.Should().Be(100);
        result.PriceCents.Should().Be(85);
        result.ChangeCents.Should().Be(15);
        result.ChangeCoins.Should().Equal(new CoinCountDto(10, 1), new CoinCountDto(5, 1));
        result.Product.Quantity.Should().Be(quantityBefore - 1);
    }

    [Fact]
    public async Task Purchase_InsufficientFunds_Returns400()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        var target = products![0];

        var response = await _client.PostAsJsonAsync("/api/vending/purchase", new PurchaseRequest(target.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.InsufficientFunds);
    }

    [Fact]
    public async Task Purchase_UnknownProduct_ReturnsProductNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/vending/purchase", new PurchaseRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.ProductNotFound);
    }

    [Fact]
    public async Task Purchase_WhenOutOfStock_Returns400()
    {
        using var factory = TestApp.Create(new Dictionary<string, string?> { ["VendingMachine:InitialQuantityPerSlot"] = "0" });
        using var client = factory.CreateClient();
        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        var target = products![0];
        await client.PostAsJsonAsync("/api/vending/coins", new InsertCoinRequest(200));

        var response = await client.PostAsJsonAsync("/api/vending/purchase", new PurchaseRequest(target.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.OutOfStock);
    }

    [Fact]
    public async Task Purchase_WhenChangeUnavailable_Returns422AndSessionStillHasTheInsertedCoins()
    {
        var emptyBank = new Dictionary<string, string?>
        {
            ["VendingMachine:CoinBank:5"] = "0",
            ["VendingMachine:CoinBank:10"] = "0",
            ["VendingMachine:CoinBank:20"] = "0",
            ["VendingMachine:CoinBank:50"] = "0",
            ["VendingMachine:CoinBank:100"] = "0",
            ["VendingMachine:CoinBank:200"] = "0",
        };
        using var factory = TestApp.Create(emptyBank);
        using var client = factory.CreateClient();
        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        var target = products!.First(p => p.PriceCents == 85); // Water, 85c
        await client.PostAsJsonAsync("/api/vending/coins", new InsertCoinRequest(100)); // needs 15c change, bank is empty

        var response = await client.PostAsJsonAsync("/api/vending/purchase", new PurchaseRequest(target.Id));

        response.StatusCode.Should().Be((HttpStatusCode)422);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error!.Code.Should().Be(ErrorCodes.ChangeUnavailable);

        var session = await client.GetFromJsonAsync<SessionDto>("/api/vending/session");
        session!.InsertedTotalCents.Should().Be(100, "a refused purchase must leave the session untouched");
    }

    [Fact]
    public async Task Reset_ReturnsTheSameDenominationsInserted_AndClearsTheSession()
    {
        await _client.PostAsJsonAsync("/api/vending/coins", new InsertCoinRequest(200));
        await _client.PostAsJsonAsync("/api/vending/coins", new InsertCoinRequest(50));

        var response = await _client.PostAsync("/api/vending/reset", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ReturnedCoinsDto>();
        result!.ReturnedTotalCents.Should().Be(250);
        result.ReturnedCoins.Should().Equal(new CoinCountDto(200, 1), new CoinCountDto(50, 1));

        var session = await _client.GetFromJsonAsync<SessionDto>("/api/vending/session");
        session!.InsertedTotalCents.Should().Be(0);
        session.InsertedCoins.Should().BeEmpty();
    }
}
