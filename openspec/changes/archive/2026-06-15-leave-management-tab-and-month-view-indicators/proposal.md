## Why

Users may take leave on some workdays, but the app has no way to record or visualize leave. Without leave tracking, the activity month view cannot distinguish leave days from regular workdays, and users must track leave outside the app.

This change adds a dedicated Leaves tab for managing leave entries and visual leave indicators on the existing activity month calendar so users can see full-day and half-day leave at a glance without disrupting manual-task highlights or activity bars.

## What Changes

- Add a new **Leaves** tab to `UiAMainWindow` with month calendar navigation and leave entry form
- Support leave duration: Full day, Morning half day, Afternoon half day
- Support leave type: Sick Leave, Casual Leave, Earned Leave, Comp Off
- Allow add, update, and delete of leave entries (one record per date)
- Persist leave data using the existing JSON file storage pattern (`AppPaths` + `JsonFile`)
- Show leave dates visually in the Leaves tab calendar
- Show themeable left-border leave indicators on activity month view tiles:
  - Full day: full-height thick left border
  - Morning half day: top-half thick left border
  - Afternoon half day: bottom-half thick left border
- Layer leave indicators alongside existing manual-task highlight and activity bar UI

## Capabilities

### New Capabilities

- `leave-management`: Leaves tab CRUD, one leave record per date, month calendar view, persistent storage
- `activity-month-leave-indicator`: Themeable left-border leave indicators on activity month view date tiles

### Modified Capabilities

None — manual task highlight, calendar navigation, and activity tracking behavior remain unchanged.

## Impact

### Affected Code

- `Operon/Views/UiAMainWindow.xaml` — new Leaves tab, activity month tile template for leave border overlay
- `Operon/ViewModels/MainWindowViewModel.cs` — leave properties, commands, month calendar data, `MainWindowTab` enum
- `Operon/Models/` — new `LeaveEntry` model and enums (`LeaveDuration`, `LeaveType`)
- `Operon/Services/` — new `LeaveService` (mirrors `ManualTaskService` pattern)
- `Operon/Utilities/AppPaths.cs` — leave file path helper
- `Operon/Styles/Styles.xaml` — theme brushes for leave indicators

### Dependencies

- Existing MVVM command patterns (`RelayCommand`)
- Existing month/year picker (`MonthYearPicker`) and calendar grid patterns from Monthly Usage tab
- `System.Text.Json` (already in use)

### User Experience Changes

- New Leaves tab between existing tabs (after Manual Tasks)
- Activity month view shows leave borders without removing manual-task green highlight or activity bars
- Clicking a leave date in Leaves tab loads details for edit/delete

### Non-Goals

- No changes to manual task add/edit/delete logic
- No changes to activity tracking logic
- No automated tests
- No new NuGet packages
