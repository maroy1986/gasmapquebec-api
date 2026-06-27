using System.Text.Json.Serialization;

namespace GasMapQuebec.Pricing.Infrastructure.Feed;

internal sealed class RegieMetadata
{
    [JsonPropertyName("generated_at")]
    public DateTime? GeneratedAt { get; init; }
}
