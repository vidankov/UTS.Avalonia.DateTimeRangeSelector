namespace UTS.DateTimeRangeSelector.Tests.Core;

/// <summary>
/// Proof-tests for mixed <see cref="DateTimeKind"/> handling in <see cref="DateTimeRangeCoercion.Clamp"/>.
/// Expected to fail until kinds are normalized before comparison or bounds are documented as UTC-only.
/// </summary>
public class DateTimeRangeCoercionKindTests
{
    [Fact]
    public void Clamp_WhenValueIsUtcAndMinIsLocal_ShouldNotThrow()
    {
        var value = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var localMin = new DateTime(2025, 6, 15, 8, 0, 0, DateTimeKind.Local);

        var act = () => DateTimeRangeCoercion.Clamp(value, localMin, null);

        act.Should().NotThrow(
            "Clamp is used with selector bounds that are not coerced to UTC; " +
            "mixed kinds must not surface as ArgumentException to callers");
    }

    [Fact]
    public void Clamp_WhenValueIsUtcAndMinIsLocal_ShouldClampUsingComparableInstants()
    {
        var value = new DateTime(2025, 6, 15, 6, 0, 0, DateTimeKind.Utc);
        var localMin = new DateTime(2025, 6, 15, 8, 0, 0, DateTimeKind.Local);

        var result = DateTimeRangeCoercion.Clamp(value, localMin, null);

        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeOnOrAfter(localMin.ToUniversalTime());
    }

    [Fact]
    public void R26_2_Clamp_UtcValueAboveLocalMax_ShouldNotThrow()
    {
        var utcHigh = new DateTime(2025, 6, 15, 20, 0, 0, DateTimeKind.Utc);
        var localMax = new DateTime(2025, 6, 15, 18, 0, 0, DateTimeKind.Local);

        var act = () => DateTimeRangeCoercion.Clamp(utcHigh, null, localMax);

        act.Should().NotThrow(
            "Clamp must not throw when value is Utc and max bound is Local");
    }

    [Fact]
    public void R26_2_Clamp_WhenValueIsUtcAndMaxIsLocal_ShouldClampUsingComparableInstants()
    {
        var value = new DateTime(2025, 6, 15, 20, 0, 0, DateTimeKind.Utc);
        var localMax = new DateTime(2025, 6, 15, 18, 0, 0, DateTimeKind.Local);

        var result = DateTimeRangeCoercion.Clamp(value, null, localMax);

        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeOnOrBefore(localMax.ToUniversalTime());
    }
}
