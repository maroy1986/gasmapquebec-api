namespace GasMapQuebec.Api.Security;

/// <summary>
/// Per-device throttle for price-correction submissions (config section <c>Security:RateLimit</c>).
/// </summary>
public sealed class RateLimitOptions
{
    public const string SectionName = "Security:RateLimit";

    /// <summary>Named rate-limiter policy applied to the corrections endpoint.</summary>
    public const string PolicyName = "price-corrections";

    /// <summary>Maximum submissions allowed per device within <see cref="WindowMinutes"/>.</summary>
    public int PermitLimit { get; set; } = 10;

    /// <summary>Length of the rolling window in minutes.</summary>
    public int WindowMinutes { get; set; } = 60;
}
