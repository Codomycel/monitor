# Timeline View Specification

## Feature: Manual Tasks Agenda Timeline

### Capability: View Mode Selection

**Scenario: User switches between Date, Week, and Month views**
- **Given** the user is on the Manual Tasks tab
- **When** the user clicks the view mode selector
- **Then** three options are visible: Date, Week, Month
- **And** the currently selected view is visually highlighted

**Scenario: Date view displays single date tasks**
- **Given** the user has selected "Date" view mode
- **When** the timeline renders
- **Then** tasks for the currently selected date are displayed
- **And** if no tasks exist, "No manual tasks for this date" message is shown

**Scenario: Week view displays tasks grouped by date**
- **Given** the user has selected "Week" view mode
- **When** the timeline renders
- **Then** only dates within the selected week that have manual tasks are shown
- **And** dates are sorted chronologically
- **And** each date group shows the date and total duration

**Scenario: Month view displays tasks grouped by date**
- **Given** the user has selected "Month" view mode
- **When** the timeline renders
- **Then** only dates within the selected month that have manual tasks are shown
- **And** dates are sorted chronologically
- **And** each date group shows the date and total duration

### Capability: Previous/Next Navigation

**Scenario: Date mode navigates to previous/next day**
- **Given** the user has selected "Date" view mode
- **And** the current selected date is displayed in the period label
- **When** the user clicks the "Previous" button
- **Then** the selected date moves to the previous day
- **And** the timeline refreshes to show tasks for the new date
- **When** the user clicks the "Next" button
- **Then** the selected date moves to the next day
- **And** the timeline refreshes to show tasks for the new date

**Scenario: Week mode navigates to previous/next week**
- **Given** the user has selected "Week" view mode
- **And** the current week range is displayed in the period label (e.g., "Jun 14 - Jun 20")
- **When** the user clicks the "Previous" button
- **Then** the selected week moves to the previous week
- **And** the timeline refreshes to show dates with tasks from the new week
- **When** the user clicks the "Next" button
- **Then** the selected week moves to the next week
- **And** the timeline refreshes to show dates with tasks from the new week

**Scenario: Month mode navigates to previous/next month**
- **Given** the user has selected "Month" view mode
- **And** the current month is displayed in the period label (e.g., "June 2026")
- **When** the user clicks the "Previous" button
- **Then** the selected month moves to the previous month
- **And** the timeline refreshes to show dates with tasks from the new month
- **When** the user clicks the "Next" button
- **Then** the selected month moves to the next month
- **And** the timeline refreshes to show dates with tasks from the new month

**Scenario: Period label updates with navigation**
- **Given** the user navigates using Previous/Next buttons
- **When** the selected period changes
- **Then** the period label immediately updates to reflect the new date/week/month

### Capability: Date Group Display

**Scenario: Date group shows header with date and duration**
- **Given** a date has manual tasks
- **When** the date group renders in Week or Month view
- **Then** the header displays: date (e.g., "Mon, Jun 14")
- **And** the header displays total duration (e.g., "2h 30m")

### Capability: Task Row Display

**Scenario: Task row shows essential information**
- **Given** a manual task exists
- **When** the task row renders
- **Then** the row displays: task title/name
- **And** the row displays: time or duration (e.g., "09:00 - 10:30" or "1h 30m")
- **And** the row displays: Edit button
- **And** the row displays: Delete button

### Capability: Task Actions

**Scenario: User edits task from timeline**
- **Given** the user sees a task row
- **When** the user clicks the Edit button
- **Then** the existing Edit Task dialog opens
- **And** the dialog is pre-populated with task data
- **And** after saving, the timeline refreshes automatically

**Scenario: User deletes task from timeline**
- **Given** the user sees a task row
- **When** the user clicks the Delete button
- **Then** the existing Delete confirmation appears
- **And** after confirming, the task is removed
- **And** the timeline refreshes automatically

**Scenario: Add Task refreshes timeline**
- **Given** the user has the timeline open
- **When** the user adds a new task via the Add button
- **And** the task is saved successfully
- **Then** the timeline automatically refreshes to show the new task

### Capability: Visual Design

**Scenario: Timeline uses existing theme**
- **Given** the timeline renders
- **Then** colors match the existing application theme
- **And** fonts match the existing application theme
- **And** spacing is consistent with existing UI patterns
- **And** no new color schemes are introduced

## Constraints

- View selector must use radio buttons (not dropdown)
- Week view shows 7-day week based on selected date
- Month view shows calendar month based on selected date
- Only dates with tasks appear in Week/Month views (no empty date groups)
- Date view shows explicit empty state message when no tasks
- Task rows must show Edit and Delete actions
- Timeline must auto-refresh after Add/Edit/Delete operations
- Must use existing Add/Edit/Delete dialogs without modification
