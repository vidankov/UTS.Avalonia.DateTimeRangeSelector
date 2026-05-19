# Changelog

Все заметные изменения в этом проекте будут задокументированы в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/)

## [0.0.9] - 19.05.2026

### Исправлено
- DateTimeRangeSelector больше не переопределяет публичное свойство `IsEnabled` при нарушении условия `MinDateTime > MaxDateTime`. Внешние привязки к `IsEnabled` теперь сохраняются и полностью управляются потребителем.

### Добавлено
- Новое свойство только для чтения `AreBoundsValid` в `DateTimeRangeSelector`. Оно показывает, являются ли границы `MinDateTime` и `MaxDateTime` непротиворечивыми (`Min <= Max` или хотя бы одна не задана). Внутренний шаблон элемента управления использует это свойство для отключения панелей ввода при противоречивых границах, не затрагивая контроль потребителя над `IsEnabled`.

### Изменено
- Шаблон элемента управления `DateTimeRangeSelector` теперь привязывает `IsEnabled` корневого контейнера макета к свойству `AreBoundsValid` вместо использования собственного свойства `IsEnabled` элемента управления.

## [0.0.8] — 15.05.2026

### Добавлено
- Core-модель `DateTimeFormatModel` с раздельными форматами `DateFormat` и `TimeFormat`, а также комбинированным `DateTimeFormat`.
  - Значение по умолчанию: `dd.MM.yyyy HH:mm:ss.fff`.
- Стилизуемые свойства `DateTimeFormat` (тип `DateTimeFormatModel`) в `DateTimeRangeSelector` и `DateTimePickerPanel`.
  - `DateTimeRangeSelector` передаёт формат дочерним панелям через `TemplateBinding`.
  - `DateTimePickerPanel` использует `DateTimeFormat.DateFormat` для настройки `CustomDateFormatString` календаря.
- В демо-приложении добавлены команды динамической смены формата даты (`SetFirstDemoFormat`, `SetSecondDemoFormat`, `SetThirdDemoFormat`) и привязка `DateTimeFormat` к `DemoFormat`.
- Маршрутизируемое событие `SelectedDateTimeChanged` в `DateTimePickerPanel`.
  - `SelectedDateTimeChangedEventArgs` (RoutedEventArgs) со свойствами `OldValue`, `NewValue`.
  - Событие зарегистрировано как `RoutedEvent` с `RoutingStrategies.Direct` и снабжено CLR-обёрткой.
  - Событие генерируется при любом изменении `SelectedDateTime`, включая начальную установку (с `OldValue = null`).
- В демо-приложении добавлено логирование `SelectedDateTimeChanged` для одного из пикеров.

### Изменено
- Внутренняя тема `DateTimePickerPanel` теперь привязывает `CustomDateFormatString` календаря к `DateTimeFormat.DateFormat` вместо жёстко заданного значения.
- Метод `OnSelectedDateTimeChanged` в `DateTimePickerPanel` теперь принимает старое и новое значения, а не считывает их из свойства.

### Исправлено
- Устранена возможная рекурсия в методе `Coerce` при вызове `SetCurrentValue` внутри клампинга. Флаг `_isCoercing` теперь устанавливается до любых изменений свойств, исключая повторные прогоны `UpdateValidation()` и гарантируя атомарность.

## [0.0.7] — 14.05.2026

### Добавлено
- Единая точка входа для стилей библиотеки — `DateTimeRangeSelectorTheme` (наследник `Styles`).
  - Загружает все темы контролов через `ResourceDictionary.MergedDictionaries`.
  - Подключается в `Application.Styles` одной строкой.
- Документирование именованных частей шаблона:
  - `DateTimePickerPanel`: `[TemplatePart("PART_Calendar", typeof(ConstrainedCalendarDatePicker))]`.
  - `DateTimeRangeSelector`: `[TemplatePart("PART_FromPanel", typeof(DateTimePickerPanel))]`, `[TemplatePart("PART_ToPanel", typeof(DateTimePickerPanel))]`.

