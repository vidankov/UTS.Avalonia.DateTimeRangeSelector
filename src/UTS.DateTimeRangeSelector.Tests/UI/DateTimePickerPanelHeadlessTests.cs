using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using UTS.DateTimeRangeSelector.Controls;
using UTS.DateTimeRangeSelector.Events;

namespace UTS.DateTimeRangeSelector.Tests.UI;

/// <summary>
/// Headless proof-tests for Issue #4:
/// <see cref="DateTimePickerPanel.OnApplyTemplate"/> unconditionally re-fires
/// <see cref="DateTimePickerPanel.SelectedDateTimeChanged"/> with OldValue=null every
/// time the template is (re-)applied, even when <see cref="DateTimePickerPanel.SelectedDateTime"/>
/// has not changed.
///
/// Every test marked with "FAILS" is EXPECTED TO FAIL on current production code.
///
/// Root cause (DateTimePickerPanel.axaml.cs ~line 474-484):
///   protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
///   {
///       base.OnApplyTemplate(e);
///       _calendar = e.NameScope.Find&lt;ConstrainedCalendarDatePicker&gt;("PART_Calendar");
///       if (SelectedDateTime.HasValue)
///           OnSelectedDateTimeChanged(null, SelectedDateTime.Value);  // ← always null OldValue
///   }
///
/// Impact:
///   (a) Subscribers who attached before the control was shown receive a synthetic
///       "null → value" event that looks like a fresh selection, even though the value
///       was set before template application.
///   (b) Any re-templating (theme switch, virtualization, window re-show) fires the
///       event again with the same null OldValue, creating misleading duplicates.
///
/// Violates: R12 ("avoid duplicating observable / event emissions on initialisation").
///
/// Note on test design:
///   A <see cref="FuncControlTemplate{T}"/> is used instead of loading the library's
///   compiled AXAML styles so these tests remain independent of the Avalonia version
///   used to compile the library resources.
/// </summary>
public class DateTimePickerPanelHeadlessTests
{
    private static readonly DateTime UtcValue =
        new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Creates a <see cref="DateTimePickerPanel"/> with a minimal inline template
    /// that contains a <see cref="Panel"/> root. PART_Calendar is absent intentionally —
    /// the test targets <see cref="DateTimePickerPanel.OnApplyTemplate"/> behavior only.
    /// </summary>
    private static DateTimePickerPanel CreatePanelWithMinimalTemplate()
    {
        var panel = new DateTimePickerPanel
        {
            Template = new FuncControlTemplate<DateTimePickerPanel>(
                (_, _) => new Panel())
        };
        return panel;
    }

    // -----------------------------------------------------------------------
    // Issue #4-A: Event fires with OldValue=null when value was pre-set
    //             before the control's template was applied.
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void Issue4_OnApplyTemplate_WhenValueIsPresetBeforeApply_EventFiredWithNullOldValue()
    {
        // Arrange: value set BEFORE the template is applied.
        var panel = CreatePanelWithMinimalTemplate();
        panel.SelectedDateTime = UtcValue;

        SelectedDateTimeChangedEventArgs? captured = null;
        panel.SelectedDateTimeChanged += (_, e) => captured = e;

        // Act: apply the template manually — triggers OnApplyTemplate.
        panel.ApplyTemplate();

        captured.Should().BeNull(
            "OnApplyTemplate must NOT fire SelectedDateTimeChanged when the value has not changed; " +
            "a pre-set value should be synchronised to components without raising a change notification");
    }

    [AvaloniaFact]
    public void Issue4_OnApplyTemplate_WithPresetValue_OldValueShouldEqualNewValue_OrEventNotFired()
    {
        // Preferred semantics: if SelectedDateTime did not change during template application,
        // either (a) the event must not fire at all, or (b) OldValue == NewValue so
        // subscribers can detect the no-op.
        var panel = CreatePanelWithMinimalTemplate();
        panel.SelectedDateTime = UtcValue;

        SelectedDateTimeChangedEventArgs? captured = null;
        panel.SelectedDateTimeChanged += (_, e) => captured = e;

        panel.ApplyTemplate();

        if (captured is not null)
        {
            // If the event fired, OldValue must equal NewValue (no actual change).
            // FAILS: captured.OldValue is null, not UtcValue.
            captured.OldValue.Should().Be(captured.NewValue,
                "if the event fires during template application without a real value change, " +
                "OldValue must equal NewValue; OldValue=null looks like a reset from empty, " +
                "which is factually incorrect");
        }
        // (If the event did not fire, the test passes — that is the preferred fix.)
    }

