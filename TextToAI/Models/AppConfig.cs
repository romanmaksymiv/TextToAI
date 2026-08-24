using System.Text.Json.Serialization;

namespace TextToAI.Models
{
    public class AppConfig
    {
        /// <summary>Number of hotkey/prompt slots the Settings window exposes.</summary>
        public const int ActionCount = 2;

        public LlmProvider Provider { get; set; } = LlmProvider.OpenRouter;
        public string OpenRouterApiKey { get; set; } = string.Empty;
        public string OpenAiApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "anthropic/claude-sonnet-4.5";
        public bool StartWithWindows { get; set; } = false;

        public List<PromptAction> Actions { get; set; } =
        [
            new() { Hotkey = "Ctrl+Shift+G", Prompt = "Process the following text:\n\n{text}" },
            new() { Hotkey = "Ctrl+Shift+H", Prompt = "Summarize this text in 2-3 sentences:\n\n{text}" }
        ];

        /// <summary>
        /// Legacy single-key field from v1 configs. Read only by ConfigService.Migrate
        /// and never written back (JsonOptions drops nulls).
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Legacy single-action fields from pre-Actions configs. Migrated into Actions[0]
        /// and then cleared. See ConfigService.Migrate.
        /// </summary>
        public string? Hotkey { get; set; }

        /// <inheritdoc cref="Hotkey"/>
        public string? Prompt { get; set; }

        [JsonIgnore]
        public string ActiveApiKey => Provider == LlmProvider.OpenAI ? OpenAiApiKey : OpenRouterApiKey;
    }
}
