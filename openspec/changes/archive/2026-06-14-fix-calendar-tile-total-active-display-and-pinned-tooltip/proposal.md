## Why

The calendar/month view tile currently has two issues: (1) Total Active hours text is not displaying in the tile, and (2) the click-pinned tooltip opens empty while the hover tooltip works correctly. These bugs prevent users from seeing their daily activity summary at a glance and from keeping the tooltip open for reference.

## What Changes

- Fix Total Active text display in calendar tile to show in `HHh MMm` format (e.g., "06h 00m")
- Center the Total Active text horizontally and vertically in the tile's main content area
- Position a compact horizontal Total Active bar below the text
- Configure the bar to fill left-to-right using 8 hours as the full width reference
- Fix the click-pinned tooltip to display the same data as the hover tooltip (not empty)
- Ensure second click closes the pinned tooltip
- Tooltip should show breakdown: Total Active, Active, Manual Tasks, Idle, Locked

## Capabilities

### New Capabilities
- `calendar-tile-total-active-display`: Display Total Active hours text and progress bar in calendar/month tile with proper formatting and positioning

### Modified Capabilities
- `horizontal-activity-bar`: Modify tooltip behavior to support click-pin with correct data persistence

## Impact

- `Operon/Views/UiAMainWindow.xaml` - Calendar tile XAML template
- `Operon/ViewModels/HorizontalActivityBarViewModel.cs` - Total Active text format and bar calculation
- `Operon/Controls/HorizontalActivityBar.xaml` - Bar visualization and tooltip
- `Operon/Views/UiAMainWindow.xaml.cs` - Tooltip click handlers
