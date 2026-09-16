using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace VM.Server.API.Tests;

/// <summary>
/// Builds a WebApplicationFactory with optional configuration overrides.
///
/// Isolation choice: every test class creates its OWN factory (see each
/// class's constructor/Dispose), not a shared IClassFixture. A
/// WebApplicationFactory's DI container - and therefore the singleton
/// VendingMachine store inside it - lives for as long as the factory does.
/// Since xUnit creates a fresh instance of a test class per test method by
/// default, giving each class its own factory field means each TEST gets its
/// own store, with no shared mutable state and no explicit reset step needed.
/// </summary>
internal static class TestApp
{
    public static WebApplicationFactory<Program> Create(IDictionary<string, string?>? configOverrides = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            if (configOverrides is { Count: > 0 })
            {
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(configOverrides));
            }
        });
}
