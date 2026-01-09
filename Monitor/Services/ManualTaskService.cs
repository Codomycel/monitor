using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using SystemActivityTracker.Models;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.Services
{
    public class ManualTaskService
    {
        public List<ManualTaskEntry> Load(DateTime date)
        {
            try
            {
                string path = AppPaths.GetManualTasksPath(date.Date);
                return JsonFile.LoadOrDefault(path, static () => new List<ManualTaskEntry>());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualTasks] Load failed: {ex}");
                return new List<ManualTaskEntry>();
            }
        }

        public void Save(DateTime date, List<ManualTaskEntry> tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));

            string path = AppPaths.GetManualTasksPath(date.Date);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonFile.Save(path, tasks, options);
        }
    }
}
