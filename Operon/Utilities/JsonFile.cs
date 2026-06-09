using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SystemActivityTracker.Utilities
{
    internal static class JsonFile
    {
        public static T LoadOrDefault<T>(string path, Func<T> defaultFactory)
        {
            if (defaultFactory == null) throw new ArgumentNullException(nameof(defaultFactory));

            try
            {
                if (!File.Exists(path))
                {
                    return defaultFactory();
                }

                string json = File.ReadAllText(path);
                var value = JsonSerializer.Deserialize<T>(json);
                return value ?? defaultFactory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JsonFile] Load failed (Path={path}): {ex}");
                return defaultFactory();
            }
        }

        public static void Save<T>(string path, T value, JsonSerializerOptions? options = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);

            string json = JsonSerializer.Serialize(value, options);
            File.WriteAllText(path, json);
        }
    }
}
