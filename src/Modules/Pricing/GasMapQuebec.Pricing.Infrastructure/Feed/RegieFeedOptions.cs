namespace GasMapQuebec.Pricing.Infrastructure.Feed;

public sealed class RegieFeedOptions
{
    public const string SectionName = "Pricing:RegieFeed";

    /// <summary>Gzipped GeoJSON feed of all Québec stations and their prices.</summary>
    public string Url { get; set; } = "https://regieessencequebec.ca/stations.geojson.gz";
}
