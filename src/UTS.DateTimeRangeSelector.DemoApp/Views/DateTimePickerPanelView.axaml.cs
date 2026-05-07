using ReactiveUI.Avalonia;
using UTS.DateTimeRangeSelector.DemoApp.ViewModels;

namespace UTS.DateTimeRangeSelector.DemoApp.Views;

public partial class DateTimePickerPanelView : ReactiveUserControl<DateTimePickerPanelViewModel>
{
    public DateTimePickerPanelView()
    {
        InitializeComponent();
    }
}