    // -----------------------------------------------------------------------
    // Issue #4-B: Re-applying the template re-fires the event with null OldValue
    //             even though SelectedDateTime never changed.
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void Issue4_ReapplyingTemplate_ShouldNotRaiseSpuriousSelectedDateTimeChanged()
    {
        // Arrange: apply template once (first application — event may fire here).
        var panel = CreatePanelWithMinimalTemplate();
        panel.SelectedDateTime = UtcValue;
        panel.ApplyTemplate(); // first apply

        // Count events fired AFTER the first template application.
        int eventCount = 0;
        panel.SelectedDateTimeChanged += (_, _) => eventCount++;

        // Act: force a second template application by changing then restoring the template.
        var original = panel.Template;
        panel.Template = new FuncControlTemplate<DateTimePickerPanel>((_, _) => new Panel());
        panel.ApplyTemplate(); // second apply — value unchanged

        // FAILS: eventCount == 1 because OnApplyTemplate fires again with null OldValue.
        eventCount.Should().Be(0,
            "re-applying the template without changing SelectedDateTime must not re-raise " +
            "SelectedDateTimeChanged; the value has not changed — only the template was " +
            "reapplied; each spurious event with OldValue=null creates false 'selection " +
            "changed' notifications (Issue #4-B)");
    }

    [AvaloniaFact]
    public void Issue4_ReapplyingTemplate_TotalEventCount_ShouldNotIncrease()
    {
        // Variant: track total event count across the full lifecycle.
        var panel = CreatePanelWithMinimalTemplate();
        panel.SelectedDateTime = UtcValue;

        int totalEvents = 0;
        panel.SelectedDateTimeChanged += (_, _) => totalEvents++;

        panel.ApplyTemplate(); // first apply
        int eventsAfterFirstApply = totalEvents; // capture baseline

        // Re-apply without value change.
        panel.Template = new FuncControlTemplate<DateTimePickerPanel>((_, _) => new Panel());
        panel.ApplyTemplate(); // second apply

        // FAILS: totalEvents > eventsAfterFirstApply (one more event per re-apply).
        totalEvents.Should().Be(eventsAfterFirstApply,
            "re-applying the template without changing SelectedDateTime must not increase " +
            "the total event count; each extra fire duplicates the notification with OldValue=null");
    }

    // -----------------------------------------------------------------------
    // Positive control: the event SHOULD fire exactly once when the value
    // is set programmatically AFTER the template is applied.
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void Issue4_SettingValueAfterTemplateApply_ShouldFireEventExactlyOnce()
    {
        // Sanity / regression guard: the event must still fire for real value changes.
        var panel = CreatePanelWithMinimalTemplate();
        panel.ApplyTemplate(); // template applied, SelectedDateTime is null — no event expected

        int eventCount = 0;
        panel.SelectedDateTimeChanged += (_, _) => eventCount++;

        // Real value change after template is applied.
        panel.SelectedDateTime = UtcValue;

        eventCount.Should().Be(1, "a real value change must raise the event exactly once");
    }

    [AvaloniaFact]
    public void Issue4_SettingValueBeforeTemplateApply_ThenSettingAgainAfter_ShouldFireTwice()
    {
        // Two distinct changes must produce two events, regardless of template timing.
        var panel = CreatePanelWithMinimalTemplate();
        panel.SelectedDateTime = UtcValue; // before template

        int eventCount = 0;
        panel.SelectedDateTimeChanged += (_, _) => eventCount++;

        panel.ApplyTemplate(); // template applies — event may fire here (spurious)
        int countAfterApply = eventCount; // save how many happened during apply

        // A real change AFTER template application.
        panel.SelectedDateTime = UtcValue.AddHours(1);

        // Regardless of spurious events during ApplyTemplate, the post-apply real change
        // must produce exactly one more event.
        (eventCount - countAfterApply).Should().Be(1,
            "a real value change after template application must always fire the event exactly " +
            "once, even if the template application itself caused spurious events");
    }
}
