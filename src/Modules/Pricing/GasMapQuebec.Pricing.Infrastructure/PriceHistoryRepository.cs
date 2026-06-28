using Microsoft.EntityFrameworkCore;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure;

internal sealed class PriceHistoryRepository(PricingDbContext dbContext) : IPriceHistoryRepository
{
    public async Task AddRangeAsync(IReadOnlyCollection<PriceHistoryEntry> entries, CancellationToken cancellationToken = default) =>
        await dbContext.PriceHistory.AddRangeAsync(entries, cancellationToken);

    public async Task<IReadOnlyList<PriceHistoryEntry>> GetForStationAsync(
        Guid stationId, FuelType? fuelType, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PriceHistory.AsNoTracking()
            .Where(h => h.StationId == stationId && h.ObservedAtUtc >= fromUtc && h.ObservedAtUtc <= toUtc);

        if (fuelType is not null)
            query = query.Where(h => h.FuelType == fuelType);

        return await query
            .OrderBy(h => h.FuelType)
            .ThenBy(h => h.ObservedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
