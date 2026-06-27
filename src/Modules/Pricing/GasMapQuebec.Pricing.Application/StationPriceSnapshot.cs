namespace GasMapQuebec.Pricing.Application;

/// <summary>A point-in-time snapshot of all station prices from the feed.</summary>
public sealed record StationPriceSnapshot(DateTime GeneratedAtUtc, IReadOnlyList<StationPriceRecord> Stations);
