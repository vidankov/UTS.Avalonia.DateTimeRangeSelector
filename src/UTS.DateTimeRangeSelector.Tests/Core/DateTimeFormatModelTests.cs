namespace UTS.DateTimeRangeSelector.Tests.Core;

public class DateTimeFormatModelTests
{
    [Fact]
    public void Default_HasCorrectSeparateFormats()
    {
        var model = DateTimeFormatModel.Default;
        model.DateFormat.Should().Be("dd.MM.yyyy");
        model.TimeFormat.Should().Be("HH:mm:ss.fff");
    }

    [Fact]
    public void DateTimeFormat_CombinesCorrectly()
    {
        var model = new DateTimeFormatModel("yyyy/MM/dd", "HH:mm");
        model.DateTimeFormat.Should().Be("yyyy/MM/dd HH:mm");
    }
}