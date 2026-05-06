# Changelog

Все заметные изменения в этом проекте будут задокументированы в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/)

## [0.0.2] — 06.05.2026

### Добавлено
- Реализован `DateTimePickerPanel` как `TemplatedControl` с основным свойством `SelectedDateTime` и границами `MinDateTime` / `MaxDateTime`.
- Внутренние компоненты времени (`Hour`, `Minute`, `Second`, `Millisecond`, `SelectedDate`) синхронизируются с `SelectedDateTime`.
- Конвертер `DecimalToIntConverter` для привязки `NumericUpDown` к целочисленным свойствам.
- Тема `DateTimePickerPanel` с календарём (`CalendarDatePicker`) и элементами управления временем (`NumericUpDown`).
- Применение ограничений `MinDateTime` / `MaxDateTime` к календарю через `DisplayDateStart` / `DisplayDateEnd`.
- Демо-приложение с четырьмя сценариями использования `DateTimePickerPanel`.

### Известные ограничения (запланировано к реализации)
- Отсутствует маршрутизируемое событие `SelectedDateTimeChanged`.
- Отсутствует нормализация значений в UTC.
- Программное изменение свойств через `SetValue` пока не сохраняет двусторонние привязки (переход на `SetCurrentValue` запланирован).
- Нет принудительной коэрции `SelectedDateTime` при изменении `Min`/`Max`.
- Опциональный `TextBox` для ввода строки даты/времени не реализован.

## [0.0.1] - 04.05.2026

### Добавлено
- Инициализирована структура решения: проекты библиотеки `UTS.DateTimeRangeSelector`, модульных тестов `UTS.DateTimeRangeSelector.Tests` и демонстрационного приложения `UTS.DateTimeRangeSelector.DemoApp` в папке `src/`.
- Настроен `Directory.Build.props` с централизованными свойствами: `TargetFramework=net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, `Charset=UTF-8`, а также переменными версий `AvaloniaVersion` и `SystemReactiveVersion`.
- В библиотеку добавлены зависимости только от `Avalonia` и `System.Reactive` (без ReactiveUI, Avalonia.ReactiveUI, DataGrid).
- Демо-приложение оставлено с `ReactiveUI.Avalonia` для удобства тестирования (зависимость не протекает в библиотеку).
- Добавлены `.gitignore` и `.gitattributes` для .NET-проектов.
- Создан минимальный `nuget.config` с единственным источником `nuget.org` для воспроизводимых сборок.