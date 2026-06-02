namespace UTS.DateTimeRangeSelector.Exceptions;

/// <summary>
/// Thrown when a preset duration cannot be satisfied without truncation
/// and <see cref="Controls.DateTimeRangeSelector.AllowPresetTruncation"/> is <see langword="false"/>.
/// </summary>
public class PresetOutOfBoundsException(
    TimeSpan requestedDuration,
    TimeSpan? resultingDuration,
    DateTime? min,
    DateTime? max) : InvalidOperationException(FormatMessage(requestedDuration, resultingDuration, min, max))
{
    /// <summary>
    /// The duration that was requested (e.g., 1 hour).
    /// </summary>
    public TimeSpan RequestedDuration { get; } = requestedDuration;

    /// <summary>
    /// The duration that would actually result after clamping,
    /// or <see langword="null"/> if the range cannot be created at all.
    /// </summary>
    public TimeSpan? ResultingDuration { get; } = resultingDuration;

    /// <summary>
    /// The minimum allowed date/time at the time of the operation.
    /// </summary>
    public DateTime? MinDateTime { get; } = min;

    /// <summary>
    /// The maximum allowed date/time at the time of the operation.
    /// </summary>
    public DateTime? MaxDateTime { get; } = max;

    private static string FormatMessage(
        TimeSpan requested,
        TimeSpan? resulting,
        DateTime? min,
        DateTime? max)
    {
        string truncInfo = resulting.HasValue
            ? $"would result in {resulting.Value}"
            : "cannot be created within current bounds";
        return $"Requested preset duration {requested} cannot be satisfied. " +
               $"Min={min?.ToString("O") ?? "unlimited"}, " +
               $"Max={max?.ToString("O") ?? "unlimited"}; {truncInfo}.";
    }
}