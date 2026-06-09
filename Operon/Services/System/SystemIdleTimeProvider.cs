using System;
using System.Diagnostics;
using SystemActivityTracker.Services.Abstractions;

namespace SystemActivityTracker.Services.Platform
{
    internal sealed class SystemIdleTimeProvider : IIdleTimeProvider
    {
        public TimeSpan GetIdleTime()
        {
            try
            {
                return IdleTimeHelper.GetIdleTime();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemIdleTimeProvider] GetIdleTime failed: {ex}");
                return TimeSpan.Zero;
            }
        }
    }
}
