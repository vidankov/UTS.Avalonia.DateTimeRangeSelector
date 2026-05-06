using Avalonia.Data.Converters;
using System.Globalization;

namespace UTS.DateTimeRangeSelector.Converters;

/// <summary>
/// Converts between decimal? (used by NumericUpDown) and int (for control properties).
/// </summary>
public class DecimalToIntConverter : IValueConverter
{
    public static readonly DecimalToIntConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return 0;
        }
        if (value is decimal d)
        {
            return (int)d;
        }
        if (value is int i)
        {
            return i;
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return 0m;
        }
        if (value is int i)
        {
            return (decimal)i;
        }
        if (value is decimal d)
        {
            return d;
        }
        return 0m;
    }
}