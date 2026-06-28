using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Application;

public sealed class PriceHistoryQueryService(
    IStationRepository stationRepository,
    IPriceHistoryRepository priceHistoryRepository) : IPriceHistoryQueryService
{
    /// <summary>Default look-back when the caller doesn't specify a window.</summary>
    public static readonly TimeSpan DefaultWindow = TimeSpan.FromDays(30);

    public async Task<StationPriceHistoryResponse?> GetHistoryAsync(
        Guid stationId,
        FuelType? fuelType,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        if (!await stationRepository.ExistsAsync(stationId, cancellationToken))
            return null;

        var to = (toUtc ?? DateTime.UtcNow).ToUniversalTime();
        var from = (fromUtc ?? to - DefaultWindow).ToUniversalTime();

        var entries = await priceHistoryRepository.GetForStationAsync(stationId, fuelType, from, to, cancellationToken);

        var grades = entries
            .GroupBy(e => e.FuelType)
            .OrderBy(g => g.Key)
            .Select(g => new FuelGradeHistory(
                FuelTypeTokens.ToToken(g.Key),
                g.OrderBy(e => e.ObservedAtUtc)
                    .Select(e => new PriceHistoryPoint(e.PriceCents, e.IsAvailable, e.ObservedAtUtc))
                    .ToList()))
            .ToList();

        return new StationPriceHistoryResponse(stationId, from, to, grades);
    }
}
