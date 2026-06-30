using Microsoft.Extensions.Logging;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Application;

public sealed class PriceRefreshService(
    IPriceService priceService,
    IStationRepository stationRepository,
    IPriceHistoryRepository priceHistoryRepository,
    IPriceCorrectionRepository priceCorrectionRepository,
    IPricingUnitOfWork unitOfWork,
    ILogger<PriceRefreshService> logger) : IPriceRefreshService
{
    public async Task<int> RefreshAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting price refresh from the Régie essence Québec feed.");

        var snapshot = await priceService.GetLatestAsync(cancellationToken);
        logger.LogInformation(
            "Fetched {Count} stations from feed generated at {GeneratedAt:o}.",
            snapshot.Stations.Count, snapshot.GeneratedAtUtc);

        var existing = (await stationRepository.GetAllWithPricesAsync(cancellationToken))
            .ToDictionary(s => s.CoordinateKey);

        var added = 0;
        var history = new List<PriceHistoryEntry>();
        // (station, grade) pairs whose official price changed this run — their community
        // corrections are now superseded and get marked outdated below.
        var changedGrades = new List<(Guid StationId, FuelType FuelType)>();

        foreach (var record in snapshot.Stations)
        {
            var coordinate = GeoCoordinate.Create(record.Latitude, record.Longitude);
            var key = coordinate.ToKey();
            var observations = record.Prices
                .Select(p => (p.FuelType, p.PriceCents, p.IsAvailable))
                .ToList();

            if (existing.TryGetValue(key, out var station))
            {
                // Existing stations are change-tracked (loaded with tracking above), so we mutate
                // them and let EF write only the rows that actually changed — no repository.Update,
                // which would force-mark the whole station + price graph as modified every run.
                station.UpdateDetails(record.Name, record.Brand, record.Status, record.Address, record.PostalCode, record.Region);
            }
            else
            {
                station = Station.Create(coordinate, record.Name, record.Brand, record.Status, record.Address, record.PostalCode, record.Region);
                await stationRepository.AddAsync(station, cancellationToken);
                existing[key] = station;
                added++;
            }

            // ApplyPrices returns only the grades that actually changed (or are new); append a
            // history point for each so the price timeline grows with real movements, not fetches.
            foreach (var change in station.ApplyPrices(observations, snapshot.GeneratedAtUtc))
            {
                history.Add(PriceHistoryEntry.Create(
                    station.Id, change.FuelType, change.PriceCents, change.IsAvailable, snapshot.GeneratedAtUtc));
                changedGrades.Add((station.Id, change.FuelType));
            }
        }

        if (history.Count > 0)
        {
            await priceHistoryRepository.AddRangeAsync(history, cancellationToken);
        }

        // A newer official price supersedes any community correction for that grade.
        if (changedGrades.Count > 0)
        {
            await priceCorrectionRepository.MarkAcceptedOutdatedAsync(
                changedGrades, snapshot.GeneratedAtUtc, cancellationToken);
        }

        var rowsWritten = await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Price refresh complete: {Added} new stations, {Changes} price changes recorded, {RowsWritten} rows written.",
            added, history.Count, rowsWritten);
        return snapshot.Stations.Count;
    }
}
