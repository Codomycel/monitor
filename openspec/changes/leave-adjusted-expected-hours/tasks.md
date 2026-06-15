## 1. Inspect Current Implementation

- [x] 1.1 Review Selected Day panel bindings in `UIAShell.xaml` and related VM properties
- [x] 1.2 Review Week Report card (`WeekReportHeaderText`, `SelectedWeekRangeText`, weekly chart) in `UIAShell.xaml` and `UiAMainWindow.xaml`
- [x] 1.3 Review `ActivityChartViewModel.ReferenceTime` usage for day and week charts
- [x] 1.4 Review `PersistLeaveMonth()`, `RefreshDate()`, `RefreshWeekIfNeeded()`, and month cell patch patterns
- [x] 1.5 Confirm no separate `WeekNumberText` row exists in week panel UI

## 2. Shared Expected Hours Calculator

- [x] 2.1 Add `ExpectedHoursCalculator` in `Operon/Utilities/` with constants (8h day, 40h week)
- [x] 2.2 Implement `GetLeaveDeductionHours(LeaveDuration?)` (0 / 4 / 8)
- [x] 2.3 Implement `GetDayExpectedHours(LeaveDuration?)` with 0h floor
- [x] 2.4 Implement `GetWeekExpectedHours` summing deductions across 7 calendar days with 0h floor

## 3. ViewModel — Selected Day Expected Hours

- [x] 3.1 Add `SelectedDayExpectedHours`, `SelectedDayExpectedHoursText`, `HasSelectedDayLeave`, `SelectedDayLeaveSummaryText`
- [x] 3.2 Implement `RefreshSelectedDayExpectedHours()` using `LeaveService.GetForDate(SelectedDate)`
- [x] 3.3 Wire `UpdateActivityChart()` to set day chart `ReferenceTime` from leave-adjusted expected hours
- [x] 3.4 Call refresh from `RefreshForSelectedDate()`, `SelectedDate` change, and leave CRUD when date matches

## 4. ViewModel — Week Report Expected Hours

- [x] 4.1 Add `WeekExpectedHours`, `WeekExpectedHoursText`, `HasWeekLeave`, `WeekLeaveSummaryText`
- [x] 4.2 Implement `RefreshWeekExpectedHours()` loading leave for all 7 days in `SelectedWeekStart` week (handle cross-month weeks)
- [x] 4.3 Wire `UpdateWeeklyActivityChart()` to set week chart `ReferenceTime` from leave-adjusted expected hours
- [x] 4.4 Call refresh from `NotifySelectedWeekChanged()`, `LoadWeeklySummary()`, and leave CRUD when date is in selected week

## 5. Targeted Refresh on Leave CRUD

- [x] 5.1 Add `RefreshLeaveSurfacesForDate(DateTime date)` orchestrating day, week, and month cell updates
- [x] 5.2 Replace full `LoadMonthlyUsage()` in `PersistLeaveMonth()` with in-place month cell leave patch when activity month is visible
- [x] 5.3 Ensure leave add/update/delete does not trigger unnecessary full month rebuild
- [x] 5.4 Reuse existing `RefreshWeekIfNeeded` / `RefreshDate` patterns where applicable without duplicating load logic

## 6. UI — Selected Day Panel

- [x] 6.1 Add leave summary row/section in `UIAShell.xaml` bound to `SelectedDayLeaveSummaryText`
- [x] 6.2 Show leave section only when `HasSelectedDayLeave` is true
- [x] 6.3 Display leave-adjusted expected hours context (label or benchmark text) on Selected Day

## 7. UI — Week Report Panel

- [x] 7.1 Add week leave summary below range text in `UIAShell.xaml` and `UiAMainWindow.xaml` week card
- [x] 7.2 Show summary only when `HasWeekLeave` is true
- [x] 7.3 Display week expected hours near totals or chart
- [x] 7.4 Verify header remains `Week Report (WK{n})` with no separate week-number row

## 8. Integration and Verification

- [x] 8.1 Manual check: full-day leave → day 0h expected, week −8h
- [x] 8.2 Manual check: half-day leave → day 4h expected, week −4h
- [x] 8.3 Manual check: multiple leave days in one week stack deductions correctly; week never below 0h
- [x] 8.4 Manual check: leave CRUD updates only affected Selected Day / Week Report without full month rebuild
- [x] 8.5 Manual check: week navigation (prev/next/picker/current week) updates header and expected hours
- [x] 8.6 Build and verify no compilation errors
