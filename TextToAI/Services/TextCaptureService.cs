using System.Windows;
using TextToAI.Interop;

namespace TextToAI.Services
{
    public class TextCaptureService
    {
        private const byte VK_CONTROL = 0x11;
        private const byte VK_SHIFT = 0x10;
        private const byte VK_MENU = 0x12;
        private const byte VK_C = 0x43;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public async Task<string> CaptureSelectedTextAsync()
        {
            // Release modifier keys from hotkey
            ReleaseModifierKeys();
            await Task.Delay(30);

            // Clear clipboard
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try { Clipboard.Clear(); } catch { }
            });

            // Simulate Ctrl+C via Win32 keybd_event
            SimulateCtrlC();
            await Task.Delay(100);

            // Read from clipboard
            string capturedText = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (Clipboard.ContainsText())
                        capturedText = Clipboard.GetText();
                }
                catch { }
            });

            return capturedText;
        }

        private void ReleaseModifierKeys()
        {
            NativeMethods.keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void SimulateCtrlC()
        {
            NativeMethods.keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(VK_C, 0, 0, UIntPtr.Zero);
            Thread.Sleep(10);
            NativeMethods.keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
