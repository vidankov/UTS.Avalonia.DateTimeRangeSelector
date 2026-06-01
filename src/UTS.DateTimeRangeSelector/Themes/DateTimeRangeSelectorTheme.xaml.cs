using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace UTS.DateTimeRangeSelector.Themes;

/// <summary>
/// Loads all styles and themes for the DateTimeRangeSelector library.
/// Add this to Application.Styles after the base theme.
/// </summary>
public class DateTimeRangeSelectorTheme : Styles
{
    public DateTimeRangeSelectorTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}