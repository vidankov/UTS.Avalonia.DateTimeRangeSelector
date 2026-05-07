using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

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
    /// Prevents reentrancy when coercing From/To values.
    /// </summary>
    private bool _isCoercing;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_isCoercing)
        {
            return;
        }

        if (change.Property == FromDateTimeProperty || change.Property == ToDateTimeProperty)
        {
            Coerce(change.Property);
        }
    }

    /// <summary>
    /// Ensures that <see cref="FromDateTime"/> is not greater than <see cref="ToDateTime"/>.
    /// Called when either property changes. If the range becomes inverted, the unchanged
    /// property is adjusted to match the changed one (From snaps To forward, or To snaps From backward).
    /// </summary>
    /// <param name="property">The property that triggered the coercion.</param>
    private void Coerce(AvaloniaProperty property)
    {
        if (!FromDateTime.HasValue || !ToDateTime.HasValue || FromDateTime.Value <= ToDateTime.Value)
        {
            return;
        }

        _isCoercing = true;
        try
        {
            if (property == FromDateTimeProperty)
            {
                SetCurrentValue(ToDateTimeProperty, FromDateTime.Value);
            }
            else if (property == ToDateTimeProperty)
            {
                SetCurrentValue(FromDateTimeProperty, ToDateTime.Value);
            }
        }
        finally
        {
            _isCoercing = false;
        }
    }
}