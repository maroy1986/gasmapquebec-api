namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>A single price observation: the value the grade changed to, and when.</summary>
public sealed record PriceHistoryPoint(
    decimal? PriceCents,
    bool IsAvailable,
    DateTime ObservedAt);
