namespace GasMapQuebec.Pricing.Domain;

/// <summary>
/// Lifecycle of a user-submitted <see cref="PriceCorrection"/>.
/// </summary>
public enum PriceCorrectionStatus
{
    /// <summary>Below the approval threshold; is the current community-reported price.</summary>
    Accepted = 0,

    /// <summary>At or above the approval threshold; awaiting manual review.</summary>
    Pending = 1,

    /// <summary>Rejected by a reviewer; never surfaced.</summary>
    Rejected = 2,

    /// <summary>Superseded by a newer official price from the feed; no longer surfaced.</summary>
    Outdated = 3
}
