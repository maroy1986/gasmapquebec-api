namespace GasMapQuebec.Pricing.Application;

/// <summary>
/// Tunables for user-submitted price corrections (config section <c>Pricing:Corrections</c>).
/// </summary>
public sealed class PriceCorrectionOptions
{
    public const string SectionName = "Pricing:Corrections";

    /// <summary>
    /// Fractional change vs the official price at or above which a correction is queued for
    /// approval instead of accepted immediately (default 0.10 = 10%).
    /// </summary>
    public decimal Threshold { get; set; } = 0.10m;

    /// <summary>Lowest plausible price in cents per litre; submissions below this are rejected.</summary>
    public decimal MinPriceCents { get; set; } = 1m;

    /// <summary>Highest plausible price in cents per litre; submissions above this are rejected.</summary>
    public decimal MaxPriceCents { get; set; } = 1000m;
}
