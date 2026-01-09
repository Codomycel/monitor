using System;
using System.IO;

namespace SystemActivityTracker.Utilities
{
    internal static class AppPaths
    {
        private const string AppFolderName = "SystemActivityTracker";
        private const string LogsFolderName = "logs";

        public static string GetAppFolder()
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(baseFolder, AppFolderName);
            Directory.CreateDirectory(appFolder);
            return appFolder;
        }

        public static string GetLogsFolder()
        {
            string logsFolder = Path.Combine(GetAppFolder(), LogsFolderName);
            Directory.CreateDirectory(logsFolder);
            return logsFolder;
        }

        public static string GetSettingsPath() => Path.Combine(GetAppFolder(), "settings.json");

        public static string GetManualTasksPath(DateTime date)
        {
            string fileName = $"manual-tasks-{date:yyyy-MM-dd}.json";
            return Path.Combine(GetAppFolder(), fileName);
        }

        public static string GetActivityLogCsvPath(DateTime date)
        {
            string fileName = $"activity-log-{date:yyyy-MM-dd}.csv";
            return Path.Combine(GetAppFolder(), fileName);
        }
    }
}
