using System;

namespace SystemActivityTracker.Models
{
    public class DailySummary
    {
        public DateTime Date { get; set; }
        public TimeSpan ActiveDuration { get; set; }
        public TimeSpan ManualTaskDuration { get; set; }
        public TimeSpan IdleDuration { get; set; }
        public TimeSpan LockedDuration { get; set; }

        public TimeSpan TotalActiveDuration => ActiveDuration + ManualTaskDuration;

        public string ActiveDurationText => ActiveDuration.ToString(@"hh\:mm");
        public string ManualTaskDurationText => ManualTaskDuration.ToString(@"hh\:mm");
        public string TotalActiveDurationText => TotalActiveDuration.ToString(@"hh\:mm");
        public string IdleDurationText   => IdleDuration.ToString(@"hh\:mm");
        public string LockedDurationText => LockedDuration.ToString(@"hh\:mm");
    }
}
