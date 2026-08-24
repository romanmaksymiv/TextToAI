namespace TextToAI.Models
{
    /// <summary>
    /// One hotkey bound to one prompt. An action with an empty hotkey is simply not registered.
    /// </summary>
    public class PromptAction
    {
        public string Hotkey { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }
}
