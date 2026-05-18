# UTS.Avalonia.DateTimeRangeSelector

Avalonia UI controls for selecting date/time ranges (`DateTimeRangeSelector` and `DateTimePickerPanel`).

## Install

Package is published to GitHub Packages.

### 1. Add the GitHub Packages feed

In `nuget.config`:

```xml
<packageSources>
  <add key="github" value="https://nuget.pkg.github.com/QuickLeopard/index.json" />
</packageSources>
```

Authenticate with a GitHub PAT that has `read:packages` (configure via `dotnet nuget add source` or your local NuGet credentials).

### 2. Add the package

```bash
dotnet add package UTS.Avalonia.DateTimeRangeSelector --version 0.0.8
```

## Requirements

- .NET 10
- Avalonia 11.3.x

## Repository

https://github.com/QuickLeopard/UTS.Avalonia.DateTimeRangeSelector
