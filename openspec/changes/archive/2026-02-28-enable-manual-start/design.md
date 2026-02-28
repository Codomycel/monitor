## Context
The application currently starts tracking automatically during app launch (startup/init flow).
We want tracking to start only when the user presses the Start button.

## Goals / Non-Goals

**Goals**
- App launch MUST default to Tracking = Stopped
- Tracking starts only on explicit user action (Start)
- UI state (buttons/labels) must always match actual tracking state
- No background tracking loops run unless Tracking = Running

**Non-Goals**
- Changing how tracking data is stored
- Changing tracking accuracy/logic (only startup behavior + state control)
- Adding Windows auto-start or scheduled start features

## Decisions

### 1) Default state on launch: Stopped
**Decision**: On application startup, initialize the tracking state as `Stopped` and do not call `StartTracking()` automatically.

**Rationale**: Ensures user consent/control and prevents unintended tracking.

### 2) Single source of truth for tracking state
**Decision**: Maintain a single state flag (e.g., `TrackingState: Stopped|Running`) owned by the Tracking service (or a dedicated state store). UI binds to this state.

**Rationale**: Prevents mismatches where UI says “Stopped” but background timer is running (or vice versa).

### 3) Button behavior rules
**Decision**:
- Start button calls `StartTracking()` only when state is `Stopped`
- Stop button calls `StopTracking()` only when state is `Running`
- Buttons are disabled accordingly (or ignored safely)

**Rationale**: Makes actions idempotent and avoids double-start bugs.

### 4) App close behavior
**Decision**: On app exit, if tracking is running, call `StopTracking()` (graceful cleanup).

**Rationale**: Ensures timers/hooks are released and prevents inconsistent state on next launch.

### 5) Persisted state handling (if applicable)
**Decision**: Even if the previous session ended in `Running`, the next launch still starts `Stopped`.

**Rationale**: Matches the requirement "stop that when I open application" and avoids surprise tracking.

## Notes / Touchpoints
- Startup path: App.xaml.cs / MainWindow initialization should not trigger tracking start
- Tracking service: must support initialize-without-start
- UI: bind button enabled state to `TrackingState`