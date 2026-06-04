namespace UTS.DateTimeRangeSelector.Tests.Core;

public class DateTimeNormalizationTests
{
    [Fact]
    public void EnsureUtc_UtcKind_ReturnsSameValue()
    {
        var dt = new DateTime(2025, 5, 15, 12, 0, 0, DateTimeKind.Utc);
        DateTimeNormalization.EnsureUtc(dt).Should().Be(dt);
    }

    [Fact]
    public void EnsureUtc_LocalKind_ConvertsToUtc()
    {
        var local = new DateTime(2025, 5, 15, 12, 0, 0, DateTimeKind.Local);
        var utc = DateTimeNormalization.EnsureUtc(local);
        utc.Kind.Should().Be(DateTimeKind.Utc);
        utc.Should().Be(local.ToUniversalTime());
    }

    [Fact]
    public void EnsureUtc_UnspecifiedKind_AssumesUtc()
    {
        var unspecified = new DateTime(2025, 5, 15, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = DateTimeNormalization.EnsureUtc(unspecified);
        utc.Kind.Should().Be(DateTimeKind.Utc);
        utc.Ticks.Should().Be(unspecified.Ticks);
    }

    /// <summary>
    /// R26-15 contract guard: documents that Unspecified ticks are treated as UTC wall-clock (design decision).
    /// </summary>
    [Fact]
    public void R26_15_EnsureUtc_Unspecified_PreservesTicksAndSetsUtcKind()
    {
        var unspecified = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Unspecified);

        var result = DateTimeNormalization.EnsureUtc(unspecified);

        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Ticks.Should().Be(unspecified.Ticks);
    }
}
