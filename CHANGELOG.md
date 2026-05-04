# Changelog

Все заметные изменения в этом проекте будут задокументированы в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/)

## [0.0.1]

### Добавлено
- Инициализирована структура решения: проекты библиотеки `UTS.DateTimeRangeSelector`, модульных тестов `UTS.DateTimeRangeSelector.Tests` и демонстрационного приложения `UTS.DateTimeRangeSelector.DemoApp` в папке `src/`.
- Настроен `Directory.Build.props` с централизованными свойствами: `TargetFramework=net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, `Charset=UTF-8`, а также переменными версий `AvaloniaVersion` и `SystemReactiveVersion`.
- В библиотеку добавлены зависимости только от `Avalonia` и `System.Reactive` (без ReactiveUI, Avalonia.ReactiveUI, DataGrid).
- Демо-приложение оставлено с `ReactiveUI.Avalonia` для удобства тестирования (зависимость не протекает в библиотеку).
- Добавлены `.gitignore` и `.gitattributes` для .NET-проектов.
- Создан минимальный `nuget.config` с единственным источником `nuget.org` для воспроизводимых сборок.