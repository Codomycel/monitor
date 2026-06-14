## Context

The existing HorizontalActivityBar control was implemented to display Total Active hours and a progress bar in calendar tiles. However, there are two bugs:

1. **Total Active text not displaying**: The text binding is not showing in the calendar tile
2. **Click-pinned tooltip empty**: When clicking to pin the tooltip, it opens empty while hover tooltip shows correct data

The current implementation uses:
- `UiAMainWindow.xaml` - Calendar tile DataTemplate with HorizontalActivityBar
- `HorizontalActivityBarViewModel.cs` - ViewModel with TotalActiveText property
- `HorizontalActivityBar.xaml` - The bar control with embedded tooltip
- `UiAMainWindow.xaml.cs` - Click handlers for tooltip pinning

## Goals / Non-Goals

**Goals:**
- Fix Total Active text display in calendar tile in `HHh MMm` format
- Center text horizontally and vertically in the tile's main content area
- Show compact horizontal bar below the text using 8h as reference width
- Fix click-pinned tooltip to show same data as hover tooltip
- Ensure pinned tooltip can be closed with second click

**Non-Goals:**
- No changes to other views (Weekly, Daily)
- No new colors or styling changes
- No changes to tracking engine or data layer
- No changes to ActivityChartViewModel

## Decisions

**Decision 1: Use inline ToolTip instead of shared resource**
- Rationale: The shared ToolTip as a DataTemplate resource loses DataContext when pinned. Inline ToolTips on each element maintain proper binding.
- Alternative considered: Code-behind tooltip creation - rejected as more complex

**Decision 2: Bind TotalActiveText directly from ViewModel**
- Rationale: The text should come from HorizontalBarViewModel which has the correct calculated value (Active + Manual)
- Alternative considered: Calculating in XAML - rejected as duplication of logic

**Decision 3: Bar shows Total Active only (not segment breakdown)**
- Rationale: The bar is a quick visual indicator of Total Active vs 8h reference. Tooltip shows the detailed breakdown.
- The bar fill represents Total Active / 8 hours

**Decision 4: Simplify tooltip pin implementation**
- Rationale: Instead of complex shared tooltip management, use individual element tooltips with synchronized data via ViewModel
- Click handler toggles IsOpen on the element's own tooltip instance

## Risks / Trade-offs

**Risk: Tooltip positioning with inline definitions**
→ Mitigation: Ensure each ToolTip has proper PlacementTarget and Placement set

**Risk: Memory overhead with multiple inline tooltips**
→ Mitigation: ToolTips are lightweight; only 2 per tile (text and bar)

**Risk: Data binding not updating when pinned**
→ Mitigation: ViewModel implements INotifyPropertyChanged; tooltips bind directly to ViewModel properties

## Migration Plan

1. Inspect current XAML bindings in UiAMainWindow.xaml
2. Update TotalActiveText binding to ensure it's using HorizontalBarViewModel
3. Move ToolTip definitions inline on TextBlock and HorizontalActivityBar
4. Verify click handlers properly toggle tooltip IsOpen
5. Build and test

Rollback: Revert XAML changes to previous working state.

## Open Questions

None - requirements are clear from bug report.
