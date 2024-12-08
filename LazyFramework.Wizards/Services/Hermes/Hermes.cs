using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json;

namespace LazyFramework.Services.Hermes
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
    }
    public class Log
    {
        public DateTime Timestamp { get; }
        public LogLevel Level { get; }
        public string Message { get; }
        public string Context { get; }

        public Log(DateTime timestamp, LogLevel level, string message, string context)
        {
            Timestamp = timestamp;
            Level = level;
            Message = message;
            Context = context;
        }

        public override string ToString()
        {
            return $"[{GetTimestamp(Timestamp)}] [{Level}] [{Context}] {Message}";
        }

        private string GetTimestamp(DateTime dateTime) => dateTime.ToString("HH:mm:ss");
    }

    public class Hermes
    {
        private readonly Queue<Log> _logs = new(2000);
        private Window _window;


        public Hermes()
        {
            InitializeWindow();
            Log("Hermes initialized.", "Hermes", LogLevel.Info);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(_logs);
        }
        public void InitializeWindow()
        {
            if (_window == null) _window = new Window(this);
            Log($"Hermes window initialized", "Hermes", LogLevel.Info);
        }

        public void ShowWindow()
        {
            if(_window == null) throw new InvalidOperationException("Hermes window is not initialized.");
            _window.Show();
        }

        public Queue<Log> GetLogs()
        {
            return _logs;
        }

        public void Log(string message, string context, LogLevel level = LogLevel.Info )
        {
            var log = new Log(DateTime.Now, level, message, context);
            _logs.EnsureCapacity(2000);
            if (_logs.Count == 2000) _logs.Dequeue();
            _logs.Enqueue(log);
            if (_window == null) return;
            _window.AddContext(context);
            _window.RefreshDisplay();
        }

        public void ClearLogs()
        {
            _logs.Clear();
            if (_window == null) return;
            _window.RefreshDisplay();
        }


    }
}
