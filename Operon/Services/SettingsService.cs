using System;
using System.Diagnostics;
using System.IO;
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
                if (!File.Exists(path))
                {
                    return new AppSettings();
                }

                string json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();

                if (!json.Contains("AutoStartTrackingOnLaunch", StringComparison.OrdinalIgnoreCase))
                {
                    settings.AutoStartTrackingOnLaunch = AppConstants.Defaults.AutoStartTrackingOnLaunch;
                }

                return settings;
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
