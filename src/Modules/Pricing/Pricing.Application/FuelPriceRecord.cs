using Pricing.Domain;

namespace Pricing.Application;

/// <summary>A single fuel grade's price, normalized from the feed.</summary>
public sealed record FuelPriceRecord(FuelType FuelType, decimal? PriceCents, bool IsAvailable);
