using Shared.Abstractions;

namespace FuelLog.Domain;

public interface IFuelLogRepository : IRepository<FuelLogEntry, Guid>
{
    Task<IReadOnlyList<FuelLogEntry>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
