using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Pricing.Domain;

/// <summary>
/// The latest observed price of a single fuel grade at a station.
/// Part of the <see cref="Station"/> aggregate; one row per station + fuel type.
/// </summary>
public sealed class FuelPrice : Entity<Guid>
{
    // EF Core materialization constructor.
    private FuelPrice(Guid id) : base(id) { }

    private FuelPrice(Guid id, Guid stationId, FuelType fuelType, decimal? priceCents, bool isAvailable, DateTime observedAtUtc)
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

    /// <summary>Price in cents per litre (e.g. 179.9). Null when unavailable.</summary>
    public decimal? PriceCents { get; private set; }
    public bool IsAvailable { get; private set; }
    public DateTime ObservedAtUtc { get; private set; }

    internal static FuelPrice Create(Guid stationId, FuelType fuelType, decimal? priceCents, bool isAvailable, DateTime observedAtUtc) =>
        new(Guid.CreateVersion7(), stationId, fuelType, priceCents, isAvailable, observedAtUtc);

    /// <summary>
    /// Applies an observation. Returns <c>true</c> when the price or availability changed (so the
    /// caller can append a history point); a no-op observation leaves the row — and its
    /// <see cref="ObservedAtUtc"/> — untouched, which keeps unchanged rows out of every refresh.
    /// </summary>
    internal bool Update(decimal? priceCents, bool isAvailable, DateTime observedAtUtc)
    {
        if (PriceCents == priceCents && IsAvailable == isAvailable)
            return false;

        PriceCents = priceCents;
        IsAvailable = isAvailable;
        ObservedAtUtc = observedAtUtc;
        return true;
    }
}
