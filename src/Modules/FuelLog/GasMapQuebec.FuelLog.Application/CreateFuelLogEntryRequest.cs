using GasMapQuebec.FuelLog.Domain;

namespace GasMapQuebec.FuelLog.Application;

public sealed record CreateFuelLogEntryRequest(
    Guid UserId,
    DateTime FilledAtUtc,
    FuelGrade FuelGrade,
    decimal Litres,
    decimal TotalCost,
    int? OdometerKm,
    Guid? StationId,
    string? StationName,
    string? Notes);
