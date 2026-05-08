namespace UTS.DateTimeRangeSelector.Core;

/// <summary>
/// Provides pure UTC normalization logic for <see cref="DateTime"/> values.
/// </summary>
public static class DateTimeNormalization
{
    /// <summary>
    /// Ensures that the specified <see cref="DateTime"/> value has <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <param name="value">The source value, potentially with any Kind.</param>
    /// <returns>
    /// If <paramref name="value"/> is <see cref="DateTimeKind.Local"/>, converts it to universal time.
    /// If <see cref="DateTimeKind.Unspecified"/>, assumes the value is already in UTC and sets Kind without changing the value.
    /// Otherwise returns the value unchanged.
    /// </returns>
    public static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}