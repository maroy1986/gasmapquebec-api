namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>
/// A single fuel grade's latest price. <see cref="FuelType"/> is a locale-free token
/// (regular | super | diesel); <see cref="PriceCents"/> is the official numeric cents per
/// litre (e.g. 179.9), null when unavailable. <see cref="ReportedPriceCents"/> /
/// <see cref="ReportedAt"/> carry the latest accepted community-reported price for the grade,
/// shown alongside the official one; both null when there is no current correction.
/// </summary>
public sealed record PriceDto(
    string FuelType,
    decimal? PriceCents,
    bool IsAvailable,
    DateTime ObservedAt,
    decimal? ReportedPriceCents = null,
    DateTime? ReportedAt = null);
