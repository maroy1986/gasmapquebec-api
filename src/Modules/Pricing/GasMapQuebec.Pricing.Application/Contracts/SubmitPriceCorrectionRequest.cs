namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>
/// A user-submitted price correction. <see cref="FuelType"/> is a locale-free token
/// (regular | super | diesel); <see cref="PriceCents"/> is the corrected price in cents
/// per litre (e.g. 169.9). The submitter's identity comes from the authenticated request,
/// not this body.
/// </summary>
public sealed record SubmitPriceCorrectionRequest(
    Guid StationId,
    string FuelType,
    decimal PriceCents);
