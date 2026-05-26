using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using UTS.DateTimeRangeSelector.Controls;

namespace UTS.DateTimeRangeSelector.Tests.UI;

/// <summary>
/// R26-4 / issue #14: <see cref="ConstrainedCalendarDatePicker"/> clamp vs text display.
/// Property clamp is covered by <see cref="R26_4_WhenSelectedDateAboveMax_SelectedDateShouldEqualMax"/>.
/// Text sync after clamp requires a template with <c>PART_TextBox</c> (see issue #14).
/// </summary>
public class ConstrainedCalendarDatePickerHeadlessTests
{
    private const string DateFormat = "yyyy-MM-dd";

    private static (ConstrainedCalendarDatePicker Picker, TextBox TextBox) CreatePickerWithPartTextBox(
        DateTime maxDate)
    {
        TextBox? partTextBox = null;

        var picker = new ConstrainedCalendarDatePicker
        {
            MaxDate = maxDate,
            SelectedDateFormat = CalendarDatePickerFormat.Custom,
            CustomDateFormatString = DateFormat,
            Template = new FuncControlTemplate<ConstrainedCalendarDatePicker>((_, _) =>
            {
                partTextBox = new TextBox { Name = "PART_TextBox" };
                return partTextBox;
            })
        };

        picker.ApplyTemplate();

        partTextBox.Should().NotBeNull("template must expose PART_TextBox for OnApplyTemplate name lookup");
        return (picker, partTextBox!);
    }

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
            "SelectedDate must be clamped to MaxDate");
    }

    [AvaloniaFact]
    public void R26_4_WhenSelectedDateClamped_TextBoxShouldShowClampedDate()
    {
        var max = new DateTime(2025, 6, 15);
        var unclamped = max.AddDays(1);
        var (picker, textBox) = CreatePickerWithPartTextBox(max);

        textBox.Text = unclamped.ToString(DateFormat);
        picker.SelectedDate = unclamped;

        picker.SelectedDate.Should().Be(max);
        textBox.Text.Should().Be(
            max.ToString(DateFormat),
            "after clamp, PART_TextBox.Text must match SelectedDate (issue #14: ForceTextUpdate skipped on reentrant clamp)");
    }

    [AvaloniaFact]
    public void R26_4_WhenSelectedDateClampedWithoutPriorText_TextBoxShouldShowClampedDate()
    {
        var max = new DateTime(2025, 6, 15);
        var (picker, textBox) = CreatePickerWithPartTextBox(max);

        picker.SelectedDate = max.AddDays(1);

        picker.SelectedDate.Should().Be(max);
        textBox.Text.Should().Be(
            max.ToString(DateFormat),
            "empty text must be refreshed to clamped SelectedDate, not left stale or blank");
    }
}
