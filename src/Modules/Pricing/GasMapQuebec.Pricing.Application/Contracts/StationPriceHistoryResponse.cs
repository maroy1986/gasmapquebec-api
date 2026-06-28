namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>A station's price timeline over [FromUtc, ToUtc], grouped by fuel grade.</summary>
public sealed record StationPriceHistoryResponse(
    Guid StationId,
    DateTime FromUtc,
    DateTime ToUtc,
    IReadOnlyList<FuelGradeHistory> Grades);
