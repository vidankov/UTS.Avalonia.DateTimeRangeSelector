using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using UTS.DateTimeRangeSelector.Controls;

namespace UTS.DateTimeRangeSelector.Tests.UI;

/// <summary>
/// R26-4: <see cref="ConstrainedCalendarDatePicker"/> clamp vs text display.
/// Text is not observable headlessly without the real template (private PART_TextBox); SelectedDate clamp is indirect proof.
/// </summary>
public class ConstrainedCalendarDatePickerHeadlessTests
{
    [AvaloniaFact]
    public void R26_4_WhenSelectedDateAboveMax_SelectedDateShouldEqualMax()
    {
        var max = new DateTime(2025, 6, 15);
        var picker = new ConstrainedCalendarDatePicker
        {
            Template = new FuncControlTemplate<ConstrainedCalendarDatePicker>((_, _) => new Panel()),
            MaxDate = max
        };

        picker.ApplyTemplate();
        picker.SelectedDate = max.AddDays(1);

        picker.SelectedDate.Should().Be(max,
            "SelectedDate must be clamped to MaxDate; text display gap is covered by R26-17 source scan (PART_TextBox not bound)");
    }
}
