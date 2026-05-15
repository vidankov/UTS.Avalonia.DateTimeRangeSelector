using Avalonia.Interactivity;

namespace UTS.DateTimeRangeSelector.Events;

/// <summary>
/// Provides data for the <see cref="Controls.DateTimePickerPanel.SelectedDateTimeChanged"/> routed event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SelectedDateTimeChangedEventArgs"/> class.
/// </remarks>
/// <param name="routedEvent">The routed event identifier.</param>
/// <param name="oldValue">The old date/time value.</param>
/// <param name="newValue">The new date/time value.</param>
public class SelectedDateTimeChangedEventArgs(
    RoutedEvent routedEvent,
    DateTime? oldValue,
    DateTime? newValue) : RoutedEventArgs(routedEvent)
{
    /// <summary>
    /// Gets the previous <see cref="DateTimePickerPanel.SelectedDateTime"/> value.
    /// </summary>
    public DateTime? OldValue { get; } = oldValue;

    /// <summary>
    /// Gets the new <see cref="DateTimePickerPanel.SelectedDateTime"/> value.
    /// </summary>
    public DateTime? NewValue { get; } = newValue;
}