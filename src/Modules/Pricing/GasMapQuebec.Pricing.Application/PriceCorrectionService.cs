using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Pricing.Domain;
using GasMapQuebec.Shared.Abstractions;
using Microsoft.Extensions.Options;

namespace GasMapQuebec.Pricing.Application;

public sealed class PriceCorrectionService(
    IStationRepository stationRepository,
    IPriceCorrectionRepository correctionRepository,
    IPricingUnitOfWork unitOfWork,
    IOptions<PriceCorrectionOptions> options) : IPriceCorrectionService
{
    private readonly PriceCorrectionOptions _options = options.Value;

    public async Task<Result<PriceCorrectionResultDto>> SubmitAsync(
        SubmitPriceCorrectionRequest request,
        string submitterId,
        CancellationToken cancellationToken = default)
    {
        if (!FuelTypeTokens.TryParse(request.FuelType, out var fuelType))
        {
            return Result.Failure<PriceCorrectionResultDto>(
                Error.Validation($"Unknown fuelType '{request.FuelType}'. Expected: regular, super, or diesel."));
        }

        if (request.PriceCents < _options.MinPriceCents || request.PriceCents > _options.MaxPriceCents)
        {
            return Result.Failure<PriceCorrectionResultDto>(Error.Validation(
                $"priceCents must be between {_options.MinPriceCents} and {_options.MaxPriceCents}."));
        }

        var station = await stationRepository.GetByIdAsync(request.StationId, cancellationToken);
        if (station is null)
        {
            return Result.Failure<PriceCorrectionResultDto>(
                Error.NotFound($"Station '{request.StationId}' was not found."));
        }

        var officialPriceCents = station.Prices
            .FirstOrDefault(p => p.FuelType == fuelType)?.PriceCents;

        var correction = PriceCorrection.Create(
            station.Id, fuelType, request.PriceCents, officialPriceCents,
            _options.Threshold, submitterId, DateTime.UtcNow);

        await correctionRepository.AddAsync(correction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PriceCorrectionResultDto(
            correction.Id, correction.Status.ToString(), correction.PercentChange);
    }
}
