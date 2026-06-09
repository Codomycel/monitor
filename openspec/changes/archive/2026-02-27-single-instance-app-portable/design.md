## Context

The Operon application is a WPF-based desktop application distributed as a portable EXE. When users run this EXE multiple times, the application should detect the existing instance and block the new one to prevent data corruption and ensure consistent user experience. The application's entry point is currently in App.xaml.cs with likely initialization logic that needs to be extended.

The application is compiled for the AnyCPU platform and distributed as a portable executable without installation requirements. This means the instance detection mechanism must be local to the machine and must work reliably between concurrent executions.

## Goals / Non-Goals

**Goals:**
- Detect when a second instance of the application attempts to launch
- Prevent the second instance from initializing the WPF application
- Bring the existing instance window to the foreground and focus when a second instance is attempted
- Use a robust mechanism that handles application crashes gracefully
- Provide clean, exit without showing error dialogs to the end user

**Non-Goals:**
- Inter-process communication beyond basic window activation
- Configuration or settings to allow multiple instances
- Network-based instance detection for remote machines
- Support for running the application in different user contexts on the same machine as separate instances

## Decisions

### Decision: Use a Named Mutex for instance detection
**Choice**: Implement instance detection using `System.Threading.Mutex` with a unique application-specific name.

**Rationale**: 
- Named Mutex is the standard, battle-tested approach for single-instance WPF applications
- Automatically released when the process terminates, even on crash
- Atomic operations prevent race conditions
- No external resources needed (no registry, no files to monitor)
- Works reliably across Windows versions

**Alternatives Considered**:
- **File-based locking**: Requires monitoring file changes and cleanup on crashes; more complex
- **Named Pipes**: Good for IPC but overkill for simple detection; adds complexity
- **Registry-based**: Registry can become corrupted and doesn't auto-cleanup on crash

### Decision: Implement instance check in App.xaml.cs OnStartup
**Choice**: Place the single-instance check in `App.OnStartup()` before the main window is created or shown.

**Rationale**:
- Earliest feasible point in the application lifecycle
- Can exit cleanly before any UI is displayed
- Allows access to application resources and current window references
- No need for separate entry point class

### Decision: Bring existing window to foreground using P/Invoke
**Choice**: Use Windows API (P/Invoke) via SetForegroundWindow and SetActiveWindow to activate the existing instance window.

**Rationale**:
- Direct access to window handle allows reliable window activation
- WPF built-in methods (Activate, Focus) may not work across process boundaries
- Standard approach for this pattern in Windows desktop applications
- Minimal code and dependency

**Alternatives Considered**:
- **WPF Activate()**: Works only for current process windows
- **Named pipes for signaling**: Adds network and messaging complexity unnecessarily

### Decision: Use application GUID as mutex name
**Choice**: Create a unique mutex name based on a deterministic GUID (or product/version hash) specific to this application.

**Rationale**:
- Prevents accidental collisions with other applications
- Allows future versioning if needed
- Follows standard practice for WPF single-instance applications

## Risks / Trade-offs

**Risk**: Window activation may fail if user is in another application and UAC/focus stealing prevention blocks it
**Mitigation**: This is unavoidable at the OS level; the application will still exit the second instance cleanly and the user can manually switch to the first instance if needed

**Risk**: Mutex name collision with other Operon installations
**Mitigation**: Use a sufficiently unique identifier (GUID-based) in the mutex name

**Risk**: Abnormal process termination may leave mutex in inconsistent state
**Mitigation**: Named Mutex is automatically released by Windows when the process terminates, even on crash. No additional cleanup is needed.

**Trade-off**: Single instance enforcement applies globally to the machine, not per-user
**Rationale**: This is the desired behavior for a standalone portable application

## Migration Plan

1. **Develop the SingleInstanceManager utility class** with mutex creation and existing instance detection logic
2. **Add window activation helper** class with P/Invoke declarations
3. **Integrate into App.OnStartup()** before MainWindow is created
4. **Test launching multiple instances** to verify blocking behavior and window activation
5. **Test crash recovery** by force-terminating the process and verifying a new instance can launch
6. **Deploy** as part of the standard release process with no configuration changes needed

No rollback strategy needed as this is an additive feature that gracefully falls back if any step fails.

## Open Questions

- Should a log entry be written when a second instance is blocked? (Answer: Nice to have, implement in tasks phase)
- Should the second instance attempt to pass command-line arguments to the first instance? (Answer: Out of scope for initial implementation)
