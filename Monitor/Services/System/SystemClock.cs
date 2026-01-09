using System;
using SystemActivityTracker.Services.Abstractions;

namespace SystemActivityTracker.Services.Platform
{
    internal sealed class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
