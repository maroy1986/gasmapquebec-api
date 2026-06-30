using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using GasMapQuebec.Api.Security;
using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class PriceController(
    IStationService stationService,
    IPriceCorrectionService priceCorrectionService) : ControllerBase
{
    /// <summary>Returns all stations and their latest prices as GeoJSON.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var featureCollection = await stationService.GetGeoJsonAsync(cancellationToken);
        return Ok(featureCollection);
    }

    /// <summary>
    /// Submits a user-corrected price for one station + fuel grade. Requires a valid HMAC
    /// signature (mobile app) and is rate-limited per device. Corrections under the configured
    /// threshold are accepted immediately (200); larger ones are queued for approval (202).
    /// </summary>
    [HttpPost("corrections")]
    [ServiceFilter(typeof(HmacSignatureFilter))]
    [EnableRateLimiting(RateLimitOptions.PolicyName)]
    public async Task<IActionResult> SubmitCorrection(
        [FromBody] SubmitPriceCorrectionRequest request,
        CancellationToken cancellationToken)
    {
        var submitterId = HttpContext.Items[HmacSignatureFilter.DeviceIdItemKey] as string ?? string.Empty;

        var result = await priceCorrectionService.SubmitAsync(request, submitterId, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "NotFound" => NotFound(result.Error.Description),
                _ => BadRequest(result.Error.Description)
            };
        }

        var dto = result.Value;
        // Accepted → applied as the community price (200); Pending → queued for approval (202).
        return dto.Status == nameof(PriceCorrectionStatus.Pending)
            ? Accepted(dto)
            : Ok(dto);
    }
}
