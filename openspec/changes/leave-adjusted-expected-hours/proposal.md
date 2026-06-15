## Why

The Selected Day panel and Week Report use fixed 8-hour and 40-hour benchmarks for charts and progress context, but recorded leave (full-day or half-day) is not reflected in expected hours. Users on leave still see a full workday or workweek target, which misrepresents how much active time they should aim for on that day or week.

This change adjusts expected hours downward based on leave entries already stored via the Leaves tab, and surfaces leave context in the Selected Day and Week Report panels.

## What Changes

- Introduce shared leave-adjusted expected-hours calculation (day baseline 8h, week baseline 40h)
- Deduct 8h for full-day leave and 4h for half-day leave (morning or afternoon); floor at 0h
- Selected Day panel: show leave summary when the selected date has leave; use leave-adjusted expected hours for day benchmark/reference display
- Week Report panel: show leave summary when any day in the selected week has leave; use leave-adjusted expected hours for weekly benchmark/reference display
- Keep Week Report header format `Week Report (WK25)` with no separate week-number row
- On leave add/update/delete, refresh only the affected Selected Day and/or Week Report surfaces; avoid full month grid rebuild unless the visible activity month also needs leave indicator updates (patch in place when possible)

## Capabilities

### New Capabilities

- `leave-adjusted-expected-hours`: Shared expected-hours calculation, Selected Day leave/expected display, Week Report leave/expected display, and targeted refresh after leave changes

### Modified Capabilities

None — leave CRUD and month-view leave indicators remain as defined in `leave-management` and `activity-month-leave-indicator`; this change consumes leave data without altering those requirements.

## Impact

### Affected Code

- `Operon/Utilities/` — new shared expected-hours helper (or small calculator class)
- `Operon/ViewModels/MainWindowViewModel.cs` — expected-hours properties, leave summary text, refresh hooks on leave persist and date/week navigation
- `Operon/ViewModels/ActivityChartViewModel.cs` — weekly/daily `ReferenceTime` driven by leave-adjusted expected hours where applicable
- `Operon/Views/Shells/UIAShell.xaml` — Selected Day leave info and expected-hours display
- `Operon/Views/UiAMainWindow.xaml` — Week Report leave summary (preserve `WeekReportHeaderText` header)

### Dependencies

- Existing `LeaveService`, `LeaveEntry`, `LeaveDuration`, `LeaveType`
- Existing `SelectedWeekStart`, `RefreshDate` / `RefreshWeekIfNeeded` refresh patterns

### User Experience Changes

- Selected Day shows leave type/duration when leave exists for that date
- Week Report shows aggregated leave summary when the selected week includes leave
- Chart reference lines and expected-hour context scale with leave (e.g. 4h expected after half-day leave)

### Non-Goals

- Changing leave CRUD rules or storage format
- Adjusting month calendar tile expected hours (month bars remain on 8h reference unless separately specified later)
- Automated tests
- New NuGet packages
