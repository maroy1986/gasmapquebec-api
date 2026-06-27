using FuelLog.Domain;

namespace FuelLog.Application;

public sealed class FuelLogService(
    IFuelLogRepository repository,
    IFuelLogUnitOfWork unitOfWork) : IFuelLogService
{
    public async Task<FuelLogEntryDto> CreateAsync(CreateFuelLogEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = FuelLogEntry.Create(
            request.UserId,
            request.FilledAtUtc,
            request.FuelGrade,
            request.Litres,
            request.TotalCost,
            request.OdometerKm,
            request.StationId,
            request.StationName,
            request.Notes);

        await repository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return FuelLogEntryDto.FromEntity(entry);
    }

    public async Task<IReadOnlyList<FuelLogEntryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entries = await repository.GetByUserAsync(userId, cancellationToken);
        return entries.Select(FuelLogEntryDto.FromEntity).ToList();
    }

    public async Task<FuelLogEntryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await repository.GetByIdAsync(id, cancellationToken);
        return entry is null ? null : FuelLogEntryDto.FromEntity(entry);
    }
}
