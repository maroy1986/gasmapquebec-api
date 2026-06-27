namespace GasMapQuebec.Pricing.Application;

/// <summary>
/// Fetches the latest station prices from the external source
/// (Régie essence Québec). Implemented in the infrastructure layer.
/// </summary>
public interface IPriceService
{
    Task<StationPriceSnapshot> GetLatestAsync(CancellationToken cancellationToken = default);
}
