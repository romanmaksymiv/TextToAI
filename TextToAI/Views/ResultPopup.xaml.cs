using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace TextToAI.Views
{
    public partial class ResultPopup : Window
    {
        private readonly Stopwatch _stopwatch = new();

        public ResultPopup()
        {
            InitializeComponent();
        }

        public void ShowLoading()
        {
            _stopwatch.Restart();
            LoadingPanel.Visibility = Visibility.Visible;
            ResultTextBox.Visibility = Visibility.Collapsed;
            ButtonPanel.Visibility = Visibility.Collapsed;
            CopyButton.Visibility = Visibility.Visible;
        }

        public void ShowResult(string text)
        {
            _stopwatch.Stop();
            LoadingPanel.Visibility = Visibility.Collapsed;
            ResultTextBox.Text = text;
            ResultTextBox.Visibility = Visibility.Visible;
            ButtonPanel.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            DurationText.Text = $"{_stopwatch.Elapsed.TotalSeconds:F1}s";
        }

        public void ShowError(string message)
        {
            _stopwatch.Stop();
            LoadingPanel.Visibility = Visibility.Collapsed;
            ResultTextBox.Text = message;
            ResultTextBox.Visibility = Visibility.Visible;
            ButtonPanel.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Collapsed;
            DurationText.Text = $"{_stopwatch.Elapsed.TotalSeconds:F1}s";
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ResultTextBox.Text))
            {
                Clipboard.SetText(ResultTextBox.Text);
            }
            Close();
        }

        private void Dismiss_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
