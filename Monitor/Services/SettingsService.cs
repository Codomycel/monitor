using System;
using System.Diagnostics;
using System.Text.Json;
using SystemActivityTracker.Models;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.Services
{
    public class SettingsService
    {
        public AppSettings Load()
        {
            try
            {
                string path = AppPaths.GetSettingsPath();
                return JsonFile.LoadOrDefault(path, static () => new AppSettings());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Settings] Load failed: {ex}");
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string path = AppPaths.GetSettingsPath();
            JsonFile.Save(path, settings, options);
        }
    }
}
