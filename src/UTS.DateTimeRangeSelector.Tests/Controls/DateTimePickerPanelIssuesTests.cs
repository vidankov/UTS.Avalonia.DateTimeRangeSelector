using UTS.DateTimeRangeSelector.Controls;

namespace UTS.DateTimeRangeSelector.Tests.Controls;

/// <summary>
/// Proof-tests for panel-level state inconsistencies not covered by <see cref="DateTimePickerPanelTests"/>.
/// Expected to fail on current production code until component state matches <see cref="DateTimePickerPanel.SelectedDateTime"/>.
/// </summary>
public class DateTimePickerPanelIssuesTests
{
    [Fact]
    public void SetSelectedDate_WhenMinDateAfterMaxDate_ShouldNotProduceDateOutsideBothBounds()
    {
        var panel = new DateTimePickerPanel
        {
            MinDateTime = new DateTime(2025, 6, 20, 0, 0, 0, DateTimeKind.Utc),
            MaxDateTime = new DateTime(2025, 6, 10, 23, 59, 59, DateTimeKind.Utc)
        };

        panel.SelectedDate = new DateTime(2025, 6, 15);

        panel.SelectedDate.Should().NotBeNull();
        panel.SelectedDate!.Value.Should().BeOnOrBefore(panel.MaxDateTime!.Value.Date);
        panel.SelectedDate!.Value.Should().BeOnOrAfter(panel.MinDateTime!.Value.Date);
    }

    [Fact]
    public void R26_10_SetHourTo24_ShouldNotShiftSelectedDateToNextDay()
    {
        var originalDate = new DateTime(2025, 6, 15);
        var panel = new DateTimePickerPanel
        {
            SelectedDate = originalDate,
            Hour = 12,
            Minute = 0,
            Second = 0,
            Millisecond = 0
        };

        panel.Hour = 24;

        panel.SelectedDate.Should().Be(originalDate,
            "Hour=24 must not silently advance SelectedDate to the next calendar day");
    }

    [Fact]
    public void R26_10_SetHourTo24_HourShouldBeCoercedToAtMost23()
    {
        var panel = new DateTimePickerPanel
        {
            SelectedDate = new DateTime(2025, 6, 15),
            Hour = 12
        };

        panel.Hour = 24;

        panel.Hour.Should().BeLessThanOrEqualTo(23,
            "invalid hour 24 must be coerced or rejected, not left as 24 or as 0 after silent date shift");
    }

    [Fact]
    public void R26_11_OnMinBoundaryDay_WhenTimeBelowMinTime_HourShouldReflectClampedValue()
    {
        var min = new DateTime(2025, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var panel = new DateTimePickerPanel
        {
            MinDateTime = min,
            SelectedDate = new DateTime(2025, 6, 15),
            Hour = 8,
            Minute = 0,
            Second = 0,
            Millisecond = 0
        };

        panel.Hour = 8;

        panel.SelectedDateTime.Should().BeOnOrAfter(min);
        panel.Hour.Should().Be(14,
            "Hour component must match the clamped SelectedDateTime, not the pre-clamp orphan value");
    }
}
