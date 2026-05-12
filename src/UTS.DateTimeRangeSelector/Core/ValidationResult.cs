namespace UTS.DateTimeRangeSelector.Core;

/// <summary>
/// Represents the result of validating a date/time range.
/// </summary>
/// <param name="IsValid">Whether the range is valid.</param>
/// <param name="Message">An error message if not valid, otherwise null.</param>
public record ValidationResult(bool IsValid, string? Message)
{
    private static readonly ValidationResult success = new(true, null);

    /// <summary>
    /// A pre-built successful validation result.
    /// </summary>
    public static ValidationResult Success => success;
}