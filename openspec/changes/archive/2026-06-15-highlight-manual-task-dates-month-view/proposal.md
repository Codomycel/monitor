## Why

### Problem
In the activity month/calendar view, dates that have manual tasks are not visually identifiable. Users cannot quickly see which days have manual task entries without clicking through each date or navigating to the Manual Tasks tab and checking individual dates.

### Motivation
Users need a visual cue in the calendar view to identify dates with manual tasks at a glance. This improves usability by:
- Providing immediate visual feedback about manual task presence
- Reducing the number of clicks needed to find dates with manual tasks
- Creating a better connection between the calendar view and manual tasks functionality

## What Changes

### New Capabilities
- **manual-task-date-highlight**: Activity month view will visually highlight dates that have one or more manual tasks with a light green gradient background
- **calendar-to-manual-task-navigation**: Clicking on a highlighted date tile will navigate to the Manual Tasks tab with that date pre-selected in Date view

### Modified Capabilities
None - this is a pure enhancement that doesn't change existing behavior for dates without manual tasks.

## Capabilities

### New Capabilities
- `manual-task-date-highlight`: Calendar date tiles show visual indicator when manual tasks exist for that date
- `calendar-manual-task-navigation`: Clicking highlighted date navigates to Manual Tasks tab with that date selected

### Modified Capabilities
None

## Impact

### Affected Code
- Activity month view tile rendering (XAML and ViewModel)
- Manual task date lookup logic
- Tab navigation between Activity and Manual Tasks tabs
- Theme/style resources for the highlight background

### Dependencies
- ManualTaskService for date-based task lookup
- Existing navigation framework
- Current theme/styling system

### User Experience Changes
- Visual highlight on calendar dates with manual tasks
- Click-to-navigate functionality on highlighted dates
- No change to dates without manual tasks
