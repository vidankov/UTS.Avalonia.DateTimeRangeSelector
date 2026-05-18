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
}
