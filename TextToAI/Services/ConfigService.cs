using System.IO;
using System.Text.Json;
using TextToAI.Models;

namespace TextToAI.Services
{
    public class ConfigService
    {
        private static readonly string ConfigFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TextToAI");

        private static readonly string ConfigPath = Path.Combine(ConfigFolder, "config.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new AppConfig();
                Save(defaultConfig);
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
            catch
            {
                // If config is corrupted, return default
                return new AppConfig();
            }
        }

        public void Save(AppConfig config)
        {
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
