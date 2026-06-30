using System.Security.Cryptography;
using System.Text;
using GasMapQuebec.Api.Security;
using GasMapQuebec.Pricing.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GasMapQuebec.Api.IntegrationTests.TestInfrastructure;

/// <summary>
/// Hosts the real <c>PriceController</c> plus the real security pipeline (HMAC filter + rate
/// limiter from <see cref="SecurityServiceCollectionExtensions.AddCorrectionSecurity"/>) on an
/// in-memory <see cref="TestServer"/> — no database or Hangfire. Each instance is isolated, so
/// replay-cache and rate-limit state don't bleed between tests.
/// </summary>
internal sealed class PriceCorrectionTestHost : IAsyncDisposable
{
    public const string Path = "/Price/corrections";
    public const string DeviceId = "device-1";

    private readonly WebApplication _app;

    public string Secret { get; }
    public StubPriceCorrectionService CorrectionService { get; }
    public HttpClient Client { get; }

    private PriceCorrectionTestHost(WebApplication app, string secret, StubPriceCorrectionService correctionService)
    {
        _app = app;
        Secret = secret;
        CorrectionService = correctionService;
        Client = app.GetTestClient();
    }

    public static async Task<PriceCorrectionTestHost> StartAsync(string secret = "test-secret", int permitLimit = 100)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Security:Hmac:Secret"] = secret,
            ["Security:Hmac:MaxClockSkew"] = "00:05:00",
            ["Security:RateLimit:PermitLimit"] = permitLimit.ToString(),
            ["Security:RateLimit:WindowMinutes"] = "60"
        });

        builder.Logging.ClearProviders();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(Controllers.PriceController).Assembly);
        builder.Services.AddCorrectionSecurity(builder.Configuration);

        var correctionService = new StubPriceCorrectionService();
        builder.Services.AddSingleton<IPriceCorrectionService>(correctionService);
        builder.Services.AddSingleton<IStationService, StubStationService>();

        var app = builder.Build();
        app.UseRateLimiter();
        app.MapControllers();
        await app.StartAsync();

        return new PriceCorrectionTestHost(app, secret, correctionService);
    }

    /// <summary>Builds a signed request for the given body, allowing overrides for negative tests.</summary>
    public HttpRequestMessage SignedRequest(
        string body,
        string? deviceId = null,
        DateTimeOffset? timestamp = null,
        string? nonce = null,
        string? overrideSignature = null)
    {
        deviceId ??= DeviceId;
        var ts = (timestamp ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds().ToString();
        nonce ??= Guid.NewGuid().ToString("N");

        var bodyHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
        var canonical = string.Join('\n', "POST", Path, ts, nonce, deviceId, bodyHash);
        var signature = overrideSignature ?? Convert.ToBase64String(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), Encoding.UTF8.GetBytes(canonical)));

        var request = new HttpRequestMessage(HttpMethod.Post, Path)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Device-Id", deviceId);
        request.Headers.Add("X-Timestamp", ts);
        request.Headers.Add("X-Nonce", nonce);
        request.Headers.Add("X-Signature", signature);
        return request;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}
