using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Application;

/// <summary>Locale-free wire tokens for <see cref="FuelType"/> (regular | super | diesel).</summary>
public static class FuelTypeTokens
{
    public static string ToToken(FuelType fuelType) => fuelType switch
    {
        FuelType.Super => "super",
        FuelType.Diesel => "diesel",
        _ => "regular"
    };

    public static bool TryParse(string? token, out FuelType fuelType)
    {
        switch (token?.Trim().ToLowerInvariant())
        {
            case "regular": fuelType = FuelType.Regular; return true;
            case "super": fuelType = FuelType.Super; return true;
            case "diesel": fuelType = FuelType.Diesel; return true;
            default: fuelType = default; return false;
        }
    }
}
