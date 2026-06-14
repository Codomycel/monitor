# Calendar to Manual Task Navigation Capability

## Scenario: Click highlighted date navigates to Manual Tasks

**GIVEN** the user sees a date tile with manual task highlight in activity month view
**WHEN** the user clicks that date tile
**THEN** the application navigates to the Manual Tasks tab
**AND** the Manual Tasks view opens in Date mode
**AND** the selected date matches the clicked date
**AND** manual tasks for that date are loaded

## Scenario: Navigation clears existing selection

**GIVEN** the user has an existing task selected in Manual Tasks
**WHEN** the user clicks a highlighted date in calendar
**THEN** any previous edit/selection state is cleared
**AND** the new date's tasks are shown

## Technical Requirements

- Click handler/command on calendar day tiles
- Tab switching mechanism (SelectedIndex or similar)
- ViewModel method to load manual tasks for specific date
- Navigation should not affect activity tracking or other tabs

## UX Requirements

- Clickable area: Entire highlighted date tile
- Visual feedback: Cursor change or subtle animation on hover
- No accidental triggers: Only highlighted dates are clickable for navigation
