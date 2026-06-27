using System.Text.Json.Serialization;

namespace GasMapQuebec.Pricing.Application.GeoJson;

public sealed class FeedMetadata
{
    [JsonPropertyName("generated_at")]
    public required DateTime GeneratedAt { get; init; }

    [JsonPropertyName("total_stations")]
    public required int TotalStations { get; init; }
}
