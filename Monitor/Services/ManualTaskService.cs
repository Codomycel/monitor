using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SystemActivityTracker.Models;

namespace SystemActivityTracker.Services
{
    public class ManualTaskService
    {
        private const string AppFolderName = "SystemActivityTracker";

        private static string GetFilePath(DateTime date)
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(baseFolder, AppFolderName);
            Directory.CreateDirectory(appFolder);
            string fileName = $"manual-tasks-{date:yyyy-MM-dd}.json";
            return Path.Combine(appFolder, fileName);
        }

        public List<ManualTaskEntry> Load(DateTime date)
        {
            try
            {
                string path = GetFilePath(date.Date);
                if (!File.Exists(path))
                {
                    return new List<ManualTaskEntry>();
                }

                string json = File.ReadAllText(path);
                var items = JsonSerializer.Deserialize<List<ManualTaskEntry>>(json);
                return items ?? new List<ManualTaskEntry>();
            }
            catch
            {
                return new List<ManualTaskEntry>();
            }
        }

        public void Save(DateTime date, List<ManualTaskEntry> tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));

            string path = GetFilePath(date.Date);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(path, json);
        }
    }
}
