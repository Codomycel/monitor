using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SystemActivityTracker.Utilities
{
    /// <summary>
    /// Helper class for activating and bringing windows to the foreground.
    /// Uses P/Invoke to interact with the Windows API to locate and activate application windows.
    /// </summary>
    /// <remarks>
    /// This class provides static utility methods for finding and activating windows of running processes.
    /// It's commonly used in single-instance application patterns to bring the existing instance to the foreground
    /// when a user attempts to launch a second instance.
    /// </remarks>
    public static class WindowActivator
    {
        /// <summary>
        /// Brings the thread that created the specified window to the foreground and activates the window.
        /// </summary>
        /// <param name="hWnd">Handle to the window that should be activated and brought to the foreground.</param>
        /// <returns>If the window was brought to the foreground, the return value is nonzero. 
        /// If the window was not brought to the foreground, the return value is zero.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Activates a window. The window will be activated to the top of the Z order, 
        /// if the window is not disabled.
        /// </summary>
        /// <param name="hWnd">Handle to the window to be activated.</param>
        /// <returns>If the window was activated, the return value is the handle to the window that was 
        /// previously active. If the window was not activated, the return value is zero.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetActiveWindow(IntPtr hWnd);

        /// <summary>
        /// Determines whether the specified window handle identifies an existing window.
        /// </summary>
        /// <param name="hWnd">Handle to the window to test.</param>
        /// <returns>If the window exists, the return value is nonzero. If the window does not exist, 
        /// the return value is zero.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindow(IntPtr hWnd);

        /// <summary>
        /// Finds the main window handle of an existing instance of this application.
        /// Enumerates all running processes with the same name and returns the main window handle
        /// of the first process found that is not the current process.
        /// </summary>
        /// <returns>
        /// The window handle (HWND) of the existing instance if found; IntPtr.Zero if no existing instance 
        /// window is found or if an error occurs.
        /// </returns>
        /// <remarks>
        /// This method will skip the current process (by comparing process IDs) and search for another
        /// instance with the same executable name. The MainWindowHandle property is populated after the 
        /// window is created, so this method may return zero if called before the other instance's window 
        /// is fully initialized.
        /// </remarks>
        public static IntPtr GetMainWindowHandle()
        {
            try
            {
                // Get the current process to use as a reference
                var currentProcess = Process.GetCurrentProcess();
                var currentProcessName = currentProcess.ProcessName;

                // Look for another running instance of this process
                var processes = Process.GetProcessesByName(currentProcessName);

                foreach (var process in processes)
                {
                    // Skip the current process
                    if (process.Id == currentProcess.Id)
                        continue;

                    // Return the main window handle of the other instance
                    // Note: MainWindowHandle is populated after the window is created
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process.MainWindowHandle;
                    }
                }

                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding main window handle: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Activates a window and brings it to the foreground.
        /// Attempts to make the specified window the active window and bring it to the foreground.
        /// </summary>
        /// <param name="handle">The handle of the window to activate.</param>
        /// <returns>
        /// True if the window was successfully activated and brought to the foreground; 
        /// false if the handle is invalid, the window no longer exists, or an error occurs.
        /// </returns>
        /// <remarks>
        /// This method first validates that the window handle is not zero and that the window still exists.
        /// It then calls SetActiveWindow() and SetForegroundWindow() to activate the window.
        /// 
        /// Note: Due to Windows focus-stealing prevention, the window may not always be brought to the foreground
        /// if called from a different process or if the user is currently interacting with another application.
        /// However, the window will be activated in the taskbar and the user can click it to bring it to the foreground.
        /// </remarks>
        public static bool ActivateWindow(IntPtr handle)
        {
            try
            {
                if (handle == IntPtr.Zero)
                {
                    Debug.WriteLine("Cannot activate: window handle is invalid (zero)");
                    return false;
                }

                // Verify the window still exists
                if (!IsWindow(handle))
                {
                    Debug.WriteLine("Cannot activate: window is no longer valid");
                    return false;
                }

                // Attempt to activate the window
                SetActiveWindow(handle);
                SetForegroundWindow(handle);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error activating window: {ex.Message}");
                return false;
            }
        }
    }
}
