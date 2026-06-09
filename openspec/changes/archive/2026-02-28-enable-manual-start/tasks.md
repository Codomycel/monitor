## 1. Stop Auto-Start Tracking on App Launch
- [x] 1.1 Find where tracking starts during app startup (App.xaml.cs, MainWindow, ViewModel init)
- [x] 1.2 Remove or disable the call that starts tracking automatically
- [x] 1.3 Ensure tracking service initializes in "Stopped" state

## 2. Ensure Start/Stop Buttons Control Tracking
- [x] 2.1 Verify Start button triggers tracking start (only if currently stopped)
- [x] 2.2 Verify Stop button stops tracking (only if currently running)
- [x] 2.3 Disable Start button while running, disable Stop button while stopped (or equivalent UX)

## 3. Persisted State Rules (if you store state)
- [x] 3.1 If last session was "Running", still start in "Stopped" on next launch (requirement decision)
- [x] 3.2 Remove any "resume tracking automatically" logic

## 4. Testing and Validation
- [x] 4.1 Launch app → verify tracking is NOT running
- [x] 4.2 Launch app → verify UI shows Stopped state
- [x] 4.3 Click Start → verify tracking begins
- [x] 4.4 Close app while running → relaunch → verify tracking is Stopped
- [x] 4.5 Regression: existing tracking + charts still work