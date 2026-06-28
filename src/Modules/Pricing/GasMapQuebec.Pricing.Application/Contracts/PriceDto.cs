namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>
/// A single fuel grade's latest price. <see cref="FuelType"/> is a locale-free token
/// (regular | super | diesel); <see cref="PriceCents"/> is numeric cents per litre
/// (e.g. 179.9), null when unavailable.
/// </summary>
public sealed record PriceDto(
    string FuelType,
    decimal? PriceCents,
    bool IsAvailable,
    DateTime ObservedAt);
