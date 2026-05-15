using NSubstitute;
using UTS.DateTimeRangeSelector.Events;
using Selector = UTS.DateTimeRangeSelector.Controls.DateTimeRangeSelector;

namespace UTS.DateTimeRangeSelector.Tests.Controls;

public class DateTimeRangeSelectorTests
{
    private readonly Selector _selector;
    private readonly TimeProvider _timeProvider;

    private static readonly DateTime Now = new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    public DateTimeRangeSelectorTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(Now));

        _selector = new Selector { TimeProvider = _timeProvider };
    }

    [Fact]
    public void SetValidFromTo_ShouldSetBothValuesAndBeValid()
    {
        // Act
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-2));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        // Assert
        _selector.FromDateTime.Should().Be(Now.AddHours(-2));
        _selector.ToDateTime.Should().Be(Now);
        _selector.IsValid.Should().BeTrue();
        _selector.ValidationMessage.Should().BeNull();
    }

    [Fact]
    public void WhenFromIsSetGreaterThanTo_ShouldPushToForward()
    {
        // Arrange
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now);
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now.AddHours(-1)); // To раньше From

        // Act: меняем From на значение, которое всё ещё больше To
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(1));

        // Assert: To должно подтянуться до нового From
        _selector.FromDateTime.Should().Be(Now.AddHours(1));
        _selector.ToDateTime.Should().Be(Now.AddHours(1));
        _selector.IsValid.Should().BeTrue(); // Равные значения валидны
    }

    [Fact]
    public void SetFromBelowMin_ShouldClampToMin()
    {
        // Arrange
        var min = Now.AddHours(-5);
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, min);

        // Act
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-10));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        // Assert
        _selector.FromDateTime.Should().Be(min);
        _selector.ToDateTime.Should().Be(Now);
        _selector.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhenMinGreaterThanMax_ShouldBeInvalid()
    {
        // Arrange
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-2));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        // Act
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(1));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);

        // Assert
        _selector.IsValid.Should().BeFalse();
        _selector.ValidationMessage.Should().Contain("MinDateTime cannot be greater than MaxDateTime");
    }

    [Fact]
    public void RangeChangedEvent_ShouldBeRaisedOnFromChange()
    {
        // Arrange
        DateTimeRangeChangedEventArgs? receivedArgs = null;
        _selector.RangeChanged += (s, e) => receivedArgs = e;

        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-1));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        // Act: меняем From
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-2));

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.OldFrom.Should().Be(Now.AddHours(-1));
        receivedArgs.NewFrom.Should().Be(Now.AddHours(-2));
        receivedArgs.OldTo.Should().Be(Now);
        receivedArgs.NewTo.Should().Be(Now); // To не изменилось
    }

    [Fact]
    public void ValidationChangedEvent_ShouldBeRaisedWhenValidityToggles()
    {
        // Arrange
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-2));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        ValidationChangedEventArgs? receivedArgs = null;
        _selector.ValidationChanged += (s, e) => receivedArgs = e;

        // Act: делаем Min > Max
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(1));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.OldIsValid.Should().BeTrue();
        receivedArgs.NewIsValid.Should().BeFalse();
    }

    [Fact]
    public void SetRange_ShouldRaiseSingleRangeChangedEvent()
    {
        // Arrange
        int eventCount = 0;
        _selector.RangeChanged += (_, _) => eventCount++;

        // Act
        _selector.SetRange(Now.AddHours(-3), Now.AddHours(3));

        // Assert
        eventCount.Should().Be(1);
        _selector.FromDateTime.Should().Be(Now.AddHours(-3));
        _selector.ToDateTime.Should().Be(Now.AddHours(3));
    }

    [Fact]
    public void ApplyPreset_WithoutMax_ShouldUseNowAsAnchor()
    {
        // Act
        _selector.ApplyPreset(TimeSpan.FromHours(2));

        // Assert
        _selector.FromDateTime.Should().Be(Now.AddHours(-2));
        _selector.ToDateTime.Should().Be(Now);
        _selector.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ApplyPreset_WithMax_ShouldUseMaxAsAnchor()
    {
        // Arrange
        var max = Now.AddHours(10);
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, max);

        // Act
        _selector.ApplyPreset(TimeSpan.FromHours(3));

        // Assert
        _selector.ToDateTime.Should().Be(max);
        _selector.FromDateTime.Should().Be(max.AddHours(-3));
    }

    [Fact]
    public void ResetToDefaults_ShouldUseNowMinusOneHour()
    {
        // Act
        _selector.ResetToDefaults();

        // Assert
        _selector.FromDateTime.Should().Be(Now.AddHours(-1));
        _selector.ToDateTime.Should().Be(Now);
    }
}