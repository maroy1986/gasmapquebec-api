using FuelLog.Domain;
using Microsoft.EntityFrameworkCore;

namespace FuelLog.Infrastructure;

internal sealed class FuelLogRepository(FuelLogDbContext dbContext) : IFuelLogRepository
{
    public Task<FuelLogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Entries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<FuelLogEntry>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.Entries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.FilledAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(FuelLogEntry entity, CancellationToken cancellationToken = default) =>
        await dbContext.Entries.AddAsync(entity, cancellationToken);

    public void Update(FuelLogEntry entity) => dbContext.Entries.Update(entity);

    public void Remove(FuelLogEntry entity) => dbContext.Entries.Remove(entity);
}
