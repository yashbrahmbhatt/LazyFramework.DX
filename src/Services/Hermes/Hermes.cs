using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Api.Theme;
using Window = LazyFramework.DX.Services.Hermes.Window;

namespace LazyFramework.DX.Services.Hermes
{
    

    public class Hermes
    {
        private readonly Queue<Log> _logs = new Queue<Log>(2000);
        private Window _window;
        private IWorkflowDesignApi _api;


        public Hermes(IWorkflowDesignApi api)
        {
            _api = api;           
            InitializeWindow();
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

        public void Log(string message, string context, LogLevel level = LogLevel.Info)
        {
            var log = new Log(DateTime.Now, level, message, context);
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