namespace GasMapQuebec.Pricing.Application;

/// <summary>
/// Pulls the latest prices from the feed and upserts stations + prices.
/// Invoked on a schedule by the Hangfire recurring job.
/// </summary>
public interface IPriceRefreshService
{
    Task<int> RefreshAsync(CancellationToken cancellationToken = default);
}
