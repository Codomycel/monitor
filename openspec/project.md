# Project Context — System Activity Tracker (Operon)

## 1. Product Overview

System Activity Tracker is a Windows desktop application that tracks computer usage time and provides daily, weekly, and monthly summaries.

The system measures:
- Active time
- Idle time
- Locked time
- Manual task time

All data is stored locally. The application is private, offline-only, and does not send data externally.

---

## 2. Core Business Principles (Non-Negotiable)

1. No internet usage.
2. No telemetry.
3. No cloud sync (unless explicitly added via spec).
4. All tracking data must remain local.
5. Existing features must not break during new implementations.
6. Avoid large refactors unless explicitly requested.

---

## 3. Tech Stack

- **Platform:** Windows only
- **Framework:** WPF (.NET 8, `net8.0-windows`)
- **Language:** C# with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- **Pattern:** MVVM (gradual adoption — not all logic is in ViewModels yet)
- **DI container:** `Microsoft.Extensions.DependencyInjection` (8.0.0), wired in `App.xaml.cs`
- **Charting:** `LiveCharts.Wpf` (0.9.7)
- **Storage:** CSV (activity logs) + JSON (settings, manual tasks, close-event logs)
- **Serialization:** `System.Text.Json` (built-in, no Newtonsoft)
- **Solution file:** `Operon.sln` → project `Operon/Operon.csproj`
- **Assembly name:** `Operon`, **Root namespace:** `SystemActivityTracker`

---

## 4. Project Structure

```
Operon/
├── App.xaml / App.xaml.cs          — startup, DI wiring, single-instance, session events
├── Models/                          — plain data models
│   ├── ActivityRecord.cs            — in-flight tracking record (StartTime, EndTime, ProcessName, IsLocked, IsIdle)
│   ├── AppSettings.cs               — user-configurable settings (idle threshold, poll interval, UI mode, etc.)
│   ├── AppUsageSummary.cs           — per-app aggregation
│   ├── DailySummary.cs              — daily rollup with TotalActiveDuration = Active + Manual
│   ├── ManualTaskEntry.cs           — INotifyPropertyChanged model for manual tasks
│   └── MonthlyAppUsageDto.cs        — monthly per-app DTO
├── Services/
│   ├── Abstractions/                — interfaces & value types used across layers
│   │   ├── ActivityLogEntry.cs      — readonly record struct (CSV row shape)
│   │   ├── IActiveWindowProvider.cs
│   │   ├── IActivityLogReader.cs
│   │   ├── IActivityLogWriter.cs
│   │   ├── IClock.cs
│   │   ├── ICrashLogReader.cs
│   │   └── IIdleTimeProvider.cs
│   ├── System/                      — platform implementations
│   │   ├── SystemActiveWindowProvider.cs
│   │   ├── SystemClock.cs
│   │   └── SystemIdleTimeProvider.cs
│   ├── ActivityLogReader.cs         — reads activity-log-YYYY-MM-DD.csv
│   ├── ActivityLogWriter.cs         — appends to CSV
│   ├── CloseTrackingService.cs      — crash/hang detection, heartbeat timers, close-event JSONL
│   ├── CloseReason.cs               — enum for shutdown classification
│   ├── CrashLogReader.cs            — reads close-event JSONL logs
│   ├── IdleTimeHelper.cs            — Win32 GetLastInputInfo wrapper
│   ├── ManualTaskService.cs         — load/save manual-tasks-YYYY-MM-DD.json
│   ├── SessionStateService.cs       — subscribes to SystemEvents.SessionSwitch
│   ├── SettingsService.cs           — load/save settings.json via JsonFile utility
│   └── TrackingService.cs           — core polling loop (System.Timers.Timer), state machine
├── ViewModels/
│   ├── ActivityChartViewModel.cs    — chart data preparation
│   ├── LastCrashViewModel.cs        — crash/last-run display
│   └── MainWindowViewModel.cs       — primary VM (largest file, ~73 KB)
├── Views/
│   ├── UiAMainWindow.xaml/.cs       — default UI skin (UIA)
│   ├── UiBMainWindow.xaml/.cs       — alternate UI skin (UIB)
│   └── Shells/                      — shell/host windows
├── Controls/
│   ├── ActivityChart.xaml/.cs       — reusable LiveCharts control
│   └── MonthYearPicker.xaml/.cs     — custom month/year picker
├── Converters/
│   ├── BooleanToLegendLocationConverter.cs
│   ├── InverseBooleanToVisibilityConverter.cs
│   └── SelectedDayChartConverters.cs
├── Styles/
│   ├── Styles.xaml                  — main resource dictionary
│   ├── ClassicStyles.xaml           — UIA-specific styles
│   └── UIBStyles.xaml               — UIB-specific styles
├── Utilities/
│   ├── AppConstants.cs              — all magic numbers (defaults, limits, timings)
│   ├── AppPaths.cs                  — all file-path helpers (%LocalAppData%\SystemActivityTracker\)
│   ├── DurationFormatter.cs
│   ├── JsonFile.cs                  — generic JSON load/save helpers
│   ├── SingleInstanceManager.cs     — named-mutex single-instance enforcement
│   ├── TimeSpanExtensions.cs
│   ├── UiConstants.cs               — UiModes: UIA / UIB (default = UIA)
│   └── WindowActivator.cs           — brings existing window to foreground
└── Resources/
```

