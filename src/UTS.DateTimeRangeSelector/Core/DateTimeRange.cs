namespace UTS.DateTimeRangeSelector.Core;

/// <summary>
/// Represents an immutable date/time range with optional From and To boundaries.
/// </summary>
/// <param name="From">The start of the range, or null if not set.</param>
/// <param name="To">The end of the range, or null if not set.</param>
public record DateTimeRange(DateTime? From, DateTime? To)
{
    private static readonly DateTimeRange empty = new(null, null);

    /// <summary>
    /// Gets the duration of the range if both <see cref="From"/> and <see cref="To"/> are set
    /// and <see cref="From"/> is not later than <see cref="To"/>; otherwise, null.
    /// </summary>
    public TimeSpan? Duration => From.HasValue && To.HasValue && To.Value >= From.Value
        ? To.Value - From.Value : null;

    /// <summary>
    /// An empty range with no boundaries set.
    /// </summary>
    public static DateTimeRange Empty => empty;
}