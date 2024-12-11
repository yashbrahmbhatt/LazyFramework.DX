using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UiPath.Studio.Api.Theme;

namespace LazyFramework.DX.Services.Hermes
{
    public partial class Window : System.Windows.Window
    {
        private static Hermes _hermes;
        private string _filterText = string.Empty;
        private async void Log(string message, LogLevel level = LogLevel.Info) => _hermes.Log(message, "Hermes.Window", level);

        private static readonly Dictionary<LogLevel, bool> _filterLevels = Enum
            .GetValues(typeof(LogLevel))
            .Cast<LogLevel>()
            .ToDictionary(level => level, _ => true);

        private static readonly Dictionary<string, bool> _filterContexts = new Dictionary<string, bool>();
        private int _theme;


        public Window(Hermes service, int theme)
        {
            _theme = theme;
            _hermes = service;
            AddContext("Hermes.Window");

            InitializeComponent();
            this.Icon = new BitmapImage(new Uri($"pack://application:,,,/LazyFramework.DX;component/Icons/" + (_theme == (int)ThemeType.Light ? "Hermes" : "Hermes_Contrast") + ".jpg", UriKind.Absolute));
            PopulateContextComboBox();
            PopulateLogLevelComboBox();

            Log("Initialized Hermes window.", LogLevel.Info);

            Show();
        }

        public void AddContext(string context)
        {
            if (_filterContexts.ContainsKey(context)) return;

            _filterContexts[context] = true;
            PopulateContextComboBox();
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (StreamTextBox == null) return;

            StreamTextBox.Dispatcher.Invoke(() =>
            {
            StreamTextBox.Document.Blocks.Clear();

                var filteredLogs = _hermes.GetLogs()
                    .Where(log => _filterLevels[log.Level])
                    .Where(log => _filterContexts[log.Context])
                    .Where(log => string.IsNullOrEmpty(_filterText) || log.Message.ToLower().Trim().Contains(_filterText.ToLower().Trim()));
                foreach (var log in filteredLogs)
                {
                    AppendLogToRichTextBox(log);
                }

                StreamTextBox.ScrollToEnd();
            });
        }

        private void AppendLogToRichTextBox(Log log)
        {
            var run = new Run(log.ToString())
            {
                Foreground = GetColorForLevel(log.Level)
            };

            StreamTextBox.Document.Blocks.Add(new Paragraph(run) { Margin = new Thickness(0) });
        }

        private System.Windows.Media.Brush GetColorForLevel(LogLevel level) => level switch
        {
            LogLevel.Debug => _theme == (int)ThemeType.Light ? Brushes.Blue : Brushes.DodgerBlue,
            LogLevel.Info => _theme == (int) ThemeType.Light ? Brushes.Black : Brushes.White,
            LogLevel.Warning => Brushes.Orange,
            LogLevel.Error => Brushes.Red,
            _ => Brushes.Black,
        };

        private void PopulateLogLevelComboBox()
        {
            // Create a list of SelectableItem from the filter levels dictionary
            var logLevels = _filterLevels
                .Select(level => new SelectableItem<LogLevel>
                {
                    Value = level.Key,
                    IsSelected = level.Value
                })
                .ToList();

            // Set the ItemsSource of the ComboBox
            LogLevelComboBox.ItemsSource = logLevels;

            // Set the Text property to display the selected log levels as a comma-separated string
            LogLevelComboBox.Text = string.Join(", ", logLevels
                .Where(c => c.IsSelected) // Only include selected log levels
                .Select(c => c.Value.ToString())); // Convert enum values to strings
        }

        private void PopulateContextComboBox()
        {
            if (ContextComboBox == null) return;

            var contexts = _filterContexts
                .Select(context => new SelectableItem<string>
                {
                    Value = context.Key,
                    IsSelected = context.Value
                })
                .ToList();

            ContextComboBox.ItemsSource = contexts;
            ContextComboBox.Text = string.Join(", ", contexts.Where(c => c.IsSelected).Select(c => c.Value));
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _filterText = SearchTextBox.Text;
            RefreshDisplay();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("Hermes cannot be closed and will be hidden instead.", "Close Hermes", MessageBoxButton.OK);
            e.Cancel = true;
            Hide();
        }

        private void LogLevel_Toggle(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkbox)) return;

            var logLevelName = checkbox.Content?.ToString();
            if (logLevelName == null) return;

            if (Enum.TryParse(logLevelName, out LogLevel logLevel))
            {
                _filterLevels[logLevel] = !_filterLevels[logLevel];
                PopulateLogLevelComboBox();
                RefreshDisplay();
            }
        }

        private void Context_Toggle(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkbox)) return;

            var contextName = checkbox.Content?.ToString();
            if (string.IsNullOrEmpty(contextName)) return;

            _filterContexts[contextName] = !_filterContexts[contextName];
            PopulateContextComboBox();
            RefreshDisplay();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the logs from the RichTextBox
            _hermes.ClearLogs();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Export logs logic (e.g., save to a file)
            var logs = _hermes.GetLogs()
                .Where(log => _filterLevels[log.Level])
                .Where(log => _filterContexts[log.Context])
                .Where(log => string.IsNullOrEmpty(_filterText) || log.Message.ToLower().Trim().Contains(_filterText.ToLower().Trim()));

            // Save logs to a file (e.g., using SaveFileDialog or another method)
            var logContent = string.Join(Environment.NewLine, logs.Select(log => log.ToString()));

            // Example: Save to a file (adjust to your needs)
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (dialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dialog.FileName, logContent);
            }
        }
    }

    public class SelectableItem<T>
    {
        public T Value { get; set; }
        public bool IsSelected { get; set; }
    }
}
