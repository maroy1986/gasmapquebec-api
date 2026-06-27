using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.FuelLog.Domain;

/// <summary>
/// A single fill-up recorded by a user. Aggregate root for the FuelLog module.
/// </summary>
public sealed class FuelLogEntry : AggregateRoot<Guid>
{
    // EF Core materialization constructor.
    private FuelLogEntry(Guid id) : base(id) { }

    private FuelLogEntry(
        Guid id,
        Guid userId,
        DateTime filledAtUtc,
        FuelGrade fuelGrade,
        decimal litres,
        decimal totalCost,
        int? odometerKm,
        Guid? stationId,
        string? stationName,
        string? notes) : base(id)
    {
        UserId = userId;
        FilledAtUtc = filledAtUtc;
        FuelGrade = fuelGrade;
        Litres = litres;
        TotalCost = totalCost;
        OdometerKm = odometerKm;
        StationId = stationId;
        StationName = stationName;
        Notes = notes;
    }

    public Guid UserId { get; private init; }
    public DateTime FilledAtUtc { get; private set; }
    public FuelGrade FuelGrade { get; private set; }
    public decimal Litres { get; private set; }
    public decimal TotalCost { get; private set; }
    public int? OdometerKm { get; private set; }

    /// <summary>Loose reference to a Pricing station (no cross-module FK), if known.</summary>
    public Guid? StationId { get; private set; }
    public string? StationName { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>Average price paid in cents per litre, derived from cost and volume.</summary>
    public decimal? PricePerLitreCents => Litres > 0 ? Math.Round(TotalCost / Litres * 100m, 1) : null;

    public static FuelLogEntry Create(
        Guid userId,
        DateTime filledAtUtc,
        FuelGrade fuelGrade,
        decimal litres,
        decimal totalCost,
        int? odometerKm = null,
        Guid? stationId = null,
        string? stationName = null,
        string? notes = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (litres <= 0)
            throw new ArgumentOutOfRangeException(nameof(litres), litres, "Litres must be positive.");
        if (totalCost < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCost), totalCost, "Total cost cannot be negative.");

        return new FuelLogEntry(
            Guid.CreateVersion7(),
            userId,
            filledAtUtc,
            fuelGrade,
            litres,
            totalCost,
            odometerKm,
            stationId,
            stationName,
            notes);
    }
}
