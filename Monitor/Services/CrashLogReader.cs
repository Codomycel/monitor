using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SystemActivityTracker.Services.Abstractions;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.Services
{
    internal sealed class CrashLogReader : ICrashLogReader
    {
        private const string DailyCloseEventsFilePrefix = "close-events-";

        public bool TryReadLatestNonGracefulEvent(out CrashLogEvent evt, out string sourcePath)
        {
            try
            {
                var logsFolder = AppPaths.GetLogsFolder();
                if (Directory.Exists(logsFolder))
                {
                    var files = Directory.GetFiles(logsFolder, $"{DailyCloseEventsFilePrefix}*.jsonl", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(p => p, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var file in files)
                    {
                        if (TryReadLatestNonGracefulEvent(file, out evt))
                        {
                            sourcePath = file;
                            return true;
                        }
                    }
                }

                var legacy = AppPaths.GetLegacyCloseEventsPath();
                if (TryReadLatestNonGracefulEvent(legacy, out evt))
                {
                    sourcePath = legacy;
                    return true;
                }

                evt = new CrashLogEvent();
                sourcePath = legacy;
                return false;
            }
            catch
            {
                evt = new CrashLogEvent();
                sourcePath = AppPaths.GetLegacyCloseEventsPath();
                return false;
            }
        }

        public bool TryReadLastRun(out LastRunRecord record)
        {
            try
            {
                string path = AppPaths.GetLastRunPath();
                if (!File.Exists(path))
                {
                    record = new LastRunRecord();
                    return false;
                }

                string json = File.ReadAllText(path);
                var parsed = JsonSerializer.Deserialize<LastRunRecord>(json);
                if (parsed == null)
                {
                    record = new LastRunRecord();
                    return false;
                }

                record = parsed;
                return true;
            }
            catch
            {
                record = new LastRunRecord();
                return false;
            }
        }

        private static bool TryReadLatestNonGracefulEvent(string closeEventsPath, out CrashLogEvent evt)
        {
            evt = new CrashLogEvent();

            try
            {
                if (!File.Exists(closeEventsPath))
                {
                    return false;
                }

                IEnumerable<string> lines = File.ReadLines(closeEventsPath);
                foreach (var line in lines.Reverse())
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    CrashLogEvent? parsed;
                    try
                    {
                        parsed = JsonSerializer.Deserialize<CrashLogEvent>(line);
                    }
                    catch
                    {
                        continue;
                    }

                    if (parsed == null)
                    {
                        continue;
                    }

                    if (IsNonGraceful(parsed.CloseReason ?? string.Empty))
                    {
                        evt = parsed;
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsNonGraceful(string closeReason)
        {
            if (string.IsNullOrWhiteSpace(closeReason))
            {
                return false;
            }

            return !string.Equals(closeReason, "Graceful", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(closeReason, "UserInitiatedExit", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(closeReason, "ShutdownOrLogoff", StringComparison.OrdinalIgnoreCase);
        }
    }
}
