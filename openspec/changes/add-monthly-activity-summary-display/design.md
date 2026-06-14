## Context

The current monthly view in `UiAMainWindow.xaml` displays activity data using a vertical `ActivityChart` control (LiveCharts column chart). This chart includes axes, legend, and takes 90-110px of vertical space per calendar cell. The requirement is to provide a more compact visualization that shows Total Active hours clearly alongside a horizontal stacked bar.

The existing architecture:
- `MonthlyDayItem` holds daily activity data and has a `ChartViewModel` property
- `ActivityChartViewModel` manages vertical chart data and color logic
- Colors are hardcoded in `ActivityChartViewModel` using the same gradient for "Total Active"
- The monthly view uses an 8-column `UniformGrid` (7 days + 1 weekly summary)

## Goals / Non-Goals

**Goals:**
- Create a reusable `HorizontalActivityBar` control for compact activity visualization
- Display Total Active hours text in each calendar day cell
- Show horizontal stacked bar with segments: Active, Manual Tasks, Idle, Locked
- Implement interactive tooltip with pin behavior on both text and bar
- Reuse existing color logic (no new colors)
- Keep existing `ChartViewModel` intact for backward compatibility

**Non-Goals:**
- No changes to other views (Weekly, Daily, Application Usage)
- No new theme colors or styling changes
- No changes to data persistence or tracking logic
- No changes to existing ActivityChartViewModel behavior

## Decisions

**Decision 1: Create new HorizontalActivityBarViewModel instead of reusing ActivityChartViewModel**
- Rationale: ActivityChartViewModel is tightly coupled to LiveCharts column series and vertical layout. A dedicated ViewModel allows cleaner separation of concerns and simpler binding for horizontal grid-based layout.
- Alternative considered: Reusing ActivityChartViewModel with new properties - rejected due to tight coupling with LiveCharts types.

**Decision 2: Use Grid with proportional ColumnDefinitions (Star sizing based on seconds)**
- Rationale: Simple, lightweight approach that scales segments proportionally without external charting libraries.
- Alternative considered: Using LiveCharts stacked bar - rejected to avoid dependency and keep the control lightweight.

**Decision 3: Duplicate color interpolation logic from ActivityChartViewModel**
- Rationale: The gradient logic for "Total Active" color is simple and duplicating it keeps HorizontalActivityBarViewModel self-contained without creating new shared abstractions.
- Alternative considered: Extracting shared color helper - deferred to keep changes localized.

**Decision 4: Add HorizontalBarViewModel alongside existing ChartViewModel in MonthlyDayItem**
- Rationale: Maintains backward compatibility. Existing code referencing ChartViewModel continues to work.
- Alternative considered: Replacing ChartViewModel - rejected to avoid breaking existing functionality.

**Decision 5: Tooltip pin behavior via Click event handling in XAML/code-behind**
- Rationale: WPF ToolTipService doesn't natively support "pin on click" behavior. Will require attached behavior or code-behind to track pinned state.
- Alternative considered: Custom tooltip control - rejected as overkill for this requirement.

## Risks / Trade-offs

**Risk: Corner radius handling for first/last visible segments**
→ Mitigation: Add computed boolean properties to ViewModel (ShowOnlyActive, ShowManualEnd, etc.) and use multiple Border elements with different corner radius configurations.

**Risk: Namespace conflicts (System.Windows.Forms vs System.Windows.Media)**
→ Mitigation: Use fully qualified type names (System.Windows.Media.Brush, System.Windows.Media.Color) throughout the new files.

**Risk: Zero total duration causes layout issues**
→ Mitigation: Show empty background bar when HasData=false; Grid column widths set to 0 for all segments.

**Risk: Tooltip stays open and blocks UI**
→ Mitigation: Implement click-to-pin with explicit close on second click; use ToolTipService.SetShowDuration for non-pinned tooltips.

## Migration Plan

1. Create HorizontalActivityBarViewModel.cs
2. Create HorizontalActivityBar.xaml and .xaml.cs
3. Update MonthlyDayItem to add HorizontalBarViewModel property
4. Update MainWindowViewModel.BuildMonthlyCalendar() to populate HorizontalBarViewModel
5. Update UiAMainWindow.xaml calendar item template to show Total Active text and HorizontalActivityBar
6. Build and test

Rollback: Revert XAML changes to restore ActivityChart; HorizontalActivityBar can remain in codebase as unused code.

## Open Questions

None - requirements are clear from user specification.
