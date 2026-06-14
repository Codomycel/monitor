using System;

namespace SystemActivityTracker.Models
{
    public class LeaveEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        public LeaveDuration Duration { get; set; }
        public LeaveType Type { get; set; }
    }
}
