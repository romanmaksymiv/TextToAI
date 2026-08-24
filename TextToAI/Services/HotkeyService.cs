using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TextToAI.Interop;
using TextToAI.Models;

namespace TextToAI.Services
{
    public class HotkeyPressedEventArgs(int actionIndex) : EventArgs
    {
        /// <summary>Index into AppConfig.Actions of the action whose hotkey fired.</summary>
        public int ActionIndex { get; } = actionIndex;
    }

    public class HotkeyService : IDisposable
    {
        // Each action gets BaseHotkeyId + its index, so WM_HOTKEY tells us which one fired.
        private const int BaseHotkeyId = 9000;
        private HwndSource? _hwndSource;
        private readonly List<int> _registeredIds = [];

        public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

        /// <summary>
        /// Registers every action that has a hotkey, replacing any previous registration.
        /// Actions with a blank hotkey are skipped.
        /// </summary>
        /// <returns>The hotkey strings that could not be registered (in use, or unparseable).</returns>
        public IReadOnlyList<string> RegisterAll(IReadOnlyList<PromptAction> actions)
        {
            UnregisterAll();

            var failed = new List<string>();

            // Create a hidden window to receive hotkey messages
            EnsureHwndSource();

            for (var i = 0; i < actions.Count; i++)
            {
                var hotkeyString = actions[i].Hotkey;

                if (string.IsNullOrWhiteSpace(hotkeyString))
                {
                    continue;
                }

                if (_hwndSource == null || !ParseHotkey(hotkeyString, out uint modifiers, out uint vk))
                {
                    failed.Add(hotkeyString);
                    continue;
                }

                var id = BaseHotkeyId + i;
                if (NativeMethods.RegisterHotKey(_hwndSource.Handle, id, modifiers | NativeMethods.MOD_NOREPEAT, vk))
                {
                    _registeredIds.Add(id);
                }
                else
                {
                    failed.Add(hotkeyString);
                }
            }

            return failed;
        }

        public void UnregisterAll()
        {
            if (_hwndSource != null)
            {
                foreach (var id in _registeredIds)
                {
                    NativeMethods.UnregisterHotKey(_hwndSource.Handle, id);
                }
            }

            _registeredIds.Clear();
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
            if (msg == NativeMethods.WM_HOTKEY && _registeredIds.Contains(wParam.ToInt32()))
            {
                HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(wParam.ToInt32() - BaseHotkeyId));
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
            UnregisterAll();
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource?.Dispose();
            _hwndSource = null;
        }
    }
}
