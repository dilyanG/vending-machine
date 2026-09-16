using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using VM.Server.Domain.Entities;
using VM.Server.Service.Abstractions;

namespace VM.Server.API.Tests;

public sealed class UnhandledExceptionTests : IDisposable
{
    // Everything in this codebase that can fail throws DomainException, which
    // the middleware maps to a §3.3 body deliberately - there is no natural
    // way to trigger a genuine unhandled failure through the real stack. To
    // test the generic 500 path without adding a test-only backdoor to
    // production code, this factory replaces IVendingMachineStore with a fake
    // that always throws, for this test only.
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder => builder.ConfigureTestServices(
            services => services.AddSingleton<IVendingMachineStore>(new ThrowingVendingMachineStore())));

    private readonly HttpClient _client;

    public UnhandledExceptionTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Get_WhenAnUnexpectedExceptionIsThrown_Returns500WithNoStackTraceInTheBody()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("StackTrace");
        body.Should().NotContain("VM.Server");
        body.Should().NotContain("at ");
        body.Should().NotContain(nameof(InvalidOperationException));
    }

    private sealed class ThrowingVendingMachineStore : IVendingMachineStore
    {
        public Task<TResult> AccessAsync<TResult>(Func<VendingMachine, TResult> operation, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Deliberate failure for the 500 test.");

        public Task ExecuteAsync(Action<VendingMachine> operation, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Deliberate failure for the 500 test.");

        public Task ReloadAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Deliberate failure for the 500 test.");
    }
}
