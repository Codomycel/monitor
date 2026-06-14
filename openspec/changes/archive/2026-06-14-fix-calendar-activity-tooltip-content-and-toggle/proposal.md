## Why

In the calendar/month view tile, the Total Active hours text tooltip is not working correctly. The horizontal total-active bar tooltip is already functioning properly. The Total Active text needs to show the same tooltip with breakdown details (Total Active, Active, Manual, Idle, Locked hours) and support hover/click toggle behavior.

## What Changes

- Fix Total Active hours text tooltip in month tile to show all five required values:
  - Total Active hrs
  - Active hrs
  - Manual hrs
  - Idle hrs
  - Locked hrs
- Reuse the same tooltip data/content that already works for the horizontal bar
- Ensure Total Active text shows tooltip on hover
- Implement click-to-toggle/pin behavior on Total Active text
- Ensure second click closes the pinned tooltip
- Fix empty tooltip issue by ensuring proper DataContext binding
- Ensure hover and pinned tooltips share the same data/content

## Capabilities

### New Capabilities
- `calendar-activity-tooltip`: Display detailed activity breakdown tooltip in calendar/month tile with toggle/pin behavior

### Modified Capabilities
- None (this is a bug fix to existing tooltip behavior, month tile only)

## Impact

- `Operon/Views/UiAMainWindow.xaml` - Month tile XAML with tooltip definitions
- `Operon/ViewModels/HorizontalActivityBarViewModel.cs` - Tooltip text properties
- `Operon/Views/UiAMainWindow.xaml.cs` - Tooltip click/toggle handlers

## Scope

**In Scope:** Calendar/month view tile Total Active hours text only
**Notes:**
- Horizontal total-active bar tooltip is already working correctly - do not modify
- Reuse the same tooltip data/content from the working horizontal bar
**Out of Scope:** Other views (Weekly, Daily), other UI elements, horizontal bar behavior
