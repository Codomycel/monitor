namespace SystemActivityTracker.Utilities
{
    internal static class AppConstants
    {
        internal static class Defaults
        {
            public const int IdleThresholdMinutes = 5;
            public const int PollIntervalSeconds = 5;
            public const bool EnableLiveRefresh = true;
            public const int LiveRefreshIntervalSeconds = 30;
            public const bool AutoStartTrackingOnLaunch = true;

            public const int CrashLogRetentionDays = 14;
            public const int CrashLogMaxSizeMB = 50;
        }

        internal static class Limits
        {
            public const int CrashRetentionDaysMin = 1;
            public const int CrashRetentionDaysMax = 365;

            public const int CrashMaxSizeMbMin = 1;
            public const int CrashMaxSizeMbMax = 2048;
        }

        internal static class Time
        {
            public const int MillisPerSecond = 1000;

            public const int UiHeartbeatInitialDelaySeconds = 10;
            public const int UiHeartbeatPeriodSeconds = 20;

            public const int HangMonitorInitialDelaySeconds = 5;
            public const int HangMonitorPeriodSeconds = 5;
        }
    }
}
