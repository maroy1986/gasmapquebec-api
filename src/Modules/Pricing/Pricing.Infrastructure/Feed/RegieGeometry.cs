using System.Text.Json.Serialization;

namespace Pricing.Infrastructure.Feed;

internal sealed class RegieGeometry
{
    /// <summary>GeoJSON order: [longitude, latitude].</summary>
    [JsonPropertyName("coordinates")]
    public double[] Coordinates { get; init; } = [];
}
