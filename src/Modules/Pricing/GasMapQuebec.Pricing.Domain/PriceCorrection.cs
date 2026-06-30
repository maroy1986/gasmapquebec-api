using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Pricing.Domain;

/// <summary>
/// A user-submitted correction to a station's price for a single fuel grade. Doubles as the
/// approval queue (status <see cref="PriceCorrectionStatus.Pending"/>) and an audit log. An
/// <see cref="PriceCorrectionStatus.Accepted"/> correction is the current community-reported
/// price for its (station, grade); it never overwrites the official <see cref="FuelPrice"/>.
/// Its own aggregate, separate from <see cref="Station"/>.
/// </summary>
public sealed class PriceCorrection : AggregateRoot<Guid>
{
    // EF Core materialization constructor.
    private PriceCorrection(Guid id) : base(id) { }

    private PriceCorrection(
        Guid id,
        Guid stationId,
        FuelType fuelType,
        decimal submittedPriceCents,
        decimal? previousPriceCents,
        decimal percentChange,
        PriceCorrectionStatus status,
        string submitterId,
        DateTime submittedAtUtc) : base(id)
    {
        StationId = stationId;
        FuelType = fuelType;
        SubmittedPriceCents = submittedPriceCents;
        PreviousPriceCents = previousPriceCents;
        PercentChange = percentChange;
        Status = status;
        SubmitterId = submitterId;
        SubmittedAtUtc = submittedAtUtc;
    }

    public Guid StationId { get; private init; }
    public FuelType FuelType { get; private init; }

    /// <summary>The corrected price in cents per litre (e.g. 169.9).</summary>
    public decimal SubmittedPriceCents { get; private init; }

    /// <summary>The official price at submit time; null when the grade had no official price.</summary>
    public decimal? PreviousPriceCents { get; private init; }

    /// <summary>Absolute fractional change vs the official price (e.g. 0.12 = 12%); 0 when no baseline.</summary>
    public decimal PercentChange { get; private init; }

    public PriceCorrectionStatus Status { get; private set; }

    /// <summary>Stable device/user identifier from the authenticated request.</summary>
    public string SubmitterId { get; private init; } = null!;

    public DateTime SubmittedAtUtc { get; private init; }

    /// <summary>When the status last changed via review or supersession; null while first set.</summary>
    public DateTime? ReviewedAtUtc { get; private set; }

    /// <summary>
    /// Records a submission. Auto-accepts when the change is below <paramref name="threshold"/>
    /// (or there is no official baseline); otherwise queues it as <see cref="PriceCorrectionStatus.Pending"/>.
    /// </summary>
    public static PriceCorrection Create(
        Guid stationId,
        FuelType fuelType,
        decimal submittedPriceCents,
        decimal? previousPriceCents,
        decimal threshold,
        string submitterId,
        DateTime submittedAtUtc)
    {
        var percentChange = previousPriceCents is > 0
            ? Math.Abs(submittedPriceCents - previousPriceCents.Value) / previousPriceCents.Value
            : 0m;

        var status = percentChange >= threshold
            ? PriceCorrectionStatus.Pending
            : PriceCorrectionStatus.Accepted;

        return new PriceCorrection(
            Guid.CreateVersion7(), stationId, fuelType, submittedPriceCents,
            previousPriceCents, percentChange, status, submitterId, submittedAtUtc);
    }

    /// <summary>Approves a pending correction so it becomes the current community price.</summary>
    public void Approve(DateTime reviewedAtUtc)
    {
        Status = PriceCorrectionStatus.Accepted;
        ReviewedAtUtc = reviewedAtUtc;
    }

    /// <summary>Rejects a pending correction; it is never surfaced.</summary>
    public void Reject(DateTime reviewedAtUtc)
    {
        Status = PriceCorrectionStatus.Rejected;
        ReviewedAtUtc = reviewedAtUtc;
    }

    /// <summary>Marks an accepted correction superseded by a newer official price.</summary>
    public void MarkOutdated(DateTime reviewedAtUtc)
    {
        Status = PriceCorrectionStatus.Outdated;
        ReviewedAtUtc = reviewedAtUtc;
    }
}
