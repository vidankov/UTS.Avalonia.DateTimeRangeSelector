using Avalonia.Interactivity;

namespace UTS.DateTimeRangeSelector.Events;

/// <summary>
/// Provides data for the <see cref="Controls.DateTimeRangeSelector.ValidationChanged"/> routed event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationChangedEventArgs"/> class.
/// </remarks>
/// <param name="routedEvent">The routed event identifier.</param>
/// <param name="oldIsValid">Previous validity state.</param>
/// <param name="newIsValid">Current validity state.</param>
/// <param name="oldMessage">Previous validation message.</param>
/// <param name="newMessage">Current validation message.</param>
public class ValidationChangedEventArgs(
    RoutedEvent<ValidationChangedEventArgs> routedEvent,
    bool oldIsValid,
    bool newIsValid,
    string? oldMessage,
    string? newMessage) : RoutedEventArgs(routedEvent)
{
    /// <summary>
    /// Gets whether the range was valid before the change.
    /// </summary>
    public bool OldIsValid { get; } = oldIsValid;

    /// <summary>
    /// Gets whether the range is valid after the change.
    /// </summary>
    public bool NewIsValid { get; } = newIsValid;

    /// <summary>
    /// Gets the validation message before the change. Null if previously valid.
    /// </summary>
    public string? OldMessage { get; } = oldMessage;

    /// <summary>
    /// Gets the validation message after the change. Null if now valid.
    /// </summary>
    public string? NewMessage { get; } = newMessage;
}