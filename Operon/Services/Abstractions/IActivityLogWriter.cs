using SystemActivityTracker.Models;

namespace SystemActivityTracker.Services.Abstractions
{
    public interface IActivityLogWriter
    {
        void AppendRecord(ActivityRecord record);
    }
}
