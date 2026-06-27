using System.Text.Json.Serialization;

namespace GasMapQuebec.Pricing.Application.GeoJson;

public sealed class PointGeometry
{
    [JsonPropertyName("type")]
    public string Type => "Point";

    /// <summary>GeoJSON order: [longitude, latitude].</summary>
    [JsonPropertyName("coordinates")]
    public required double[] Coordinates { get; init; }
}
