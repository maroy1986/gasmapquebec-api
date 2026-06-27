using Microsoft.Extensions.Logging;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Application;

public sealed class PriceRefreshService(
    IPriceService priceService,
    IStationRepository stationRepository,
    IPricingUnitOfWork unitOfWork,
    ILogger<PriceRefreshService> logger) : IPriceRefreshService
{
    public async Task<int> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await priceService.GetLatestAsync(cancellationToken);
        logger.LogInformation(
            "Fetched {Count} stations from feed generated at {GeneratedAt:o}.",
            snapshot.Stations.Count, snapshot.GeneratedAtUtc);

        var existing = (await stationRepository.GetAllWithPricesAsync(cancellationToken))
            .ToDictionary(s => s.CoordinateKey);

        var added = 0;
        var updated = 0;

        foreach (var record in snapshot.Stations)
        {
            var coordinate = GeoCoordinate.Create(record.Latitude, record.Longitude);
            var key = coordinate.ToKey();
            var observations = record.Prices
                .Select(p => (p.FuelType, p.PriceCents, p.IsAvailable))
                .ToList();

            if (existing.TryGetValue(key, out var station))
            {
                station.UpdateDetails(record.Name, record.Brand, record.Status, record.Address, record.PostalCode, record.Region);
                station.ApplyPrices(observations, snapshot.GeneratedAtUtc);
                stationRepository.Update(station);
                updated++;
            }
            else
            {
                station = Station.Create(coordinate, record.Name, record.Brand, record.Status, record.Address, record.PostalCode, record.Region);
                station.ApplyPrices(observations, snapshot.GeneratedAtUtc);
                await stationRepository.AddAsync(station, cancellationToken);
                existing[key] = station;
                added++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Price refresh complete: {Added} added, {Updated} updated.", added, updated);
        return added + updated;
    }
}
