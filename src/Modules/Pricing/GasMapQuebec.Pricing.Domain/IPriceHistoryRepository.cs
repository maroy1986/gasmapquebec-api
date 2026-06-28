namespace GasMapQuebec.Pricing.Domain;

/// <summary>Append-only store of price changes; read back per station for the history endpoint.</summary>
public interface IPriceHistoryRepository
{
    Task AddRangeAsync(IReadOnlyCollection<PriceHistoryEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// History points for a station within [<paramref name="fromUtc"/>, <paramref name="toUtc"/>],
    /// optionally limited to a single <paramref name="fuelType"/>, oldest first.
    /// </summary>
    Task<IReadOnlyList<PriceHistoryEntry>> GetForStationAsync(
        Guid stationId, FuelType? fuelType, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
