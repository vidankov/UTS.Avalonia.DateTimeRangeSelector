using Avalonia.Interactivity;

namespace UTS.DateTimeRangeSelector.Events;

/// <summary>
/// Provides data for the <see cref="Controls.DateTimeRangeSelector.RangeChanged"/> routed event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DateTimeRangeChangedEventArgs"/> class.
/// </remarks>
/// <param name="routedEvent">The routed event identifier.</param>
/// <param name="oldFrom">The previous From value.</param>
/// <param name="newFrom">The new From value.</param>
/// <param name="oldTo">The previous To value.</param>
/// <param name="newTo">The new To value.</param>
public class DateTimeRangeChangedEventArgs(
    RoutedEvent<DateTimeRangeChangedEventArgs> routedEvent,
    DateTime? oldFrom,
    DateTime? newFrom,
    DateTime? oldTo,
    DateTime? newTo) : RoutedEventArgs(routedEvent)
{
    /// <summary>
    /// Gets the From value before the change. May be null.
    /// </summary>
    public DateTime? OldFrom { get; } = oldFrom;

    /// <summary>
    /// Gets the From value after the change. May be null.
    /// </summary>
    public DateTime? NewFrom { get; } = newFrom;

    /// <summary>
    /// Gets the To value before the change. May be null.
    /// </summary>
    public DateTime? OldTo { get; } = oldTo;

    /// <summary>
    /// Gets the To value after the change. May be null.
    /// </summary>
    public DateTime? NewTo { get; } = newTo;
}