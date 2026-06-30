using Microsoft.EntityFrameworkCore;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure;

internal sealed class PriceCorrectionRepository(PricingDbContext dbContext) : IPriceCorrectionRepository
{
    public Task<PriceCorrection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.PriceCorrections.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(PriceCorrection entity, CancellationToken cancellationToken = default) =>
        await dbContext.PriceCorrections.AddAsync(entity, cancellationToken);

    public void Update(PriceCorrection entity) => dbContext.PriceCorrections.Update(entity);

    public void Remove(PriceCorrection entity) => dbContext.PriceCorrections.Remove(entity);

    public Task<int> CountBySubmitterSinceAsync(string submitterId, DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        dbContext.PriceCorrections
            .CountAsync(c => c.SubmitterId == submitterId && c.SubmittedAtUtc >= sinceUtc, cancellationToken);

    public async Task<IReadOnlyList<PriceCorrection>> GetLatestAcceptedAsync(CancellationToken cancellationToken = default)
    {
        // Accepted rows are pruned on every feed change (every ~10 min), so this set stays small;
        // reduce to the latest per (station, grade) in memory to avoid fragile group-by SQL.
        var accepted = await dbContext.PriceCorrections
            .AsNoTracking()
            .Where(c => c.Status == PriceCorrectionStatus.Accepted)
            .ToListAsync(cancellationToken);

        return LatestPerGrade(accepted);
    }

    public async Task<IReadOnlyList<PriceCorrection>> GetLatestAcceptedForStationAsync(Guid stationId, CancellationToken cancellationToken = default)
    {
        var accepted = await dbContext.PriceCorrections
            .AsNoTracking()
            .Where(c => c.Status == PriceCorrectionStatus.Accepted && c.StationId == stationId)
            .ToListAsync(cancellationToken);

        return LatestPerGrade(accepted);
    }

    public async Task MarkAcceptedOutdatedAsync(
        IReadOnlyCollection<(Guid StationId, FuelType FuelType)> changed,
        DateTime asOfUtc,
        CancellationToken cancellationToken = default)
    {
        if (changed.Count == 0)
        {
            return;
        }

        var pairs = changed.ToHashSet();
        var stationIds = changed.Select(c => c.StationId).Distinct().ToList();

        // Tracked (no AsNoTracking) so the caller's SaveChanges persists the status flips.
        var rows = await dbContext.PriceCorrections
            .Where(c => c.Status == PriceCorrectionStatus.Accepted && stationIds.Contains(c.StationId))
            .ToListAsync(cancellationToken);

        foreach (var row in rows.Where(r => pairs.Contains((r.StationId, r.FuelType))))
        {
            row.MarkOutdated(asOfUtc);
        }
    }

    private static IReadOnlyList<PriceCorrection> LatestPerGrade(IEnumerable<PriceCorrection> corrections) =>
        corrections
            .GroupBy(c => (c.StationId, c.FuelType))
            .Select(g => g.OrderByDescending(c => c.SubmittedAtUtc).First())
            .ToList();
}
