using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System.Reactive.Disposables.Fluent;

namespace UTS.DateTimeRangeSelector.DemoApp.ViewModels;

public partial class MainViewModel : ReactiveObject, IScreen, IActivatableViewModel
{
    [Reactive] private bool _isDateTimePickerPanelActive;
    [Reactive] private bool _isDateTimeRangeSelectorActive;

    public RoutingState Router { get; } = new();
    public ViewModelActivator Activator { get; } = new();

    public MainViewModel()
    {
        Router.CurrentViewModel.Subscribe(UpdateTabsState);

        this.WhenActivated(disposables =>
        {
            NavigateToDateTimePickerPanelCommand
                .Execute()
                .Subscribe()
                .DisposeWith(disposables);
        });
    }

    private void UpdateTabsState(IRoutableViewModel? viewModel)
    {
        IsDateTimePickerPanelActive = viewModel is DateTimePickerPanelViewModel;
        IsDateTimeRangeSelectorActive = viewModel is DateTimeRangeSelectorViewModel;
    }

    [ReactiveCommand]
    private IObservable<IRoutableViewModel> NavigateToDateTimePickerPanel()
    {
        var model = Locator.Current.GetService<DateTimePickerPanelViewModel>();
        return Router.Navigate.Execute(model!);
    }

    [ReactiveCommand]
    private IObservable<IRoutableViewModel> NavigateToDateTimeRangeSelector()
    {
        var model = Locator.Current.GetService<DateTimeRangeSelectorViewModel>();
        return Router.Navigate.Execute(model!);
    }
}