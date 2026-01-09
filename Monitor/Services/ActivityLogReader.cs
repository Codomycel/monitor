using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SystemActivityTracker.Services.Abstractions;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.Services
{
    internal sealed class ActivityLogReader : IActivityLogReader
    {
        public bool TryReadDay(DateTime date, out IReadOnlyList<ActivityLogEntry> entries)
        {
            try
            {
                string path = AppPaths.GetActivityLogCsvPath(date.Date);
                if (!File.Exists(path))
                {
                    entries = Array.Empty<ActivityLogEntry>();
                    return false;
                }

                var list = new List<ActivityLogEntry>();
                foreach (var entry in ReadFile(path))
                {
                    list.Add(entry);
                }

                entries = list;
                return true;
            }
            catch
            {
                entries = Array.Empty<ActivityLogEntry>();
                return false;
            }
        }

        public IEnumerable<ActivityLogEntry> ReadRange(DateTime startDate, DateTime endDate)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            if (end < start)
            {
                yield break;
            }

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                string path = AppPaths.GetActivityLogCsvPath(date);
                if (!File.Exists(path))
                {
                    continue;
                }

                foreach (var entry in ReadFile(path))
                {
                    yield return entry;
                }
            }
        }

        private static IEnumerable<ActivityLogEntry> ReadFile(string path)
        {
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("StartTime", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!TryParseLine(line, out var entry))
                {
                    continue;
                }

                yield return entry;
            }
        }

        private static bool TryParseLine(string line, out ActivityLogEntry entry)
        {
            entry = default;

            var fields = ParseCsvLine(line);
            if (fields.Length < 6)
            {
                return false;
            }

            if (!DateTime.TryParse(fields[0], null, DateTimeStyles.RoundtripKind, out var startTime))
            {
                return false;
            }

            if (!DateTime.TryParse(fields[1], null, DateTimeStyles.RoundtripKind, out var endTime))
            {
                return false;
            }

            if (endTime < startTime)
            {
                return false;
            }

            string processName = fields.Length > 2 ? fields[2] : string.Empty;
            string windowTitle = fields.Length > 3 ? fields[3] : string.Empty;

            if (!bool.TryParse(fields[4], out var isLocked))
            {
                return false;
            }

            if (!bool.TryParse(fields[5], out var isIdle))
            {
                return false;
            }

            entry = new ActivityLogEntry(startTime, endTime, processName, windowTitle, isLocked, isIdle);
            return true;
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
