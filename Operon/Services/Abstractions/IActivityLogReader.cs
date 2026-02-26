using System;
using System.Collections.Generic;

namespace SystemActivityTracker.Services.Abstractions
{
    public interface IActivityLogReader
    {
        bool TryReadDay(DateTime date, out IReadOnlyList<ActivityLogEntry> entries);
        IEnumerable<ActivityLogEntry> ReadRange(DateTime startDate, DateTime endDate);
    }
}
