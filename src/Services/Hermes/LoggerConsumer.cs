using System;
using System.Collections.Generic;
using System.Windows;

namespace LazyFramework.Services.Hermes
{
    public class LoggerConsumer
    {
        public string LoggerContext { get; set; }
        public Hermes Logger { get; set; }
        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (Logger == null)
            {
                throw new InvalidOperationException("Logger is not initialized.");
            }

            Logger.Log(message, LoggerContext, level);
        }
    }
}
