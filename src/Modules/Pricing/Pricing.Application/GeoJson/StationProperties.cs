using System.Text.Json.Serialization;

namespace Pricing.Application.GeoJson;

public sealed class StationProperties
{
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("brand")]
    public string? Brand { get; init; }

    [JsonPropertyName("Status")]
    public required string Status { get; init; }

    [JsonPropertyName("Address")]
    public required string Address { get; init; }

    [JsonPropertyName("PostalCode")]
    public string? PostalCode { get; init; }

    [JsonPropertyName("Region")]
    public string? Region { get; init; }

    [JsonPropertyName("Prices")]
    public required IReadOnlyList<PriceProperty> Prices { get; init; }
}
