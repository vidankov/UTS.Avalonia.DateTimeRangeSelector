using System.Reflection;
using Selector = UTS.DateTimeRangeSelector.Controls.DateTimeRangeSelector;

namespace UTS.DateTimeRangeSelector.Tests.Infrastructure;

/// <summary>
/// R26-17 API-shape proof tests (reflection only, no file I/O).
/// </summary>
public class LibraryApiShapeTests
{
    [Fact]
    public void R26_17_TimeProvider_ShouldBeRegisteredAsStyledProperty()
    {
        var field = typeof(Selector).GetField(
            "TimeProviderProperty",
            BindingFlags.Public | BindingFlags.Static);

        field.Should().NotBeNull("TimeProvider should be a StyledProperty for XAML and theme binding");
        field!.FieldType.Name.Should().Contain("StyledProperty",
            "TimeProviderProperty must be Avalonia StyledProperty, not a plain CLR property only");
    }

    [Fact]
    public void R26_17_TemplatePart_PART_FromPanel_ShouldHaveCorrespondingPrivateField()
    {
        typeof(Selector).GetField(
                "_fromPanel",
                BindingFlags.NonPublic | BindingFlags.Instance)
            .Should()
            .NotBeNull("[TemplatePart(PART_FromPanel)] should be resolved in OnApplyTemplate for maintainability");
    }

    [Fact]
    public void R26_17_TemplatePart_PART_ToPanel_ShouldHaveCorrespondingPrivateField()
    {
        typeof(Selector).GetField(
                "_toPanel",
                BindingFlags.NonPublic | BindingFlags.Instance)
            .Should()
            .NotBeNull("[TemplatePart(PART_ToPanel)] should be resolved in OnApplyTemplate for maintainability");
    }
}
