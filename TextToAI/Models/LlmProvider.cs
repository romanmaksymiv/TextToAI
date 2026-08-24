namespace TextToAI.Models
{
    public enum LlmProvider
    {
        OpenRouter,
        OpenAI
    }

    public sealed record ProviderInfo(
        LlmProvider Provider,
        string DisplayName,
        string ChatCompletionsUrl,
        string DefaultModel,
        string[] ModelPresets,
        string KeysUrl)
    {
        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Single source of truth for provider endpoints, defaults and model presets.
    /// Both LlmService and SettingsWindow read from here so they cannot drift apart.
    /// </summary>
    public static class ProviderCatalog
    {
        public static ProviderInfo OpenRouter { get; } = new(
            LlmProvider.OpenRouter,
            "OpenRouter",
            "https://openrouter.ai/api/v1/chat/completions",
            "anthropic/claude-sonnet-4.5",
            [
                "openrouter/free",
                "anthropic/claude-sonnet-4.5",
                "openai/gpt-4o",
                "google/gemini-2.5-pro",
                "deepseek/deepseek-chat"
            ],
            "https://openrouter.ai/keys");

        public static ProviderInfo OpenAI { get; } = new(
            LlmProvider.OpenAI,
            "OpenAI",
            "https://api.openai.com/v1/chat/completions",
            "gpt-4o",
            [
                "gpt-4o",
                "gpt-4o-mini",
                "gpt-4-turbo",
                "gpt-3.5-turbo"
            ],
            "https://platform.openai.com/api-keys");

        public static IReadOnlyList<ProviderInfo> All { get; } = [OpenRouter, OpenAI];

        public static ProviderInfo Get(LlmProvider provider) =>
            provider == LlmProvider.OpenAI ? OpenAI : OpenRouter;
    }
}
