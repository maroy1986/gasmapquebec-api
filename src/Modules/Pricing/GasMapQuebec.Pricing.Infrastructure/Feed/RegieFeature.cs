using System.Text.Json.Serialization;

namespace GasMapQuebec.Pricing.Infrastructure.Feed;

internal sealed class RegieFeature
{
    [JsonPropertyName("geometry")]
    public RegieGeometry? Geometry { get; init; }

    [JsonPropertyName("properties")]
    public RegieProperties? Properties { get; init; }
}
