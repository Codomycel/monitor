## Why
Currently, tracking starts automatically when the application opens. This removes user control and can record activity without explicit consent.

## What Changes
- Stop tracking from starting automatically on application launch
- Tracking starts only when user presses the Start button
- Default state on app launch is Stopped

## Capabilities

### New Capabilities
- `manual-tracking-start`: Tracking begins only after explicit user action (Start button)

### Modified Capabilities
- `start-stop-tracking`: Update current start/stop behavior to ensure startup defaults to Stopped

## Impact
- **Code**: App startup flow, tracking service initialization, UI default state
- **User Experience**: User must press Start to begin tracking