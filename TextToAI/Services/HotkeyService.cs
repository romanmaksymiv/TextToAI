using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TextToAI.Interop;

namespace TextToAI.Services
{
    public class HotkeyService : IDisposable
    {
        private const int HOTKEY_ID = 9000;
        private HwndSource? _hwndSource;
        private bool _isRegistered;

        public event EventHandler? HotkeyPressed;

        public bool Register(string hotkeyString)
        {
            if (_isRegistered)
            {
                Unregister();
            }

            if (!ParseHotkey(hotkeyString, out uint modifiers, out uint vk))
            {
                return false;
            }

            // Create a hidden window to receive hotkey messages
            EnsureHwndSource();

            if (_hwndSource == null)
            {
                return false;
            }

            _isRegistered = NativeMethods.RegisterHotKey(_hwndSource.Handle, HOTKEY_ID, modifiers | NativeMethods.MOD_NOREPEAT, vk);
            return _isRegistered;
        }

        public void Unregister()
        {
            if (_isRegistered && _hwndSource != null)
            {
                NativeMethods.UnregisterHotKey(_hwndSource.Handle, HOTKEY_ID);
                _isRegistered = false;
            }
        }

        private void EnsureHwndSource()
        {
            if (_hwndSource != null)
            {
                return;
            }

            // Create a message-only window
            var parameters = new HwndSourceParameters("TextToAI_HotkeyWindow")
            {
                Width = 0,
                Height = 0,
                PositionX = -100,
                PositionY = -100,
                WindowStyle = 0x800000 // WS_POPUP - invisible window
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public static bool ParseHotkey(string hotkeyString, out uint modifiers, out uint vk)
        {
            modifiers = 0;
            vk = 0;

            if (string.IsNullOrWhiteSpace(hotkeyString))
            {
                return false;
            }

            var parts = hotkeyString.Split('+');
            if (parts.Length == 0)
            {
                return false;
            }

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                switch (trimmed.ToLower())
                {
                    case "ctrl":
                    case "control":
                        modifiers |= NativeMethods.MOD_CONTROL;
                        break;
                    case "alt":
                        modifiers |= NativeMethods.MOD_ALT;
                        break;
                    case "shift":
                        modifiers |= NativeMethods.MOD_SHIFT;
                        break;
                    case "win":
                    case "windows":
                        modifiers |= NativeMethods.MOD_WIN;
                        break;
                    default:
                        // Try to parse as a Key enum and convert to virtual key code
                        if (Enum.TryParse<Key>(trimmed, true, out var key))
                        {
                            vk = (uint)KeyInterop.VirtualKeyFromKey(key);
                        }
                        break;
                }
            }

            return vk != 0;
        }

        public void Dispose()
        {
            Unregister();
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource?.Dispose();
            _hwndSource = null;
        }
    }
}
