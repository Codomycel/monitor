## 1. Inspect Current Implementation

- [x] 1.1 Review `UiAMainWindow.xaml` tab order and `MainWindowTab` enum
- [x] 1.2 Review Monthly Usage calendar grid (`MonthlyCalendarDays`, `CalendarDayCellStyle`, `MonthlyDayItem`)
- [x] 1.3 Review `ManualTaskService`, `AppPaths`, and `JsonFile` persistence pattern
- [x] 1.4 Review `Styles/Styles.xaml` theme brush conventions
- [x] 1.5 Identify `LoadMonthlyUsage()` and month navigation hooks for leave data injection

## 2. Data Model and Storage

- [x] 2.1 Add `LeaveDuration` enum (FullDay, MorningHalf, AfternoonHalf)
- [x] 2.2 Add `LeaveType` enum (SickLeave, CasualLeave, EarnedLeave, CompOff)
- [x] 2.3 Add `LeaveEntry` model (Id, Date, Duration, Type)
- [x] 2.4 Add `AppPaths.GetLeavesPath(year, month)`
- [x] 2.5 Implement `LeaveService` with LoadMonth/SaveMonth/GetForDate and one-record-per-date enforcement

## 3. ViewModel — Leave Management

- [x] 3.1 Extend `MainWindowTab` enum with `Leaves` and update tab index usages
- [x] 3.2 Add leave form properties (selected date, duration, type, edit mode)
- [x] 3.3 Add `LeaveCalendarDays` collection and month navigation for Leaves tab
- [x] 3.4 Implement `LoadLeavesForSelectedMonth()` and leave calendar day item model
- [x] 3.5 Add commands: Add/Save leave, Delete leave, Cancel edit, Select leave date, Prev/Next month
- [x] 3.6 Enforce one leave per date in add/update logic
- [x] 3.7 Refresh leave calendar and activity month data after persist/delete

## 4. Leaves Tab UI

- [x] 4.1 Add Leaves `TabItem` to `UiAMainWindow.xaml` after Manual Tasks
- [x] 4.2 Add month/year navigation header (reuse `MonthYearPicker` or equivalent pattern)
- [x] 4.3 Add Leaves month calendar grid with leave-highlighted day cells
- [x] 4.4 Add leave detail form (date, duration combo, type combo, Add/Save/Delete/Cancel)
- [x] 4.5 Wire calendar day click to load leave details or prepare new entry
- [x] 4.6 Add leave-specific calendar cell styling for identifiable leave dates

## 5. Activity Month Leave Indicators

- [x] 5.1 Add `LeaveDuration`/`LeaveType` properties to `MonthlyDayItem`
- [x] 5.2 Populate leave properties in `LoadMonthlyUsage()` via `LeaveService`
- [x] 5.3 Add `LeaveIndicatorBrush` (and related) to `Styles/Styles.xaml`
- [x] 5.4 Add left-border overlay to activity month `DataTemplate` for full-day leave
- [x] 5.5 Add top-half left border for morning half-day leave
- [x] 5.6 Add bottom-half left border for afternoon half-day leave
- [x] 5.7 Verify indicator layers cleanly with manual-task highlight and activity bar

## 6. Integration and Verification

- [x] 6.1 Verify manual task highlight and calendar navigation still work unchanged
- [x] 6.2 Verify activity tracking and unrelated tabs are unaffected
- [x] 6.3 Manual check: add/update/delete all duration and type combinations
- [x] 6.4 Manual check: Leaves tab calendar and activity month indicators stay in sync
- [x] 6.5 Build and verify no compilation errors
