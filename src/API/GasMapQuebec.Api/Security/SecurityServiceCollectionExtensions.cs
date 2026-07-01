using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace GasMapQuebec.Api.Security;

/// <summary>
/// Wires up authenticity (HMAC) and the per-device throttle for write endpoints. Lives in one
/// place so the app and integration tests share the exact same configuration.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddCorrectionSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<HmacOptions>(configuration.GetSection(HmacOptions.SectionName));
        services.AddScoped<HmacSignatureFilter>();

        var rateLimitOptions = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
            ?? new RateLimitOptions();
        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy(RateLimitOptions.PolicyName, httpContext =>
            {
                // The rate limiter runs before MVC filters, so the HMAC-validated device id isn't
                // in Items yet — partition on the X-Device-Id header (signed, so it can't be spoofed
                // by a request that would actually pass HMAC); fall back to remote IP if absent.
                var partitionKey = httpContext.Request.Headers["X-Device-Id"].ToString();
                if (string.IsNullOrEmpty(partitionKey))
                {
                    partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                }

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.PermitLimit,
                    Window = TimeSpan.FromMinutes(rateLimitOptions.WindowMinutes)
                });
            });
        });

        return services;
    }
}
