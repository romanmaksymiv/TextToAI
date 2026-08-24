using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // v1 configs were written in PascalCase; keep reading them.
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
                var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();

                if (Migrate(config))
                {
                    Save(config);
                }

                return config;
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

        /// <summary>
        /// Upgrades older config layouts to the current schema:
        /// the single "apiKey" field (OpenAI only) and the single "hotkey"/"prompt" pair.
        /// Existing users keep OpenAI so their setup keeps working; OpenRouter is
        /// only the default for fresh installs.
        /// </summary>
        /// <returns>True if the config was changed and should be re-saved.</returns>
        private static bool Migrate(AppConfig config)
        {
            var changed = false;

            if (!string.IsNullOrWhiteSpace(config.ApiKey))
            {
                if (string.IsNullOrWhiteSpace(config.OpenAiApiKey))
                {
                    config.OpenAiApiKey = config.ApiKey;
                }

                config.Provider = LlmProvider.OpenAI;
                config.ApiKey = null;
                changed = true;
            }

            // Legacy single hotkey/prompt becomes the first action; the second slot keeps
            // its default so the new feature is discoverable.
            if (!string.IsNullOrWhiteSpace(config.Hotkey) || !string.IsNullOrWhiteSpace(config.Prompt))
            {
                config.Actions ??= [];
                if (config.Actions.Count == 0)
                {
                    config.Actions.Add(new PromptAction());
                }

                if (!string.IsNullOrWhiteSpace(config.Hotkey))
                {
                    config.Actions[0].Hotkey = config.Hotkey;
                }

                if (!string.IsNullOrWhiteSpace(config.Prompt))
                {
                    config.Actions[0].Prompt = config.Prompt;
                }

                config.Hotkey = null;
                config.Prompt = null;
                changed = true;
            }

            return NormalizeActions(config) || changed;
        }

        /// <summary>
        /// Guarantees AppConfig.ActionCount slots exist so the Settings window can index
        /// them without bounds checks.
        /// </summary>
        private static bool NormalizeActions(AppConfig config)
        {
            config.Actions ??= [];

            if (config.Actions.Count >= AppConfig.ActionCount)
            {
                return false;
            }

            var defaults = new AppConfig().Actions;
            while (config.Actions.Count < AppConfig.ActionCount)
            {
                var index = config.Actions.Count;
                config.Actions.Add(index < defaults.Count
                    ? defaults[index]
                    : new PromptAction());
            }

            return true;
        }
    }
}
