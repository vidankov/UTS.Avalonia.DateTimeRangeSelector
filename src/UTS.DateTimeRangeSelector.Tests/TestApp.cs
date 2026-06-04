using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(UTS.DateTimeRangeSelector.Tests.TestApp))]

namespace UTS.DateTimeRangeSelector.Tests;

/// <summary>
/// Minimal Avalonia application used by [AvaloniaFact] / [AvaloniaTheory] headless tests.
///
/// Loads FluentTheme for base dynamic resources (CalendarDatePickerBackground, etc.)
/// but deliberately does NOT load <c>DateTimeRangeSelectorTheme</c>.
///
/// Reason: the library's AXAML resources are compiled against Avalonia 11.3.14 while
/// the headless runtime (via Avalonia.Headless.XUnit 12.0.3) targets Avalonia 12.0.3.
/// Loading the compiled AXAML would throw <see cref="MissingMethodException"/> on
/// <c>RelativeSourceExtension..ctor(RelativeSourceMode)</c> due to the version mismatch.
///
/// All headless tests that need a ControlTemplate use <see cref="FuncControlTemplate{T}"/>
/// to define inline templates, which bypasses the compiled AXAML entirely.
/// </summary>
public sealed class TestApp
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder
            .Configure<HeadlessApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

internal sealed class HeadlessApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        // DateTimeRangeSelectorTheme intentionally omitted — see class doc.
    }
}
