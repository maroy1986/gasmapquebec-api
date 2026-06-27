# Issue #1 — Build base API — Handoff / Context

Status snapshot for continuing the work on another machine. The code is committed and
pushed to `origin/main` (commit "Add CleanArch DDD project structure. Price and FuelLog
domain."). This file captures the context that isn't obvious from the code alone.

## What issue #1 asked for

- Base API following **clean-architecture modular monolith** principles
- Two domains: **Pricing** and **Fuel Log**
- **PostgreSQL** (Npgsql) + **EF Core** behind the **repository pattern**
- **Architecture tests** to enforce module/layer boundaries
- **PostgreSQL wired into Aspire**
- **Hangfire** recurring job pulling latest prices **every 10 minutes**

## Decisions made (confirmed with the owner)

- **Project-per-module** structure (not single-project-with-folders).
- Price source = **Régie essence Québec gzipped GeoJSON feed**:
  `https://regieessencequebec.ca/stations.geojson.gz`
  (FeatureCollection, ~2465 stations; `metadata.generated_at` is the observation time;
  `properties`: `Name`, `brand`, `Status`, `Address`, `PostalCode`, `Region`;
  `Prices[]`: `{GasType: Régulier|Super|Diesel, Price: "179.9¢", IsAvailable}`;
  `geometry.coordinates = [lng, lat]`).
- Station identity = **GUIDv7** primary key (`Guid.CreateVersion7()`), plus a unique
  **`CoordinateKey` = "lat,lng"** used to upsert the keyless feed. This "lat,lng" value
  mirrors the id the mobile app derives locally (see `gasmapquebec` Flutter repo,
  `lib/models/station.dart`), so the GUID stays internal and the mobile app is unaffected.
- Architecture tests use **NetArchTest**.
- **Mobile compatibility:** the API exposes `GET /stations.geojson` returning a gzipped
  GeoJSON FeatureCollection byte-compatible with the Régie feed, so switching the mobile
  app to our API is a one-line change to its feed URL (`lib/services/station_service.dart`).

## Working-style preferences (apply going forward)

- **One type per file** — never multiple classes/records/interfaces in a single `.cs` file.
- **No unrequested tests / E2E** — only the explicitly requested tests (architecture tests
  here). Don't spin up live end-to-end verification unless asked.

## Solution layout

```
src/
  Shared/Shared.Abstractions/        # Entity, AggregateRoot, ValueObject, IRepository, IUnitOfWork, Result/Error, IDomainEvent
  Modules/
    Pricing/{Pricing.Domain, Pricing.Application, Pricing.Infrastructure}
    FuelLog/{FuelLog.Domain, FuelLog.Application, FuelLog.Infrastructure}
  API/                               # composition root: controllers, module registration, Hangfire
tests/ArchitectureTests/             # NetArchTest rules
gasmapquebec-api.AppHost/            # Aspire orchestrator (Postgres + api)
gasmapquebec-api.ServiceDefaults/
```

- Each module owns its own `DbContext` + Postgres schema (`pricing`, `fuellog`) and a
  module-scoped unit of work (`IPricingUnitOfWork` / `IFuelLogUnitOfWork`) to avoid DI
  ambiguity in the shared container.
- DI entry points: `builder.AddPricingModule()` / `builder.AddFuelLogModule()`.

## Key components

- Pricing: `Station` aggregate (+ `FuelPrice`, `GeoCoordinate`), `IStationRepository` /
  `IPriceRepository`, `RegieEssenceQuebecPriceService` (downloads/gunzips/parses the feed),
  `PriceRefreshService` (upsert), `StationQueryService` (GeoJSON projection).
- FuelLog: `FuelLogEntry` aggregate, `IFuelLogRepository`, `FuelLogService`.
- API: `PriceController`, `StationsController` (`GET /stations.geojson`, `POST /stations/refresh`),
  `FuelLogController`; Hangfire (Postgres storage, schema `hangfire`) with recurring job
  `pricing:refresh-prices` at `*/10 * * * *`; EF migrations applied at startup in Development.
- Aspire AppHost: `AddPostgres("postgres").WithDataVolume().WithPgAdmin()` + `AddDatabase("gasmapdb")`,
  referenced by the `api` project. Both DbContexts use the Npgsql client integration
  (connection name `gasmapdb`).

## Environment / toolchain gotchas

- Requires the **.NET 10 SDK** (built with 10.0.301). `.slnx` also needs .NET 10 tooling.
- **EF Core Design is pinned to 10.0.8** in API + both `*.Infrastructure` projects to match
  the EF version pulled by `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` 13.4.6. Bumping
  Design to 10.0.9 reintroduces a CS1705 assembly-version conflict in ArchitectureTests.
- Running the **Aspire AppHost needs Docker** (Postgres container). Not required to build or
  run the architecture tests.

## How to build / test

```
dotnet build gasmapquebec-api.slnx
dotnet test tests/ArchitectureTests        # 7 rules, all passing
```

EF migrations already exist (`Migrations/InitialCreate` in each `*.Infrastructure`). To add
more, use the design-time factories already present, e.g.:

```
dotnet ef migrations add <Name> \
  --project src/Modules/Pricing/Pricing.Infrastructure \
  --startup-project src/Modules/Pricing/Pricing.Infrastructure \
  --context PricingDbContext --output-dir Migrations
```

## Current state

- Solution builds clean (0 warnings, 0 errors); 7 architecture tests pass.
- All work is committed and pushed to `origin/main`.
- No end-to-end/live run was performed (by request).

## Possible next steps (not started)

- Run via Aspire (`dotnet run --project gasmapquebec-api.AppHost`) once Docker is available,
  to confirm migrations + the 10-min refresh job + endpoints against a live DB.
- Authn/authz for FuelLog (currently `userId` is passed in).
- Price history/time-series (current design keeps latest price per station+fuel type).
- Switch the mobile app's feed URL to the API's `/stations.geojson` once deployed.
```
