namespace UTS.DateTimeRangeSelector.Tests.Core;

public class DateTimeRangeTests
{
    [Fact]
    public void Duration_WhenFromAfterTo_ShouldNotReturnNegativeDuration()
    {
        var from = new DateTime(2025, 6, 15, 13, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var range = new DateTimeRange(from, to);

        range.Duration.Should().BeNull("an inverted range is invalid and should not expose a negative duration as if it were usable");
    }
}
