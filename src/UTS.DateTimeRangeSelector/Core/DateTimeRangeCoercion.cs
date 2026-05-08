namespace UTS.DateTimeRangeSelector.Core;

/// <summary>
/// Pure functions for coercing <see cref="DateTime"/> values within min/max boundaries.
/// </summary>
public static class DateTimeRangeCoercion
{
    /// <summary>
    /// Clamps <paramref name="value"/> so that it lies within [<paramref name="min"/>, <paramref name="max"/>].
    /// Null bounds mean no limit on the corresponding side.
    /// </summary>
    public static DateTime Clamp(DateTime value, DateTime? min, DateTime? max)
    {
        if (min.HasValue && value < min.Value)
        {
            value = min.Value;
        }
        if (max.HasValue && value > max.Value)
        {
            value = max.Value;
        }
        return value;
    }
}