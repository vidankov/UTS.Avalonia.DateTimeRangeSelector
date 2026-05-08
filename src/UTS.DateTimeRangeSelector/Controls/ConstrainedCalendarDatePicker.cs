using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace UTS.DateTimeRangeSelector.Controls;

/// <summary>
/// A <see cref="CalendarDatePicker"/> that constrains selected dates to
/// <see cref="MinDate"/>..<see cref="MaxDate"/>, enforces boundaries on the calendar,
/// and automatically corrects invalid manual input on focus loss.
/// </summary>
public class ConstrainedCalendarDatePicker : CalendarDatePicker
{
    /// <summary>
    /// Defines the <see cref="MinDate"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty =
        AvaloniaProperty.Register<ConstrainedCalendarDatePicker, DateTime?>(
            nameof(MinDate));

    /// <summary>
    /// Gets or sets the minimum allowed date.
    /// </summary>
    public DateTime? MinDate
    {
        get => GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MaxDate"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        AvaloniaProperty.Register<ConstrainedCalendarDatePicker, DateTime?>(
            nameof(MaxDate));

    /// <summary>
    /// Gets or sets the maximum allowed date.
    /// </summary>
    public DateTime? MaxDate
    {
        get => GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    private TextBox? _textBox;
    private Calendar? _calendar;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _calendar = e.NameScope.Find<Calendar>("PART_Calendar");

        EnforceBoundaries();
        ForceTextUpdate();
    }

    /// <inheritdoc />
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        // Before base class finalises the selection, correct the text if it's out of bounds.
        CorrectInputIfNeeded();
        base.OnLostFocus(e);
    }

    private bool _isUpdating;

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        try
        {
            if (change.Property == SelectedDateProperty)
            {
                var clamped = ClampDate(SelectedDate);
                if (clamped != SelectedDate)
                {
                    SetCurrentValue(SelectedDateProperty, clamped);
                }
                else
                {
                    ForceTextUpdate();
                    EnforceBoundaries();
                }
            }
            else if (change.Property == MinDateProperty || change.Property == MaxDateProperty)
            {
                // Re-clamp current SelectedDate only if the bounds are not contradictory.
                if (MinDate == null || MaxDate == null || MinDate <= MaxDate)
                {
                    if (SelectedDate != null)
                    {
                        var clamped = ClampDate(SelectedDate);
                        if (clamped != SelectedDate)
                            SetCurrentValue(SelectedDateProperty, clamped);
                    }
                }
                // Always restore calendar boundaries (even if Min>Max, calendar will just ignore invalid range).
                EnforceBoundaries();
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    /// <summary>
    /// Parses the current text, clamps the resulting date to <see cref="MinDate"/>..<see cref="MaxDate"/>,
    /// and updates <see cref="SelectedDate"/> if it has changed.
    /// </summary>
    private void CorrectInputIfNeeded()
    {
        if (_textBox is null)
        {
            return;
        }

        var text = _textBox.Text;
        if (TryParseDate(text!, out DateTime? parsed))
        {
            var clamped = ClampDate(parsed);
            if (clamped != SelectedDate)
            {
                SetCurrentValue(SelectedDateProperty, clamped);
                // ForceTextUpdate will be called by OnPropertyChanged after set.
            }
            else
            {
                // Date is already correct, just ensure text consistency
                ForceTextUpdate();
            }
        }
        else
        {
            // Not a valid date at all – revert to last valid date.
            if (SelectedDate.HasValue)
            {
                ForceTextUpdate();
            }
            else
                _textBox.Text = string.Empty;
        }
    }

    /// <summary>
    /// Forces the internal TextBox to display <see cref="SelectedDate"/> using the correct format.
    /// </summary>
    private void ForceTextUpdate()
    {
        if (_textBox is null)
        {
            return;
        }

        if (SelectedDate.HasValue)
        {
            _textBox.Text = SelectedDate.Value.ToString(GetCurrentFormat());
        }
        else
            _textBox.Text = string.Empty;
    }

    /// <summary>
    /// Restores <see cref="Calendar.DisplayDateStart"/> and <see cref="Calendar.DisplayDateEnd"/>
    /// to <see cref="MinDate"/> and <see cref="MaxDate"/>.
    /// </summary>
    private void EnforceBoundaries()
    {
        if (_calendar is null) return;

        var min = MinDate?.Date;
        var max = MaxDate?.Date;

        _calendar.IsEnabled = !(min.HasValue && max.HasValue && min.Value == max.Value);
        _calendar.DisplayDateStart = min;
        _calendar.DisplayDateEnd = max;
    }

    private DateTime? ClampDate(DateTime? date)
    {
        if (date is null)
        {
            return null;
        }

        var min = MinDate?.Date;
        var max = MaxDate?.Date;

        if (min.HasValue && date.Value < min.Value)
        {
            return min.Value;
        }
        if (max.HasValue && date.Value > max.Value)
        {
            return max.Value;
        }
        return date;
    }

    /// <summary>
    /// TryParses text using configured format and falls back to generic DateTime.TryParse.
    /// </summary>
    private bool TryParseDate(string text, out DateTime? date)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string format = GetCurrentFormat();
        if (DateTime.TryParseExact(text, format, null, System.Globalization.DateTimeStyles.None, out DateTime exact))
        {
            date = exact;
            return true;
        }
        if (DateTime.TryParse(text, out DateTime generic))
        {
            date = generic;
            return true;
        }
        return false;
    }

    private string GetCurrentFormat()
    {
        if (SelectedDateFormat == CalendarDatePickerFormat.Custom)
        {
            return CustomDateFormatString ?? "dd.MM.yyyy";
        }

        return SelectedDateFormat switch
        {
            CalendarDatePickerFormat.Short => "d",
            CalendarDatePickerFormat.Long => "D",
            _ => "d"
        };
    }
}