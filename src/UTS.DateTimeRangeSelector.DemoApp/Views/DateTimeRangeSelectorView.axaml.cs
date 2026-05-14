using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using UTS.DateTimeRangeSelector.DemoApp.ViewModels;
using UTS.DateTimeRangeSelector.Events;

namespace UTS.DateTimeRangeSelector.DemoApp.Views;

public partial class DateTimeRangeSelectorView : ReactiveUserControl<DateTimeRangeSelectorViewModel>
{
    private Controls.DateTimeRangeSelector? _rangeSelectorControl;

    public DateTimeRangeSelectorView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            _rangeSelectorControl = this.FindControl<Controls.DateTimeRangeSelector>("RangeSelector");

            if (_rangeSelectorControl != null)
            {
                _rangeSelectorControl.RangeChanged += OnRangeChanged;
                _rangeSelectorControl.ValidationChanged += OnValidationChanged;

                Disposable.Create(() =>
                {
                    _rangeSelectorControl.RangeChanged -= OnRangeChanged;
                    _rangeSelectorControl.ValidationChanged -= OnValidationChanged;
                }).DisposeWith(disposables);
            }
        });
    }

    private void OnRangeChanged(object? sender, DateTimeRangeChangedEventArgs e)
    {
        var msg =
            $"RangeChanged: From {e.OldFrom:dd.MM.yyyy HH:mm:ss.fff} -> {e.NewFrom:dd.MM.yyyy HH:mm:ss.fff}, " +
            $"To {e.OldTo:dd.MM.yyyy HH:mm:ss.fff} -> {e.NewTo:dd.MM.yyyy HH:mm:ss.fff}";

        Debug.WriteLine(msg);
        ViewModel?.AddLog(msg);
    }

    private void OnValidationChanged(object? sender, ValidationChangedEventArgs e)
    {
        var msg = $"ValidationChanged: {e.OldIsValid} -> {e.NewIsValid}, Msg: '{e.OldMessage}' -> '{e.NewMessage}'";
        Debug.WriteLine(msg);
        ViewModel?.AddLog(msg);
    }
}