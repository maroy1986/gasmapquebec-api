using System.Text.Json.Serialization;

namespace GasMapQuebec.Pricing.Infrastructure.Feed;

internal sealed class RegieProperties
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("brand")]
    public string? Brand { get; init; }

    [JsonPropertyName("Status")]
    public string? Status { get; init; }

    [JsonPropertyName("Address")]
    public string? Address { get; init; }

    [JsonPropertyName("PostalCode")]
    public string? PostalCode { get; init; }

    [JsonPropertyName("Region")]
    public string? Region { get; init; }

    [JsonPropertyName("Prices")]
    public List<RegiePrice> Prices { get; init; } = [];
}
