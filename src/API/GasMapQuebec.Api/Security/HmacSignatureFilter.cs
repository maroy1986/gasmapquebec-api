using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GasMapQuebec.Api.Security;

/// <summary>
/// Authorizes a write request by verifying a shared-secret HMAC signature, proving the call
/// originates from our mobile app. Validates required headers, clock skew, nonce replay, and the
/// signature itself. On success the device id is exposed via <c>HttpContext.Items["DeviceId"]</c>
/// for the controller and rate limiter.
/// </summary>
/// <remarks>
/// Signature = Base64(HMACSHA256(secret,
/// $"{method}\n{path}\n{timestamp}\n{nonce}\n{deviceId}\n{sha256hex(body)}")).
/// The device id is signed so it can't be spoofed to evade the per-device throttle.
/// </remarks>
public sealed class HmacSignatureFilter(
    IOptions<HmacOptions> options,
    IMemoryCache replayCache,
    ILogger<HmacSignatureFilter> logger) : IAsyncAuthorizationFilter
{
    public const string DeviceIdItemKey = "DeviceId";

    private const string DeviceIdHeader = "X-Device-Id";
    private const string TimestampHeader = "X-Timestamp";
    private const string NonceHeader = "X-Nonce";
    private const string SignatureHeader = "X-Signature";

    private readonly HmacOptions _options = options.Value;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        if (string.IsNullOrEmpty(_options.Secret))
        {
            // Fail closed: an unconfigured secret must not leave the endpoint open.
            logger.LogError("HMAC secret is not configured; rejecting correction request.");
            context.Result = new UnauthorizedResult();
            return;
        }

        var deviceId = request.Headers[DeviceIdHeader].ToString();
        var timestamp = request.Headers[TimestampHeader].ToString();
        var nonce = request.Headers[NonceHeader].ToString();
        var signature = request.Headers[SignatureHeader].ToString();

        if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(timestamp)
                                           || string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(signature) || !long.TryParse(timestamp, out var unixSeconds))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        if ((DateTimeOffset.UtcNow - requestTime).Duration() > _options.MaxClockSkew)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Reject replays within the skew window. SetAbsoluteExpiration so the entry self-evicts.
        var replayKey = $"hmac-nonce:{nonce}";
        if (replayCache.TryGetValue(replayKey, out _))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var bodyHash = await ComputeBodyHashAsync(request, context.HttpContext.RequestAborted);

        var canonical = string.Join('\n',
            request.Method, request.Path.Value ?? string.Empty, timestamp, nonce, deviceId, bodyHash);

        var expected = Convert.ToBase64String(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(_options.Secret), Encoding.UTF8.GetBytes(canonical)));

        if (!FixedTimeEquals(expected, signature))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        replayCache.Set(replayKey, true, _options.MaxClockSkew);
        context.HttpContext.Items[DeviceIdItemKey] = deviceId;
    }

    private static async Task<string> ComputeBodyHashAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        request.EnableBuffering();
        request.Body.Position = 0;

        using var memory = new MemoryStream();
        await request.Body.CopyToAsync(memory, cancellationToken);
        // Rewind so MVC model binding can read the body after us.
        request.Body.Position = 0;

        return Convert.ToHexStringLower(SHA256.HashData(memory.ToArray()));
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
