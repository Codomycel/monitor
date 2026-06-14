## Purpose

Visually identify activity month/calendar dates that have manual task entries.

## Requirements

### Requirement: Dates with manual tasks are highlighted
The activity month view SHALL apply a light green gradient background to calendar day tiles that have one or more manual tasks.

#### Scenario: Date with manual tasks shows highlight
- **WHEN** a calendar day tile renders and `HasManualTasks` is true
- **THEN** the tile uses `ManualTaskHighlightBrush` as its background
- **AND** the tile border uses `ManualTaskHighlightBorderBrush`
- **AND** the cursor changes to Hand

#### Scenario: Date without manual tasks remains default
- **WHEN** a calendar day tile renders and `HasManualTasks` is false
- **THEN** the tile uses the default calendar styling without the manual-task highlight

#### Scenario: Highlighted date hover feedback
- **WHEN** the user hovers a highlighted calendar date tile
- **THEN** the tile uses `ManualTaskHighlightHoverBrush` and `ManualTaskHighlightHoverBorderBrush`

#### Scenario: Weekend styling does not override manual-task highlight
- **WHEN** a weekend date has manual tasks
- **THEN** the manual-task highlight styling takes precedence over the default weekend background

### Requirement: Highlight data is derived from manual task storage
The view model SHALL set `MonthlyDayItem.HasManualTasks` during month load using `HasManualTasksForDate(DateTime date)` backed by `ManualTaskService`.

#### Scenario: Month reload refreshes highlight flags
- **WHEN** monthly calendar data is loaded
- **THEN** each in-month day tile receives `HasManualTasks` based on whether manual tasks exist for that date

### Requirement: Highlight brushes are theme resources
Highlight colors SHALL be defined in the application theme resource dictionary (`Styles/Styles.xaml`) so they can be reused across views.

#### Scenario: Calendar style references shared brushes
- **WHEN** `CalendarDayCellStyle` applies manual-task highlighting
- **THEN** it references shared static resources rather than inline-only colors
