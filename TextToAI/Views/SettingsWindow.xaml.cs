using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextToAI.Models;
using TextToAI.Services;

namespace TextToAI.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigService _configService;
        private readonly AppConfig _config;
        private bool _isApiKeyVisible;
        private bool _isLoading;

        // Which hotkey box is currently capturing keystrokes, if any.
        private TextBox? _recordingTarget;
        private Button? _recordingButton;

        // Keys and models are remembered per provider so switching back and forth
        // in the dialog never blanks the other provider's settings.
        private readonly Dictionary<LlmProvider, string> _apiKeys = [];
        private readonly Dictionary<LlmProvider, string> _models = [];
        private LlmProvider _currentProvider;

        public SettingsWindow()
        {
            InitializeComponent();

            _configService = new ConfigService();
            _config = _configService.Load();

            LoadConfigToUI();
        }

        private TextBox[] HotkeyBoxes => [Hotkey1TextBox, Hotkey2TextBox];

        private TextBox[] PromptBoxes => [Prompt1TextBox, Prompt2TextBox];

        private void LoadConfigToUI()
        {
            _isLoading = true;

            _apiKeys[LlmProvider.OpenRouter] = _config.OpenRouterApiKey;
            _apiKeys[LlmProvider.OpenAI] = _config.OpenAiApiKey;

            foreach (var info in ProviderCatalog.All)
            {
                _models[info.Provider] = info.DefaultModel;
            }
            _models[_config.Provider] = _config.Model;

            _currentProvider = _config.Provider;

            ProviderComboBox.ItemsSource = ProviderCatalog.All;
            ProviderComboBox.SelectedItem = ProviderCatalog.Get(_config.Provider);

            for (var i = 0; i < HotkeyBoxes.Length; i++)
            {
                var action = i < _config.Actions.Count ? _config.Actions[i] : new PromptAction();
                HotkeyBoxes[i].Text = action.Hotkey;
                PromptBoxes[i].Text = action.Prompt;
            }

            StartWithWindowsCheckBox.IsChecked = AutoStartService.IsAutoStartEnabled();

            _isLoading = false;

            ApplyProvider(_config.Provider);
        }

        /// <summary>
        /// Points the key box, label, hint and model list at the given provider.
        /// </summary>
        private void ApplyProvider(LlmProvider provider)
        {
            var info = ProviderCatalog.Get(provider);

            ApiKeyLabel.Text = $"{info.DisplayName} API Key";
            ApiKeyHint.Text = $"Get a key at {info.KeysUrl}";

            var key = _apiKeys.TryGetValue(provider, out var stored) ? stored : string.Empty;
            ApiKeyPasswordBox.Password = key;
            ApiKeyTextBox.Text = key;

            ModelComboBox.ItemsSource = info.ModelPresets;
            ModelComboBox.Text = _models.TryGetValue(provider, out var model) && !string.IsNullOrWhiteSpace(model)
                ? model
                : info.DefaultModel;

            _currentProvider = provider;
        }

        private void Provider_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading || ProviderComboBox.SelectedItem is not ProviderInfo selected)
            {
                return;
            }

            if (selected.Provider == _currentProvider)
            {
                return;
            }

            // Stash whatever is on screen before swapping providers.
            _apiKeys[_currentProvider] = CurrentApiKey();
            _models[_currentProvider] = ModelComboBox.Text;

            ApplyProvider(selected.Provider);
        }

        private string CurrentApiKey() =>
            _isApiKeyVisible ? ApiKeyTextBox.Text : ApiKeyPasswordBox.Password;

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
            var button = (Button)sender;

            // Clicking the button that is already recording cancels it.
            if (_recordingButton == button)
            {
                StopRecording(restore: true);
                return;
            }

            StopRecording(restore: true);

            _recordingButton = button;
            _recordingTarget = button == RecordHotkey1Button ? Hotkey1TextBox : Hotkey2TextBox;
            _previousHotkeyText = _recordingTarget.Text;

            button.Content = "Press keys...";
            _recordingTarget.Text = "Waiting for input...";
            _recordingTarget.Focus();
        }

        private string _previousHotkeyText = string.Empty;

        private void StopRecording(bool restore)
        {
            if (_recordingButton == null || _recordingTarget == null)
            {
                return;
            }

            if (restore)
            {
                _recordingTarget.Text = _previousHotkeyText;
            }

            _recordingButton.Content = "Record";
            _recordingButton = null;
            _recordingTarget = null;
        }

        private void ClearHotkey2_Click(object sender, RoutedEventArgs e)
        {
            StopRecording(restore: true);
            Hotkey2TextBox.Text = string.Empty;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (_recordingTarget == null)
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

            // Escape cancels recording and keeps the previous hotkey.
            if (key == Key.Escape && modifiers == ModifierKeys.None)
            {
                StopRecording(restore: true);
                return;
            }

            // Build hotkey string
            var parts = new List<string>();

            if ((modifiers & ModifierKeys.Control) != 0)
                parts.Add("Ctrl");
            if ((modifiers & ModifierKeys.Alt) != 0)
                parts.Add("Alt");
            if ((modifiers & ModifierKeys.Shift) != 0)
                parts.Add("Shift");
            if ((modifiers & ModifierKeys.Windows) != 0)
                parts.Add("Win");

            parts.Add(key.ToString());

            _recordingTarget.Text = string.Join("+", parts);
            StopRecording(restore: false);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            StopRecording(restore: true);

            var provider = (ProviderComboBox.SelectedItem as ProviderInfo)?.Provider ?? _currentProvider;
            var info = ProviderCatalog.Get(provider);

            _apiKeys[provider] = CurrentApiKey();
            _models[provider] = ModelComboBox.Text;

            if (string.IsNullOrWhiteSpace(_apiKeys[provider]))
            {
                Invalid($"Please enter your {info.DisplayName} API key.");
                return;
            }

            var actions = new List<PromptAction>();
            for (var i = 0; i < HotkeyBoxes.Length; i++)
            {
                actions.Add(new PromptAction
                {
                    Hotkey = NormalizeHotkey(HotkeyBoxes[i].Text),
                    Prompt = PromptBoxes[i].Text
                });
            }

            if (string.IsNullOrWhiteSpace(actions[0].Hotkey))
            {
                Invalid("Please set a hotkey for Action 1.");
                return;
            }

            for (var i = 0; i < actions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(actions[i].Hotkey) && string.IsNullOrWhiteSpace(actions[i].Prompt))
                {
                    Invalid($"Action {i + 1} has a hotkey but no prompt.");
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(actions[1].Hotkey) &&
                actions[0].Hotkey.Equals(actions[1].Hotkey, StringComparison.OrdinalIgnoreCase))
            {
                Invalid("Action 1 and Action 2 use the same hotkey. Pick a different one.");
                return;
            }

            var model = _models[provider].Trim();

            _config.Provider = provider;
            _config.OpenRouterApiKey = _apiKeys.TryGetValue(LlmProvider.OpenRouter, out var orKey) ? orKey : string.Empty;
            _config.OpenAiApiKey = _apiKeys.TryGetValue(LlmProvider.OpenAI, out var oaKey) ? oaKey : string.Empty;
            _config.Model = string.IsNullOrWhiteSpace(model) ? info.DefaultModel : model;
            _config.Actions = actions;
            _config.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

            _configService.Save(_config);
            AutoStartService.SetAutoStart(_config.StartWithWindows);

            DialogResult = true;
            Close();
        }

        /// <summary>A half-finished recording leaves placeholder text in the box; treat it as empty.</summary>
        private static string NormalizeHotkey(string text) =>
            string.IsNullOrWhiteSpace(text) || text == "Waiting for input..."
                ? string.Empty
                : text.Trim();

        private void Invalid(string message) =>
            MessageBox.Show(this, message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
