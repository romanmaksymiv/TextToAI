using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;

namespace TextToAI
{
    public partial class App : Application
    {
        private const string MutexName = "TextToAI_SingleInstance_Mutex";
        private Mutex? _mutex;
        private TaskbarIcon? _trayIcon;

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

            // Initialize system tray icon
            InitializeTrayIcon();
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

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            // Placeholder - will open SettingsWindow in Task 4
            MessageBox.Show("Settings window coming soon!", "TextToAI", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
