## Purpose

Enable navigation from the activity month/calendar view to the Manual Tasks tab for a specific highlighted date.

## Requirements

### Requirement: Highlighted calendar date navigates to Manual Tasks
The application SHALL navigate to the Manual Tasks tab in Date view when the user clicks a highlighted calendar date tile, loading manual tasks for that date.

#### Scenario: Click highlighted date opens Manual Tasks in Date view
- **WHEN** the user clicks a calendar date tile with `HasManualTasks` true in the activity month view
- **THEN** the application selects the Manual Tasks tab via `MainWindowTab.ManualTasks`
- **AND** `TimelineViewMode` is set to Date
- **AND** `ManualTasksDate` and `TimelineCurrentDate` are set to the clicked date
- **AND** manual tasks for that date are loaded into the Date view grid

#### Scenario: Non-highlighted dates do not navigate
- **WHEN** the user clicks a calendar date tile without manual tasks
- **THEN** the application does not navigate to the Manual Tasks tab

#### Scenario: Navigation clears edit and selection state
- **WHEN** the user clicks a highlighted calendar date while Manual Tasks has an active edit or selection
- **THEN** edit/selection state is cleared before the new date's tasks are shown

### Requirement: Calendar navigation uses existing command wiring
The calendar SHALL use `NavigateToManualTasksFromCalendarCommand`, delegating to `NavigateToManualTasksForDate(DateTime date)` in the view model.

#### Scenario: Command receives calendar day item
- **WHEN** a highlighted day tile raises a left-click command
- **THEN** `NavigateToManualTasksFromCalendar` resolves the clicked date and calls `NavigateToManualTasksForDate`
- **AND** weekly summary tiles and dates outside the current month are ignored
