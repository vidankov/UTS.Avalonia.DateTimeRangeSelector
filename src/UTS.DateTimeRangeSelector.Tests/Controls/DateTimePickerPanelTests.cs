using System.Reflection;
using UTS.DateTimeRangeSelector.Controls;

namespace UTS.DateTimeRangeSelector.Tests.Controls;

public class DateTimePickerPanelTests
{
    [Fact]
    public void DateTimeFormatProperty_ShouldBeOwnedByDateTimePickerPanel()
    {
        var ownerTypeProperty = DateTimePickerPanel.DateTimeFormatProperty
            .GetType()
            .GetProperty("OwnerType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        ownerTypeProperty.Should().NotBeNull("Avalonia styled properties carry owner metadata that must match the declaring control");

        var ownerType = ownerTypeProperty!.GetValue(DateTimePickerPanel.DateTimeFormatProperty);

        ownerType.Should().Be(typeof(DateTimePickerPanel),
            "DateTimePickerPanel.DateTimeFormatProperty is declared on DateTimePickerPanel and should not be registered as DateTimeRangeSelector-owned metadata");
    }

    [Fact]
    public void SetTimeComponentsBeyondMax_ShouldRevertToMaxBoundary()
    {
        var panel = new DateTimePickerPanel
        {
            MaxDateTime = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc),
            SelectedDate = new DateTime(2026, 5, 15),
            Hour = 12,
            Minute = 0,
            Second = 0,
            Millisecond = 0
        };

        panel.Hour = 13;

        Assert.Equal(12, panel.Hour);
        Assert.Equal(panel.MaxDateTime, panel.SelectedDateTime);
        Assert.Equal(0, panel.Minute);
        Assert.Equal(0, panel.Second);
        Assert.Equal(0, panel.Millisecond);
    }

    [Fact]
    public void SetTimeComponentsBelowMin_ShouldRevertToMinBoundary()
    {
        var panel = new DateTimePickerPanel
        {
            MinDateTime = new DateTime(2026, 5, 15, 8, 0, 0, DateTimeKind.Utc),
            SelectedDate = new DateTime(2026, 5, 15),
            Hour = 8,
            Minute = 0,
            Second = 0,
            Millisecond = 0
        };

        panel.Hour = 7;

        Assert.Equal(8, panel.Hour);
        Assert.Equal(panel.MinDateTime, panel.SelectedDateTime);
    }
}
