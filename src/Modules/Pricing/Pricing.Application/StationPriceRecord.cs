namespace Pricing.Application;

/// <summary>A single station and its prices, normalized from the feed.</summary>
public sealed record StationPriceRecord(
    double Latitude,
    double Longitude,
    string Name,
    string? Brand,
    string Status,
    string Address,
    string? PostalCode,
    string? Region,
    IReadOnlyList<FuelPriceRecord> Prices);
