using Microsoft.Win32;

namespace TextToAI.Services
{
    public class AutoStartService
    {
        private const string AppName = "TextToAI";
        private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static void SetAutoStart(bool enable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }

        public static bool IsAutoStartEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
            return key?.GetValue(AppName) != null;
        }
    }
}
