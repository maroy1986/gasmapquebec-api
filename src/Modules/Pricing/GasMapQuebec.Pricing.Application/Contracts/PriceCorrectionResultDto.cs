namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>
/// Outcome of a submitted correction: its id, status (<c>Accepted</c> when applied as the
/// community price, <c>Pending</c> when queued for approval), and the computed fractional
/// change vs the official price.
/// </summary>
public sealed record PriceCorrectionResultDto(
    Guid Id,
    string Status,
    decimal PercentChange);
