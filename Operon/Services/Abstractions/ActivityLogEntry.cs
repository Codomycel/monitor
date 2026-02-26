using System;

namespace SystemActivityTracker.Services.Abstractions
{
    public readonly record struct ActivityLogEntry(
        DateTime StartTime,
        DateTime EndTime,
        string ProcessName,
        string WindowTitle,
        bool IsLocked,
        bool IsIdle);
}
