using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using LazyFramework.Services.Hermes;

namespace LazyFramework.Services.Hermes
{
    public partial class Window : System.Windows.Window
    {
        private static Hermes _hermes;
        private Logger _logger;
        private string _filterText = string.Empty;

        private static readonly Dictionary<LogLevel, bool> _filterLevels = Enum
            .GetValues(typeof(LogLevel))
            .Cast<LogLevel>()
            .ToDictionary(level => level, _ => true);

        private static readonly Dictionary<string, bool> _filterContexts = new();

        private class Logger : LoggerConsumer
        {
            public Logger(Hermes hermes)
            {
                Logger = hermes;
                LoggerContext = "Hermes.Window";
            }
        }

        public Window(Hermes service)
        {
            _hermes = service;
            AddContext("Hermes.Window");
            _logger = new Logger(_hermes);

            InitializeComponent();

            PopulateContextComboBox();
            PopulateLogLevelComboBox();

            _logger.Log("Initialized and services started.", LogLevel.Info);

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
                    .Where(log => string.IsNullOrEmpty(_filterText) || log.Message.Contains(_filterText, StringComparison.OrdinalIgnoreCase));

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
            LogLevel.Debug => Brushes.Blue,
            LogLevel.Info => Brushes.Black,
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
            if (sender is not CheckBox checkbox) return;

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
            if (sender is not CheckBox checkbox) return;

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
                .Where(log => string.IsNullOrEmpty(_filterText) || log.Message.Contains(_filterText, StringComparison.OrdinalIgnoreCase));

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
