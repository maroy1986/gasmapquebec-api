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
- **Read endpoints (additive):**
  - `GET /stations.geojson` — legacy: gzipped GeoJSON FeatureCollection byte-compatible with the
    Régie feed, so switching the mobile app is a one-line feed-URL change
    (`lib/services/station_service.dart`). Unchanged.
  - `GET /api/v1/stations` — owned v1 contract (latest prices): flat JSON, camelCase, numeric
    `priceCents`, locale-free `fuelType` tokens (`regular|super|diesel`), named
    `location.{latitude,longitude}`, stable GUIDv7 `id`, per-price `observedAt`. Adopting it needs a
    mobile parser update (`lib/models/station.dart` + `station_service.dart`), tracked in the Flutter repo.
  - `GET /api/v1/stations/{id}/prices/history` — price timeline for one station, grouped by grade;
    defaults to the last 30 days, with `?from=&to=` (ISO-8601 UTC) and `?fuelType=` filters.
    404 for unknown station, 400 for a bad `fuelType` token.
  - **Price freshness semantics:** `observedAt` (v1) and `generated_at` (both endpoints) now mean
    "price **last changed** at", not "last fetched" — a refresh only writes/stamps a grade when its
    price or availability actually changed (see Price history below).
  - **Transport compression is the Caddy ingress's job** (`encode zstd gzip`), not the app's.
    Caddy must be configured to **skip `/stations.geojson`** (pre-gzipped payload with no
    `Content-Encoding`; re-compressing it would double-gzip and break the mobile client).

## Working-style preferences (apply going forward)

- **One type per file** — never multiple classes/records/interfaces in a single `.cs` file.
- **No unrequested tests / E2E** — only the explicitly requested tests (architecture tests
  here). Don't spin up live end-to-end verification unless asked.

## Solution layout

```
src/
  Shared/GasMapQuebec.Shared.Abstractions/   # Entity, AggregateRoot, ValueObject, IRepository, IUnitOfWork, Result/Error, IDomainEvent
  Modules/
    Pricing/GasMapQuebec.Pricing.{Domain, Application, Infrastructure}
    FuelLog/GasMapQuebec.FuelLog.{Domain, Application, Infrastructure}
  GasMapQuebec.Api/                  # composition root: controllers, module registration, Hangfire
tests/GasMapQuebec.ArchitectureTests/  # NetArchTest rules
GasMapQuebec.AppHost/                # Aspire orchestrator (Postgres + api)
GasMapQuebec.ServiceDefaults/
```

- Each module owns its own `DbContext` + Postgres schema (`pricing`, `fuellog`) and a
  module-scoped unit of work (`IPricingUnitOfWork` / `IFuelLogUnitOfWork`) to avoid DI
  ambiguity in the shared container.
- DI entry points: `builder.AddPricingModule()` / `builder.AddFuelLogModule()`.

## Key components

- Pricing: `Station` aggregate (+ `FuelPrice`, `GeoCoordinate`) holds the **latest** price per grade;
  `PriceHistoryEntry` is a **separate append-only aggregate** (table `price_history`) for the timeline.
  `IStationRepository` / `IPriceRepository` / `IPriceHistoryRepository`,
  `RegieEssenceQuebecPriceService` (downloads/gunzips/parses the feed),
  `PriceRefreshService` (change-detecting upsert — see Price history), `StationQueryService`
  (v1 `Contracts/*` + legacy `GeoJson/*`), `PriceHistoryQueryService`.
- **Price history:** `PriceRefreshService` upserts the latest `FuelPrice` in place *and* appends a
  `PriceHistoryEntry` only when a grade's price/availability actually changes (or on first sight).
  `Station.ApplyPrices` returns the changed grades; unchanged grades write nothing, so each
  10-minute refresh rewrites only what moved and the history table grows with real movements.
- FuelLog: `FuelLogEntry` aggregate, `IFuelLogRepository`, `FuelLogService`.
- API: `PriceController`, `StationsController` (`GET /api/v1/stations`,
  `GET /api/v1/stations/{id}/prices/history`, `GET /stations.geojson`, `POST /stations/refresh`),
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
dotnet test tests/GasMapQuebec.ArchitectureTests        # 7 rules, all passing
```

EF migrations already exist (`Migrations/InitialCreate` in each `*.Infrastructure`). To add
more, use the design-time factories already present, e.g.:

```
dotnet ef migrations add <Name> \
  --project src/Modules/Pricing/GasMapQuebec.Pricing.Infrastructure \
  --startup-project src/Modules/Pricing/GasMapQuebec.Pricing.Infrastructure \
  --context PricingDbContext --output-dir Migrations
```

## Current state

- Solution builds clean (0 warnings, 0 errors); 7 architecture tests pass.
- All work is committed and pushed to `origin/main`.
- No end-to-end/live run was performed (by request).

## Deployment (docker-compose + host Caddy)

`deploy/` is a self-contained server deploy: `docker-compose.yml` (API + Postgres + volume) plus a
sample `Caddyfile` and `.env.example`. Caddy runs on the host (not in compose) and reverse-proxies
to the API, which is published on `127.0.0.1:8080` only (never `0.0.0.0`).

```
cd deploy && cp .env.example .env    # set POSTGRES_PASSWORD
docker compose up -d --build
```

- Runs as `Production`; `RunMigrationsAtStartup=true` applies EF migrations on boot (single
  instance). `UseForwardedHeaders` trusts Caddy's `X-Forwarded-Proto/For` so HTTPS redirect works.
- **Transport compression is owned by Caddy** (`encode zstd gzip`); the app no longer compresses.
  The Caddyfile must exclude `/stations.geojson` (pre-gzipped payload — re-compressing double-gzips
  and breaks the mobile client).
- The **Hangfire dashboard is Dev-only** (unauthenticated). To use it in prod, enable it for
  Production in `Program.cs` and protect `/hangfire` with Caddy `basic_auth` (commented snippet in
  the Caddyfile).
- `deploy/.env` is gitignored; only `.env.example` is tracked.

## Possible next steps (not started)

- Run via Aspire (`dotnet run --project GasMapQuebec.AppHost`) once Docker is available,
  to confirm migrations + the 10-min refresh job + endpoints against a live DB.
- Authn/authz for FuelLog (currently `userId` is passed in).
- Price history/time-series (current design keeps latest price per station+fuel type).
- Switch the mobile app's feed URL to the API's `/stations.geojson` once deployed.
