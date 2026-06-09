## Why

A portable EXE application should prevent multiple instances from running simultaneously to avoid data corruption, resource conflicts, and ensure a consistent user experience. Users may accidentally launch the application multiple times, and the system should gracefully handle this by blocking the second instance.

## What Changes

- Implement single instance enforcement mechanism using a mutex or named pipe approach
- Detect when a second instance is launched and gracefully block it
- Optionally bring the already-running instance to the foreground when a second instance attempts to start
- Add startup logic before the main WPF application window is displayed

## Capabilities

### New Capabilities
- `single-instance-enforcement`: Prevents multiple instances of the portable EXE from running simultaneously by detecting and blocking subsequent launch attempts. Includes optional activation of the existing instance window.

### Modified Capabilities
<!-- No existing capabilities are modified by this change -->

## Impact

- **Application Startup**: Modification to the application entry point (likely App.xaml.cs and Program.cs) to check for existing instances before initializing the WPF application
- **Code Structure**: Addition of utility classes/services for instance detection and mutex management
- **User Experience**: More robust application behavior for portable EXE deployments
