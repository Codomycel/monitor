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

**Decision 3: Call existing ActivityChartViewModel.GetTotalActiveColor logic via extraction or duplication**
- Rationale: The color logic for "Total Active" already exists in ActivityChartViewModel. We should either:
  - Option A: Extract GetTotalActiveColor to a shared static helper class (preferred)
  - Option B: Duplicate the method in HorizontalActivityBarViewModel (acceptable if extraction is too invasive)
- Alternative considered: Creating new color logic - rejected to ensure color consistency.

**Decision 4: Use existing gradient color for Manual Tasks (temporary)**
- Rationale: ActivityChartViewModel does NOT have a Manual Tasks color. We'll use #FBA73C (orange from the existing gradient) as a temporary color, centralized in the ViewModel as a property for easy theme changes later.
- Alternative considered: Introducing new #F59E0B amber color - rejected as it's not in the existing chart.

**Decision 5: Centralize all colors in HorizontalActivityBarViewModel properties**
- Rationale: Even though we're reusing existing color values, we define them as properties in the ViewModel. This allows the user to change the theme later by modifying a single location.
- ManualBrush will use #FBA73C (marked as temporary in comments)

**Decision 6: Add HorizontalBarViewModel alongside existing ChartViewModel in MonthlyDayItem**
- Rationale: Maintains backward compatibility. Existing code referencing ChartViewModel continues to work.
- Alternative considered: Replacing ChartViewModel - rejected to avoid breaking existing functionality.

**Decision 7: Display Total Active hours text in HHh MMm format, centered in tile**
- Rationale: Requirement updated for HHh MMm format (e.g., "06h 00m"), larger font, centered both horizontally and vertically in the tile. The text is the primary visual element; the bar is a secondary compact indicator.

**Decision 8: Use 8-hour reference for bar length**
- Rationale: The bar uses 8 hours as the visual reference width. Segments are sized proportionally to 8 hours (not to total duration). For tracked time > 8 hours, the bar either extends or scales to remain visually understandable without breaking layout.
- Implementation: Calculate widths as (duration / 8 hours) * referenceWidth, with capping or scaling for overflow.

**Decision 9: Compact horizontal bar below centered text**
- Rationale: The bar should not occupy the full tile. It should be a compact indicator positioned below the centered Total Active text, using limited vertical space.
- Implementation: Use fixed or percentage-based bar height; position at bottom of tile; center the combined content vertically.

**Decision 10: Tooltip pin behavior with data binding fix**
- Rationale: The pinned tooltip was showing empty because the tooltip instance was created as a static resource without proper data context. Fix: Create tooltips with explicit DataContext bindings to HorizontalBarViewModel, ensuring the same tooltip content is used for hover and pinned states.
- Implementation: Use ToolTip with PlacementTarget and bind TextBlock.Text directly to ViewModel properties; ensure the tooltip DataContext is set correctly before showing.

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
