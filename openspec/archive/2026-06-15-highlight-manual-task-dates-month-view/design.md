## Context

This feature adds visual highlighting for dates with manual tasks in the activity month/calendar view and enables click-to-navigate functionality.

## Goals

1. Visually identify dates with manual tasks in calendar view
2. Enable quick navigation from calendar to manual tasks for specific dates
3. Maintain existing theme/styling patterns
4. Preserve current behavior for dates without manual tasks

## Design Decisions

### Visual Highlight
- Light green gradient background for dates with manual tasks
- Use existing resource dictionary for themeable colors
- Gradient provides visual depth without being distracting
- Only applies to calendar day tiles in month view

### Navigation Flow
1. User sees highlighted date in calendar
2. User clicks highlighted date tile
3. App navigates to Manual Tasks tab
4. Manual Tasks opens in Date view
5. Selected date is set to clicked date
6. Manual tasks for that date are loaded

### Technical Approach
- Query ManualTaskService to check for task existence per date
- Use Background/Opacity bound properties for visual state
- Add command binding for click navigation
- Leverage existing SelectedDate binding pattern

## Risks & Mitigation

| Risk | Mitigation |
|------|------------|
| Performance impact checking all dates | Query only visible month dates, cache results |
| Theme compatibility | Use existing brush resources, test with all themes |
| Conflicting with existing tile colors | Ensure gradient overlays cleanly |

## Migration Plan

No migration needed - this is a pure feature addition with no breaking changes.

## Open Questions

None
