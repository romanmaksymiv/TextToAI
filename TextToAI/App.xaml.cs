using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using TextToAI.Models;
using TextToAI.Services;
using TextToAI.Views;

namespace TextToAI
{
    public partial class App : Application
    {
        private const string MutexName = "TextToAI_SingleInstance_Mutex";
        private Mutex? _mutex;
        private TaskbarIcon? _trayIcon;
        private ConfigService? _configService;
        private HotkeyService? _hotkeyService;
        private AppConfig? _config;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Single instance check
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show("TextToAI is already running.", "TextToAI", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            // Load configuration
            _configService = new ConfigService();
            _config = _configService.Load();

            // Initialize system tray icon
            InitializeTrayIcon();

            // Register global hotkey
            InitializeHotkey();
        }

        private void InitializeHotkey()
        {
            _hotkeyService = new HotkeyService();
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            if (_hotkeyService == null || _config == null)
            {
                return;
            }

            var failed = _hotkeyService.RegisterAll(_config.Actions);

            if (failed.Count > 0)
            {
                var list = string.Join(", ", failed);
                _trayIcon?.ShowBalloonTip("TextToAI", $"Failed to register hotkey: {list}", BalloonIcon.Warning);
            }
        }

        private readonly TextCaptureService _textCaptureService = new();
        private readonly LlmService _llmService = new();
        private ResultPopup? _resultPopup;

        private async void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs e)
        {
            var action = _config != null && e.ActionIndex >= 0 && e.ActionIndex < _config.Actions.Count
                ? _config.Actions[e.ActionIndex]
                : null;

            if (action == null)
            {
                return;
            }

            // Check if API key is configured for the selected provider
            if (string.IsNullOrWhiteSpace(_config?.ActiveApiKey))
            {
                var providerName = ProviderCatalog.Get(_config?.Provider ?? LlmProvider.OpenRouter).DisplayName;
                ShowResultPopup();
                _resultPopup?.ShowError($"Please configure your {providerName} API key in Settings");
                return;
            }

            // Capture selected text
            var capturedText = await _textCaptureService.CaptureSelectedTextAsync();

            if (string.IsNullOrWhiteSpace(capturedText))
            {
                ShowResultPopup();
                _resultPopup?.ShowError("No text selected");
                return;
            }

            // Show popup with loading state
            ShowResultPopup();
            _resultPopup?.ShowLoading();

            // Send to the configured provider
            var result = await _llmService.SendAsync(capturedText, _config!, action.Prompt);

            // Show result or error
            if (result.IsSuccess)
            {
                _resultPopup?.ShowResult(result.Content!);
            }
            else
            {
                _resultPopup?.ShowError(result.ErrorMessage!);
            }
        }

        private void ShowResultPopup()
        {
            // Close existing popup if open
            if (_resultPopup != null && _resultPopup.IsVisible)
            {
                _resultPopup.Close();
            }

            _resultPopup = new ResultPopup();
            _resultPopup.Show();
            _resultPopup.Activate();
        }

        private void InitializeTrayIcon()
        {
            // Create context menu
            var contextMenu = new ContextMenu();

            var settingsItem = new MenuItem { Header = "Settings" };
            settingsItem.Click += OnSettingsClick;
            contextMenu.Items.Add(settingsItem);

            contextMenu.Items.Add(new Separator());

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += OnExitClick;
            contextMenu.Items.Add(exitItem);

            // Create tray icon
            _trayIcon = new TaskbarIcon
            {
                Icon = new System.Drawing.Icon(GetResourceStream(new Uri("pack://application:,,,/Resources/tray-icon.ico")).Stream),
                ToolTipText = "TextToAI - Press hotkey to process selected text",
                ContextMenu = contextMenu
            };
        }

        private SettingsWindow? _settingsWindow;

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            // Prevent multiple settings windows
            if (_settingsWindow != null && _settingsWindow.IsVisible)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow();
            if (_settingsWindow.ShowDialog() == true)
            {
                // Reload config and re-register hotkeys
                _config = _configService?.Load();
                RegisterHotkeys();
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyService?.Dispose();
            _trayIcon?.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
