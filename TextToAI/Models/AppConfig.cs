namespace TextToAI.Models
{
    public class AppConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o";
        public string Hotkey { get; set; } = "Ctrl+Shift+G";
        public string Prompt { get; set; } = "Process the following text:\n\n{text}";
        public bool StartWithWindows { get; set; } = false;
    }
}
