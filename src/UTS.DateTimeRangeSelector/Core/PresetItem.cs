namespace UTS.DateTimeRangeSelector.Core;

/// <summary>
/// Represents a preset time range that can be applied to a <see cref="DateTimeRangeSelector"/>.
/// </summary>
/// <param name="Label">Display text for the preset.</param>
/// <param name="Duration">The duration to apply (positive). The range will be from (Now - Duration) to Now.</param>
public record PresetItem(string Label, TimeSpan Duration)
{
    /// <summary>
    /// Gets a list of commonly used presets.
    /// </summary>
    public static IReadOnlyList<PresetItem> Defaults { get; } =
    [
        new("Last hour", TimeSpan.FromHours(1)),
        new("Last 6 hours", TimeSpan.FromHours(6)),
        new("Last 24 hours", TimeSpan.FromDays(1)),
        new("Last 7 days", TimeSpan.FromDays(7)),
        new("Last 30 days", TimeSpan.FromDays(30))
    ];
}