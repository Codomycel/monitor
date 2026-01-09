using SystemActivityTracker.Services.Abstractions;

namespace SystemActivityTracker.Services.Platform
{
    internal sealed class SystemActiveWindowProvider : IActiveWindowProvider
    {
        public bool TryGetActiveWindow(out string processName, out string windowTitle) =>
            ActiveWindowHelper.TryGetActiveWindow(out processName, out windowTitle);
    }
}
