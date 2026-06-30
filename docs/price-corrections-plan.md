# Spec: User-submitted price corrections

Status: Draft · Module: Pricing · Author: Marc-André Roy · Date: 2026-06-29

## 1. Overview

The API currently serves read-only station prices sourced from the Régie essence Québec feed
(refreshed every 10 minutes by `PriceRefreshService`). This feature lets the mobile app submit
*corrected* prices when the official feed is wrong or stale. Corrections are community signal shown
**alongside** the official price — they never overwrite it.

## 2. Goals / Non-goals

**Goals**
- Authenticated endpoint for the mobile app to submit a corrected price for one station + fuel grade.
- Reject submissions that don't provably originate from our app.
- Throttle how many corrections a given device/user can submit per time window.
- Auto-accept small corrections; queue large ones (≥10% off the official price) for manual approval.
- Surface the accepted community price next to the official price on the read API.
- Expire a community price once the official feed publishes a newer price for that grade.

**Non-goals (now)**
- No approval review UI — the queue is populated and inspectable in the DB only.
- No end-user accounts/auth system (none exists today); submitter identity is a device id.
- No changes to the legacy Régie-shaped GeoJSON feed (kept byte-compatible for the mobile parser).

## 3. Definitions

- **Official price** — the latest feed-sourced `FuelPrice` for a (station, grade). Source of truth
  for "the posted price."
- **Community price** — the most recent **Accepted** user correction for a (station, grade).
- **Correction** — one submission record (`PriceCorrection`), also serving as audit log + queue.

## 4. Functional requirements

### 4.1 Submission endpoint
`POST /price/corrections`

Request body (`application/json`):
```json
{ "stationId": "<guid>", "fuelType": "regular|super|diesel", "priceCents": 169.9 }
```
Submitter identity (device id) is **not** taken from the body — it comes from the validated
authenticity headers (§5).

Responses:
| Condition | Status | Body |
|---|---|---|
| Accepted (delta < threshold) | `200 OK` | `{ id, status: "Accepted", percentChange }` |
| Queued (delta ≥ threshold) | `202 Accepted` | `{ id, status: "Pending", percentChange }` |
| Bad fuel type / non-positive / out-of-range price | `400` | error |
| Unknown station | `404` | error |
| Missing/invalid/expired/replayed signature | `401` | — |
| Rate limit exceeded | `429` | — |

### 4.2 Approval threshold
- Percent change = `|submitted − official| / official` measured against the current **official**
  `FuelPrice.PriceCents` for that grade. Threshold default **0.10**, configurable.
- No official price yet for the grade → treat as below threshold (Accepted).
- `< threshold` → record as **Accepted** (becomes the community price immediately).
- `≥ threshold` → record as **Pending**; not surfaced and not applied until a future approval flow.
- A correction never mutates `Station` / `FuelPrice` / `PriceHistoryEntry`.

### 4.3 Read model — show both prices
The owned v1 contract (`GET /api/v1/stations`) returns, per grade, the official price **and** the
community price when one exists:
- `priceCents` / `isAvailable` / `observedAtUtc` — unchanged, official.
- **new** `reportedPriceCents` (decimal?), `reportedAtUtc` (DateTime?) — the latest Accepted
  correction for that grade, or null.

The legacy `/stations.geojson` feed is **unchanged**.

### 4.4 Lifecycle — feed supersedes community prices
When `PriceRefreshService` records a *change* to the official price of a (station, grade), the
current Accepted correction for that grade is marked **Outdated**. Outdated corrections are no
longer surfaced, so the app falls back to the official price. Refreshes that don't change a grade
leave its community price intact.

Correction state machine:
```
            submit (<10%)        submit (>=10%)
              │                       │
              ▼                       ▼
          [Accepted] ──feed change──► [Outdated]
              ▲                    [Pending] ──approve──► [Accepted]
              └───────approve──────────┘   └──reject────► [Rejected]
```
(`Approve`/`Reject` transitions are modeled in the domain now but exercised later by the review UI.)

## 5. Authenticity — HMAC-signed requests

The app holds a shared secret (from configuration; injectable via env/secret store in prod) and
signs each correction request. Read endpoints are unaffected.

Headers: `X-Device-Id`, `X-Timestamp` (unix seconds, UTC), `X-Nonce` (random), `X-Signature`.

Signature = `Base64(HMACSHA256(secret, $"{method}\n{path}\n{timestamp}\n{nonce}\n{deviceId}\n{sha256hex(body)}"))`.
The device id is part of the signed string so it can't be spoofed to dodge the per-device throttle.

Server validation (else `401`):
- All headers present.
- `|now − timestamp|` within `MaxClockSkew` (default 5 min).
- Nonce unseen within the skew window (replay cache via `IMemoryCache`, TTL = skew).
- Constant-time signature match.
- On success the device id is stashed in `HttpContext.Items` for the controller + rate limiter.

> Note: the secret is embedded in the app binary and is therefore extractable by a determined
> attacker. This raises the bar (blocks casual URL discovery + replay) but is not attestation;
> platform attestation (Play Integrity / App Attest) is a future hardening option.

