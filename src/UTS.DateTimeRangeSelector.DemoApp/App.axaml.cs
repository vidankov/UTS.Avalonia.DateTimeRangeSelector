using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Splat;
using UTS.DateTimeRangeSelector.DemoApp.ViewModels;
using UTS.DateTimeRangeSelector.DemoApp.Views;

namespace UTS.DateTimeRangeSelector.DemoApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RegisterDependencies();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Locator.Current.GetService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterDependencies()
    {
        var mainViewModel = new MainViewModel();
        Locator.CurrentMutable.RegisterConstant(mainViewModel);

        Locator.CurrentMutable.RegisterLazySingleton(() =>
            new DateTimePickerPanelViewModel(mainViewModel), typeof(DateTimePickerPanelViewModel));

        Locator.CurrentMutable.RegisterLazySingleton(() =>
            new DateTimeRangeSelectorViewModel(mainViewModel), typeof(DateTimeRangeSelectorViewModel));

        Locator.CurrentMutable.Register(() => new DateTimePickerPanelView(),
            typeof(IViewFor<DateTimePickerPanelViewModel>));

        Locator.CurrentMutable.Register(() => new DateTimeRangeSelectorView(),
            typeof(IViewFor<DateTimeRangeSelectorViewModel>));
    }
}