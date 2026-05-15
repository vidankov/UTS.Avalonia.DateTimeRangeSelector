namespace UTS.DateTimeRangeSelector.Tests.Core;

public class DateTimeRangeCoercionTests
{
    private static readonly DateTime Min = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Max = new(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    [Fact]
    public void Clamp_ValueWithinBounds_ReturnsSameValue()
    {
        var value = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        DateTimeRangeCoercion.Clamp(value, Min, Max).Should().Be(value);
    }

    [Fact]
    public void Clamp_ValueBelowMin_ReturnsMin()
    {
        var value = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTimeRangeCoercion.Clamp(value, Min, Max).Should().Be(Min);
    }

    [Fact]
    public void Clamp_ValueAboveMax_ReturnsMax()
    {
        var value = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTimeRangeCoercion.Clamp(value, Min, Max).Should().Be(Max);
    }

    [Fact]
    public void Clamp_NullBounds_ReturnsSameValue()
    {
        var value = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        DateTimeRangeCoercion.Clamp(value, null, null).Should().Be(value);
    }
}
