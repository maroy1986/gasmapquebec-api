using System.Text.Json.Serialization;

namespace Pricing.Infrastructure.Feed;

internal sealed class RegiePrice
{
    [JsonPropertyName("GasType")]
    public string? GasType { get; init; }

    [JsonPropertyName("Price")]
    public string? Price { get; init; }

    [JsonPropertyName("IsAvailable")]
    public bool IsAvailable { get; init; }
}
