using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using System.Windows.Input;
using UTS.DateTimeRangeSelector.Core;
using UTS.DateTimeRangeSelector.Events;

namespace UTS.DateTimeRangeSelector.Controls;

/// <summary>
/// A composite control for selecting a date/time range (From – To).
/// Composes two <see cref="DateTimePickerPanel"/> instances.
/// </summary>
public class DateTimeRangeSelector : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="FromDateTime"/> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> FromDateTimeProperty =
        AvaloniaProperty.Register<DateTimeRangeSelector, DateTime?>(
            nameof(FromDateTime),
            defaultBindingMode: BindingMode.TwoWay);

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
            defaultBindingMode: BindingMode.TwoWay);

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
    /// Initializes a new instance of the <see cref="DateTimeRangeSelector"/> class.
    /// </summary>
    public DateTimeRangeSelector()
    {
        _applyPresetCommand = new PresetCommand(
            execute: duration =>
            {
                var oldFrom = FromDateTime;
                var oldTo = ToDateTime;
                var oldIsValid = IsValid;
                var oldValidationMessage = ValidationMessage;

                _suppressEvents = true;
                try
                {
                    var (start, end) = CalculateRangeFromAnchor(duration);
                    SetCurrentValue(FromDateTimeProperty, start);
                    SetCurrentValue(ToDateTimeProperty, end);
                    Coerce(FromDateTimeProperty);
                }
                finally
                {
                    _suppressEvents = false;
                }

                if (oldFrom != FromDateTime || oldTo != ToDateTime)
                {
                    RaiseEvent(new DateTimeRangeChangedEventArgs(
                        RangeChangedEvent, oldFrom, FromDateTime, oldTo, ToDateTime));
                }
                if (oldIsValid != IsValid || oldValidationMessage != ValidationMessage)
                {
                    RaiseEvent(new ValidationChangedEventArgs(
                        ValidationChangedEvent, oldIsValid, IsValid, oldValidationMessage, ValidationMessage));
                }
            },
            canExecute: () => ShowPresets
        );
    }

    /// <summary>
    /// Returns the anchor (right edge) used for range calculations.
    /// If <see cref="MaxDateTime"/> is set, it is the anchor;
    /// otherwise, the current UTC time from <see cref="TimeProvider"/>.
    /// </summary>
    private DateTime GetAnchor() => MaxDateTime ?? TimeProvider.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Calculates a time range of the given <paramref name="duration"/> ending at the anchor.
    /// The start is clamped to <see cref="MinDateTime"/> if a lower bound exists.
    /// </summary>
    /// <param name="duration">The length of the range (positive).</param>
    /// <returns>A tuple containing the calculated (start, end) values in UTC.</returns>
    private (DateTime start, DateTime end) CalculateRangeFromAnchor(TimeSpan duration)
    {
        var end = GetAnchor();
        var start = end - duration;
        if (MinDateTime.HasValue && start < MinDateTime.Value)
        {
            start = MinDateTime.Value;
        }
        return (start, end);
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

        if (_isCoercing)
        {
            return;
        }

        if (change.Property == ShowPresetsProperty)
        {
            _applyPresetCommand.RaiseCanExecuteChanged();
        }

        if (change.Property == FromDateTimeProperty ||
            change.Property == ToDateTimeProperty ||
            change.Property == MinDateTimeProperty ||
            change.Property == MaxDateTimeProperty)
        {
            if (!_suppressEvents)
            {
                DateTime? oldFrom;
                DateTime? oldTo;

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
                else // MinDateTime or MaxDateTime
                {
                    oldFrom = FromDateTime;
                    oldTo = ToDateTime;
                }

                bool oldIsValid = IsValid;
                string? oldValidationMessage = ValidationMessage;

                Coerce(change.Property);

                if (oldFrom != FromDateTime || oldTo != ToDateTime)
                {
                    RaiseEvent(new DateTimeRangeChangedEventArgs(
                        RangeChangedEvent, oldFrom, FromDateTime, oldTo, ToDateTime));
                }

                if (oldIsValid != IsValid || oldValidationMessage != ValidationMessage)
                {
                    RaiseEvent(new ValidationChangedEventArgs(
                        ValidationChangedEvent, oldIsValid, IsValid, oldValidationMessage, ValidationMessage));
                }
            }
            else
            {
                Coerce(change.Property);
            }
        }
    }

    /// <summary>
    /// Coerces the current <see cref="FromDateTime"/> and <see cref="ToDateTime"/> to be 
    /// within <see cref="MinDateTime"/>..<see cref="MaxDateTime"/> (clamping), 
    /// normalizes them to UTC, and ensures <c>From &lt;= To</c>.
    /// </summary>
    /// <param name="property">The property that triggered the coercion.</param>
    /// <remarks>
    /// When the range becomes inverted after clamping, the unchanged property 
    /// is adjusted to match the changed one (From pushes To forward, or To pulls From backward).
    /// After coercion, <see cref="UpdateValidation"/> is called to refresh validation state.
    /// </remarks>
    private void Coerce(AvaloniaProperty property)
    {
        if (_isCoercing)
        {
            return;
        }

        bool oldSuppress = _suppressEvents;
        _suppressEvents = true;
        try
        {
            // 1. Нормализуем и клампим From/To к границам Min/Max
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

            // 2. Если после клампинга From > To – восстанавливаем порядок,
            //    сохраняя намерение того свойства, которое изменилось.
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                _isCoercing = true;
                try
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
                finally
                {
                    _isCoercing = false;
                }
            }
            UpdateValidation();
        }
        finally
        {
            _suppressEvents = oldSuppress;
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

    private void UpdateValidation()
    {
        if (!FromDateTime.HasValue || !ToDateTime.HasValue)
        {
            IsValid = false;
            ValidationMessage = "Both From and To must be set.";
        }
        else if (MinDateTime.HasValue && MaxDateTime.HasValue && MinDateTime.Value > MaxDateTime.Value)
        {
            IsValid = false;
            ValidationMessage = "MinDateTime cannot be greater than MaxDateTime.";
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
    /// </summary>
    private void ApplyDefaultRange()
    {
        if (FromDateTime.HasValue && ToDateTime.HasValue)
        {
            return;
        }

        var oldFrom = FromDateTime;
        var oldTo = ToDateTime;
        var oldIsValid = IsValid;
        var oldValidationMessage = ValidationMessage;

        bool oldSuppress = _suppressEvents;
        _suppressEvents = true;

        try
        {
            var anchor = GetAnchor();
            var defaultDuration = TimeSpan.FromHours(1);

            if (!FromDateTime.HasValue && !ToDateTime.HasValue)
            {
                var (start, end) = CalculateRangeFromAnchor(defaultDuration);
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
        }
        finally
        {
            _suppressEvents = oldSuppress;
        }

        if (oldFrom != FromDateTime || oldTo != ToDateTime)
        {
            RaiseEvent(new DateTimeRangeChangedEventArgs(
                RangeChangedEvent, oldFrom, FromDateTime, oldTo, ToDateTime));
        }
        if (oldIsValid != IsValid || oldValidationMessage != ValidationMessage)
        {
            RaiseEvent(new ValidationChangedEventArgs(
                ValidationChangedEvent, oldIsValid, IsValid, oldValidationMessage, ValidationMessage));
        }
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

    public bool CanExecute(object? parameter) => _canExecute() && parameter is TimeSpan;

    public void Execute(object? parameter)
    {
        if (parameter is TimeSpan duration)
        {
            _execute(duration);
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}