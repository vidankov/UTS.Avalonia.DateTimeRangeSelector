using ReactiveUI.Avalonia;
using UTS.DateTimeRangeSelector.DemoApp.ViewModels;

namespace UTS.DateTimeRangeSelector.DemoApp.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
    }
}