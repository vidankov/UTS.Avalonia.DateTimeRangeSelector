using ReactiveUI.Avalonia;
using UTS.DateTimeRangeSelector.DemoApp.ViewModels;

namespace UTS.DateTimeRangeSelector.DemoApp.Views;

public partial class DateTimeRangeSelectorView : ReactiveUserControl<DateTimeRangeSelectorViewModel>
{
    public DateTimeRangeSelectorView()
    {
        InitializeComponent();
    }
}