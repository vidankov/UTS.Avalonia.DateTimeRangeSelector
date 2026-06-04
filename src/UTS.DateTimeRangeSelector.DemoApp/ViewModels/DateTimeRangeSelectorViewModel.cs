using Avalonia.Layout;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using UTS.DateTimeRangeSelector.Core;

namespace UTS.DateTimeRangeSelector.DemoApp.ViewModels;

public partial class DateTimeRangeSelectorViewModel : ReactiveObject, IRoutableViewModel
{
    private const int MaxLogEntries = 10;
    private readonly CompositeDisposable _disposables = [];

    public string? UrlPathSegment => "DateTimeRangeSelector";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; } = new();

    public ObservableCollection<PresetItem> Presets { get; } = [];
    public ObservableCollection<string> EventLog { get; } = [];
    public ObservableCollection<string> ObservableLog { get; } = [];

    [Reactive] private DateTime? _rangeFrom = DateTime.UtcNow.AddDays(-1);
    [Reactive] private DateTime? _rangeTo = DateTime.UtcNow.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
    [Reactive] private DateTime? _rangeMin = DateTime.UtcNow.Date.AddDays(-7).AddHours(-5).AddMinutes(-30).AddSeconds(-30).AddMilliseconds(-500);
    [Reactive] private DateTime? _rangeMax = DateTime.UtcNow.Date.AddDays(7).AddHours(10).AddMinutes(25).AddSeconds(45).AddMilliseconds(777);
    [Reactive] private Orientation _orientation = Orientation.Vertical;
    [Reactive] private bool _showPresets = false;
    [Reactive] private bool _eventLogVisible = true;
    [Reactive] private bool _observableLogVisible = false;
    [Reactive] private DateTimeFormatModel _demoFormat = DateTimeFormatModel.Default;

    public DateTimeRangeSelectorViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;

        Presets.Add(new PresetItem("Последний час", TimeSpan.FromHours(1)));
        Presets.Add(new PresetItem("Последние сутки", TimeSpan.FromDays(1)));

        this.WhenAnyValue(
            vm => vm.RangeMin,
            vm => vm.RangeMax,
            (min, max) => min.HasValue && max.HasValue)
            .Where(hasLimits => hasLimits)
            .Subscribe(_ => UpdateFullRangePreset())
            .DisposeWith(_disposables);

        Observable.Merge(
            IncreaseMinCommand,
            DecreaseMinCommand,
            IncreaseMaxCommand,
            DecreaseMaxCommand,
            SimulateBoundsChangeCommand)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => UpdateFullRangePreset())
            .DisposeWith(_disposables);
    }

    [ReactiveCommand]
    private void ToggleOrientation() =>
        Orientation = Orientation == Orientation.Vertical
            ? Orientation.Horizontal : Orientation.Vertical;
    [ReactiveCommand] private void IncreaseMin() => RangeMin = RangeMin?.AddDays(1);
    [ReactiveCommand] private void DecreaseMin() => RangeMin = RangeMin?.AddDays(-1);
    [ReactiveCommand] private void IncreaseMax() => RangeMax = RangeMax?.AddDays(1);
    [ReactiveCommand] private void DecreaseMax() => RangeMax = RangeMax?.AddDays(-1);
    [ReactiveCommand] private void ToggleShowPresets() => ShowPresets = !ShowPresets;
    [ReactiveCommand] private void ShowEventLog() { EventLogVisible = true; ObservableLogVisible = false; }
    [ReactiveCommand] private void ShowObservableLog() { ObservableLogVisible = true; EventLogVisible = false; }
    [ReactiveCommand] private void ClearLogs() { EventLog.Clear(); ObservableLog.Clear(); }
    [ReactiveCommand] private void SetFirstDemoFormat() => DemoFormat = new("dd.MM.yyyy", "HH:mm:ss.fff");
    [ReactiveCommand] private void SetSecondDemoFormat() => DemoFormat = new("dd-MM-yyyy", "HH:mm:ss.fff");
    [ReactiveCommand] private void SetThirdDemoFormat() => DemoFormat = new("yyyy/MM/dd", "HH:mm:ss.fff");


    [ReactiveCommand]
    private async Task SimulateBoundsChangeAsync()
    {
        var oldRangeMin = RangeMin;

        RangeMin = null;
        RangeMax = null;

        await Task.Delay(500);

        var now = DateTime.Now;

        if (oldRangeMin.HasValue && oldRangeMin.Value.Month == now.Month)
        {
            RangeMin = DateTime.UtcNow.Date.AddMonths(-1).AddDays(-2).AddHours(-5).AddMinutes(-30);
            RangeMax = DateTime.UtcNow.Date.AddMonths(-1).AddDays(5).AddHours(10).AddMinutes(25);
        }
        else
        {
            RangeMin = DateTime.UtcNow.Date.AddDays(-2).AddHours(-5).AddMinutes(-30);
            RangeMax = DateTime.UtcNow.Date.AddDays(5).AddHours(10).AddMinutes(25);
        }
    }

    public void AddEventLog(string message) => AddLog(EventLog, message);
    public void AddObservableLog(string message) => AddLog(ObservableLog, message);

    public void AddLog(ObservableCollection<string> log, string message)
    {
        log.Add(message);
        while (log.Count > MaxLogEntries)
        {
            log.RemoveAt(0);
        }
    }

    private void UpdateFullRangePreset()
    {
        if (RangeMin.HasValue && RangeMax.HasValue)
        {
            var fullRange = RangeMax.Value - RangeMin.Value;
            var fullRangeItem = Presets.FirstOrDefault(p => p.Label == "За весь период");

            if (fullRangeItem != null)
            {
                var index = Presets.IndexOf(fullRangeItem);
                Presets[index] = fullRangeItem with { Duration = fullRange };
            }
            else
            {
                Presets.Add(new PresetItem("За весь период", fullRange));
            }
        }
    }
}