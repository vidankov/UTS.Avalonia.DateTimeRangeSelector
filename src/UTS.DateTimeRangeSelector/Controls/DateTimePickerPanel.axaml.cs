using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using UTS.DateTimeRangeSelector.Core;

namespace UTS.DateTimeRangeSelector.Controls;

/// <summary>
/// A TemplatedControl for selecting a single date and time value with optional minimum and maximum boundaries.
/// Composes a CalendarDatePicker and multiple NumericUpDown controls for time components.
/// </summary>
[TemplatePart("PART_Calendar", typeof(ConstrainedCalendarDatePicker))]
public class DateTimePickerPanel : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="SelectedDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        AvaloniaProperty.Register<DateTimePickerPanel, DateTime?>(
            nameof(SelectedDateTime),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceSelectedDateTime);

    /// <summary>
    /// Gets or sets the currently selected date and time (UTC).
    /// </summary>
    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MinDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateTimeProperty =
        AvaloniaProperty.Register<DateTimePickerPanel, DateTime?>(
            nameof(MinDateTime),
            coerce: CoerceMinMaxDateTime);

    /// <summary>
    /// Gets or sets the minimum allowed date and time. Null means no lower limit.
    /// </summary>
    public DateTime? MinDateTime
    {
        get => GetValue(MinDateTimeProperty);
        set => SetValue(MinDateTimeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MaxDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MaxDateTimeProperty =
        AvaloniaProperty.Register<DateTimePickerPanel, DateTime?>(
            nameof(MaxDateTime),
            coerce: CoerceMinMaxDateTime);

    /// <summary>
    /// Gets or sets the maximum allowed date and time. Null means no upper limit.
    /// </summary>
    public DateTime? MaxDateTime
    {
        get => GetValue(MaxDateTimeProperty);
        set => SetValue(MaxDateTimeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DateTimeFormat"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTimeFormatModel> DateTimeFormatProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, DateTimeFormatModel>(
            nameof(DateTimeFormat),
            defaultValue: DateTimeFormatModel.Default);

    /// <summary>
    /// Gets or sets the format model used to display date and time values.
    /// The <see cref="Core.DateTimeFormatModel.DateFormat"/> is used for the calendar date picker,
    /// and the combined <see cref="Core.DateTimeFormatModel.DateTimeFormat"/> is intended for text input.
    /// Default is "dd.MM.yyyy HH:mm:ss.fff".
    /// </summary>
    public DateTimeFormatModel DateTimeFormat
    {
        get => GetValue(DateTimeFormatProperty);
        set => SetValue(DateTimeFormatProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="SelectedDate"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimePickerPanel, DateTime?> SelectedDateProperty =
        AvaloniaProperty.RegisterDirect<DateTimePickerPanel, DateTime?>(
            nameof(SelectedDate),
            o => o.SelectedDate,
            (o, v) => o.SelectedDate = v,
            defaultBindingMode: BindingMode.TwoWay);

    private DateTime? _selectedDate;
    /// <summary>
    /// Gets or sets the date component (without time). Changes update <see cref="SelectedDateTime"/> preserving time components.
    /// </summary>
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            // Clamp to allowed date range (date part only)
            DateTime? clampedValue = value;
            if (clampedValue.HasValue)
            {
                var minDate = MinDateTime?.Date;
                var maxDate = MaxDateTime?.Date;

                if (minDate.HasValue && clampedValue.Value < minDate.Value)
                {
                    clampedValue = minDate.Value;
                }
                if (maxDate.HasValue && clampedValue.Value > maxDate.Value)
                {
                    clampedValue = maxDate.Value;
                }
            }

            if (clampedValue == _selectedDate)
            {
                return;
            }

            SetAndRaise(SelectedDateProperty, ref _selectedDate, clampedValue);

            if (!_updatingComponents)
            {
                if (clampedValue == null)
                {
                    SetCurrentValue(SelectedDateTimeProperty, null);
                }
                else
                {
                    UpdateSelectedDateTime();
                }
            }

            // Force calendar bounds reset in case the control altered them
            ApplyMinMaxToCalendar();
        }
    }


    /// <summary>
    /// Defines the <see cref="Hour"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimePickerPanel, int> HourProperty =
        AvaloniaProperty.RegisterDirect<DateTimePickerPanel, int>(
            nameof(Hour),
            o => o.Hour,
            (o, v) => o.Hour = v,
            defaultBindingMode: BindingMode.TwoWay);

    private int _hour;
    /// <summary>Gets or sets the hours component (0-23).</summary>
    public int Hour
    {
        get => _hour;
        set
        {
            if (value == _hour)
            {
                return;
            }
            SetAndRaise(HourProperty, ref _hour, value);
            if (!_updatingComponents)
            {
                UpdateSelectedDateTime();
            }
        }
    }

    /// <summary>
    /// Defines the <see cref="Minute"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimePickerPanel, int> MinuteProperty =
        AvaloniaProperty.RegisterDirect<DateTimePickerPanel, int>(
            nameof(Minute),
            o => o.Minute,
            (o, v) => o.Minute = v,
            defaultBindingMode: BindingMode.TwoWay);

    private int _minute;
    /// <summary>Gets or sets the minutes component (0-59).</summary>
    public int Minute
    {
        get => _minute;
        set
        {
            if (value == _minute)
            {
                return;
            }
            SetAndRaise(MinuteProperty, ref _minute, value);
            if (!_updatingComponents)
            {
                UpdateSelectedDateTime();
            }
        }
    }

    /// <summary>
    /// Defines the <see cref="Second"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimePickerPanel, int> SecondProperty =
        AvaloniaProperty.RegisterDirect<DateTimePickerPanel, int>(
            nameof(Second),
            o => o.Second,
            (o, v) => o.Second = v,
            defaultBindingMode: BindingMode.TwoWay);

    private int _second;
    /// <summary>Gets or sets the seconds component (0-59).</summary>
    public int Second
    {
        get => _second;
        set
        {
            if (value == _second)
            {
                return;
            }
            SetAndRaise(SecondProperty, ref _second, value);
            if (!_updatingComponents)
            {
                UpdateSelectedDateTime();
            }
        }
    }

    /// <summary>
    /// Defines the <see cref="Millisecond"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimePickerPanel, int> MillisecondProperty =
        AvaloniaProperty.RegisterDirect<DateTimePickerPanel, int>(
            nameof(Millisecond),
            o => o.Millisecond,
            (o, v) => o.Millisecond = v,
            defaultBindingMode: BindingMode.TwoWay);

    private int _millisecond;
    /// <summary>Gets or sets the milliseconds component (0-999).</summary>
    public int Millisecond
    {
        get => _millisecond;
        set
        {
            if (value == _millisecond)
            {
                return;
            }
            SetAndRaise(MillisecondProperty, ref _millisecond, value);
            if (!_updatingComponents)
            {
                UpdateSelectedDateTime();
            }
        }
    }

    private bool _updatingComponents;
    private ConstrainedCalendarDatePicker? _calendar;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDateTimeProperty)
        {
            OnSelectedDateTimeChanged(change.NewValue as DateTime?);
        }
        else if (change.Property == MinDateTimeProperty || change.Property == MaxDateTimeProperty)
        {
            CoerceValue(SelectedDateTimeProperty);
            ApplyMinMaxToCalendar();
        }
    }

    /// <summary>
    /// Handles changes to <see cref="SelectedDateTime"/> and updates all time components and the date part.
    /// Ensures that the individual components stay in sync with the source property.
    /// </summary>
    /// <param name="newValue">The new value of <see cref="SelectedDateTime"/>, or null if it was cleared.</param>
    private void OnSelectedDateTimeChanged(DateTime? newValue)
    {
        if (_updatingComponents)
        {
            return;
        }
        _updatingComponents = true;

        if (newValue.HasValue)
        {
            var dt = newValue.Value;
            Hour = dt.Hour;
            Minute = dt.Minute;
            Second = dt.Second;
            Millisecond = dt.Millisecond;
            SelectedDate = dt.Date;
        }
        else
        {
            Hour = 0;
            Minute = 0;
            Second = 0;
            Millisecond = 0;
            SelectedDate = null;
        }

        _calendar?.SetCurrentValue(CalendarDatePicker.SelectedDateProperty, SelectedDate);

        _updatingComponents = false;
    }

    /// <summary>
    /// Recalculates <see cref="SelectedDateTime"/> from the current values of
    /// <see cref="SelectedDate"/> and the time components.
    /// Does nothing if <see cref="SelectedDate"/> is not set.
    /// </summary>
    private void UpdateSelectedDateTime()
    {
        if (!SelectedDate.HasValue)
        {
            return;
        }

        try
        {
            var newDateTime = SelectedDate.Value.Date + new TimeSpan(0, Hour, Minute, Second, Millisecond);
            SetCurrentValue(SelectedDateTimeProperty, newDateTime);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Invalid time combination (e.g. 24:00:00.000) – silently ignore.
        }
    }

    /// <summary>
    /// Coerces a <see cref="DateTime"/> value assigned to <see cref="SelectedDateTime"/>.
    /// Ensures the value is in UTC via <see cref="DateTimeNormalization.EnsureUtc"/>,
    /// then clamps it to <see cref="MinDateTime"/> and <see cref="MaxDateTime"/>.
    /// </summary>
    /// <param name="sender">The <see cref="DateTimePickerPanel"/> instance.</param>
    /// <param name="value">The incoming value to coerce, or null.</param>
    /// <returns>The normalized and clamped UTC DateTime, or null.</returns>
    private static DateTime? CoerceSelectedDateTime(AvaloniaObject sender, DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        var dt = DateTimeNormalization.EnsureUtc(value.Value);
        var panel = (DateTimePickerPanel)sender;
        return DateTimeRangeCoercion.Clamp(dt, panel.MinDateTime, panel.MaxDateTime);
    }

    /// <summary>Ensures Min/Max are stored as UTC.</summary>
    private static DateTime? CoerceMinMaxDateTime(AvaloniaObject sender, DateTime? value)
    {
        if (value is null)
        {
            return null;
        }
        return DateTimeNormalization.EnsureUtc(value.Value);
    }

    /// <summary>
    /// Initializes the control with default zero values for time components.
    /// </summary>
    public DateTimePickerPanel()
    {
        _hour = _minute = _second = _millisecond = 0;
    }

    /// <summary>
    /// Called when the control template is applied. Forces synchronization of the
    /// individual time/date components with the current value of <see cref="SelectedDateTime"/>,
    /// if it was set before the template was loaded.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _calendar = e.NameScope.Find<ConstrainedCalendarDatePicker>("PART_Calendar");

        ApplyMinMaxToCalendar();

        if (SelectedDateTime.HasValue)
        {
            OnSelectedDateTimeChanged(SelectedDateTime.Value);
        }
    }

    /// <summary>
    /// Applies <see cref="MinDateTime"/> and <see cref="MaxDateTime"/> to the calendar's
    /// <c>DisplayDateStart</c> and <c>DisplayDateEnd</c>.
    /// </summary>
    private void ApplyMinMaxToCalendar()
    {
        if (_calendar is null)
        {
            return;
        }

        var min = MinDateTime?.Date;
        var max = MaxDateTime?.Date;
        _calendar.IsEnabled = !(min.HasValue && max.HasValue && min.Value == max.Value);
    }
}