using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using NSubstitute;
using UTS.DateTimeRangeSelector.Events;
using Selector = UTS.DateTimeRangeSelector.Controls.DateTimeRangeSelector;

namespace UTS.DateTimeRangeSelector.Tests.Controls;

/// <summary>
/// Proof-tests for design and logic issues found during the deep audit of the solution
/// (branch: fix/independent-calendar-template).
///
/// Every test in this file is EXPECTED TO FAIL on current production code.
/// They serve as living specifications: each one encodes exactly what "correct" looks like,
/// so when the corresponding fix lands, the test turns green without modification.
///
/// Test naming: Issue{N}_{Scenario}_{ExpectedOutcome}
/// </summary>
public class DateTimeRangeSelectorIssuesTests
{
    // -----------------------------------------------------------------------
    // Shared fixture
    // -----------------------------------------------------------------------

    private readonly Selector _selector;
    private readonly TimeProvider _timeProvider;

    // Fixed "now" so tests are deterministic.
    private static readonly DateTime Now = new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    public DateTimeRangeSelectorIssuesTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(Now));

        _selector = new Selector { TimeProvider = _timeProvider };
    }

    private static Selector CreateSelectorWithTemplate(TimeProvider timeProvider)
    {
        var selector = new Selector
        {
            TimeProvider = timeProvider,
            Template = new FuncControlTemplate<Selector>((_, _) => new Panel())
        };
        selector.ApplyTemplate();
        return selector;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ISSUE #1 — Unspecified-Kind values bypass UTC normalization in Coerce()
    //
    // Root cause (DateTimeRangeSelector.axaml.cs ~line 551-558):
    //   DateTime? from = ClampToBounds(FromDateTime);  // returns Kind=Utc, same Ticks
    //   if (from != FromDateTime)                       // DateTime.Equals ignores Kind →
    //       SetCurrentValue(...)                        // this is NEVER called
    //
    // ClampToBounds calls DateTimeNormalization.EnsureUtc which, for Unspecified kind,
    // calls DateTime.SpecifyKind(value, Utc). That keeps the ticks identical.
    // DateTime's equality operator compares only Ticks, so the guard fails and
    // SetCurrentValue is skipped. The stored value retains Kind=Unspecified.
    //
    // Violates: R2 ("values coerced to UTC after normalization").
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Issue1_SetFromDateTime_WithUnspecifiedKind_ShouldBeNormalizedToUtc()
    {
        // FAILS: stored value keeps Kind=Unspecified because the DateTime equality
        // check in Coerce() short-circuits before SetCurrentValue is called.
        var unspecified = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Unspecified);
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, unspecified);
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, unspecified.AddHours(1));

        _selector.FromDateTime!.Value.Kind.Should().Be(DateTimeKind.Utc,
            "R2 requires FromDateTime to be stored as UTC after coercion, " +
            "regardless of input Kind — Unspecified must become UTC, not be silently kept as-is");
    }

    [Fact]
    public void Issue1_SetToDateTime_WithUnspecifiedKind_ShouldBeNormalizedToUtc()
    {
        // Symmetric test for the ToDateTime path — same root cause.
        var unspecified = new DateTime(2025, 6, 15, 14, 0, 0, DateTimeKind.Unspecified);
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, unspecified.AddHours(-1));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, unspecified);

        _selector.ToDateTime!.Value.Kind.Should().Be(DateTimeKind.Utc,
            "ToDateTime must also be stored as UTC after coercion when the input Kind is Unspecified");
    }

    [Fact]
    public void Issue1_SetFromDateTime_WithUnspecifiedKind_IsValidShouldBeTrue_AfterSettingBothEndpoints()
    {
        // Secondary symptom: if the stored values are Unspecified, validation compares them
        // by ticks (which works), but downstream consumers and RangeChanges receive non-UTC values.
        // At minimum, verifying the IsValid check catches whether the coercion-gate failure
        // affects validation state.
        var unspecified = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Unspecified);
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, unspecified);
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, unspecified.AddHours(2));

        // Sanity: the range IS valid by value, regardless of Kind.
        _selector.IsValid.Should().BeTrue();

        // The real assertion: RangeChanges should report UTC values, not Unspecified.
        DateTimeRange? lastRange = null;
        using var _ = _selector.RangeChanges.Subscribe(r => lastRange = r);

        lastRange.Should().NotBeNull();
        lastRange!.From!.Value.Kind.Should().Be(DateTimeKind.Utc,
            "RangeChanges observable must emit UTC values — " +
            "Unspecified-Kind leak through the observable interface breaks the UTC contract for all subscribers");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ISSUE #2 — PresetCommand.Execute skips the positive-duration guard
    //             that ApplyPreset(TimeSpan) enforces.
    //
    // Root cause (DateTimeRangeSelector.axaml.cs ~line 286-303):
    //   The _applyPresetCommand lambda calls CalculateRangeFromAnchor directly,
    //   without checking duration <= 0. Calling ApplyPreset(TimeSpan.Zero) throws
    //   ArgumentOutOfRangeException; executing the same through the command does not.
    //
    // Violates: R14 ("ApplyPreset — only positive duration").
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Issue2_ApplyPresetCommand_WithZeroDuration_ShouldThrowLikeApplyPresetMethod()
    {
        // Verify the public method contract first (sanity — this SHOULD pass already).
        Action applyPresetDirect = () => _selector.ApplyPreset(TimeSpan.Zero);
        applyPresetDirect.Should().Throw<ArgumentOutOfRangeException>(
            "ApplyPreset(TimeSpan) already enforces positive duration; " +
            "this assertion just documents the expected contract");

        // Now prove the command violates that same contract.
        // FAILS: the command lambda has no duration guard → silently sets From == To == anchor.
        Action applyPresetViaCommand = () => _selector.ApplyPresetCommand.Execute(TimeSpan.Zero);
        applyPresetViaCommand.Should().Throw<ArgumentOutOfRangeException>(
            "ApplyPresetCommand.Execute must enforce the same positive-duration contract as " +
            "ApplyPreset(TimeSpan) — a zero TimeSpan passed as the command parameter must not " +
            "silently produce From == To == anchor");
    }

    [Fact]
    public void Issue2_ApplyPresetCommand_WithNegativeDuration_ShouldThrowLikeApplyPresetMethod()
    {
        // Same as above for negative duration — belt-and-suspenders.
        var negative = TimeSpan.FromHours(-3);

        Action applyPresetViaCommand = () => _selector.ApplyPresetCommand.Execute(negative);
        applyPresetViaCommand.Should().Throw<ArgumentOutOfRangeException>(
            "ApplyPresetCommand.Execute must reject negative durations just as ApplyPreset does");
    }

    [Fact]
    public void Issue2_ApplyPresetCommand_CanExecute_ShouldReturnFalseForNonPositiveTimeSpan()
    {
        // Even if Execute is not hardened, CanExecute should signal ineligibility.
        // FAILS: current CanExecute only checks ShowPresets and whether parameter is a TimeSpan;
        // it does not check whether the TimeSpan is positive.
        _selector.ApplyPresetCommand.CanExecute(TimeSpan.Zero).Should().BeFalse(
            "CanExecute must return false for TimeSpan.Zero — a zero-duration preset is meaningless " +
            "and consistent with the non-negative contract of ApplyPreset(TimeSpan)");

        _selector.ApplyPresetCommand.CanExecute(TimeSpan.FromSeconds(-1)).Should().BeFalse(
            "CanExecute must return false for negative TimeSpan");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ISSUE #3 — Requested preset duration is silently destroyed when
    //             MinDateTime > anchor (anchor = MaxDateTime ?? UtcNow).
    //
    // Root cause (DateTimeRangeSelector.axaml.cs ~line 441-450):
    //   var end   = GetAnchor();         // e.g. UtcNow = Now
    //   var start = end - duration;      // e.g. Now - 1h
    //   if (MinDateTime.HasValue && start < Min)
    //       start = Min;                 // Now + 5h — start > end
    //   // start is returned as-is; Coerce later clamps To UP to Min,
    //   // collapsing the range to [Min, Min] — zero duration.
    //
    // Preferred fix: anchor-shift forward — when Min > anchor, treat Min
    // as the new start and set end = start + duration.
    //
    // Same defect affects ApplyDefaultRange (the default 1-hour window).
    //
    // Violates: implicit contract that "Apply Last Nh" yields an Nh window.
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Issue3_ApplyPreset_WhenMinExceedsAnchor_ShouldPreserveRequestedDuration()
    {
        // Anchor = UtcNow = Now; MinDateTime = Now + 5h (future lower bound).
        // CalculateRangeFromAnchor: end=Now, start=Now-1h → clamped to Min=Now+5h.
        // start (Now+5h) > end (Now) → Coerce clamps To up to Min, result: [Min, Min].
        // FAILS: Duration == 0, not TimeSpan.FromHours(1).
        var futureMin = Now.AddHours(5);
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, futureMin);

        _selector.ApplyPreset(TimeSpan.FromHours(1));

        var duration = _selector.ToDateTime - _selector.FromDateTime;
        duration.Should().Be(TimeSpan.FromHours(1),
            "ApplyPreset('Last 1h') must always produce a 1-hour window; " +
            "when MinDateTime > anchor the range should be anchored at Min " +
            "(start=Min, end=Min+1h) rather than collapsing to [Min, Min] (zero duration)");
    }

    [Fact]
    public void Issue3_ResetToDefaults_WhenMinExceedsAnchor_ShouldPreserveDefaultOneHourWindow()
    {
        // Same defect in ApplyDefaultRange which also calls CalculateRangeFromAnchor.
        var futureMin = Now.AddHours(5);
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, futureMin);

        _selector.ResetToDefaults();

        var duration = _selector.ToDateTime - _selector.FromDateTime;
        duration.Should().Be(TimeSpan.FromHours(1),
            "ResetToDefaults must produce the default 1-hour window even when " +
            "MinDateTime lies ahead of UtcNow; the range should shift forward, not collapse");
    }

    [Fact]
    public void Issue3_ApplyPreset_WhenMinExceedsAnchor_FromAndToShouldBothBeValid()
    {
        // Even if duration is lost, From must be <= To after the operation.
        // Currently both collapse to Min, which means From == To (valid by comparison
        // but the range has zero duration, which is a silent data loss).
        var futureMin = Now.AddHours(5);
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, futureMin);

        _selector.ApplyPreset(TimeSpan.FromHours(2));

        _selector.FromDateTime.Should().NotBeNull();
        _selector.ToDateTime.Should().NotBeNull();
        _selector.FromDateTime!.Value.Should().BeOnOrBefore(_selector.ToDateTime!.Value);
        _selector.ToDateTime!.Value.Should().BeAfter(_selector.FromDateTime.Value,
            "after applying a non-zero preset the resulting To must be strictly greater than From; " +
            "From == To means the requested duration was discarded");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ISSUE #5 — Early subscribers must not see pre-initialization placeholders.
    //
    // Contract (Option B): ReplaySubject(1) starts empty; the first meaningful range
    // and validation are published after initialization (OnApplyTemplate or ResetToDefaults).
    // Early subscribers should receive exactly one initialized snapshot, not (null, null)
    // followed by the default range (R12).
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Issue5_RangeChanges_EarlySubscriber_ShouldNotReceiveStaleNullTuple()
    {
        // Use a fresh selector (not the shared one) to control the init sequence precisely.
        var selector = new Selector { TimeProvider = _timeProvider };
        var received = new List<DateTimeRange>();

        // Subscribe BEFORE any defaults are applied.
        using var sub = selector.RangeChanges.Subscribe(received.Add);

        // Trigger default application (simulates what OnApplyTemplate does).
        selector.ResetToDefaults();

        received.Should().NotContain(
            r => r.From == null && r.To == null,
            "early subscribers must not be exposed to the pre-initialisation (null, null) " +
            "placeholder; R12 states that the final initial range is emitted once, not twice " +
            "(once as null-null, then as the actual range)");
    }

    [Fact]
    public void Issue5_ValidationChanges_EarlySubscriber_ShouldNotReceiveStaleInvalidState()
    {
        var selector = new Selector { TimeProvider = _timeProvider };
        var received = new List<ValidationResult>();

        using var sub = selector.ValidationChanges.Subscribe(received.Add);

        // Trigger default application.
        selector.ResetToDefaults();

        received.Should().NotContain(
            r => !r.IsValid && r.Message == null,
            "early subscribers must not be exposed to the uninitialised (false, null) " +
            "validation state that the BehaviorSubject is seeded with at construction time");
    }

    [Fact]
    public void Issue5_RangeChanges_EarlySubscriberShouldReceiveExactlyOneInitialEmission()
    {
        var selector = new Selector { TimeProvider = _timeProvider };
        var received = new List<DateTimeRange>();

        using var sub = selector.RangeChanges.Subscribe(received.Add);
        selector.ResetToDefaults();

        received.Should().HaveCount(1,
            "R12 requires the initial range to be emitted exactly once after initialization");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ISSUE #6 — When Min > Max forces From/To to null, restoring valid
    //             bounds leaves From/To permanently null.
    //
    // Root cause (DateTimeRangeSelector.axaml.cs ~line 530-545):
    //   When Min > Max is detected, Coerce nulls both From and To.
    //   When Max is later corrected so that Min <= Max, Coerce re-runs but
    //   finds From=null and To=null — nothing to clamp or reorder.
    //   UpdateValidation sets IsValid=false ("Both From and To must be set").
    //   The user must manually re-enter dates even though they had valid ones before.
    //
    // Preferred fix: do not null From/To on contradictory bounds; only set
    // AreBoundsValid=false and IsValid=false. The template already uses
    // AreBoundsValid to disable the UI (0.0.9 change), so nulling is redundant
    // and destructive.
    //
    // Violates: principle of least surprise and the 0.0.9 design intent
    // (AreBoundsValid drives template IsEnabled, not From/To destruction).
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Issue6_AfterBoundsBecomeValidAgain_RangeShouldNotRemainNull()
    {
        // Arrange: establish a valid range.
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-1));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        // Act 1: make bounds contradictory — From/To get nulled.
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(2));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);
        _selector.FromDateTime.Should().BeNull("precondition: values are currently cleared");

        // Act 2: restore valid bounds.
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now.AddHours(5));

        // FAILS: From/To remain null even though bounds are now valid.
        _selector.FromDateTime.Should().NotBeNull(
            "once Min <= Max is restored, the control must re-apply defaults or recover a usable " +
            "range; permanently null From/To after transient contradictory bounds is destructive");
        _selector.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Issue6_TransientMinExceedsMax_ShouldNotPermanentlyWipeUserRange()
    {
        // Variation: set a range, briefly violate bounds, then restore — confirm range recovers.
        var originalFrom = Now.AddHours(-3);
        var originalTo = Now;

        _selector.SetCurrentValue(Selector.FromDateTimeProperty, originalFrom);
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, originalTo);

        // Brief contradiction.
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(10));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now.AddHours(5));

        // Restore sensible bounds.
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, null);
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, null);

        // FAILS: From/To remain null — user data is gone permanently.
        _selector.FromDateTime.Should().NotBeNull(
            "removing contradictory bounds should allow the control to recover; " +
            "once Min and Max are both null (unbounded), the previously set range should " +
            "be restored or defaults applied — it must not stay permanently empty");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ISSUE #7 — OldFrom / OldTo in RangeChangedEventArgs may not be UTC
    //             when the previous value was stored as Unspecified Kind
    //             (downstream consequence of Issue #1).
    //
    // Root cause (DateTimeRangeSelector.axaml.cs ~line 481-484):
    //   oldFrom = change.GetOldValue<DateTime?>();
    //   // This returns the raw stored value BEFORE the current change.
    //   // If Bug #1 prevented normalization of the previous value,
    //   // the stored value is still Unspecified → oldFrom.Kind == Unspecified.
    //
    // Violates: R2 and the expectation that all DateTime values flowing through
    // the control's public surface are UTC.
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Issue7_RangeChanged_OldFrom_ShouldBeUtc_WhenPreviousValueWasUnspecifiedKind()
    {
        // Step 1: set From with Unspecified kind at 09:00.
        // Bug #1 means this value is stored AS Unspecified (not normalized).
        // IMPORTANT: use 09:00, NOT 10:00 = Now-2h, so that the subsequent
        // change to 10:00 UTC has DIFFERENT ticks and the event fires.
        // (If ticks were the same, Bug #1 causes a compound effect where the
        // change is silently swallowed — see Issue1_CannotUpdateSameTicks_FromUnspecifiedToUtc.)
        var unspecifiedFrom = new DateTime(2025, 6, 15, 9, 0, 0, DateTimeKind.Unspecified);
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, unspecifiedFrom);
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        DateTimeRangeChangedEventArgs? captured = null;
        _selector.RangeChanged += (_, e) => captured = e;

        // Step 2: change From to a UTC value with different ticks (10:00 UTC ≠ 09:00).
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-2));

        captured.Should().NotBeNull(
            "changing From from 09:00 to 10:00 (different ticks) must fire RangeChanged");
        captured!.OldFrom.Should().NotBeNull();

        // FAILS: OldFrom.Kind == Unspecified because Bug #1 left it un-normalized.
        captured.OldFrom!.Value.Kind.Should().Be(DateTimeKind.Utc,
            "OldFrom in RangeChangedEventArgs must be UTC; if the previous value was stored as " +
            "Unspecified (due to Issue #1's coercion gate failure), the event leaks a non-UTC " +
            "value that violates R2 for every subscriber of RangeChanged");
    }

    [Fact]
    public void Issue7_RangeChanges_Observable_ShouldEmitUtcValues_WhenPreviousValueWasUnspecifiedKind()
    {
        // Mirror of the above but for the RangeChanges observable path.
        var selector = new Selector { TimeProvider = _timeProvider };

        var unspecifiedFrom = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Unspecified);
        selector.SetCurrentValue(Selector.FromDateTimeProperty, unspecifiedFrom);
        selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        DateTimeRange? latestRange = null;
        using var sub = selector.RangeChanges.Subscribe(r => latestRange = r);

        // Trigger a range change so the observable emits the latest snapshot.
        selector.SetCurrentValue(Selector.ToDateTimeProperty, Now.AddMinutes(30));

        latestRange.Should().NotBeNull();

        // FAILS: latestRange.From.Kind == Unspecified.
        latestRange!.From!.Value.Kind.Should().Be(DateTimeKind.Utc,
            "RangeChanges observable must always carry UTC From values; " +
            "Unspecified Kind in the snapshot means the coercion contract is broken end-to-end");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // R26-5 — AreBoundsValid true while IsValid false after contradictory bounds restored
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void R26_5_AfterBoundsRestored_AreBoundsValidTrue_WhileIsValidFalse_AndValidationMessageNonNull()
    {
        _selector.SetCurrentValue(Selector.FromDateTimeProperty, Now.AddHours(-1));
        _selector.SetCurrentValue(Selector.ToDateTimeProperty, Now);

        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddHours(2));
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now);

        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now.AddHours(5));

        _selector.AreBoundsValid.Should().BeTrue(
            "valid bounds must re-enable the template via AreBoundsValid");
        _selector.IsValid.Should().BeFalse(
            "From/To were wiped and not recovered — control must not report valid");
        _selector.ValidationMessage.Should().NotBeNullOrEmpty(
            "callers need a validation message when the range is empty but bounds are valid");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // R26-7 — Preset duration silently shortened when clamp eats into requested window
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void R26_7_ApplyPreset_WhenStartClampsToMin_DurationShouldBePreservedOrInvalidated()
    {
        _selector.SetCurrentValue(Selector.MinDateTimeProperty, Now.AddMinutes(-30));

        _selector.ApplyPreset(TimeSpan.FromHours(1));

        var duration = _selector.ToDateTime - _selector.FromDateTime;
        var isValidWithMessage = !_selector.IsValid && !string.IsNullOrEmpty(_selector.ValidationMessage);

        (duration == TimeSpan.FromHours(1) || isValidWithMessage).Should().BeTrue(
            "ApplyPreset('Last 1h') must produce a 1-hour window or explicit invalid state; " +
            "must not silently shrink to 30 minutes when Min is anchor - 30min");
    }

    [Fact]
    public void R26_7_ApplyPreset_WhenEndClampsToMax_DurationShouldBePreservedOrInvalidated()
    {
        _selector.SetCurrentValue(Selector.MaxDateTimeProperty, Now.AddMinutes(-15));

        _selector.ApplyPreset(TimeSpan.FromHours(1));

        var duration = _selector.ToDateTime - _selector.FromDateTime;
        var isValidWithMessage = !_selector.IsValid && !string.IsNullOrEmpty(_selector.ValidationMessage);

        (duration == TimeSpan.FromHours(1) || isValidWithMessage).Should().BeTrue(
            "ApplyPreset('Last 1h') must produce a 1-hour window or explicit invalid state; " +
            "must not silently shrink when Max narrows the anchor");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // R26-8 — Replay after initialization (Option B), not at construction
    // ═══════════════════════════════════════════════════════════════════════

    [AvaloniaFact]
    public void R26_8_RangeChanges_AfterTemplateApplied_ShouldReplayCurrentRange()
    {
        var selector = CreateSelectorWithTemplate(_timeProvider);
        var received = new List<DateTimeRange>();

        using var _ = selector.RangeChanges.Subscribe(received.Add);

        received.Should().NotBeEmpty(
            "RangeChanges must replay the latest range after template initialization");
        received[0].From.Should().NotBeNull();
        received[0].To.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void R26_8_ValidationChanges_AfterTemplateApplied_ShouldReplayCurrentValidation()
    {
        var selector = CreateSelectorWithTemplate(_timeProvider);
        var received = new List<ValidationResult>();

        using var _ = selector.ValidationChanges.Subscribe(received.Add);

        received.Should().NotBeEmpty(
            "ValidationChanges must replay the latest validation state after template initialization");
    }

    [AvaloniaFact]
    public void R26_8_RangeChanges_SubscribeBeforeTemplate_ShouldEmitOneInitializedRange()
    {
        var selector = new Selector
        {
            TimeProvider = _timeProvider,
            Template = new FuncControlTemplate<Selector>((_, _) => new Panel())
        };
        var received = new List<DateTimeRange>();
        using var sub = selector.RangeChanges.Subscribe(received.Add);

        selector.ApplyTemplate();

        received.Should().HaveCount(1,
            "initialization should emit the final initial range exactly once");
        received[0].From.Should().NotBeNull();
        received[0].To.Should().NotBeNull();
        received.Should().NotContain(r => r.From == null && r.To == null);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // R26-12 — ShowPresets gates CanExecute but not ApplyPreset (design asymmetry)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "DesignDecision")]
    public void R26_12_ShowPresetsFalse_CanExecuteReturnsFalse_ButApplyPresetStillApplies()
    {
        _selector.ShowPresets = false;
        var before = _selector.FromDateTime;

        _selector.ApplyPresetCommand.CanExecute(TimeSpan.FromHours(1)).Should().BeFalse(
            "command CanExecute is gated on ShowPresets");

        var act = () => _selector.ApplyPreset(TimeSpan.FromHours(1));
        act.Should().NotThrow("ApplyPreset is not gated on ShowPresets in current API");

        _selector.FromDateTime.Should().NotBe(before,
            "documents asymmetry: public ApplyPreset still mutates range when presets are hidden");
    }
}
