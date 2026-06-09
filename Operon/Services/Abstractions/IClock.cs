using System;

namespace SystemActivityTracker.Services.Abstractions
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
