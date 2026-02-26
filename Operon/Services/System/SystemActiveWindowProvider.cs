using System.Diagnostics;
using SystemActivityTracker.Services.Abstractions;

namespace SystemActivityTracker.Services.Platform
{
    internal sealed class SystemActiveWindowProvider : IActiveWindowProvider
    {
        public bool TryGetActiveWindow(out string processName, out string windowTitle)
        {
            try
            {
                bool ok = ActiveWindowHelper.TryGetActiveWindow(out processName, out windowTitle);
                processName ??= string.Empty;
                windowTitle ??= string.Empty;
                return ok;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[SystemActiveWindowProvider] TryGetActiveWindow failed: {ex}");
                processName = string.Empty;
                windowTitle = string.Empty;
                return false;
            }
        }
    }
}
