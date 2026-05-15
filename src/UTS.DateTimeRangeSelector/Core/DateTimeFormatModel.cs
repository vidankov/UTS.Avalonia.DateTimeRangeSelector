namespace UTS.DateTimeRangeSelector.Core;

/// <summary>
/// Represents separate format strings for the date and time components of a date/time display.
/// </summary>
/// <param name="DateFormat">Format for the date part (e.g., "dd.MM.yyyy").</param>
/// <param name="TimeFormat">Format for the time part (e.g., "HH:mm:ss.fff").</param>
public record DateTimeFormatModel(string DateFormat, string TimeFormat)
{
    /// <summary>
    /// Gets the combined date and time format string.
    /// </summary>
    public string DateTimeFormat => $"{DateFormat} {TimeFormat}";

    /// <summary>
    /// The default format: "dd.MM.yyyy HH:mm:ss.fff".
    /// </summary>
    public static DateTimeFormatModel Default { get; } = new("dd.MM.yyyy", "HH:mm:ss.fff");
}