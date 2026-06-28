using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Pricing.Domain;

/// <summary>
/// An immutable point in a station's price timeline: the price of one fuel grade as observed at a
/// moment in time. A new entry is appended only when the price or availability actually changes
/// (and on first observation), so the table grows with real price movements — not refresh frequency.
/// Its own aggregate, separate from <see cref="Station"/>, so the station aggregate stays bounded.
/// </summary>
public sealed class PriceHistoryEntry : AggregateRoot<Guid>
{
    // EF Core materialization constructor.
    private PriceHistoryEntry(Guid id) : base(id) { }

    private PriceHistoryEntry(Guid id, Guid stationId, FuelType fuelType, decimal? priceCents, bool isAvailable, DateTime observedAtUtc)
        : base(id)
    {
        StationId = stationId;
        FuelType = fuelType;
        PriceCents = priceCents;
        IsAvailable = isAvailable;
        ObservedAtUtc = observedAtUtc;
    }

    public Guid StationId { get; private init; }
    public FuelType FuelType { get; private init; }

    /// <summary>Price in cents per litre (e.g. 179.9); null when unavailable.</summary>
    public decimal? PriceCents { get; private init; }
    public bool IsAvailable { get; private init; }

    /// <summary>When this price became the station's current value for the grade.</summary>
    public DateTime ObservedAtUtc { get; private init; }

    public static PriceHistoryEntry Create(Guid stationId, FuelType fuelType, decimal? priceCents, bool isAvailable, DateTime observedAtUtc) =>
        new(Guid.CreateVersion7(), stationId, fuelType, priceCents, isAvailable, observedAtUtc);
}
