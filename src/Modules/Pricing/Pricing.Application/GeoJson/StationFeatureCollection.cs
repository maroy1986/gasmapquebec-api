using System.Text.Json.Serialization;

namespace Pricing.Application.GeoJson;

// These models reproduce the Régie essence Québec feed shape byte-for-byte so the
// mobile app (gasmapquebec) can switch its source URL with no parser changes.
// Property names here are part of that contract — do not rename casually.

public sealed class StationFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type => "FeatureCollection";

    [JsonPropertyName("metadata")]
    public required FeedMetadata Metadata { get; init; }

    [JsonPropertyName("features")]
    public required IReadOnlyList<StationFeature> Features { get; init; }
}
