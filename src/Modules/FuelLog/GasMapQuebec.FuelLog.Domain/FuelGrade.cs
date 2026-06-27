namespace GasMapQuebec.FuelLog.Domain;

/// <summary>
/// Fuel grade for a logged fill-up. Defined within the FuelLog module so it stays
/// independent of the Pricing module.
/// </summary>
public enum FuelGrade
{
    Regular = 0,
    Super = 1,
    Diesel = 2
}