## 6. Throttle — rate limiting

ASP.NET Core built-in rate limiter, named policy `price-corrections`, partitioned on the device id
(from §5, falling back to remote IP). Configurable `PermitLimit` and `WindowMinutes`. Over-limit →
`429`. In-memory and per-instance (acceptable for the single-instance deploy).

## 7. Data model

New table `pricing.price_corrections` (aggregate `PriceCorrection : AggregateRoot<Guid>`):

| Column | Type | Notes |
|---|---|---|
| Id | guid | PK, `Guid.CreateVersion7()` |
| StationId | guid | references a station |
| FuelType | text | `HasConversion<string>()` |
| SubmittedPriceCents | numeric(8,2) | |
| PreviousPriceCents | numeric(8,2)? | official price at submit time |
| PercentChange | numeric | absolute fraction |
| Status | text | `Accepted` \| `Pending` \| `Rejected` \| `Outdated` |
| SubmitterId | text | device id from signed request |
| SubmittedAtUtc | timestamptz | |
| ReviewedAtUtc | timestamptz? | set on approve/reject/outdate |

Indexes: `(SubmitterId, SubmittedAtUtc)`, `(Status)`, `(StationId, FuelType, Status)`.

## 8. Configuration

- `Pricing:Corrections` → `PriceCorrectionOptions`: `Threshold` (0.10), price bounds (min/max).
- `Security:Hmac` → `HmacOptions`: `Secret`, `MaxClockSkew`.
- `Security:RateLimit` → `RateLimitOptions`: `PermitLimit`, `WindowMinutes`.

Empty/secret-less defaults in `appsettings.json`; real dev values in `appsettings.Development.json`.

## 9. Acceptance criteria

1. Valid signed request, delta <10% → `200`, row `Accepted`; official price/history untouched;
   `GET /api/v1/stations` returns official `priceCents` **and** `reportedPriceCents`/`reportedAtUtc`.
2. Valid signed request, delta ≥10% → `202`, row `Pending`; nothing surfaced; official unchanged.
3. Missing/invalid signature → `401`; replayed nonce → `401`; stale timestamp → `401`.
4. Exceed configured rate limit in window → `429`.
5. Unknown station → `404`; bad fuel type / non-positive / out-of-range price → `400`.
6. After an Accepted correction, a feed refresh that changes that grade flips the correction to
   `Outdated` and removes `reportedPriceCents` from the read response.
7. Architecture tests stay green (no cross-module deps; repos only in Infrastructure).

---

## Appendix A — Implementation map (Pricing module + API)

Reuses existing patterns: write flow via `IPricingUnitOfWork.SaveChangesAsync`
(`PriceRefreshService`), `Result`/`Error`, `FuelTypeTokens` parsing, options binding + DI in
`PricingModule`, EF config/migration conventions, and startup migration in `Program.cs`.

**Create — Domain:** `PriceCorrection.cs` (factory computes `PercentChange` + initial status;
`Approve()`/`Reject()`/`MarkOutdated()`), `PriceCorrectionStatus.cs`,
`IPriceCorrectionRepository.cs` (`IRepository<PriceCorrection,Guid>` +
`CountBySubmitterSinceAsync`, `GetLatestAcceptedAsync`, `GetLatestAcceptedForStationAsync`,
`MarkAcceptedOutdatedAsync(pairs, ct)`).

**Create — Application:** `IPriceCorrectionService.cs` + `PriceCorrectionService.cs` (validate →
compute delta vs official → persist Accepted/Pending → `Result<PriceCorrectionResultDto>`),
`PriceCorrectionOptions.cs`, `Contracts/SubmitPriceCorrectionRequest.cs`,
`Contracts/PriceCorrectionResultDto.cs`.

**Create — Infrastructure:** `PriceCorrectionRepository.cs`,
`Configurations/PriceCorrectionConfiguration.cs`, EF migration `AddPriceCorrections`.

**Create — Api:** `Security/HmacOptions.cs`, `Security/HmacSignatureFilter.cs`,
`Security/RateLimitOptions.cs`.

**Modify:** `PricingDbContext.cs` (DbSet), `PricingModule.cs` (DI + options),
`Contracts/PriceDto.cs` (reported-price fields), `StationService.cs` (inject
`IPriceCorrectionRepository`, merge community prices into v1 response only),
`PriceRefreshService.cs` (collect changed (stationId, fuelType) pairs → `MarkAcceptedOutdatedAsync`
before final save), `PriceController.cs` (`POST corrections` with HMAC filter + rate-limit policy),
`Program.cs` (`AddRateLimiter` + `UseRateLimiter` + filter registration), `appsettings*.json`.
`Station.cs` is **not** modified.

## Appendix B — Verification

- `dotnet build` + `dotnet test` (architecture tests green).
- Run via AppHost; confirm migration created `pricing.price_corrections`.
- Manual end-to-end with a small HMAC-signing script covering each acceptance criterion in §9,
  including triggering `POST /stations/refresh` to exercise the Outdated transition. See
  [testing-hmac-endpoints.md](testing-hmac-endpoints.md) for a ready-to-use signing helper and the
  Scalar walkthrough.
