# Testing HMAC-signed endpoints in Scalar

`POST /Price/corrections` is protected by the `HmacSignatureFilter`: every request must carry a
valid `X-Signature` (plus `X-Device-Id`, `X-Timestamp`, `X-Nonce`). Scalar's "Send" button can't
compute the signature for you, so the workflow is:

1. Run a small signing helper (below) for the exact body you want to send.
2. Paste the four headers it prints into Scalar.
3. Paste the **identical** body string into Scalar's request body and send.

Scalar (Development only) is at **`/scalar/v1`**. The dev shared secret lives in
[appsettings.Development.json](../src/GasMapQuebec.Api/appsettings.Development.json)
(`Security:Hmac:Secret` = `dev-only-shared-secret-change-me`). In other environments the secret is
empty and the endpoint **fails closed** (every request → `401`) until one is configured.

## How the signature is built

```
canonical = "{method}\n{path}\n{timestamp}\n{nonce}\n{deviceId}\n{sha256hex(body)}"
X-Signature = Base64(HMACSHA256(secret, canonical))
```

What this means in practice:
- **`path`** is signed verbatim — use exactly `/Price/corrections` (the route token `[controller]`
  resolves to `Price`). Sign the same casing/string you actually send.
- **`body`** is hashed byte-for-byte. Paste the same string you hashed into Scalar — if Scalar (or
  your editor) reformats the JSON, the hash won't match and you'll get `401`.
- **`X-Timestamp`** is unix seconds (UTC) and must be within ±5 min of server time (`MaxClockSkew`).
- **`X-Nonce`** must be unique per request — replays inside the skew window are rejected (`401`).

So you regenerate the headers for **every** attempt (fresh timestamp + nonce).

## PowerShell signing helper

```powershell
$secret   = "dev-only-shared-secret-change-me"   # Security:Hmac:Secret (dev)
$method   = "POST"
$path     = "/Price/corrections"
$deviceId = "dev-device-1"
# Keep this on one line; paste the IDENTICAL string into Scalar's body.
$body     = '{"stationId":"00000000-0000-0000-0000-000000000000","fuelType":"regular","priceCents":169.9}'

$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
$nonce     = [guid]::NewGuid().ToString("N")

$bodyBytes = [Text.Encoding]::UTF8.GetBytes($body)
$bodyHash  = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bodyBytes)).ToLower()

$canonical = "$method`n$path`n$timestamp`n$nonce`n$deviceId`n$bodyHash"
$hmac      = [Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($secret))
$signature = [Convert]::ToBase64String($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonical)))

Write-Host "X-Device-Id: $deviceId"
Write-Host "X-Timestamp: $timestamp"
Write-Host "X-Nonce:     $nonce"
Write-Host "X-Signature: $signature"
Write-Host "`nBody (paste verbatim):`n$body"
```

## In Scalar

1. Open `/scalar/v1`, pick **POST /Price/corrections**.
2. Under **Headers**, add: `X-Device-Id`, `X-Timestamp`, `X-Nonce`, `X-Signature` with the values
   the helper printed.
3. Under **Body**, paste the exact `$body` string.
4. **Send.** Expect:
   - `200` — accepted (change < 10% of the official price); it becomes the community price.
   - `202` — queued (change ≥ 10%); row stored as `Pending`.
   - `400` — bad fuel type / price out of range; `404` — unknown station.
   - `401` — bad/missing/expired/replayed signature (re-run the helper for fresh headers).
   - `429` — per-device rate limit hit (default 10 / hour; see `Security:RateLimit`).

Use a real `stationId` from `GET /api/v1/stations`. After an accepted correction, that endpoint
returns both the official `priceCents` and the new `reportedPriceCents`/`reportedAt`.

## curl alternative (bash)

```bash
secret="dev-only-shared-secret-change-me"
method="POST"; path="/Price/corrections"; device="dev-device-1"
body='{"stationId":"00000000-0000-0000-0000-000000000000","fuelType":"regular","priceCents":169.9}'

ts=$(date -u +%s)
nonce=$(uuidgen | tr -d '-')
body_hash=$(printf '%s' "$body" | openssl dgst -sha256 -hex | awk '{print $2}')
canonical=$(printf '%s\n%s\n%s\n%s\n%s\n%s' "$method" "$path" "$ts" "$nonce" "$device" "$body_hash")
sig=$(printf '%s' "$canonical" | openssl dgst -sha256 -hmac "$secret" -binary | base64)

curl -k -X POST "https://localhost:7080$path" \
  -H "Content-Type: application/json" \
  -H "X-Device-Id: $device" -H "X-Timestamp: $ts" -H "X-Nonce: $nonce" -H "X-Signature: $sig" \
  -d "$body"
```

> Adjust the host/port to your local run. The body in `-d` is sent verbatim, so its bytes match the
> hash — no copy/paste mismatch to worry about with this variant.
