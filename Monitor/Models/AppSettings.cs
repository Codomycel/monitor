namespace SystemActivityTracker.Models
{
    public class AppSettings
    {
        public int IdleThresholdMinutes { get; set; } = Utilities.AppConstants.Defaults.IdleThresholdMinutes;
        public int PollIntervalSeconds { get; set; } = Utilities.AppConstants.Defaults.PollIntervalSeconds;
        public bool EnableLiveRefresh { get; set; } = Utilities.AppConstants.Defaults.EnableLiveRefresh;
        public int LiveRefreshIntervalSeconds { get; set; } = Utilities.AppConstants.Defaults.LiveRefreshIntervalSeconds;
        public bool AutoStartTrackingOnLaunch { get; set; } = Utilities.AppConstants.Defaults.AutoStartTrackingOnLaunch;

        public string UiMode { get; set; } = Utilities.UiModes.Default;

        public int CrashLogRetentionDays { get; set; } = Utilities.AppConstants.Defaults.CrashLogRetentionDays;
        public int CrashLogMaxSizeMB { get; set; } = Utilities.AppConstants.Defaults.CrashLogMaxSizeMB;
    }
}
