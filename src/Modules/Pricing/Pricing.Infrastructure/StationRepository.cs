using Microsoft.EntityFrameworkCore;
using Pricing.Domain;

namespace Pricing.Infrastructure;

internal sealed class StationRepository(PricingDbContext dbContext) : IStationRepository
{
    public Task<Station?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Stations.Include(s => s.Prices).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Station>> GetAllWithPricesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Stations.Include(s => s.Prices).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Station>> GetAllForReadAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Stations.Include(s => s.Prices).AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Station entity, CancellationToken cancellationToken = default) =>
        await dbContext.Stations.AddAsync(entity, cancellationToken);

    public void Update(Station entity) => dbContext.Stations.Update(entity);

    public void Remove(Station entity) => dbContext.Stations.Remove(entity);
}
