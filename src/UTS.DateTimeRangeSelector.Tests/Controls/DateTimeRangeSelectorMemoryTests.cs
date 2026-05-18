using System.Runtime.CompilerServices;
using Selector = UTS.DateTimeRangeSelector.Controls.DateTimeRangeSelector;

namespace UTS.DateTimeRangeSelector.Tests.Controls;

public class DateTimeRangeSelectorMemoryTests
{
    [Fact]
    public void RangeChanges_WhenSubscriptionDisposed_ShouldNotKeepSubscriberAlive()
    {
        var probe = CreateRangeSubscriberAndDispose();

        ForceFullGc();

        probe.WeakReference.IsAlive.Should().BeFalse();
        GC.KeepAlive(probe.Selector);
    }

    [Fact]
    public void RangeChanges_WhenSubscriptionDisposed_ShouldNotKeepSelectorAlive()
    {
        var weakReference = CreateSelectorWithDisposedRangeSubscription();

        ForceFullGc();

        weakReference.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void ValidationChanges_WhenSubscriptionDisposed_ShouldNotKeepSubscriberAlive()
    {
        var probe = CreateValidationSubscriberAndDispose();

        ForceFullGc();

        probe.WeakReference.IsAlive.Should().BeFalse();
        GC.KeepAlive(probe.Selector);
    }

    [Fact]
    public void ValidationChanges_WhenSubscriptionDisposed_ShouldNotKeepSelectorAlive()
    {
        var weakReference = CreateSelectorWithDisposedValidationSubscription();

        ForceFullGc();

        weakReference.IsAlive.Should().BeFalse();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RetentionProbe CreateRangeSubscriberAndDispose()
    {
        var selector = new Selector();
        var captured = new object();

        var subscription = selector.RangeChanges.Subscribe(_ => GC.KeepAlive(captured));
        subscription.Dispose();

        return new RetentionProbe(selector, new WeakReference(captured));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateSelectorWithDisposedRangeSubscription()
    {
        var selector = new Selector();

        var subscription = selector.RangeChanges.Subscribe(_ => { });
        subscription.Dispose();

        return new WeakReference(selector);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RetentionProbe CreateValidationSubscriberAndDispose()
    {
        var selector = new Selector();
        var captured = new object();

        var subscription = selector.ValidationChanges.Subscribe(_ => GC.KeepAlive(captured));
        subscription.Dispose();

        return new RetentionProbe(selector, new WeakReference(captured));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateSelectorWithDisposedValidationSubscription()
    {
        var selector = new Selector();

        var subscription = selector.ValidationChanges.Subscribe(_ => { });
        subscription.Dispose();

        return new WeakReference(selector);
    }

    private static void ForceFullGc()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed record RetentionProbe(Selector Selector, WeakReference WeakReference);
}
