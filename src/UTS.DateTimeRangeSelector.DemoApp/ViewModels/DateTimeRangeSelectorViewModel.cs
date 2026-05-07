using Avalonia.Layout;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace UTS.DateTimeRangeSelector.DemoApp.ViewModels;

public partial class DateTimeRangeSelectorViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "DateTimeRangeSelector";
    public IScreen HostScreen { get; } = hostScreen;
    public ViewModelActivator Activator { get; } = new();

    [Reactive] private DateTime? _rangeFrom = DateTime.UtcNow.AddDays(-1);
    [Reactive] private DateTime? _rangeTo = DateTime.UtcNow.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
    [Reactive] private Orientation _orientation = Orientation.Vertical;

    [ReactiveCommand] private void ToggleOrientation() =>
        Orientation = Orientation == Orientation.Vertical
            ? Orientation.Horizontal : Orientation.Vertical;
}