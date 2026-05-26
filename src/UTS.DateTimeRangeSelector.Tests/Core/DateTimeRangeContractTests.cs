namespace UTS.DateTimeRangeSelector.Tests.Core;

/// <summary>
/// Proof-tests for public <see cref="DateTimeRange"/> contract gaps (R26-13, R26-14).
/// </summary>
public class DateTimeRangeContractTests
{
    [Fact]
    public void R26_13_SameTicksDifferentKind_ShouldBeConsideredEqual()
    {
        var ticks = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var utcFrom = new DateTime(ticks.Ticks, DateTimeKind.Utc);
        var unspecifiedFrom = new DateTime(ticks.Ticks, DateTimeKind.Unspecified);
        var utcTo = ticks.AddHours(1);

        var a = new DateTimeRange(utcFrom, utcTo);
        var b = new DateTimeRange(unspecifiedFrom, utcTo);

        a.Should().Be(b,
            "semantically identical instants must compare equal for observables and DistinctUntilChanged");
    }

    [Fact]
    public void R26_14_Duration_WhenFromEqualsTo_ShouldBeNull()
    {
        var dt = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var range = new DateTimeRange(dt, dt);

        range.Duration.Should().BeNull(
            "point-in-time ranges should not expose Duration as TimeSpan.Zero; null distinguishes from open/invalid ranges");
    }
}
