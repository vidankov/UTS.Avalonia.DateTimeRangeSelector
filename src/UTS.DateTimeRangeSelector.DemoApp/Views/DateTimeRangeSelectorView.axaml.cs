using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
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
                _rangeSelectorControl.RangeChanges
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(range =>
                    {
                        var msg =
                            $"RangeChanges: From {range.From:dd.MM.yyyy HH:mm:ss.fff}, " +
                            $"To {range.To:dd.MM.yyyy HH:mm:ss.fff}, " +
                            $"Duration {range.Duration?.ToString("c") ?? "null"}";
                        ViewModel?.AddObservableLog(msg);
                    })
                    .DisposeWith(disposables);

                _rangeSelectorControl.ValidationChanges
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(validation =>
                    {
                        var msg = $"ValidationChanges: IsValid: {validation.IsValid}, Message: '{validation.Message}'";
                    })
                    .DisposeWith(disposables);

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
        ViewModel?.AddEventLog(msg);
    }

    private void OnValidationChanged(object? sender, ValidationChangedEventArgs e)
    {
        var msg = $"ValidationChanged: {e.OldIsValid} -> {e.NewIsValid}, Msg: '{e.OldMessage}' -> '{e.NewMessage}'";
        ViewModel?.AddEventLog(msg);
    }
}