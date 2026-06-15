## Context

Operon already persists leave via `LeaveService` (`leaves-YYYY-MM.json`) and shows leave indicators on the activity month calendar. The Selected Day sidebar (`UIAShell.xaml`) displays tracked active, manual, idle, and locked totals. The Week Report card uses `WeekReportHeaderText` (e.g. `Week Report (WK25)`), `SelectedWeekRangeText`, and a weekly activity chart with a fixed `ReferenceTime` of 40 hours.

`ActivityChartViewModel` and `HorizontalActivityBarViewModel` use `ReferenceTime` for benchmark lines, fill ratios, and color thresholds. Day charts default to 8h; the weekly chart uses 40h. Leave data is available per date through `LeaveService.GetForDate` but is not applied to expected hours or panel summaries outside the Leaves tab and month indicators.

Recent refresh work introduced `RefreshDate`, `RefreshWeekIfNeeded`, and in-place month cell updates — leave changes should follow the same targeted refresh pattern instead of always calling `LoadMonthlyUsage()`.

## Goals / Non-Goals

**Goals:**

- Single shared calculator for day and week expected hours from leave
- Rules: day baseline 8h, week baseline 40h; full-day deduction 8h; half-day deduction 4h; result never below 0h
- Selected Day: leave info row/section when leave exists; expected hours reflect leave adjustment
- Week Report: leave summary when the selected week has any leave; expected hours reflect leave adjustment
- Preserve `Week Report (WK25)` header; no separate week-number row
- Targeted refresh on leave CRUD and when selected date/week changes

**Non-Goals:**

- Changing leave entry model or Leaves tab CRUD
- Rebuilding the full month grid on every leave edit (patch month cell leave fields only when the activity month is visible)
- Adjusting month-view horizontal bar 8h reference per leave (out of scope for this change)
- Automated test projects

## Decisions

### 1. Shared calculator — `ExpectedHoursCalculator` in Utilities

**Decision:** Add a static helper (e.g. `Operon/Utilities/ExpectedHoursCalculator.cs`) with:

| Method | Behavior |
|--------|----------|
| `GetLeaveDeductionHours(LeaveDuration?)` | `null` → 0; `FullDay` → 8; `MorningHalf` / `AfternoonHalf` → 4 |
| `GetDayExpectedHours(LeaveDuration?)` | `max(0, 8 - deduction)` |
| `GetWeekExpectedHours(IEnumerable<LeaveDuration?>)` | `max(0, 40 - sum(deductions))` for each of the 7 days in the week |

**Rationale:** One source of truth for day and week panels, charts, and refresh logic. Constants (`StandardDayHours = 8`, `StandardWeekHours = 40`) colocated for easy tuning.

**Alternatives considered:**
- Inline logic in `MainWindowViewModel` — rejected; duplicates day/week rules
- Deduction only on weekdays — rejected; user rules apply per calendar day with recorded leave

### 2. Week expected hours — sum deductions across the 7-day window

**Decision:** For `SelectedWeekStart` … `+6 days`, load leave per date (via `LeaveService.GetForDate` or cached month list when available), sum per-day deductions, subtract from 40h baseline.

**Rationale:** Matches “half day reduces 4h, full day reduces 8h” independently per date; multiple leave days stack (e.g. two half-days → −8h).

### 3. ViewModel surface properties

**Decision:** Add computed/read-only properties on `MainWindowViewModel`:

| Property | Purpose |
|----------|---------|
| `SelectedDayExpectedHours` / `SelectedDayExpectedHoursText` | Leave-adjusted day benchmark |
| `SelectedDayLeaveSummaryText` | e.g. `Leave: Sick Leave — Morning half` (empty when no leave) |
| `HasSelectedDayLeave` | Visibility for leave UI |
| `WeekExpectedHours` / `WeekExpectedHoursText` | Leave-adjusted week benchmark |
| `WeekLeaveSummaryText` | Aggregated summary (e.g. `2 leave day(s), −12h expected`) |
| `HasWeekLeave` | Visibility for week leave UI |

Recalculate via `RefreshSelectedDayExpectedHours()` and `RefreshWeekExpectedHours()` — not on every property get if leave/activity data is cached on the VM.

### 4. Chart reference time follows expected hours

**Decision:** When updating day chart (`UpdateActivityChart`) and week chart (`UpdateWeeklyActivityChart`), set `ReferenceTime` to leave-adjusted expected hours (not fixed 8/40 when leave applies). Use at least a small positive minimum (e.g. 1 minute) only if needed for chart rendering when expected is 0h.

**Rationale:** Reference line and fill ratio should match displayed expected hours.

### 5. Refresh strategy on leave CRUD

**Decision:** Replace unconditional `LoadMonthlyUsage()` in `PersistLeaveMonth()` with:

1. `RefreshLeaveSurfacesForDate(leaveDate)`:
   - If `leaveDate == SelectedDate` → refresh selected-day leave + expected hours + day chart
   - If `leaveDate` in selected week → `RefreshWeekExpectedHours()` + weekly chart (reuse `RefreshWeekIfNeeded` pattern or dedicated leave hook)
   - If `leaveDate` in visible activity month → patch `MonthlyDayItem` leave fields in place (no full rebuild)
2. Reload Leaves tab calendar via existing `LoadLeavesForSelectedMonth()`

**Rationale:** Aligns with `RefreshDate` / manual-task targeted updates; avoids month grid flicker.

### 6. UI placement

**Decision:**

- **Selected Day (`UIAShell.xaml`):** Add leave summary row after date bounds or before activity rows; show only when `HasSelectedDayLeave`. Optionally show `Expected: 04:00` (or formatted) when leave reduces benchmark.
- **Week Report (`UIAShell.xaml` + `UiAMainWindow.xaml` week card):** Add leave summary below `SelectedWeekRangeText` when `HasWeekLeave`; show week expected hours near totals or chart. Do not reintroduce `WeekNumberText` row — header stays `WeekReportHeaderText`.

### 7. Refresh triggers

**Decision:** Call expected-hours refresh from:

- `RefreshForSelectedDate()` / `SelectedDate` change
- `NotifySelectedWeekChanged()` / `LoadWeeklySummary()`
- `PersistLeaveMonth()` for affected date(s)
- Initial app load after leave/month data is available

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Extra `LeaveService` reads per week navigation | Cache leave entries for loaded months in VM when building week; fall back to `GetForDate` |
| Week spans two months | Load leave for both months when computing week deductions |
| Expected 0h breaks chart scale | Clamp display reference to `TimeSpan.Zero` with chart handling for empty benchmark |
| Month leave indicators stale after leave edit | Patch `MonthlyDayItem.LeaveDuration` / `LeaveType` in `RefreshMonthDayCell` extension without `LoadMonthlyUsage()` |

## Migration Plan

No data migration. Existing leave JSON files are consumed as-is. On first run after deploy, expected hours recalculate from stored leave.

## Open Questions

None — deduction rules and UI scope are fully specified.
