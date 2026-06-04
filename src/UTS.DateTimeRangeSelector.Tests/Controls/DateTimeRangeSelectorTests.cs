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

    [Fact]
    public void WhenMinExceedsMax_ValuesArePreserved_And_ControlIsInvalid()
    {
        var min = new DateTime(2026, 06, 03, 12, 0, 0);
        var max = new DateTime(2026, 06, 03, 15, 0, 0);

        var from = new DateTime(2026, 06, 03, 13, 0, 0);
        var to = new DateTime(2026, 06, 03, 14, 0, 0);

        var minExcedingMax = new DateTime(2026, 06, 03, 16, 0, 0);

        _selector.SetCurrentValue(Selector.MinDateTimeProperty, min);
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, max);

        _selector.SetCurrentValue(Selector.FromDateTimeProperty, from);
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, to);

        _selector.SetCurrentValue(Selector.MinDateTimeProperty, minExcedingMax);

        _selector.FromDateTime.Should().Be(from, "values must not be wiped when bounds become contradictory");
        _selector.ToDateTime.Should().Be(to, "values must not be wiped when bounds become contradictory");
        _selector.AreBoundsValid.Should().BeFalse("Min > Max makes bounds invalid");
        _selector.IsValid.Should().BeFalse("control must report invalid state when bounds are contradictory");
        _selector.ValidationMessage.Should().NotBeNullOrEmpty("validation message must explain the issue");
    }

    [Fact]
    public void WhenMinExceedsMax_AreBoundsValidShouldBeFalse_And_IsEnabledShouldBeUnchanged()
    {
        // Arrange & Act
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(2));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);

        // Assert
        _selector.AreBoundsValid.Should().BeFalse();
        _selector.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void WhenBoundsBecomeValid_AreBoundsValidShouldBeTrue()
    {
        // Arrange
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(2));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);
        _selector.AreBoundsValid.Should().BeFalse(); // precondition

        // Act
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now.AddHours(3));

        // Assert
        _selector.AreBoundsValid.Should().BeTrue();
        _selector.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void WhenMinExceedsMax_RangeChangedShouldBeRaised()
    {
        // Arrange
        var eventRaised = false;
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-1));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);
        _selector.RangeChanged += (_, _) => eventRaised = true;

        // Act
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(2));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);

        // Assert
        eventRaised.Should().BeTrue("сброс значений при Min > Max должен порождать событие RangeChanged");
    }

    [Fact]
    public void WhenMinExceedsMax_ValidationChangedShouldBeRaised()
    {
        // Arrange
        var eventRaised = false;
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-1));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);
        _selector.ValidationChanged += (_, _) => eventRaised = true;

        // Act
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(2));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);

        // Assert
        eventRaised.Should().BeTrue("изменение валидности должно порождать ValidationChanged");
    }

    /// <summary>
    /// Documents <see cref="Selector.SetRange"/> coercion intent: sets <c>From</c> then <c>To</c>, then
    /// <c>Coerce(FromDateTimeProperty)</c>. The intermediate <c>To</c> assignment runs coercion with
    /// <c>ToDateTimeProperty</c> intent, so inverted endpoints pull <c>From</c> down to <c>To</c> (not the reverse).
    /// </summary>
    [Fact]
    public void R26_9_SetRange_InvertedEndpoints_FromIsAdjustedToMatchTo()
    {
        var later = Now.AddHours(2);
        var earlier = Now.AddHours(-1);

        _selector.SetRange(later, earlier);

        _selector.FromDateTime.Should().Be(earlier,
            "SetRange assigns To after From; inverted range coerces with To intent on the To assignment");
        _selector.ToDateTime.Should().Be(earlier);
    }
}