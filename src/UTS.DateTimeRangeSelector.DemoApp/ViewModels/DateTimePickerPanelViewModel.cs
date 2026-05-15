using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;

namespace UTS.DateTimeRangeSelector.DemoApp.ViewModels;

public partial class DateTimePickerPanelViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
{
    private const int MaxLogEntries = 10;
    public string? UrlPathSegment => "DateTimePickerPanel";
    public IScreen HostScreen { get; } = hostScreen;
    public ViewModelActivator Activator { get; } = new();

    [Reactive] private DateTime? _freeDateTime = DateTime.UtcNow;
    [Reactive] private DateTime? _boundedDateTime = DateTime.UtcNow;
    [Reactive] private DateTime? _disabledCalendarDateTime = DateTime.UtcNow;
    [Reactive] private DateTime? _dynamicDateTime = DateTime.UtcNow;
    [Reactive] private DateTime _dynamicMin = DateTime.UtcNow.AddDays(-7);
    [Reactive] private DateTime _dynamicMax = DateTime.UtcNow.AddDays(7).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
    public ObservableCollection<string> EventLog { get; } = [];
    public DateTime BoundedMin => DateTime.UtcNow.AddDays(-7);
    public DateTime BoundedMax => DateTime.UtcNow.AddDays(7).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
    public DateTime DisabledMin => DateTime.UtcNow.Date;
    public DateTime DisabledMax => DateTime.UtcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

    [ReactiveCommand] private void ChangeDynamicMin() => DynamicMin = DynamicMin.AddDays(1);
    [ReactiveCommand] private void ChangeDynamicMax() => DynamicMax = DynamicMax.AddDays(1);
    [ReactiveCommand] private void ClearLogs() => EventLog.Clear();
    public void AddEventLog(string message) => AddLog(EventLog, message);
    public void AddLog(ObservableCollection<string> log, string message)
    {
        log.Add(message);
        while (log.Count > MaxLogEntries)
        {
            log.RemoveAt(0);
        }
    }
}