---

## 5. Data Storage Layout

All files live under `%LocalAppData%\SystemActivityTracker\`.

| File | Purpose |
|---|---|
| `settings.json` | User settings (`AppSettings` model) |
| `activity-log-YYYY-MM-DD.csv` | Per-day activity rows (`ActivityLogEntry`) |
| `manual-tasks-YYYY-MM-DD.json` | Per-day manual task entries |
| `LastRun.json` | Previous-run metadata for crash detection |
| `logs/close-events-YYYY-MM-DD.jsonl` | Close/crash event records (JSONL) |

Schema must NOT be changed unless explicitly requested.
Historical data is never rewritten.
Aggregation is always recalculated on demand from the raw CSV files.

---

## 6. Core Time Definitions

Active:
- User interacting (keyboard/mouse activity detected).

Idle:
- System unlocked but no activity beyond configured idle threshold.

Locked:
- Windows session locked.

Manual:
- User-entered task duration (HH/MM/SS).

Total Active:
Active + Manual (see `DailySummary.TotalActiveDuration`)

This definition must remain consistent across:
- Daily view
- Weekly view
- Monthly view
- All charts
- All summaries

---

## 7. Productivity Color Classification Rule

Total Active hours must be classified into color categories.

The same classification logic must be used everywhere:
- Daily chart
- Weekly chart
- Monthly calendar
- Monthly weekly totals

Color thresholds must be centralized in a single reusable service/helper.

Color logic must NOT be:
- Hardcoded in XAML
- Duplicated in multiple chart components
- Embedded separately in each view

If thresholds change in future, they must update system-wide automatically.

---

## 8. Monthly View Requirements

Monthly view contains:
- Calendar-based layout
- Total Active per day
- Weekly grouping

For each week inside a selected month:
- Calculate Weekly Total Active
- Weekly total must include only days within selected month
- Weekly total must follow the same color classification rule

---

## 9. Key Conventions

### Naming
- Interfaces prefixed with `I` (e.g., `IActivityLogReader`)
- Platform/system implementations in `Services/System/` (e.g., `SystemClock`)
- UI windows prefixed with `UiA` or `UiB` to indicate skin
- All magic numbers live in `AppConstants` — never inline

### Code Style
- C# `readonly record struct` for immutable value types (e.g., `ActivityLogEntry`, `TrackingSnapshot`)
- `INotifyPropertyChanged` implemented manually with `[CallerMemberName]` (no third-party MVVM framework)
- `IDisposable` required on anything owning timers or event subscriptions
- `System.Timers.Timer` for background work; `DispatcherTimer` for UI heartbeats
- `System.Text.Json` exclusively — no Newtonsoft.Json

### Threading
- Background tracking runs on `System.Timers.Timer` (thread-pool thread)
- All UI updates must go through `Dispatcher.Invoke` / `Dispatcher.BeginInvoke`
- `_syncRoot` lock used inside `TrackingService` for shared state

### DI
- Services are registered and resolved in `App.xaml.cs OnStartup`
- Constructor injection is used; no service-locator pattern

### Settings
- All configurable defaults live in `AppConstants.Defaults`
- Settings are written/read via `SettingsService` using `JsonFile` helpers
- Settings take effect only after explicit Save — tracking must restart timers after save

---

## 10. Architecture Guardrails

Preferred pattern:
- MVVM (gradual adoption allowed)

Rules:
- Do not block UI thread.
- Background tracking must remain lightweight.
- Dispatcher must be used when updating UI from background threads.
- Avoid introducing performance-heavy loops.
- Avoid memory leaks (unsubscribe events, dispose timers properly).

---

## 11. Tracking Engine Rules

Tracking must:
- Remain lightweight
- Respect poll interval setting
- Respect idle threshold setting
- Pause correctly during lock state
- Resume correctly after unlock

Any modification to tracking logic must:
- Include reasoning
- Include edge-case validation (lock/unlock, sleep/resume)

---

## 12. Data Rules

- Do not modify storage schema unless explicitly requested.
- All time calculations must use centralized logic.
- Aggregation logic must not be duplicated across views.

---

## 13. Definition of Done (For Any Feature)

Every spec must include:
- What changed
- Files touched
- How to verify
- Edge cases considered
- Performance impact (if any)

---

## 14. AI Working Constraints

AI must:
- Keep changes localized
- Avoid renaming unrelated files
- Avoid structural refactors
- Follow existing patterns unless explicitly told otherwise
- Maintain privacy-first principle