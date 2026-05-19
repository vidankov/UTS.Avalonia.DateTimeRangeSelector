using Avalonia;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Selector = UTS.DateTimeRangeSelector.Controls.DateTimeRangeSelector;

namespace UTS.DateTimeRangeSelector.Tests.UI;

public class DateTimeRangeSelectorTests
{
    [AvaloniaFact]
    public void IsEnabled_ExternalBinding_ShouldNotBeOverriddenByCoercion()
    {
        // Arrange
        var selector = new Selector();
        var source = new TestBindingSource { IsEnabled = true };

        // Устанавливаем внешнюю привязку
        selector.Bind(Selector.IsEnabledProperty,
            new Binding(nameof(TestBindingSource.IsEnabled))
            {
                Source = source,
                Mode = BindingMode.OneWay
            });

        // Исходное значение через привязку
        Assert.True(selector.IsEnabled);

        // Act: устанавливаем противоречивые границы (Min > Max)
        // Это вызовет UpdateValidation и в текущем коде установит IsEnabled = false
        selector.MinDateTime = DateTime.MaxValue;
        selector.MaxDateTime = DateTime.MinValue;

        // Пытаемся изменить внешнее свойство
        source.IsEnabled = true;

        // Assert: привязка должна была сохраниться и контрол должен быть Enabled.
        // Но если контрол перезаписал IsEnabled локально, привязка сброшена,
        // и изменение источника не дойдёт до контрола.
        Assert.True(selector.IsEnabled);
    }

    private class TestBindingSource : AvaloniaObject
    {
        public static readonly StyledProperty<bool> IsEnabledProperty =
            AvaloniaProperty.Register<TestBindingSource, bool>(nameof(IsEnabled), true);

        public bool IsEnabled
        {
            get => GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }
    }
}