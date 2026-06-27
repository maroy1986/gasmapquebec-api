using System.Text.Json.Serialization;

namespace Pricing.Application.GeoJson;

public sealed class PriceProperty
{
    [JsonPropertyName("GasType")]
    public required string GasType { get; init; }

    /// <summary>Formatted like "179.9¢"; null when unavailable.</summary>
    [JsonPropertyName("Price")]
    public string? Price { get; init; }

    [JsonPropertyName("IsAvailable")]
    public required bool IsAvailable { get; init; }
}
