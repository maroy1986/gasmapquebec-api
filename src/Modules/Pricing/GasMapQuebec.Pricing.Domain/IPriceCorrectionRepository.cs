using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Pricing.Domain;

/// <summary>
/// Persistence for user-submitted price corrections: the approval queue, the read-side
/// community-price lookup, and the feed-driven supersession.
/// </summary>
public interface IPriceCorrectionRepository : IRepository<PriceCorrection, Guid>
{
    /// <summary>Count of corrections from a submitter since <paramref name="sinceUtc"/> (persisted-quota hook).</summary>
    Task<int> CountBySubmitterSinceAsync(string submitterId, DateTime sinceUtc, CancellationToken cancellationToken = default);

    /// <summary>Latest accepted correction per (station, fuel type) across all stations, for the list endpoint.</summary>
    Task<IReadOnlyList<PriceCorrection>> GetLatestAcceptedAsync(CancellationToken cancellationToken = default);

    /// <summary>Latest accepted correction per fuel type for one station.</summary>
    Task<IReadOnlyList<PriceCorrection>> GetLatestAcceptedForStationAsync(Guid stationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the accepted corrections for the supplied (station, fuel type) pairs as outdated.
    /// Mutations are tracked; the caller persists them via its unit of work.
    /// </summary>
    Task MarkAcceptedOutdatedAsync(
        IReadOnlyCollection<(Guid StationId, FuelType FuelType)> changed,
        DateTime asOfUtc,
        CancellationToken cancellationToken = default);
}
