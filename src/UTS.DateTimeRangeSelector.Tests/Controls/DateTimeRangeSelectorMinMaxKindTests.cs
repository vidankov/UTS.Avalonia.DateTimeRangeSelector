using NSubstitute;
using Selector = UTS.DateTimeRangeSelector.Controls.DateTimeRangeSelector;

namespace UTS.DateTimeRangeSelector.Tests.Controls;

/// <summary>
/// Proof-tests: <see cref="Selector.MinDateTime"/> / <see cref="Selector.MaxDateTime"/> are not UTC-coerced
/// on the range selector (unlike <see cref="Selector.FromDateTime"/> / <see cref="Selector.ToDateTime"/>).
/// </summary>
public class DateTimeRangeSelectorMinMaxKindTests
{
    private static readonly DateTime Now = new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void SetMinDateTime_LocalKind_ShouldBeStoredAsUtc()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(Now));

        var selector = new Selector { TimeProvider = timeProvider };
        var localMin = new DateTime(2025, 6, 15, 8, 0, 0, DateTimeKind.Local);

        selector.SetCurrentValue(Selector.MinDateTimeProperty, localMin);

        selector.MinDateTime.Should().NotBeNull();
        selector.MinDateTime!.Value.Kind.Should().Be(DateTimeKind.Utc,
            "MinDateTime is compared against UTC-normalized From/To; bounds must be UTC at the storage boundary");
    }

    [Fact]
    public void SetFromAndLocalMin_ShouldNotThrowDuringCoerce()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(Now));

        var selector = new Selector { TimeProvider = timeProvider };
        var localMin = new DateTime(2025, 6, 15, 8, 0, 0, DateTimeKind.Local);
        var from = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        selector.SetCurrentValue(Selector.MinDateTimeProperty, localMin);

        var act = () => selector.SetCurrentValue(Selector.FromDateTimeProperty, from);

        act.Should().NotThrow(
            "ClampToBounds compares Local Min against Utc From via DateTimeRangeCoercion.Clamp; " +
            "this must not throw ArgumentException for mixed kinds");
    }

    [Fact]
    public void R26_2_SetMaxDateTime_LocalKind_ShouldBeStoredAsUtc()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(Now));

        var selector = new Selector { TimeProvider = timeProvider };
        var localMax = new DateTime(2025, 6, 15, 18, 0, 0, DateTimeKind.Local);

        selector.SetCurrentValue(Selector.MaxDateTimeProperty, localMax);

        selector.MaxDateTime.Should().NotBeNull();
        selector.MaxDateTime!.Value.Kind.Should().Be(DateTimeKind.Utc,
            "MaxDateTime is compared against UTC-normalized From/To; bounds must be UTC at the storage boundary");
    }

    [Fact]
    public void R26_2_SetFromAndLocalMax_ShouldNotThrowDuringCoerce()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(Now));

        var selector = new Selector { TimeProvider = timeProvider };
        var localMax = new DateTime(2025, 6, 15, 18, 0, 0, DateTimeKind.Local);
        var from = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        selector.SetCurrentValue(Selector.MaxDateTimeProperty, localMax);

        var act = () => selector.SetCurrentValue(Selector.FromDateTimeProperty, from);

        act.Should().NotThrow(
            "ClampToBounds compares Local Max against Utc From via DateTimeRangeCoercion.Clamp; " +
            "this must not throw ArgumentException for mixed kinds");
    }
}
