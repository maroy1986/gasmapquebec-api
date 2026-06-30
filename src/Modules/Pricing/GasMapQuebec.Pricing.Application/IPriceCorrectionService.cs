using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Pricing.Application;

/// <summary>
/// Handles user-submitted price corrections: validates, compares against the official price,
/// and either accepts (community price) or queues (≥ threshold) the submission.
/// </summary>
public interface IPriceCorrectionService
{
    Task<Result<PriceCorrectionResultDto>> SubmitAsync(
        SubmitPriceCorrectionRequest request,
        string submitterId,
        CancellationToken cancellationToken = default);
}
