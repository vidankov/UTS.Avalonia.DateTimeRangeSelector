using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using UTS.DateTimeRangeSelector.Core;
using UTS.DateTimeRangeSelector.Events;
using UTS.DateTimeRangeSelector.Exceptions;

namespace UTS.DateTimeRangeSelector.Controls;

/// <summary>
/// A composite control for selecting a date/time range (From – To).
/// Composes two <see cref="DateTimePickerPanel"/> instances.
/// </summary>
[TemplatePart("PART_FromPanel", typeof(DateTimePickerPanel))]
[TemplatePart("PART_ToPanel", typeof(DateTimePickerPanel))]
public class DateTimeRangeSelector : TemplatedControl
{
    private readonly ReplaySubject<DateTimeRange> _rangeSubject;
    private readonly ReplaySubject<ValidationResult> _validationSubject;

    private DateTimePickerPanel? _fromPanel;
    private DateTimePickerPanel? _toPanel;

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
    /// Defines the <see cref="FromLabel"/> property.
    /// </summary>
    public static readonly StyledProperty<string> FromLabelProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, string>(
            nameof(FromLabel),
            defaultValue: "From:");

    /// <summary>
    /// Gets or sets the label text for the From date-time panel.
    /// The default is "From:".
    /// </summary>
    public string FromLabel
    {
        get => GetValue(FromLabelProperty);
        set => SetValue(FromLabelProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ToLabel"/> property.
    /// </summary>
    public static readonly StyledProperty<string> ToLabelProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, string>(
            nameof(ToLabel),
            defaultValue: "To:");

    /// <summary>
    /// Gets or sets the label text for the To date-time panel.
    /// The default is "To:".
    /// </summary>
    public string ToLabel
    {
        get => GetValue(ToLabelProperty);
        set => SetValue(ToLabelProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MinDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateTimeProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, DateTime?>(
            nameof(MinDateTime),
            coerce: CoerceDateTimeToUtc);

    /// <summary>
    /// Gets or sets the minimum allowed date and time for the range.
    /// The value is coerced to UTC. Null means no lower limit.
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
            nameof(MaxDateTime),
            coerce: CoerceDateTimeToUtc);

    /// <summary>
    /// Gets or sets the maximum allowed date and time for the range.
    /// The value is coerced to UTC. Null means no upper limit.
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
    /// Determines whether a preset or reset operation is allowed to silently truncate
    /// the requested duration when it does not fit within <see cref="MinDateTime"/>..<see cref="MaxDateTime"/>.
    /// If <see langword="false"/> (default), an <see cref="PresetOutOfBoundsException"/> is thrown
    /// and the current range remains unchanged.
    /// If <see langword="true"/>, the range is clamped to the available bounds
    /// (behaviour identical to earlier versions).
    /// </summary>
    public static readonly StyledProperty<bool> AllowPresetTruncationProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, bool>(
            nameof(AllowPresetTruncation),
            defaultValue: false);

    public bool AllowPresetTruncation
    {
        get => GetValue(AllowPresetTruncationProperty);
        set => SetValue(AllowPresetTruncationProperty, value);
    }

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
    /// Defines the <see cref="TimeProvider"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeProvider?> TimeProviderProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, TimeProvider?>(
            nameof(TimeProvider),
            defaultValue: TimeProvider.System);

    /// <summary>
    /// Gets or sets the <see cref="System.TimeProvider"/> used to obtain the current UTC time
    /// when applying presets or initializing the default range.
    /// The default is <see cref="System.TimeProvider.System"/>.
    /// </summary>
    public TimeProvider? TimeProvider
    {
        get => GetValue(TimeProviderProperty);
        set => SetValue(TimeProviderProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="HourSuffix"/> property.
    /// </summary>
    public static readonly StyledProperty<string> HourSuffixProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, string>(
            nameof(HourSuffix),
            defaultValue: "ч.");

    /// <summary>
    /// Gets or sets the suffix displayed after the hours control.
    /// The default is "ч.".
    /// </summary>
    public string HourSuffix
    {
        get => GetValue(HourSuffixProperty);
        set => SetValue(HourSuffixProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MinuteSuffix"/> property.
    /// </summary>
    public static readonly StyledProperty<string> MinuteSuffixProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, string>(
            nameof(MinuteSuffix),
            defaultValue: "мин.");

    /// <summary>
    /// Gets or sets the suffix displayed after the minutes control.
    /// The default is "мин.".
    /// </summary>
    public string MinuteSuffix
    {
        get => GetValue(MinuteSuffixProperty);
        set => SetValue(MinuteSuffixProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="SecondSuffix"/> property.
    /// </summary>
    public static readonly StyledProperty<string> SecondSuffixProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, string>(
            nameof(SecondSuffix),
            defaultValue: "сек.");

    /// <summary>
    /// Gets or sets the suffix displayed after the seconds control.
    /// The default is "сек.".
    /// </summary>
    public string SecondSuffix
    {
        get => GetValue(SecondSuffixProperty);
        set => SetValue(SecondSuffixProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MillisecondSuffix"/> property.
    /// </summary>
    public static readonly StyledProperty<string> MillisecondSuffixProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, string>(
            nameof(MillisecondSuffix),
            defaultValue: "мс.");

    /// <summary>
    /// Gets or sets the suffix displayed after the milliseconds control.
    /// The default is "мс.".
    /// </summary>
    public string MillisecondSuffix
    {
        get => GetValue(MillisecondSuffixProperty);
        set => SetValue(MillisecondSuffixProperty, value);
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
    /// Defines the <see cref="ApplyPresetCommand"/> property.
    /// </summary>
    public static readonly DirectProperty<DateTimeRangeSelector, ICommand> ApplyPresetCommandProperty =
        AvaloniaProperty.RegisterDirect<DateTimeRangeSelector, ICommand>(
            nameof(ApplyPresetCommand),
            o => o.ApplyPresetCommand);

    private PresetCommand _applyPresetCommand;

    /// <summary>
    /// Gets an observable sequence of <see cref="DateTimeRange"/> values representing
    /// the current range. The observable is hot and replays the latest value to new subscribers
    /// once the control has initialized (after <see cref="OnApplyTemplate"/> applies defaults).
    /// Subscribers attached before template application receive the first value when initialization completes.
    /// Changes are distinct (no consecutive duplicates).
    /// </summary>
    public IObservable<DateTimeRange> RangeChanges =>
        _rangeSubject.AsObservable().DistinctUntilChanged();

    /// <summary>
    /// Gets an observable sequence of <see cref="ValidationResult"/> values representing
    /// the current validation state. The observable is hot and replays the latest value
    /// once the control has initialized (after <see cref="OnApplyTemplate"/> applies defaults).
    /// Subscribers attached before template application receive the first value when initialization completes.
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

        _rangeSubject = new ReplaySubject<DateTimeRange>(1);
        _validationSubject = new ReplaySubject<ValidationResult>(1);
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
    /// Applies a preset duration to the range.
    /// See <see cref="ApplyPresetCore"/> for details on truncation behaviour and exceptions.
    /// </summary>
    /// <param name="duration">A positive <see cref="TimeSpan"/> representing the desired range length.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="duration"/> is zero or negative.</exception>
    /// <exception cref="PresetOutOfBoundsException">Thrown when the requested duration cannot be satisfied
    /// and <see cref="AllowPresetTruncation"/> is <see langword="false"/>.</exception>
    public void ApplyPreset(TimeSpan duration) => ApplyPresetCore(duration, isExplicitPreset: true);

    /// <summary>
    /// Resets <see cref="FromDateTime"/> and <see cref="ToDateTime"/> to the default 1‑hour window.
    /// See <see cref="ApplyPresetCore"/> for details on truncation behaviour and exceptions.
    /// </summary>
    /// <exception cref="PresetOutOfBoundsException">Thrown when the default 1‑hour window cannot be satisfied
    /// and <see cref="AllowPresetTruncation"/> is <see langword="false"/>.</exception>
    public void ResetToDefaults() => ApplyPresetCore(TimeSpan.FromHours(1), isExplicitPreset: false);

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
    private DateTime GetAnchor() => MaxDateTime ?? (TimeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;

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
    /// If <see cref="MinDateTime"/> > <see cref="MaxDateTime"/>, the values are left untouched,
    /// <see cref="AreBoundsValid"/> is set to <see langword="false"/>, and the control is disabled
    /// until valid bounds are restored.
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
            if (MinDateTime.HasValue && MaxDateTime.HasValue
                && MinDateTime.Value > MaxDateTime.Value)
            {
                UpdateValidation();
                return;
            }

            DateTime? originalFrom = FromDateTime;
            DateTime? originalTo = ToDateTime;

            var range = ComputeCoercedRange(originalFrom, originalTo, property);

            if (range.From != originalFrom)
            {
                SetCurrentValue(FromDateTimeProperty, range.From);
            }
            if (range.To != originalTo)
            {
                SetCurrentValue(ToDateTimeProperty, range.To);
            }

            UpdateValidation();
        }
        finally
        {
            _isCoercing = false;
        }
    }

    /// <summary>
    /// Pure computation of the final From/To values after normalisation, clamping,
    /// and order enforcement. Assumes that the bounds are valid
    /// (<see cref="MinDateTime"/> &lt;= <see cref="MaxDateTime"/>).
    /// Callers must check for contradictory bounds before invoking this method.
    /// </summary>
    /// <returns>A non-null <see cref="DateTimeRange"/> with both ends set.</returns>
    private DateTimeRange ComputeCoercedRange(
        DateTime? rawFrom,
        DateTime? rawTo,
        AvaloniaProperty leader)
    {
        DateTime? from = ClampToBounds(rawFrom);
        DateTime? to = ClampToBounds(rawTo);

        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            if (leader == FromDateTimeProperty)
            {
                to = from;
            }
            else if (leader == ToDateTimeProperty)
            {
                from = to;
            }
            else if (leader == MinDateTimeProperty)
            {
                to = from;
            }
            else if (leader == MaxDateTimeProperty)
            {
                from = to;
            }
            else
            {
                to = from;
            }
        }

        return new DateTimeRange(from, to);
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

        _fromPanel = e.NameScope.Find<DateTimePickerPanel>("PART_FromPanel");
        _toPanel = e.NameScope.Find<DateTimePickerPanel>("PART_ToPanel");

        if (!_defaultsApplied)
        {
            ApplyDefaultRange();
            _defaultsApplied = true;
        }

        UpdateValidation();
        PublishObservableState();
    }

    /// <summary>
    /// Publishes the current range and validation snapshots to replay subjects.
    /// Does not raise <see cref="RangeChanged"/> or <see cref="ValidationChanged"/> routed events.
    /// </summary>
    private void PublishObservableState()
    {
        _rangeSubject.OnNext(new DateTimeRange(FromDateTime, ToDateTime));
        _validationSubject.OnNext(new ValidationResult(IsValid, ValidationMessage));
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

    /// <summary>
    /// Core logic for applying a preset duration or resetting to defaults.
    /// If <see cref="AllowPresetTruncation"/> is <see langword="false"/> (default),
    /// throws <see cref="PresetOutOfBoundsException"/> when the requested duration
    /// cannot be fully satisfied within the current <see cref="MinDateTime"/>..<see cref="MaxDateTime"/> bounds.
    /// In truncation mode (<see cref="AllowPresetTruncation"/> = <see langword="true"/>)
    /// the range is silently clamped to the available space.
    /// </summary>
    /// <param name="duration">A positive <see cref="TimeSpan"/> requested for the range.</param>
    /// <param name="isExplicitPreset">
    /// <see langword="true"/> when called from <see cref="ApplyPreset"/>,
    /// <see langword="false"/> from <see cref="ResetToDefaults"/>.
    /// Currently unused; reserved for future differentiation of behaviour.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="duration"/> is zero or negative.
    /// </exception>
    /// <exception cref="PresetOutOfBoundsException">
    /// Thrown when the requested duration cannot be satisfied and <see cref="AllowPresetTruncation"/> is <see langword="false"/>.
    /// </exception>
    private void ApplyPresetCore(TimeSpan duration, bool isExplicitPreset)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        // Contradictory bounds → range is impossible.
        if (MinDateTime.HasValue && MaxDateTime.HasValue && MinDateTime.Value > MaxDateTime.Value)
        {
            if (!AllowPresetTruncation)
            {
                throw new PresetOutOfBoundsException(duration, resultingDuration: null, MinDateTime, MaxDateTime);
            }
            // Soft mode: leave current range unchanged.
            return;
        }

        var (rawStart, rawEnd) = CalculatePresetRange(duration);
        DateTimeRange finalRange = ComputeCoercedRange(rawStart, rawEnd, FromDateTimeProperty);

        // Determine whether the requested duration was fully preserved.
        bool durationPreserved = duration == finalRange.Duration;

        if (!AllowPresetTruncation && !durationPreserved)
        {
            throw new PresetOutOfBoundsException(duration, finalRange.Duration, MinDateTime, MaxDateTime);
        }

        // Apply the computed (possibly truncated) range.
        ApplyRangeChange(
            action: () =>
            {
                SetCurrentValue(FromDateTimeProperty, finalRange.From);
                SetCurrentValue(ToDateTimeProperty, finalRange.To);
                Coerce(FromDateTimeProperty);
            },
            oldFrom: FromDateTime,
            oldTo: ToDateTime,
            oldIsValid: IsValid,
            oldValidationMessage: ValidationMessage
        );
    }

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