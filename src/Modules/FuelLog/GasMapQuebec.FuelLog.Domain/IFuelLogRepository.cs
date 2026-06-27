using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.FuelLog.Domain;

public interface IFuelLogRepository : IRepository<FuelLogEntry, Guid>
{
    Task<IReadOnlyList<FuelLogEntry>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
