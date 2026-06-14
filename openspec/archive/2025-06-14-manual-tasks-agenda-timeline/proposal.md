# Manual Tasks Agenda Timeline

## Why

Manual tasks are currently only visible for one selected date at a time. Users need a more efficient way to view and manage tasks across dates, weeks, and months without constantly switching dates. An agenda-style timeline view will provide better task visibility and improve workflow efficiency.

## What Changes

- Add new "Agenda Timeline" view to the Manual Tasks tab
- Add view selector with three modes: Date, Week, Month
- Date mode: display tasks for the currently selected date (existing behavior, enhanced with empty state message)
- Week mode: display all dates in the selected week that have manual tasks, grouped by date
- Month mode: display all dates in the selected month that have manual tasks, grouped by date
- Add Previous / Next navigation buttons based on selected mode:
  - Date mode: navigate to previous/next day
  - Week mode: navigate to previous/next week
  - Month mode: navigate to previous/next month
- Show current selected date/week/month label near the navigation controls
- Each date group shows: date header, total manual duration for that date, and individual task rows
- Each task row shows: task title/name, time/duration, Edit button, Delete button
- Reuse existing Add Task dialog and Edit/Delete flows without modification
- Timeline auto-refreshes after adding, editing, or deleting tasks

## Capabilities

- **View tasks by time period**: Switch between Date, Week, and Month views to see tasks at different granularities
- **Navigate time periods**: Previous/Next buttons move through days, weeks, or months based on current view mode
- **Current period indicator**: Label shows the currently selected date, week, or month near navigation controls
- **Grouped task display**: Tasks are grouped by date with clear visual separation
- **Duration summary**: Each date group shows total manual task duration in `HHh MMm` format
- **Quick task management**: Edit and Delete buttons on each task row for immediate action
- **Smart empty states**: Date mode shows "No manual tasks for this date" when empty; Week/Month modes only show dates with tasks
- **Seamless integration**: New Add Task saves automatically refresh the timeline

## Impact

### Affected Components
- Manual Tasks tab UI (`UiAMainWindow.xaml` Manual Tasks section)
- Manual Tasks ViewModel (`ManualTasksViewModel.cs` or equivalent)
- Task service/data layer (for date-range queries)

### APIs/Dependencies
- Existing Manual Task CRUD operations (Add, Edit, Delete)
- Existing date selection mechanism
- Existing theme/styles (reused without modification)

### Backward Compatibility
- No breaking changes to existing Add/Edit/Delete flows
- Existing single-date view preserved as "Date" mode default

## Scope

### In Scope
- Agenda Timeline view with Date/Week/Month selector
- Previous/Next navigation for day/week/month based on view mode
- Current period label (date/week/month) near navigation
- Date grouping and duration summaries
- Task row rendering with Edit/Delete actions
- Timeline refresh on task changes
- Reuse of existing task dialogs and flows

### Out of Scope
- Modifying Add/Edit/Delete dialogs
- New color schemes or visual themes
- Task filtering or search
- Task sorting customization
- Export/print functionality
- Mobile/responsive layout changes
