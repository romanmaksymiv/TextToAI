using System.Windows;
using System.Windows.Input;
using TextToAI.Models;
using TextToAI.Services;

namespace TextToAI.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigService _configService;
        private readonly AppConfig _config;
        private bool _isRecordingHotkey;
        private bool _isApiKeyVisible;

        public SettingsWindow()
        {
            InitializeComponent();

            _configService = new ConfigService();
            _config = _configService.Load();

            LoadConfigToUI();
        }

        private void LoadConfigToUI()
        {
            ApiKeyPasswordBox.Password = _config.ApiKey;
            ApiKeyTextBox.Text = _config.ApiKey;
            HotkeyTextBox.Text = _config.Hotkey;
            PromptTextBox.Text = _config.Prompt;
            StartWithWindowsCheckBox.IsChecked = AutoStartService.IsAutoStartEnabled();

            // Set selected model
            foreach (System.Windows.Controls.ComboBoxItem item in ModelComboBox.Items)
            {
                if (item.Content.ToString() == _config.Model)
                {
                    ModelComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ToggleApiKeyVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isApiKeyVisible = !_isApiKeyVisible;

            if (_isApiKeyVisible)
            {
                ApiKeyTextBox.Text = ApiKeyPasswordBox.Password;
                ApiKeyPasswordBox.Visibility = Visibility.Collapsed;
                ApiKeyTextBox.Visibility = Visibility.Visible;
                ToggleApiKeyButton.Content = "Hide";
            }
            else
            {
                ApiKeyPasswordBox.Password = ApiKeyTextBox.Text;
                ApiKeyTextBox.Visibility = Visibility.Collapsed;
                ApiKeyPasswordBox.Visibility = Visibility.Visible;
                ToggleApiKeyButton.Content = "Show";
            }
        }

        private void RecordHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecordingHotkey)
            {
                _isRecordingHotkey = false;
                RecordHotkeyButton.Content = "Record";
                return;
            }

            _isRecordingHotkey = true;
            RecordHotkeyButton.Content = "Press keys...";
            HotkeyTextBox.Text = "Waiting for input...";
            HotkeyTextBox.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (!_isRecordingHotkey)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            e.Handled = true;

            // Ignore modifier-only presses
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LWin || e.Key == Key.RWin ||
                e.Key == Key.System)
            {
                return;
            }

            var modifiers = Keyboard.Modifiers;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Build hotkey string
            var parts = new System.Collections.Generic.List<string>();

            if ((modifiers & ModifierKeys.Control) != 0)
                parts.Add("Ctrl");
            if ((modifiers & ModifierKeys.Alt) != 0)
                parts.Add("Alt");
            if ((modifiers & ModifierKeys.Shift) != 0)
                parts.Add("Shift");
            if ((modifiers & ModifierKeys.Windows) != 0)
                parts.Add("Win");

            parts.Add(key.ToString());

            HotkeyTextBox.Text = string.Join("+", parts);
            _isRecordingHotkey = false;
            RecordHotkeyButton.Content = "Record";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Get API key from visible control
            var apiKey = _isApiKeyVisible ? ApiKeyTextBox.Text : ApiKeyPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show(this, "Please enter an API key.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(HotkeyTextBox.Text) || HotkeyTextBox.Text == "Waiting for input...")
            {
                MessageBox.Show(this, "Please set a hotkey.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _config.ApiKey = apiKey;
            _config.Model = (ModelComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "gpt-4o";
            _config.Hotkey = HotkeyTextBox.Text;
            _config.Prompt = PromptTextBox.Text;
            _config.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

            _configService.Save(_config);
            AutoStartService.SetAutoStart(_config.StartWithWindows);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
