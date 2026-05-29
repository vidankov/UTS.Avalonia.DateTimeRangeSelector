namespace UTS.DateTimeRangeSelector.Tests.Infrastructure;

/// <summary>
/// R26-17 / R26-5 backlog proof tests via source file scans. Skips when repo source is not available.
/// </summary>
public class LibrarySourceScanTests
{
    private const string SelectorAxaml = "UTS.DateTimeRangeSelector/Styles/DateTimeRangeSelector.axaml";
    private const string PanelAxaml = "UTS.DateTimeRangeSelector/Styles/DateTimePickerPanel.axaml";
    private const string CalendarAxaml = "UTS.DateTimeRangeSelector/Styles/ConstrainedCalendarDatePicker.axaml";
    private const string Csproj = "UTS.DateTimeRangeSelector/UTS.DateTimeRangeSelector.csproj";

    [Fact]
    [Trait("Category", "FileSystem")]
    public void R26_5_Scan_SelectorTemplate_ShouldBindValidationMessage()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(SelectorAxaml));

        content.Should().Contain("ValidationMessage",
            "production template must surface ValidationMessage to users when IsValid is false");
    }

    [Fact]
    [Trait("Category", "FileSystem")]
    public void R26_17_Scan_SelectorTemplate_NoHardcodedFromLabel()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(SelectorAxaml));

        content.Should().NotContain("Text=\"From:\"",
            "section labels must be localizable, not hardcoded English in library AXAML");
    }

    [Fact]
    [Trait("Category", "FileSystem")]
    public void R26_17_Scan_SelectorTemplate_NoHardcodedToLabel()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(SelectorAxaml));

        content.Should().NotContain("Text=\"To:\"",
            "section labels must be localizable, not hardcoded English in library AXAML");
    }

    [Fact]
    [Trait("Category", "FileSystem")]
    public void R26_17_Scan_PanelTemplate_NoRussianUnitSuffixes()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(PanelAxaml));

        content.Should().NotContain("Text=\"ч.\"", "time unit labels must not be hardcoded Russian in library AXAML");
        content.Should().NotContain("Text=\"м.\"", "time unit labels must not be hardcoded Russian in library AXAML");
    }

    [Fact]
    [Trait("Category", "FileSystem")]
    public void R26_17_Scan_Csproj_NoAxamlCompileExclusion()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(Csproj));

        content.Should().NotContain("AvaloniaXaml Remove=",
            "library style AXAML should participate in compile-time validation");
    }

    [Fact]
    [Trait("Category", "FileSystem")]
    public void R26_17_Scan_Csproj_NoAxamlCompileExclusion_AllowedThemeOnly()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(Csproj));

        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                           .Where(l => l.Contains("AvaloniaXaml Remove="))
                           .ToList();

        var allowedRemove = "Themes\\DateTimeRangeSelectorTheme.axaml";
        var forbidden = lines.Where(l => !l.Contains(allowedRemove)).ToList();

        forbidden.Should().BeEmpty(
            "all AvaloniaXaml Remove entries except for the theme file (with x:Class) must be deleted");
    }

    [Fact(Skip = "Skipped: binding deferred pending potential refactoring of this internal control (see #21).")]
    [Trait("Category", "FileSystem")]
    public void R26_17_Scan_CalendarTemplate_PartTextBoxShouldBindText()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(CalendarAxaml));

        // В случае, если тест станет актуален, необходимо поправить регулярное выражение,
        // используемое для поиска привязки. Сейчас даёт ложноположительный результат,
        // т.к. не привязывает наличие "{Binding" конкретно к "PART_TextBox", а лишь проверяет,
        // что где-то во всём файле есть "{Binding" после "PART_TextBox"

        content.Should().MatchRegex(
            @"PART_TextBox[\s\S]*Text=""\{Binding",
            "PART_TextBox should bind Text to SelectedDate for reliable display instead of imperative ForceTextUpdate only");
    }

    [Fact]
    [Trait("Category", "FileSystem")]
    public void R26_17_Scan_CalendarTemplate_NoFluentOnlyErrorBrush()
    {
        var content = File.ReadAllText(SourceFileLocator.RequireSourceFile(CalendarAxaml));

        content.Should().NotContain("SystemControlErrorTextForegroundBrush",
            "error styling must not depend on Fluent-only resource keys without theme fallback");
    }
}
