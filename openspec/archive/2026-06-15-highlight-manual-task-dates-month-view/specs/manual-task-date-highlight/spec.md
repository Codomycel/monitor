# Manual Task Date Highlight Capability

## Scenario: Date with manual tasks shows visual highlight

**GIVEN** a date in the activity month view has one or more manual tasks
**WHEN** the calendar tile renders
**THEN** the tile shows a light green gradient background

## Scenario: Date without manual tasks appears normal

**GIVEN** a date in the activity month view has no manual tasks
**WHEN** the calendar tile renders
**THEN** the tile shows the default background (no highlight)

## Scenario: Highlight is themeable

**GIVEN** the application uses a theme resource dictionary
**WHEN** the highlight renders
**THEN** it uses theme-defined brushes for the gradient colors

## Visual Design

- Gradient: Light green, subtle (10-20% opacity suggested)
- Direction: Top-to-bottom or left-to-right
- Applies to: Calendar day tile background only
- Does not affect: Text color, borders, or other tile elements

## Technical Requirements

- Check ManualTaskService for task count per date during tile binding
- Use DataTrigger or converter for conditional styling
- Background should be bindable to ViewModel property
