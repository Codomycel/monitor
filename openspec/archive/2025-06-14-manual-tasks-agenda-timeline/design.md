## Context

The Manual Tasks tab currently displays tasks for a single selected date. Users navigate between dates using a date picker, but cannot see tasks across multiple dates without repeatedly changing the selection. This creates friction when reviewing weekly or monthly task patterns.

The application uses WPF with MVVM pattern. Manual tasks are stored with timestamps and associated with specific dates. The existing Add/Edit/Delete flows are functional and should be preserved.

## Goals / Non-Goals

**Goals:**
- Provide three view modes (Date, Week, Month) for task visibility at different granularities
- Enable Previous/Next navigation through days, weeks, or months based on current view
- Display current selected period (date/week/month) with navigation controls
- Display tasks grouped by date with visual separation between date groups
- Show total manual duration per date in human-readable format
- Enable quick Edit/Delete actions on individual tasks
- Auto-refresh timeline after task modifications
- Reuse existing dialogs and flows without modification

**Non-Goals:**
- Modifying Add/Edit/Delete dialog designs
- Creating new color schemes or themes
- Adding task filtering, sorting, or search capabilities
- Implementing drag-and-drop reordering
- Adding export/print functionality
- Changing database schema or storage format

## Decisions

### 1. View Selector: Radio Buttons vs ComboBox
**Decision:** Use Radio Buttons for view mode selection
- **Rationale:** Only 3 options (Date/Week/Month), radio buttons provide immediate visibility of all choices without dropdown interaction
- **Alternative considered:** ComboBox - rejected because it adds an extra click and hides options

### 2. Timeline Layout: ItemsControl vs ListView vs DataGrid
**Decision:** Use ItemsControl with custom DataTemplate
- **Rationale:** Simplest layout for grouped date headers with nested task rows; no need for DataGrid's column features or ListView's selection model
- **DataTemplate structure:** Date Group Header (date + duration) → Task Items (title, time, actions)

### 3. Date Range Queries: Repository Pattern Extension
**Decision:** Extend existing task repository with date-range query methods
- **Rationale:** Keeps data access consistent with existing patterns; week query gets 7 days, month query gets calendar month
- **Caching:** Each ViewModel maintains its own loaded task collection; no shared static state

### 4. View State Persistence: In-Memory vs Settings
**Decision:** Do not persist selected view mode
- **Rationale:** Simpler implementation; user can quickly switch views; no settings pollution
- **Alternative considered:** Save last used view to settings - rejected as unnecessary complexity

### 5. Empty State Handling
**Decision:** Different behavior per view mode
- **Date mode:** Show explicit "No manual tasks for this date" message
- **Week/Month modes:** Only show dates that have tasks; completely empty states are possible but acceptable
- **Rationale:** Date mode is the "detail" view where user expects to see something; Week/Month are "overview" views where absence of tasks is normal

### 6. MVVM Structure: New ViewModel vs Extending Existing
**Decision:** Extend existing ManualTasksViewModel with timeline properties
- **Rationale:** Timeline is a view concern, not a separate feature; keeps task management logic centralized
- **New properties:** `TimelineViewMode` (enum), `TimelineItems` (grouped collection), `RefreshTimelineCommand`, `NavigatePreviousCommand`, `NavigateNextCommand`

### 7. Navigation Controls: Button Placement and Format
**Decision:** Place Previous/Next buttons flanking the period label
- **Rationale:** Standard calendar/agenda pattern; left arrow moves backward, right arrow moves forward, center shows current context
- **Layout:** `[<] [Current Period] [>]` where Current Period is formatted per view mode:
  - Date: "June 14, 2026"
  - Week: "Jun 14 - Jun 20, 2026"
  - Month: "June 2026"
- **Alternative considered:** Separate date picker navigation - rejected because it duplicates existing picker; this approach provides quick navigation within timeline

## Risks / Trade-offs

**Risk:** Timeline could become slow with many tasks in Month view
- **Mitigation:** Implement virtualized scrolling (ItemsControl with VirtualizingStackPanel); load tasks asynchronously; consider pagination if needed later

**Risk:** Week/Month modes might show too many dates, creating visual noise
- **Mitigation:** Only show dates with tasks; use collapsible date groups if needed; keep date header compact

**Risk:** Auto-refresh on every task change might cause flicker or performance issues
- **Mitigation:** Use ObservableCollection for incremental updates rather than full refresh; debounce rapid changes

**Risk:** Edit/Delete from timeline might be confusing if dialog expects different context
- **Mitigation:** Pass full task context to existing dialogs; dialogs remain unchanged and unaware of timeline view

## Migration Plan

Not applicable - this is a new feature addition with no breaking changes to existing functionality.

Deployment steps:
1. Implement timeline ViewModel extensions (including navigation commands)
2. Add XAML UI components (view selector, navigation controls with period label, timeline ItemsControl)
3. Wire up existing Add/Edit/Delete flows to trigger timeline refresh
4. Manual testing of all three view modes and navigation
5. No rollback strategy needed (feature is purely additive)

## Open Questions

None - design is ready for implementation.
