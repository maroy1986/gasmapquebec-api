namespace GasMapQuebec.Pricing.Application.Contracts;

public sealed record StationDto(
    Guid Id,
    string Name,
    string? Brand,
    string Status,
    string Address,
    string? PostalCode,
    string? Region,
    LocationDto Location,
    IReadOnlyList<PriceDto> Prices);
