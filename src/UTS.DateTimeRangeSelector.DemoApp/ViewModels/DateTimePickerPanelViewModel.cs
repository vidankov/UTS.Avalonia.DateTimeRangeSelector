using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace UTS.DateTimeRangeSelector.DemoApp.ViewModels;

public partial class DateTimePickerPanelViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "DateTimePickerPanel";
    public IScreen HostScreen { get; } = hostScreen;
    public ViewModelActivator Activator { get; } = new();

    [Reactive] private DateTime? _freeDateTime = DateTime.UtcNow;
    [Reactive] private DateTime? _boundedDateTime = DateTime.UtcNow;
    [Reactive] private DateTime? _disabledCalendarDateTime = DateTime.UtcNow;
    [Reactive] private DateTime? _dynamicDateTime = DateTime.UtcNow;
    [Reactive] private DateTime _dynamicMin = DateTime.UtcNow.AddDays(-7);
    [Reactive] private DateTime _dynamicMax = DateTime.UtcNow.AddDays(7).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

    public DateTime BoundedMin => DateTime.UtcNow.AddDays(-7);
    public DateTime BoundedMax => DateTime.UtcNow.AddDays(7).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
    public DateTime DisabledMin => DateTime.UtcNow.Date;
    public DateTime DisabledMax => DateTime.UtcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

    [ReactiveCommand] private void ChangeDynamicMin() => DynamicMin = DynamicMin.AddDays(1);
    [ReactiveCommand] private void ChangeDynamicMax() => DynamicMax = DynamicMax.AddDays(1);
}