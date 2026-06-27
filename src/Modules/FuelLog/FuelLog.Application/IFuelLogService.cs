namespace FuelLog.Application;

public interface IFuelLogService
{
    Task<FuelLogEntryDto> CreateAsync(CreateFuelLogEntryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FuelLogEntryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FuelLogEntryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
