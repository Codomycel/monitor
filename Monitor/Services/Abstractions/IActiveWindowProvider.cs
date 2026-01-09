namespace SystemActivityTracker.Services.Abstractions
{
    public interface IActiveWindowProvider
    {
        bool TryGetActiveWindow(out string processName, out string windowTitle);
    }
}
