using System.Text.Json;
using System.Text.Json.Serialization;
using VM.Server.Domain.Entities;
using VM.Server.Service.Abstractions;

namespace VM.Server.Repository.MockExternalApi;

public sealed class FileExternalCatalogSource(string filePath) : IExternalCatalogSource
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<Product>> GetCatalogueAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"External catalogue file not found at '{filePath}'.");
        }

        List<CatalogueEntry>? entries;
        await using (var stream = File.OpenRead(filePath))
        {
            try
            {
                entries = await JsonSerializer.DeserializeAsync<List<CatalogueEntry>>(stream, SerializerOptions, cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"External catalogue file at '{filePath}' is not valid JSON.", ex);
            }
        }

        if (entries is null || entries.Count == 0)
        {
            throw new InvalidOperationException($"External catalogue file at '{filePath}' contained no products.");
        }

        return entries
            .Select(entry => new Product { Id = entry.Id, Name = entry.Name, PriceCents = entry.PriceCents, ImageUrl = entry.ImageUrl })
            .ToList();
    }

    private sealed record CatalogueEntry(
        [property: JsonRequired] Guid Id,
        [property: JsonRequired] string Name,
        [property: JsonRequired] int PriceCents,
        string? ImageUrl);
}
