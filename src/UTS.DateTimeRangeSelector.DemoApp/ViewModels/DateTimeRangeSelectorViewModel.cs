using Avalonia.Layout;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using UTS.DateTimeRangeSelector.Core;

namespace UTS.DateTimeRangeSelector.DemoApp.ViewModels;

public partial class DateTimeRangeSelectorViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "DateTimeRangeSelector";
    public IScreen HostScreen { get; } = hostScreen;
    public ViewModelActivator Activator { get; } = new();
    public List<PresetItem> Presets =>
    [
        new ("Последний час", TimeSpan.FromHours(1)),
        new ("Последние сутки", TimeSpan.FromDays(1)),
        new ("За весь период", RangeMax!.Value - RangeMin!.Value)
    ];

    [Reactive] private DateTime? _rangeFrom = DateTime.UtcNow.AddDays(-1);
    [Reactive] private DateTime? _rangeTo = DateTime.UtcNow.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
    [Reactive] private DateTime? _rangeMin = DateTime.UtcNow.Date.AddDays(-7);
    [Reactive] private DateTime? _rangeMax = DateTime.UtcNow.Date.AddDays(7).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
    [Reactive] private Orientation _orientation = Orientation.Vertical;
    [Reactive] private bool _showPresets = false;

    [ReactiveCommand] private void ToggleOrientation() =>
        Orientation = Orientation == Orientation.Vertical
            ? Orientation.Horizontal : Orientation.Vertical;
    [ReactiveCommand] private void IncreaseMin() => RangeMin = RangeMin?.AddDays(1);
    [ReactiveCommand] private void DecreaseMin() => RangeMin = RangeMin?.AddDays(-1);
    [ReactiveCommand] private void IncreaseMax() => RangeMax = RangeMax?.AddDays(1);
    [ReactiveCommand] private void DecreaseMax() => RangeMax = RangeMax?.AddDays(-1);
    [ReactiveCommand] private void ToggleShowPresets() => ShowPresets = !ShowPresets;
}