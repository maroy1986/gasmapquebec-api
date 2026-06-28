namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>The ordered (oldest-first) price points for one fuel grade.</summary>
public sealed record FuelGradeHistory(
    string FuelType,
    IReadOnlyList<PriceHistoryPoint> Points);
