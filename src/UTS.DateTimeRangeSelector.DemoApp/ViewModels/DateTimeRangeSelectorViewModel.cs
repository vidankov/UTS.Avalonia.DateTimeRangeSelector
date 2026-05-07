using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace UTS.DateTimeRangeSelector.DemoApp.ViewModels;

public partial class DateTimeRangeSelectorViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "DateTimeRangeSelector";
    public IScreen HostScreen { get; } = hostScreen;
    public ViewModelActivator Activator { get; } = new();

    [Reactive] private string _message = "Work in progress";
}