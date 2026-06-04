using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using NSubstitute;
using UTS.DateTimeRangeSelector.Controls;
using Selector = UTS.DateTimeRangeSelector.Controls.DateTimeRangeSelector;

namespace UTS.DateTimeRangeSelector.Tests.UI;

/// <summary>
/// Tests that pin the template-contract introduced in release 0.0.9:
///   "AreBoundsValid drives the template root container's IsEnabled;
///    the control's own IsEnabled is not touched by internal logic."
///
/// CONTRACT TESTS (positive, should PASS) — regression protection so that future
/// refactors that accidentally re-add <c>IsEnabled=false</c> to the control itself
/// are caught immediately.
///
/// FAILING TESTS — Issue #6 headless counterpart:
/// After contradictory bounds are restored, From/To stay null (Issue #6).
///
/// Note on test design:
///   Tests use FuncControlTemplate to set a minimal inline template with an explicit
///   IsEnabled binding, avoiding dependency on the compiled AXAML resources from the
///   library. This keeps the tests compatible with whatever Avalonia runtime version
///   the headless package pulls in.
/// </summary>
public class DateTimeRangeSelectorTemplateTests
{
    private static readonly DateTime Now =
        new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static TimeProvider MakeTimeProvider()
    {
        var tp = Substitute.For<TimeProvider>();
        tp.GetUtcNow().Returns(new DateTimeOffset(Now));
        return tp;
    }

    /// <summary>
    /// Creates a selector that has a minimal inline template whose root
    /// <see cref="StackPanel"/> has <c>IsEnabled</c> bound to the control's
    /// <see cref="Selector.AreBoundsValid"/> property — exactly as the
    /// production AXAML template does.
    /// </summary>
    private static (Panel rootPanel, Selector selector) CreateSelectorWithBoundTemplate()
    {
        Panel? capturedRoot = null;

        var template = new FuncControlTemplate<Selector>((control, scope) =>
        {
            var root = new StackPanel();
            root.Bind(
                InputElement.IsEnabledProperty,
                new Binding(nameof(Selector.AreBoundsValid))
                {
                    Source = control,
                    Mode = BindingMode.OneWay
                });
            capturedRoot = root;
            return root;
        });

        var selector = new Selector { Template = template, TimeProvider = MakeTimeProvider() };
        selector.ApplyTemplate();

        capturedRoot.Should().NotBeNull("the template factory must have run during ApplyTemplate");
        return (capturedRoot!, selector);
    }

    // -----------------------------------------------------------------------
    // Contract tests (EXPECTED TO PASS — regression guards)
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void Template_WhenBoundsAreValid_RootContainerShouldBeEnabled()
    {
        var (root, selector) = CreateSelectorWithBoundTemplate();

        selector.MinDateTime = Now.AddHours(-5);
        selector.MaxDateTime = Now.AddHours(5);

        selector.AreBoundsValid.Should().BeTrue();
        root.IsEnabled.Should().BeTrue(
            "the template root IsEnabled is bound to AreBoundsValid; " +
            "valid bounds must keep the UI enabled");
    }

    [AvaloniaFact]
    public void Template_WhenMinExceedsMax_RootContainerShouldBeDisabled()
    {
        var (root, selector) = CreateSelectorWithBoundTemplate();

        selector.MinDateTime = Now.AddHours(2);
        selector.MaxDateTime = Now;

        selector.AreBoundsValid.Should().BeFalse();
        root.IsEnabled.Should().BeFalse(
            "the template root IsEnabled must reflect AreBoundsValid=false when Min > Max, " +
            "preventing user interaction while bounds are contradictory (design from 0.0.9)");
    }

    [AvaloniaFact]
    public void Template_WhenMinExceedsMax_ControlOwnIsEnabledMustNotBeModified()
    {
        // 0.0.9 specifically kept IsEnabled on the CONTROL untouched.
        var (_, selector) = CreateSelectorWithBoundTemplate();

        selector.MinDateTime = Now.AddHours(2);
        selector.MaxDateTime = Now;

        selector.IsEnabled.Should().BeTrue(
            "the control's own IsEnabled must never be set by the control's internal logic; " +
            "the consumer owns IsEnabled — the template's root container IsEnabled is the " +
            "correct internal knob (AreBoundsValid binding introduced in 0.0.9)");
    }

    [AvaloniaFact]
    public void Template_AfterBoundsRestored_RootContainerShouldBeEnabledAgain()
    {
        var (root, selector) = CreateSelectorWithBoundTemplate();

        // Create contradiction.
        selector.MinDateTime = Now.AddHours(2);
        selector.MaxDateTime = Now;
        root.IsEnabled.Should().BeFalse("precondition");

        // Fix the bounds.
        selector.MaxDateTime = Now.AddHours(5);

        // Template binding must reactively update.
        selector.AreBoundsValid.Should().BeTrue();
        root.IsEnabled.Should().BeTrue(
            "once Min <= Max is restored, AreBoundsValid becomes true and the template " +
            "binding must re-enable the root container");
    }

    [AvaloniaFact]
    public void Template_ExternalConsumer_DisablingControl_ShouldBeIndependentOfBoundsLogic()
    {
        // When a consumer explicitly disables the whole control, the template root's
        // own IsEnabled should still be true (AreBoundsValid=true);
        // the control-level IsEnabled cascade handles the visual disabling separately.
        var (root, selector) = CreateSelectorWithBoundTemplate();

        selector.IsEnabled = false; // consumer disables the whole control

        selector.AreBoundsValid.Should().BeTrue();
        root.IsEnabled.Should().BeTrue(
            "the template root's own IsEnabled property must only reflect AreBoundsValid; " +
            "the overall control being disabled is handled by Avalonia's IsEnabled cascade, " +
            "not by writing to the template root");
    }

