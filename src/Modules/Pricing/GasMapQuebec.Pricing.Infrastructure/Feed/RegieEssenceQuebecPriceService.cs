using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure.Feed;

/// <summary>
/// Fetches the Régie essence Québec gzipped GeoJSON feed and maps it to the
/// application's normalized <see cref="StationPriceSnapshot"/>.
/// </summary>
internal sealed class RegieEssenceQuebecPriceService(
    HttpClient httpClient,
    IOptions<RegieFeedOptions> options,
    ILogger<RegieEssenceQuebecPriceService> logger) : IPriceService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<StationPriceSnapshot> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var url = options.Value.Url;
        await using var responseStream = await httpClient.GetStreamAsync(url, cancellationToken);
        await using var decompressed = new GZipStream(responseStream, CompressionMode.Decompress);

        var feed = await JsonSerializer.DeserializeAsync<RegieFeatureCollection>(decompressed, SerializerOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Régie feed returned no content.");

        var generatedAt = (feed.Metadata?.GeneratedAt ?? DateTime.UtcNow).ToUniversalTime();

        var stations = new List<StationPriceRecord>(feed.Features.Count);
        var skipped = 0;

        foreach (var feature in feed.Features)
        {
            var record = MapFeature(feature);
            if (record is null)
            {
                skipped++;
                continue;
            }
            stations.Add(record);
        }

        if (skipped > 0)
            logger.LogWarning("Skipped {Skipped} feed features with missing coordinates or name.", skipped);

        return new StationPriceSnapshot(generatedAt, stations);
    }

    private static StationPriceRecord? MapFeature(RegieFeature feature)
    {
        var coordinates = feature.Geometry?.Coordinates;
        var props = feature.Properties;
        if (coordinates is not { Length: >= 2 } || props is null || string.IsNullOrWhiteSpace(props.Name))
            return null;

        // GeoJSON order is [longitude, latitude].
        var longitude = coordinates[0];
        var latitude = coordinates[1];

        var prices = props.Prices
            .Select(p => new FuelPriceRecord(MapFuelType(p.GasType), ParsePrice(p.Price), p.IsAvailable))
            .ToList();

        return new StationPriceRecord(
            latitude,
            longitude,
            props.Name!,
            string.IsNullOrWhiteSpace(props.Brand) ? null : props.Brand,
            props.Status ?? string.Empty,
            props.Address ?? string.Empty,
            string.IsNullOrWhiteSpace(props.PostalCode) ? null : props.PostalCode,
            string.IsNullOrWhiteSpace(props.Region) ? null : props.Region,
            prices);
    }

    private static FuelType MapFuelType(string? gasType) => gasType switch
    {
        "Super" => FuelType.Super,
        "Diesel" => FuelType.Diesel,
        _ => FuelType.Regular // "Régulier" and anything else
    };

    /// <summary>Parses strings like "179.9¢" to a decimal; null when absent/unparseable.</summary>
    private static decimal? ParsePrice(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        Span<char> buffer = stackalloc char[raw.Length];
        var length = 0;
        foreach (var c in raw)
        {
            if (char.IsAsciiDigit(c) || c == '.')
                buffer[length++] = c;
        }

        return decimal.TryParse(buffer[..length], NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }
}
