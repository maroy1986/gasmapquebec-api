using GasMapQuebec.FuelLog.Domain;

namespace GasMapQuebec.FuelLog.Application;

public sealed record FuelLogEntryDto(
    Guid Id,
    Guid UserId,
    DateTime FilledAtUtc,
    FuelGrade FuelGrade,
    decimal Litres,
    decimal TotalCost,
    decimal? PricePerLitreCents,
    int? OdometerKm,
    Guid? StationId,
    string? StationName,
    string? Notes)
{
    public static FuelLogEntryDto FromEntity(FuelLogEntry entry) => new(
        entry.Id,
        entry.UserId,
        entry.FilledAtUtc,
        entry.FuelGrade,
        entry.Litres,
        entry.TotalCost,
        entry.PricePerLitreCents,
        entry.OdometerKm,
        entry.StationId,
        entry.StationName,
        entry.Notes);
}
