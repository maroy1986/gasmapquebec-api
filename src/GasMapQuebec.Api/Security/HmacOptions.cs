namespace GasMapQuebec.Api.Security;

/// <summary>
/// Shared-secret HMAC settings for authenticating mobile-app write requests
/// (config section <c>Security:Hmac</c>).
/// </summary>
public sealed class HmacOptions
{
    public const string SectionName = "Security:Hmac";

    /// <summary>Secret shared with the mobile app; signs requests. Empty disables enforcement is NOT allowed — see filter.</summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>Maximum allowed difference between the request timestamp and server time.</summary>
    public TimeSpan MaxClockSkew { get; set; } = TimeSpan.FromMinutes(5);
}