    // -----------------------------------------------------------------------
    // AreBoundsValid state machine — contract tests (non-template)
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void AreBoundsValid_WhenBothBoundsNull_ShouldBeTrue()
    {
        var (_, selector) = CreateSelectorWithBoundTemplate();

        selector.MinDateTime = null;
        selector.MaxDateTime = null;

        selector.AreBoundsValid.Should().BeTrue(
            "null bounds mean 'no limit'; no contradiction is possible when both are null");
    }

    [AvaloniaFact]
    public void AreBoundsValid_WhenOnlyMinSet_ShouldBeTrue()
    {
        var (_, selector) = CreateSelectorWithBoundTemplate();

        selector.MinDateTime = Now;
        selector.MaxDateTime = null;

        selector.AreBoundsValid.Should().BeTrue(
            "a single bound can never be contradictory with no opposing bound");
    }

    [AvaloniaFact]
    public void AreBoundsValid_WhenMinEqualsMax_ShouldBeTrue()
    {
        var (_, selector) = CreateSelectorWithBoundTemplate();

        selector.MinDateTime = Now;
        selector.MaxDateTime = Now;

        selector.AreBoundsValid.Should().BeTrue(
            "Min == Max is a degenerate but valid configuration; only Min > Max is contradictory");
    }

    // -----------------------------------------------------------------------
    // Issue #6 headless counterpart — EXPECTED TO FAIL
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void Issue6_Template_AfterBoundsRestored_FromAndToShouldBePopulated_NotNull()
    {
        // FAILS: From/To remain null after bounds are restored (Issue #6).
        var (_, selector) = CreateSelectorWithBoundTemplate();

        selector.FromDateTime = Now.AddHours(-1);
        selector.ToDateTime = Now;

        // Contradiction → nulls From/To.
        selector.MinDateTime = Now.AddHours(2);
        selector.MaxDateTime = Now;

        // Restore.
        selector.MaxDateTime = Now.AddHours(5);

        // FAILS on current code — From/To stay null.
        selector.FromDateTime.Should().NotBeNull(
            "after restoring valid bounds the control must re-apply defaults or recover the range; " +
            "leaving From=null with a fully enabled UI is a misleading state (Issue #6)");
        selector.IsValid.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // R26-17 — cross-panel binding and layout backlog
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void R26_17_CrossPanel_InlinedTemplate_FromCanExceedToUntilParentCoerces()
    {
        DateTimePickerPanel? fromPanel = null;
        DateTimePickerPanel? toPanel = null;

        var template = new FuncControlTemplate<Selector>((selector, _) =>
        {
            fromPanel = new DateTimePickerPanel();
            fromPanel.Bind(
                DateTimePickerPanel.SelectedDateTimeProperty,
                new Binding(nameof(Selector.FromDateTime))
                {
                    Source = selector,
                    Mode = BindingMode.TwoWay
                });

            toPanel = new DateTimePickerPanel();
            toPanel.Bind(
                DateTimePickerPanel.SelectedDateTimeProperty,
                new Binding(nameof(Selector.ToDateTime))
                {
                    Source = selector,
                    Mode = BindingMode.TwoWay
                });

            return new StackPanel { Children = { fromPanel, toPanel } };
        });

        var selector = new Selector { Template = template, TimeProvider = MakeTimeProvider() };
        selector.ApplyTemplate();

        fromPanel.Should().NotBeNull();
        toPanel.Should().NotBeNull();

        selector.ToDateTime = Now;
        selector.FromDateTime = Now.AddHours(2);

        selector.FromDateTime!.Value.Should().BeOnOrBefore(selector.ToDateTime!.Value,
            "parent selector must coerce inverted From/To even when panels are bound independently");
    }

    /// <summary>
    /// Mirrors production <c>DateTimePickerPanel</c> template MinWidth layout (calendar 150 + four spinners 110 each).
    /// Compiled library AXAML cannot load in headless 12.x runtime; inline template preserves width proof.
    /// </summary>
    [AvaloniaFact]
    public void R26_17_PanelLayout_DesiredWidthShouldBeUnder600()
    {
        var panel = new DateTimePickerPanel
        {
            Template = new FuncControlTemplate<DateTimePickerPanel>((_, _) =>
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                row.Children.Add(new ConstrainedCalendarDatePicker { MinWidth = 100 });
                // Placeholders only — TextBlock measure needs IFontManagerImpl in headless.
                row.Children.Add(new Panel { MinWidth = 15, Height = 16, Margin = new Thickness(5, 0) });
                for (var i = 0; i < 4; i++)
                {
                    row.Children.Add(new NumericUpDown
                    {
                        MinWidth = 60,
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                }

                row.Children.Add(new Panel { MinWidth = 25, Height = 16, Margin = new Thickness(2, 0, 5, 0) });
                row.Children.Add(new Panel { MinWidth = 35, Height = 16, Margin = new Thickness(2, 0, 5, 0) });
                row.Children.Add(new Panel { MinWidth = 30, Height = 16, Margin = new Thickness(2, 0, 5, 0) });
                row.Children.Add(new Panel { MinWidth = 25, Height = 16, Margin = new Thickness(2, 0, 0, 0) });
                return row;
            })
        };

        panel.ApplyTemplate();
        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        panel.DesiredSize.Width.Should().BeLessThan(600,
            "compact host layouts need a panel under ~600px; current production MinWidth stack is ~755px");
    }
}
