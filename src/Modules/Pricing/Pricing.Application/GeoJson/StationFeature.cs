using System.Text.Json.Serialization;

namespace Pricing.Application.GeoJson;

public sealed class StationFeature
{
    [JsonPropertyName("type")]
    public string Type => "Feature";

    [JsonPropertyName("geometry")]
    public required PointGeometry Geometry { get; init; }

    [JsonPropertyName("properties")]
    public required StationProperties Properties { get; init; }
}
