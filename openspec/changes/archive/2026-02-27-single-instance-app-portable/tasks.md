## 1. Create SingleInstanceManager Utility Class

- [x] 1.1 Create new file `Utilities/SingleInstanceManager.cs`
- [x] 1.2 Define unique mutex name based on application GUID
- [x] 1.3 Implement `AcquireInstance()` method to create/check mutex
- [x] 1.4 Implement `IsInstanceAlreadyRunning` property
- [x] 1.5 Implement `ReleaseMutex()` for cleanup
- [x] 1.6 Store mutex reference as class field for lifetime management

## 2. Create WindowActivator Utility Class

- [x] 2.1 Create new file `Utilities/WindowActivator.cs`
- [x] 2.2 Add P/Invoke declarations for SetForegroundWindow and SetActiveWindow
- [x] 2.3 Implement `GetMainWindowHandle()` method to find existing instance window using process enumeration
- [x] 2.4 Implement `ActivateWindow(IntPtr handle)` method to bring window to foreground
- [x] 2.5 Add error handling for cases where window handle is invalid

## 3. Integrate Single Instance Check into App Startup

- [x] 3.1 Modify `App.xaml.cs` OnStartup method signature to allow shutdown before MainWindow creation
- [x] 3.2 Add SingleInstanceManager call at the beginning of OnStartup()
- [x] 3.3 If existing instance is detected, call WindowActivator to bring it to foreground
- [x] 3.4 Exit the second instance cleanly using `this.Shutdown()`
- [x] 3.5 Ensure MainWindow is NOT created if instance check fails
- [x] 3.6 Keep SingleInstanceManager instance active (store as App field) for application lifetime


## 9. Fix: Single-instance still allows multiple windows/processes

- [x] 9.1 Determine whether the issue is multiple processes or multiple windows:
      - Check Task Manager → number of Operon.exe processes after 2 launches (investigation showed only one process, problem was window activation logic)
- [x] 9.2 If StartupUri is present in App.xaml, remove it and create MainWindow manually after mutex acquisition (StartupUri already absent)
- [x] 9.3 Ensure mutex is held for full app lifetime (static field; not disposed early) — converted mutex to static field in `SingleInstanceManager`
- [x] 9.4 Replace process-enumeration window activation with EventWaitHandle activation signal (first instance listens; second instance signals) — implemented event-based signalling and callback
- [ ] 9.5 Re-run build and re-test Section 4 (manual instance launch)

## 1. Fix CS0103: _disposed does not exist

- [x] 1.1 Open `Utilities/SingleInstanceManager.cs` and locate references to `_disposed` (around lines 171 and 179)
- [x] 1.2 Add missing field: `private bool _disposed;` in the class scope (near other fields)
- [x] 1.3 Ensure Dispose/Release logic uses `_disposed` correctly and compiles
- [x] 1.4 Build solution to confirm error is fixed