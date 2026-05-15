using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using UTS.DateTimeRangeSelector.Controls;
using UTS.DateTimeRangeSelector.DemoApp.ViewModels;
using UTS.DateTimeRangeSelector.Events;

namespace UTS.DateTimeRangeSelector.DemoApp.Views;

public partial class DateTimePickerPanelView : ReactiveUserControl<DateTimePickerPanelViewModel>
{
    private DateTimePickerPanel? _dateTimePickerControl;

    public DateTimePickerPanelView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            _dateTimePickerControl = this.FindControl<DateTimePickerPanel>("TestEventsPicker");

            if (_dateTimePickerControl != null)
            {
                _dateTimePickerControl.SelectedDateTimeChanged += OnSelectedDateTimeChanged;

                Disposable.Create(() =>
                {
                    _dateTimePickerControl.SelectedDateTimeChanged -= OnSelectedDateTimeChanged;
                })
                .DisposeWith(disposables);
            }
        });
    }

    private void OnSelectedDateTimeChanged(object? sender, SelectedDateTimeChangedEventArgs e)
    {
        var msg = $"SelectedDateTimeChanged: {e.OldValue:dd.MM.yyyy HH:mm:ss.fff} -> {e.NewValue:dd.MM.yyyy HH:mm:ss.fff}";
        ViewModel?.AddEventLog(msg);
    }
}