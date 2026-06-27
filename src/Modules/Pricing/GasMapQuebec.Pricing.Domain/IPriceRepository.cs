namespace GasMapQuebec.Pricing.Domain;

/// <summary>
/// Read-only queries over fuel prices that span stations.
/// </summary>
public interface IPriceRepository
{
    Task<IReadOnlyList<FuelPrice>> GetByStationIdAsync(Guid stationId, CancellationToken cancellationToken = default);
}
