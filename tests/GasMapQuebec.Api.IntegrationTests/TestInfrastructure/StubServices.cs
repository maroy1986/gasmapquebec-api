using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Pricing.Application.GeoJson;
using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Api.IntegrationTests.TestInfrastructure;

/// <summary>
/// Stand-in for <see cref="IPriceCorrectionService"/> so the endpoint can be exercised without a
/// database. Returns a configurable result and records the submitter id the controller passed in.
/// </summary>
internal sealed class StubPriceCorrectionService : IPriceCorrectionService
{
    public Func<SubmitPriceCorrectionRequest, string, Result<PriceCorrectionResultDto>> Handler { get; set; }
        = (_, _) => new PriceCorrectionResultDto(Guid.NewGuid(), "Accepted", 0.05m);

    public string? LastSubmitterId { get; private set; }

    public Task<Result<PriceCorrectionResultDto>> SubmitAsync(
        SubmitPriceCorrectionRequest request, string submitterId, CancellationToken cancellationToken = default)
    {
        LastSubmitterId = submitterId;
        return Task.FromResult(Handler(request, submitterId));
    }
}

/// <summary>The controller depends on <see cref="IStationService"/>; the corrections path never calls it.</summary>
internal sealed class StubStationService : IStationService
{
    public Task<StationsResponse> GetStationsAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by the corrections endpoint.");

    public Task<StationFeatureCollection> GetGeoJsonAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by the corrections endpoint.");
}
