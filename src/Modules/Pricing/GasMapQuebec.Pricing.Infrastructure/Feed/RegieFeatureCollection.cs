using System.Text.Json.Serialization;

namespace GasMapQuebec.Pricing.Infrastructure.Feed;

// DTOs mirroring the raw Régie essence Québec GeoJSON feed.

internal sealed class RegieFeatureCollection
{
    [JsonPropertyName("metadata")]
    public RegieMetadata? Metadata { get; init; }

    [JsonPropertyName("features")]
    public List<RegieFeature> Features { get; init; } = [];
}