### Изменено
- Файлы тем `DateTimePickerPanel.axaml` и `DateTimeRangeSelector.axaml` преобразованы в `ResourceDictionary` и автоматически подхватываются через `DateTimeRangeSelectorTheme`.
- Демо-приложение использует `<themes:DateTimeRangeSelectorTheme />` вместо отдельных `ResourceInclude`.

## [0.0.6] — 14.05.2026

### Добавлено
- Маршрутизируемые события `RangeChanged` и `ValidationChanged` в `DateTimeRangeSelector`.
  - `DateTimeRangeChangedEventArgs` (RoutedEventArgs) со свойствами `OldFrom`, `NewFrom`, `OldTo`, `NewTo`.
  - `ValidationChangedEventArgs` (RoutedEventArgs) со свойствами `OldIsValid`, `NewIsValid`, `OldMessage`, `NewMessage`.
  - События зарегистрированы как `RoutedEvent` с `RoutingStrategies.Direct` и снабжены CLR-обёртками.
  - События генерируются однократно после завершения коэрции и валидации, даже при последовательном изменении `From` и `To`.
- Механизм подавления дублирующихся событий (`_suppressEvents`) для атомарных операций (пресеты, начальный диапазон).
- Реактивные потоки `RangeChanges` и `ValidationChanges` (горячие `IObservable` с повтором последнего значения и `DistinctUntilChanged`).
- Публичные методы управления диапазоном:
  - `SetRange(DateTime?, DateTime?)` — атомарная установка `From` и `To`.
  - `ApplyPreset(TimeSpan)` — программное применение длительности (проверяет положительность).
  - `ResetToDefaults()` — сброс `From`/`To` к начальному диапазону с сохранением границ.
- В демо-приложении:
  - UI-лог последних 10 событий с переключением между логами событий и наблюдаемых потоков.
  - Кнопки для вызова `SetRange`, `ApplyPreset`, `ResetToDefaults`.
  - Кнопка очистки логов.

### Изменено
- Внутренняя логика атомарного изменения диапазона вынесена в метод `ApplyRangeChange`, устраняя дублирование кода в команде пресетов, `ApplyDefaultRange` и `OnPropertyChanged`.
- Метод `Coerce` освобождён от ответственности за подавление событий и генерацию уведомлений.
- В демо-приложении исправлена отсутствовавшая запись в лог для `ValidationChanges`.

## [0.0.5] — 12.05.2026

### Добавлено
- Core-типы `DateTimeRange` (неизменяемый диапазон), `PresetItem` (пресет с меткой и длительностью), `ValidationResult` (результат валидации).
- В `DateTimeRangeSelector`:
  - Стилизуемые свойства `Presets` (список пресетов) и `ShowPresets` (видимость панели пресетов).
  - CLR-свойство `TimeProvider` для получения текущего времени при применении пресетов и начального диапазона.
  - Команда `ApplyPresetCommand` (внутренний класс `PresetCommand`), принимающая `TimeSpan` в качестве параметра.
  - Логика пресетов: правый край определяется `MaxDateTime`, если он задан, иначе — `TimeProvider.GetUtcNow()`; левый край = правый − длительность, но не ранее `MinDateTime`.
  - Логика начального диапазона по умолчанию согласована с пресетами: если `MaxDateTime` задан, `To = MaxDateTime` (иначе `Now`); `From = To - 1 час` с учётом `MinDateTime`.
  - Защита от повторного применения значений по умолчанию через флаг `_defaultsApplied`.
- В теме `DateTimeRangeSelector.axaml`:
  - Панель пресетов: `ItemsControl` с `WrapPanel`, привязанный к `Presets` и `ShowPresets`.
  - Кнопки пресетов с привязкой команды через `Binding` с `RelativeSource={RelativeSource FindAncestor}` к родительскому `DateTimeRangeSelector`.
