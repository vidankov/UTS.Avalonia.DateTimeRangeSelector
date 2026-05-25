using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using UTS.DateTimeRangeSelector.Core;
using UTS.DateTimeRangeSelector.Events;

namespace UTS.DateTimeRangeSelector.Controls;

/// <summary>
/// A composite control for selecting a date/time range (From – To).
/// Composes two <see cref="DateTimePickerPanel"/> instances.
/// </summary>
[TemplatePart("PART_FromPanel", typeof(DateTimePickerPanel))]
[TemplatePart("PART_ToPanel", typeof(DateTimePickerPanel))]
public class DateTimeRangeSelector : TemplatedControl
{
    private readonly BehaviorSubject<DateTimeRange> _rangeSubject;
    private readonly BehaviorSubject<ValidationResult> _validationSubject;

    /// <summary>
    /// Defines the <see cref="FromDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> FromDateTimeProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, DateTime?>(
            nameof(FromDateTime),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceDateTimeToUtc);

    /// <summary>
    /// Gets or sets the start of the date/time range (UTC).
    /// </summary>
    public DateTime? FromDateTime
    {
        get => GetValue(FromDateTimeProperty);
        set => SetValue(FromDateTimeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ToDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> ToDateTimeProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, DateTime?>(
            nameof(ToDateTime),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceDateTimeToUtc);

    /// <summary>
    /// Gets or sets the end of the date/time range (UTC).
    /// </summary>
    public DateTime? ToDateTime
    {
        get => GetValue(ToDateTimeProperty);
        set => SetValue(ToDateTimeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MinDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateTimeProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, DateTime?>(
            nameof(MinDateTime));

    /// <summary>
    /// Gets or sets the minimum allowed date and time for the range.
    /// Null means no lower limit.
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
        AvaloniaProperty.Register<DateTimeRangeSelector, DateTime?>(
            nameof(MaxDateTime));

    /// <summary>
    /// Gets or sets the maximum allowed date and time for the range.
    /// Null means no upper limit.
    /// </summary>
    public DateTime? MaxDateTime
    {
        get => GetValue(MaxDateTimeProperty);
        set => SetValue(MaxDateTimeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Layout.Orientation> OrientationProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, Avalonia.Layout.Orientation>(
            nameof(Orientation),
            defaultValue: Avalonia.Layout.Orientation.Vertical);

    /// <summary>
    /// Gets or sets the layout orientation of the From and To panels.
    /// </summary>
    public Avalonia.Layout.Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Presets"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<PresetItem>> PresetsProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, IReadOnlyList<PresetItem>>(
            nameof(Presets),
            defaultValue: PresetItem.Defaults);

    /// <summary>
    /// Gets or sets the list of preset time ranges available for quick selection.
    /// </summary>
    public IReadOnlyList<PresetItem> Presets
    {
        get => GetValue(PresetsProperty);
        set => SetValue(PresetsProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ShowPresets"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowPresetsProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, bool>(
            nameof(ShowPresets),
            defaultValue: true);

    /// <summary>
    /// Gets or sets whether the preset buttons panel is visible.
    /// </summary>
    public bool ShowPresets
    {
        get => GetValue(ShowPresetsProperty);
        set => SetValue(ShowPresetsProperty, value);
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
    /// Defines the read-only <see cref="IsValid"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimeRangeSelector, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<DateTimeRangeSelector, bool>(
            nameof(IsValid), o => o.IsValid);

    private bool _isValid;
    /// <summary>Gets whether the current range is valid.</summary>
    public bool IsValid
    {
        get => _isValid;
        private set => SetAndRaise(IsValidProperty, ref _isValid, value);
    }

    /// <summary>
    /// Defines the read-only <see cref="ValidationMessage"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimeRangeSelector, string?> ValidationMessageProperty =
        AvaloniaProperty.RegisterDirect<DateTimeRangeSelector, string?>(
            nameof(ValidationMessage), o => o.ValidationMessage);

    private string? _validationMessage;
    /// <summary>
    /// Gets the validation error message, or null if the range is valid.
    /// </summary>
    public string? ValidationMessage
    {
        get => _validationMessage;
        private set => SetAndRaise(ValidationMessageProperty, ref _validationMessage, value);
    }

    /// <summary>
    /// Defines the read-only <see cref="AreBoundsValid"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimeRangeSelector, bool> AreBoundsValidProperty =
        AvaloniaProperty.RegisterDirect<DateTimeRangeSelector, bool>(
            nameof(AreBoundsValid), o => o.AreBoundsValid);

    private bool _areBoundsValid = true;

    /// <summary>
    /// Gets a value indicating whether the current <see cref="MinDateTime"/> and <see cref="MaxDateTime"/>
    /// bounds are valid (i.e., not contradictory: Min &lt;= Max or not both set).
    /// This property is intended for internal template use and is read-only.
    /// </summary>
    public bool AreBoundsValid
    {
        get => _areBoundsValid;
        private set => SetAndRaise(AreBoundsValidProperty, ref _areBoundsValid, value);
    }

    /// <summary>
    /// Identifies the <see cref="RangeChanged"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<DateTimeRangeChangedEventArgs> RangeChangedEvent =
        RoutedEvent.Register<DateTimeRangeSelector, DateTimeRangeChangedEventArgs>(
            nameof(RangeChanged),
            RoutingStrategies.Direct);

    /// <summary>
    /// Identifies the <see cref="ValidationChanged"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<ValidationChangedEventArgs> ValidationChangedEvent =
        RoutedEvent.Register<DateTimeRangeSelector, ValidationChangedEventArgs>(
            nameof(ValidationChanged),
            RoutingStrategies.Direct);

    /// <inheritdoc cref="RangeChangedEvent"/>
    public event EventHandler<DateTimeRangeChangedEventArgs>? RangeChanged
    {
        add => AddHandler(RangeChangedEvent, value);
        remove => RemoveHandler(RangeChangedEvent, value);
    }

    /// <inheritdoc cref="ValidationChangedEvent"/>
    public event EventHandler<ValidationChangedEventArgs>? ValidationChanged
    {
        add => AddHandler(ValidationChangedEvent, value);
        remove => RemoveHandler(ValidationChangedEvent, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="System.TimeProvider"/> used to obtain the current UTC time
    /// when applying presets or initializing the default range.
    /// </summary>
    /// <remarks>
    /// Must be set before the control is fully loaded to take effect during initialization.
    /// </remarks>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>
    /// Defines the <see cref="ApplyPresetCommand"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimeRangeSelector, ICommand> ApplyPresetCommandProperty =
        AvaloniaProperty.RegisterDirect<DateTimeRangeSelector, ICommand>(
            nameof(ApplyPresetCommand),
            o => o.ApplyPresetCommand);

    private PresetCommand _applyPresetCommand;

    /// <summary>
    /// Gets an observable sequence of <see cref="DateTimeRange"/> values representing
    /// the current range. The observable is hot and replays the latest value to new subscribers.
    /// Changes are distinct (no consecutive duplicates).
    /// </summary>
    public IObservable<DateTimeRange> RangeChanges =>
        _rangeSubject.AsObservable().DistinctUntilChanged();

    /// <summary>
    /// Gets an observable sequence of <see cref="ValidationResult"/> values representing
    /// the current validation state. The observable is hot and replays the latest value.
    /// Changes are distinct (no consecutive duplicates).
    /// </summary>
    public IObservable<ValidationResult> ValidationChanges =>
        _validationSubject.AsObservable().DistinctUntilChanged();

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeRangeSelector"/> class.
    /// </summary>
    public DateTimeRangeSelector()
    {
        _applyPresetCommand = new PresetCommand(ApplyPreset, canExecute: () => ShowPresets);

        _rangeSubject = new(new(FromDateTime, ToDateTime));
        _validationSubject = new(new(IsValid, ValidationMessage));
    }

    /// <summary>
    /// Programmatically sets both <see cref="FromDateTime"/> and <see cref="ToDateTime"/> in one atomic operation.
    /// Values are normalized to UTC, clamped to <see cref="MinDateTime"/>/<see cref="MaxDateTime"/>,
    /// and the range order is enforced.
    /// </summary>
    /// <param name="from">The desired start of the range. Null clears the value.</param>
    /// <param name="to">The desired end of the range. Null clears the value.</param>
    public void SetRange(DateTime? from, DateTime? to)
    {
        ApplyRangeChange(
            action: () =>
            {
                SetCurrentValue(FromDateTimeProperty, from);
                SetCurrentValue(ToDateTimeProperty, to);
                Coerce(FromDateTimeProperty);
            },
            oldFrom: FromDateTime,
            oldTo: ToDateTime,
            oldIsValid: IsValid,
            oldValidationMessage: ValidationMessage
        );
    }

    /// <summary>
    /// Applies a preset duration to the range, using the same logic as the preset command.
    /// The range end is <see cref="MaxDateTime"/> if set, otherwise the current UTC time.
    /// The range start is end - <paramref name="duration"/>, clamped to <see cref="MinDateTime"/>.
    /// </summary>
    /// <param name="duration">A positive <see cref="TimeSpan"/> representing the desired range length.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="duration"/> is zero or negative.</exception>
    public void ApplyPreset(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        ApplyRangeChange(
            action: () =>
            {
                var (start, end) = CalculatePresetRange(duration);
                SetCurrentValue(FromDateTimeProperty, start);
                SetCurrentValue(ToDateTimeProperty, end);
                Coerce(FromDateTimeProperty);
            },
            oldFrom: FromDateTime,
            oldTo: ToDateTime,
            oldIsValid: IsValid,
            oldValidationMessage: ValidationMessage
        );
    }

    /// <summary>
    /// Resets <see cref="FromDateTime"/> and <see cref="ToDateTime"/> to their default values,
    /// as if the control was just initialized. <see cref="MinDateTime"/> and <see cref="MaxDateTime"/>
    /// are left unchanged.
    /// </summary>
    public void ResetToDefaults()
    {
        ApplyRangeChange(
            action: () =>
            {
                ClearValue(FromDateTimeProperty);
                ClearValue(ToDateTimeProperty);
                ApplyDefaultRange();
            },
            oldFrom: FromDateTime,
            oldTo: ToDateTime,
            oldIsValid: IsValid,
            oldValidationMessage: ValidationMessage
        );
    }

    /// <summary>
    /// Executes <paramref name="action"/> atomically, suppressing recursive event generation.
    /// Compares the range and validation state before and after the action, and raises
    /// <see cref="RangeChanged"/> / <see cref="ValidationChanged"/> along with updating the
    /// reactive subjects if changes occurred.
    /// </summary>
    private void ApplyRangeChange(
        Action action,
        DateTime? oldFrom,
        DateTime? oldTo,
        bool oldIsValid,
        string? oldValidationMessage)
    {
        if (_suppressEvents)
        {
            action();
            return;
        }

        bool oldSuppress = _suppressEvents;
        _suppressEvents = true;
        try
        {
            action();
        }
        finally
        {
            _suppressEvents = oldSuppress;
        }

        if (oldFrom != FromDateTime || oldTo != ToDateTime)
        {
            RaiseEvent(new DateTimeRangeChangedEventArgs(
                RangeChangedEvent, oldFrom, FromDateTime, oldTo, ToDateTime));
            _rangeSubject?.OnNext(new(FromDateTime, ToDateTime));
        }

        if (oldIsValid != IsValid || oldValidationMessage != ValidationMessage)
        {
            RaiseEvent(new ValidationChangedEventArgs(
                ValidationChangedEvent, oldIsValid, IsValid, oldValidationMessage, ValidationMessage));
            _validationSubject?.OnNext(new(IsValid, ValidationMessage));
        }
    }

    /// <summary>
    /// Returns the anchor (right edge) used for range calculations.
    /// If <see cref="MaxDateTime"/> is set, it is the anchor;
    /// otherwise, the current UTC time from <see cref="TimeProvider"/>.
    /// </summary>
    private DateTime GetAnchor() => MaxDateTime ?? TimeProvider.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Calculates a time range of the given <paramref name="duration"/> relative to the anchor
    /// (<see cref="MaxDateTime"/> if set, otherwise the current UTC time).
    /// If <see cref="MinDateTime"/> is later than the anchor, the range is shifted forward
    /// so that it starts at <see cref="MinDateTime"/> and ends at Min + duration.
    /// The returned values are not yet clamped to <see cref="MaxDateTime"/> — 
    /// that is handled later by <see cref="Coerce"/>.
    /// </summary>
    /// <returns>A tuple containing (start, end) in UTC.</returns>
    private (DateTime Start, DateTime End) CalculatePresetRange(TimeSpan duration)
    {
        var anchor = GetAnchor(); // MaxDateTime ?? UtcNow
        if (MinDateTime.HasValue && MinDateTime.Value > anchor)
        {
            return (MinDateTime.Value, MinDateTime.Value + duration);
        }
        return (anchor - duration, anchor);
    }

    /// <summary>
    /// Gets the command that applies a preset duration to the range.
    /// </summary>
    public ICommand ApplyPresetCommand => _applyPresetCommand;

    /// <summary>
    /// Prevents reentrancy when coercing From/To values.
    /// </summary>
    private bool _isCoercing;

    private bool _suppressEvents;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShowPresetsProperty)
        {
            _applyPresetCommand.RaiseCanExecuteChanged();
        }

        if (change.Property == FromDateTimeProperty ||
            change.Property == ToDateTimeProperty ||
            change.Property == MinDateTimeProperty ||
            change.Property == MaxDateTimeProperty)
        {
            DateTime? oldFrom, oldTo;

            if (change.Property == FromDateTimeProperty)
            {
                oldFrom = change.GetOldValue<DateTime?>();
                oldTo = ToDateTime;
            }
            else if (change.Property == ToDateTimeProperty)
            {
                oldFrom = FromDateTime;
                oldTo = change.GetOldValue<DateTime?>();
            }
            else // MinDateTime or MaxDateTime changes
            {
                oldFrom = FromDateTime;
                oldTo = ToDateTime;
            }

            bool oldIsValid = IsValid;
            string? oldValidationMessage = ValidationMessage;

            ApplyRangeChange(
                action: () => Coerce(change.Property),
                oldFrom: oldFrom,
                oldTo: oldTo,
                oldIsValid: oldIsValid,
                oldValidationMessage: oldValidationMessage
            );
        }
    }

    /// <summary>
    /// Normalizes <see cref="FromDateTime"/> and <see cref="ToDateTime"/> to UTC,
    /// clamps them to <see cref="MinDateTime"/> / <see cref="MaxDateTime"/>,
    /// and ensures <c>From &lt;= To</c> by adjusting the opposite boundary when the range becomes inverted.
    /// If <see cref="MinDateTime"/> > <see cref="MaxDateTime"/>, both range values are reset to null
    /// and the control is disabled until valid bounds are restored.
    /// Calls <see cref="UpdateValidation"/> after enforcement.
    /// </summary>
    /// <param name="property">The property that triggered the coercion, used to preserve intent
    /// when restoring the order.</param>
    private void Coerce(AvaloniaProperty property)
    {
        if (_isCoercing)
        {
            return;
        }

        _isCoercing = true;
        try
        {
            // 1. Если границы противоречивы – сбрасываем значения и прекращаем обработку.
            if (MinDateTime.HasValue && MaxDateTime.HasValue
                && MinDateTime.Value > MaxDateTime.Value)
            {
                if (FromDateTime.HasValue)
                {
                    SetCurrentValue(FromDateTimeProperty, null);
                }
                if (ToDateTime.HasValue)
                {
                    SetCurrentValue(ToDateTimeProperty, null);
                }

                UpdateValidation();
                return;
            }

            // 2. Нормализуем и клампим From/To к границам Min/Max
            DateTime? from = ClampToBounds(FromDateTime);
            DateTime? to = ClampToBounds(ToDateTime);

            if (from != FromDateTime)
            {
                SetCurrentValue(FromDateTimeProperty, from);
            }
            if (to != ToDateTime)
            {
                SetCurrentValue(ToDateTimeProperty, to);
            }

            // 3. Если после клампинга From > To – восстанавливаем порядок,
            //    сохраняя намерение того свойства, которое изменилось.
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {

                if (property == FromDateTimeProperty)
                {
                    SetCurrentValue(ToDateTimeProperty, from.Value);
                }
                else if (property == ToDateTimeProperty)
                {
                    SetCurrentValue(FromDateTimeProperty, to.Value);
                }
                else if (property == MinDateTimeProperty)
                {
                    SetCurrentValue(ToDateTimeProperty, from.Value);   // Min сдвинулся вправо → To подтягиваем
                }
                else if (property == MaxDateTimeProperty)
                {
                    SetCurrentValue(FromDateTimeProperty, to.Value);   // Max сдвинулся влево → From подтягиваем
                }
                else
                {
                    SetCurrentValue(ToDateTimeProperty, from.Value);   // fallback
                }
            }

            UpdateValidation();
        }
        finally
        {
            _isCoercing = false;
        }
    }

    private DateTime? ClampToBounds(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }
        var dt = DateTimeNormalization.EnsureUtc(value.Value);
        return DateTimeRangeCoercion.Clamp(dt, MinDateTime, MaxDateTime);
    }

    /// <summary>
    /// Re-evaluates <see cref="IsValid"/> and <see cref="ValidationMessage"/> based on the current
    /// state of <see cref="MinDateTime"/>, <see cref="MaxDateTime"/>, <see cref="FromDateTime"/>,
    /// and <see cref="ToDateTime"/>. Enables or disables the control based on boundary validity.
    /// </summary>
    private void UpdateValidation()
    {
        bool boundsValid = !(MinDateTime.HasValue && MaxDateTime.HasValue && MinDateTime.Value > MaxDateTime.Value);
        AreBoundsValid = boundsValid;

        if (!boundsValid)
        {
            IsValid = false;
            ValidationMessage = "MinDateTime cannot be greater than MaxDateTime.";
            return;
        }

        if (!FromDateTime.HasValue || !ToDateTime.HasValue)
        {
            IsValid = false;
            ValidationMessage = "Both From and To must be set.";
        }
        else if (FromDateTime.Value > ToDateTime.Value)
        {
            IsValid = false;
            ValidationMessage = "From must be less than or equal to To.";
        }
        else if ((MinDateTime.HasValue && FromDateTime.Value < MinDateTime.Value) ||
                 (MaxDateTime.HasValue && ToDateTime.Value > MaxDateTime.Value))
        {
            IsValid = false;
            ValidationMessage = "Range exceeds allowed boundaries.";
        }
        else
        {
            IsValid = true;
            ValidationMessage = null;
        }
    }

    private bool _defaultsApplied;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (!_defaultsApplied)
        {
            ApplyDefaultRange();
            _defaultsApplied = true;
        }
    }

    /// <summary>
    /// Applies sensible default values for <see cref="FromDateTime"/> and <see cref="ToDateTime"/>
    /// based on available bounds (<see cref="MinDateTime"/>, <see cref="MaxDateTime"/>) 
    /// and the current UTC time from <see cref="TimeProvider"/>.
    /// 
    /// The right edge (anchor) is <see cref="MaxDateTime"/> if set, otherwise the current UTC time.
    /// The left edge is anchor minus 1 hour, but never earlier than <see cref="MinDateTime"/>.
    /// If only one edge was set by the consumer, the missing edge is derived from the anchor.
    /// The operation is atomic: it raises <see cref="RangeChanged"/> and <see cref="ValidationChanged"/>
    /// only once if the range changed.
    /// </summary>
    private void ApplyDefaultRange() => ApplyRangeChange(
        action: () =>
        {
            if (FromDateTime.HasValue && ToDateTime.HasValue)
            {
                return;
            }

            var anchor = GetAnchor();
            var defaultDuration = TimeSpan.FromHours(1);

            if (!FromDateTime.HasValue && !ToDateTime.HasValue)
            {
                var (start, end) = CalculatePresetRange(defaultDuration);
                SetCurrentValue(FromDateTimeProperty, start);
                SetCurrentValue(ToDateTimeProperty, end);
            }
            else if (FromDateTime.HasValue)
            {
                var to = anchor;
                if (to < FromDateTime.Value)
                {
                    to = FromDateTime.Value;
                }
                SetCurrentValue(ToDateTimeProperty, to);
            }
            else
            {
                var from = anchor - defaultDuration;
                if (MinDateTime.HasValue && from < MinDateTime.Value)
                {
                    from = MinDateTime.Value;
                }
                if (from > ToDateTime!.Value)
                {
                    from = ToDateTime.Value;
                }
                SetCurrentValue(FromDateTimeProperty, from);
            }

            Coerce(FromDateTimeProperty);
        },
        oldFrom: FromDateTime,
        oldTo: ToDateTime,
        oldIsValid: IsValid,
        oldValidationMessage: ValidationMessage
    );

    private static DateTime? CoerceDateTimeToUtc(AvaloniaObject sender, DateTime? value)
    {
        if (value is null)
        {
            return null;
        }
        return DateTimeNormalization.EnsureUtc(value.Value);
    }
}

/// <summary>
/// A simple reusable command implementation for preset execution.
/// </summary>
internal class PresetCommand : ICommand
{
    private readonly Action<TimeSpan> _execute;
    private readonly Func<bool> _canExecute;

    public PresetCommand(Action<TimeSpan> execute, Func<bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) =>
        _canExecute() && parameter is TimeSpan ts && ts > TimeSpan.Zero;

    public void Execute(object? parameter)
    {
        if (parameter is TimeSpan duration)
        {
            _execute(duration);
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}