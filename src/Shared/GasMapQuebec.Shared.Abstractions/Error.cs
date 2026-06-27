namespace GasMapQuebec.Shared.Abstractions;

/// <summary>A coded error describing why a <see cref="Result"/> failed.</summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string description) => new("NotFound", description);
    public static Error Validation(string description) => new("Validation", description);
    public static Error Conflict(string description) => new("Conflict", description);
}
