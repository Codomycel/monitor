using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using SystemActivityTracker.Models;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.Services
{
    public class LeaveService
    {
        public List<LeaveEntry> LoadMonth(int year, int month)
        {
            try
            {
                string path = AppPaths.GetLeavesPath(year, month);
                return JsonFile.LoadOrDefault(path, static () => new List<LeaveEntry>());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Leaves] Load failed: {ex}");
                return new List<LeaveEntry>();
            }
        }

        public LeaveEntry? GetForDate(DateTime date)
        {
            return LoadMonth(date.Year, date.Month)
                .FirstOrDefault(e => e.Date.Date == date.Date);
        }

        public void SaveMonth(int year, int month, List<LeaveEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            string path = AppPaths.GetLeavesPath(year, month);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonFile.Save(path, entries, options);
        }
    }
}
