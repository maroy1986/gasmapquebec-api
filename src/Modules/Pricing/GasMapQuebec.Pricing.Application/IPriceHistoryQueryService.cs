using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Application;

public interface IPriceHistoryQueryService
{
    /// <summary>
    /// Price timeline for a station, grouped by fuel grade. Defaults to the last 30 days when the
    /// window is unspecified. Returns <c>null</c> when the station does not exist.
    /// </summary>
    Task<StationPriceHistoryResponse?> GetHistoryAsync(
        Guid stationId,
        FuelType? fuelType,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default);
}
