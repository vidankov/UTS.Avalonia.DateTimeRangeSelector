namespace UTS.DateTimeRangeSelector.Core;

/// <summary>
/// Pure functions for coercing <see cref="DateTime"/> values within min/max boundaries.
/// </summary>
public static class DateTimeRangeCoercion
{
    /// <summary>
    /// Clamps <paramref name="value"/> so that it lies within [<paramref name="min"/>, <paramref name="max"/>].
    /// Null bounds mean no limit on the corresponding side.
    /// All inputs are normalized to UTC before comparison; the result is always a UTC <see cref="DateTime"/>.
    /// If both bounds are set and <paramref name="min"/> > <paramref name="max"/>, the value is returned unchanged.
    /// </summary>
    public static DateTime Clamp(DateTime value, DateTime? min, DateTime? max)
    {
        value = DateTimeNormalization.EnsureUtc(value);
        DateTime? utcMin = min.HasValue ? DateTimeNormalization.EnsureUtc(min.Value) : null;
        DateTime? utcMax = max.HasValue ? DateTimeNormalization.EnsureUtc(max.Value) : null;

        if (utcMin.HasValue && utcMax.HasValue && utcMin.Value > utcMax.Value)
        {
            return value;
        }
        if (utcMin.HasValue && value < utcMin.Value)
        {
            value = utcMin.Value;
        }
        if (utcMax.HasValue && value > utcMax.Value)
        {
            value = utcMax.Value;
        }
        return value;
    }
}