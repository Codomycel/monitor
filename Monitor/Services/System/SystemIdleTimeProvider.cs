using System;
using SystemActivityTracker.Services.Abstractions;

namespace SystemActivityTracker.Services.Platform
{
    internal sealed class SystemIdleTimeProvider : IIdleTimeProvider
    {
        public TimeSpan GetIdleTime() => IdleTimeHelper.GetIdleTime();
    }
}
