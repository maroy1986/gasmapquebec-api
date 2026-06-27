using Microsoft.EntityFrameworkCore;
using Pricing.Domain;

namespace Pricing.Infrastructure;

internal sealed class PriceRepository(PricingDbContext dbContext) : IPriceRepository
{
    public async Task<IReadOnlyList<FuelPrice>> GetByStationIdAsync(Guid stationId, CancellationToken cancellationToken = default) =>
        await dbContext.Set<FuelPrice>()
            .AsNoTracking()
            .Where(p => p.StationId == stationId)
            .ToListAsync(cancellationToken);
}
