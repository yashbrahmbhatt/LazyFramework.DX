using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using LazyFramework.DX.Models.Consumers;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Api.Theme;
using Window = LazyFramework.DX.Services.Hermes.Window;

namespace LazyFramework.DX.Services.Hermes
{


    public class Hermes : BaseConsumer 
    {
        private readonly Queue<Log> _logs = new Queue<Log>(2000);
        private Window _window;
        private Timer _debounceTimer = new Timer()
        {
            Interval = 500,
            AutoReset = false,
        };
        private bool _refreshPending = false;

        public Hermes(IWorkflowDesignApi api) : base(api)
        {
            InitializeWindow();
            _debounceTimer.Elapsed += OnDebounceElapsed;


            
            Log("Hermes initialized.", "Hermes", LogLevel.Info);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(_logs);
        }
        public void InitializeWindow()
        {
            var theme = _api.Theme.GetThemeType();
            if (_window == null) _window = new Window(this, theme);
            //_window.Closed += (sender, args) => _window = null;
            Log($"Hermes window initialized", "Hermes", LogLevel.Info);
        }

        public void ShowWindow()
        {
            if (_window == null) throw new InvalidOperationException("Hermes window is not initialized.");
            _window.Show();
        }

        public Queue<Log> GetLogs()
        {
            return _logs;
        }

        private readonly object _lock = new object();
        public void Log(string message, string context, LogLevel level = LogLevel.Info)
        {
            var log = new Log(DateTime.Now, level, message, context);
            lock (_lock)
            {
            if (_logs.Count == 2000) _logs.Dequeue();
            _logs.Enqueue(log);
            }
            if (_window == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {

            _window.AddContext(context);
                TriggerDebouncedRefresh();
            });
        }
        private void TriggerDebouncedRefresh()
        {
            lock (_lock)
            {
                if (_refreshPending) return;
                _refreshPending = true;
            }

            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
        private void OnDebounceElapsed(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    _refreshPending = false;
                }
                _window.RefreshDisplay();
            });
        }

        public void ClearLogs()
        {
            _logs.Clear();
            if (_window == null) return;
            _window.RefreshDisplay();
        }


    }
}