- В демо-приложении:
  - В `DateTimeRangeSelectorViewModel` добавлены свойства `Presets` (три пресета, включая «За весь период») и `ShowPresets`, а также команда `ToggleShowPresets`.
  - В `DateTimeRangeSelectorView` добавлены кнопка переключения видимости пресетов и привязки `Presets`, `ShowPresets` к контролу.

### Изменено
- В `DateTimeRangeSelector` метод `OnPropertyChanged` теперь вызывает `_applyPresetCommand.RaiseCanExecuteChanged()` при изменении `ShowPresets`, обеспечивая корректное обновление доступности кнопок пресетов.

## [0.0.4] — 08.05.2026

### Добавлено
- Core-утилиты `DateTimeNormalization.EnsureUtc` и `DateTimeRangeCoercion.Clamp` для чистой нормализации и клампинга дат без зависимостей от UI.
- В `DateTimePickerPanel` автоматическая коэрция `SelectedDateTime`: приведение к UTC и ограничение границами `MinDateTime`/`MaxDateTime` через `CoerceValueCallback`.
- Новый контрол `ConstrainedCalendarDatePicker` (наследник `CalendarDatePicker`), гарантирующий соблюдение границ и корректную синхронизацию текста при потере фокуса.
- В `DateTimeRangeSelector` свойства границ `MinDateTime` и `MaxDateTime`, проброшенные в дочерние пикеры.
- Свойства валидации `IsValid` и `ValidationMessage`, отражающие состояние диапазона (не задан, нарушен порядок, выход за границы, неверные Min/Max).
- Улучшенная коэрция диапазона с учётом границ: клампинг, нормализация UTC, сохранение намерения пользователя при восстановлении порядка From/To.
- Полноценная демонстрация `DateTimeRangeSelector` с кнопками изменения границ и отображением статуса валидации.

### Изменено
- Внутренний метод `UpdateSelectedDateTime` в пикере теперь использует `SetCurrentValue` для сохранения двусторонних привязок.
- Пикер больше не зависит от стандартного поведения `CalendarDatePicker` в отношении границ – все проверки вынесены в собственные Core-классы.
- Метод `ApplyMinMaxToCalendar` упрощён: границы календаря управляются через `ConstrainedCalendarDatePicker`.

### Исправлено
- Устранена возможность ввода даты вне допустимых границ в `CalendarDatePicker` – теперь после потери фокуса значение автоматически корректируется.
- Исправлено «расползание» `DisplayDateStart`/`DisplayDateEnd` при ручном вводе некорректных дат.
- Устранены ошибки смещения времени при использовании локальных дат в границах (теперь всё хранится в UTC).

## [0.0.3] — 07.05.2026

### Добавлено
- `DateTimeRangeSelector` — TemplatedControl для выбора диапазона дат и времени со свойствами `FromDateTime`, `ToDateTime` (двусторонние), `Orientation` (Vertical/Horizontal) и базовой коэрцией (From <= To).
- Тема `DateTimeRangeSelector` с привязкой дочерних `DateTimePickerPanel` через `Binding` для надёжной синхронизации значений.
- `DateTimeRangeSelectorView` и `DateTimeRangeSelectorViewModel` с полноценной демонстрацией: установка начального диапазона, отображение выбранных значений и кнопка переключения ориентации.
- В `DateTimePickerPanel` внутренние обновления `SelectedDateTime` переведены на `SetCurrentValue`, чтобы сохранять активные двусторонние привязки.
- В `App.axaml` добавлен ресурс темы `DateTimeRangeSelector.axaml`.

### Изменено
- Демонстрационное приложение переведено на навигацию с вкладками (`RoutedViewHost`).
- Демонстрация `DateTimePickerPanel` вынесена в отдельную пару `DateTimePickerPanelView` / `DateTimePickerPanelViewModel`.
- Главное окно заменено на `MainView` с управлением вкладками; `MainViewModel` управляет маршрутизацией (`RoutingState`).
- В `App.axaml.cs` через `Locator` реализована регистрация зависимостей (ViewModels, Views).

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