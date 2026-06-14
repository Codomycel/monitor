## Context

The calendar/month view tile has a Total Active text that should display a tooltip with activity breakdown details. The horizontal activity bar tooltip is already working correctly. Currently for the Total Active text:
1. The tooltip content may be incomplete or empty
2. The click-to-toggle/pin behavior may not be working correctly
3. The tooltip may not share the same data source as the horizontal bar

The existing working implementation for the horizontal bar uses:
- `HorizontalActivityBarViewModel` with tooltip text properties (`TooltipTotalActive`, `TooltipActive`, `TooltipManual`, `TooltipIdle`, `TooltipLocked`)
- Inline ToolTip definitions in XAML with direct bindings to ViewModel
- Click handlers in `UiAMainWindow.xaml.cs` for toggling tooltip IsOpen

The Total Active text needs to reuse this same approach.

## Goals / Non-Goals

**Goals:**
- Fix tooltip on month tile Total Active hours text only
- Ensure Total Active text tooltip displays all five required values: Total Active, Active, Manual, Idle, Locked
- Reuse the same tooltip data/content that already works for the horizontal bar
- Fix click-to-toggle behavior on Total Active text
- Ensure second click closes the pinned tooltip
- Ensure hover and pinned tooltips share the same data/content

**Non-Goals:**
- No changes to horizontal bar tooltip (already working)
- No changes to other views (Weekly, Daily)
- No new colors or styling changes
- No changes to data persistence or tracking logic

## Decisions

**Decision 1: Scope limited to Total Active text in month tile**
- Rationale: The fix should only affect the Total Active hours text in the calendar/month view tile
- The horizontal bar tooltip is already working - do not modify
- Weekly and Daily views should remain unaffected

**Decision 2: Reuse horizontal bar tooltip pattern for Total Active text**
- Rationale: The horizontal bar has a working tooltip with direct ViewModel bindings; the Total Active text should use the same approach
- Use inline ToolTip with direct bindings to `HorizontalBarViewModel` properties
- This ensures both elements share the same data source and prevents "empty tooltip" issues

**Decision 3: Use existing ToggleTooltipPin helper**
- Rationale: The existing `ToggleTooltipPin` method in code-behind already works for the horizontal bar
- Wire the Total Active text to use the same handler with proper placement target

**Decision 4: Total Active text tooltip bindings via HorizontalBarViewModel**
- Rationale: The ViewModel already has working `TooltipTotalActive`, `TooltipActive`, `TooltipManual`, `TooltipIdle`, `TooltipLocked` properties used by the horizontal bar
- The Total Active text should bind to the same properties via `DataContext.HorizontalBarViewModel.TooltipXXX`

**Decision 5: Ensure ToolTipService properties are set correctly**
- Rationale: `ToolTipService.ShowDuration` and `ToolTipService.InitialShowDelay` should be set on the element (not the ToolTip) for consistent behavior

## Risks / Trade-offs

**Risk: ToolTip content not updating when data changes**
→ Mitigation: Ensure ViewModel implements `INotifyPropertyChanged` and fires property change events for tooltip properties

**Risk: Click handler not triggering properly**
→ Mitigation: Ensure `MouseLeftButtonDown` event is handled and `e.Handled = true` is set to prevent bubbling issues

**Risk: Pinned tooltip stays open when clicking elsewhere**
→ Mitigation: Track `_activeTooltip` in code-behind and close it when clicking a different element

**Risk: Changes affect other views**
→ Mitigation: Scope XAML changes to month tile DataTemplate only; avoid global changes

## Migration Plan

1. Inspect month tile XAML DataTemplate for tooltip definitions
2. Verify all five tooltip text properties exist in HorizontalActivityBarViewModel
3. Fix any missing or incorrect bindings in month tile only
4. Verify click handlers are properly wired for month tile elements
5. Build and test month tile specifically
6. Verify Weekly and Daily views are unaffected

Rollback: Revert XAML and code-behind changes.

## Open Questions

None - requirements are clear from bug report.
