using System;

namespace SystemActivityTracker.Services.Abstractions
{
    public interface IIdleTimeProvider
    {
        TimeSpan GetIdleTime();
    }
}
