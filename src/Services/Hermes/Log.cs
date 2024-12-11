using System;

namespace LazyFramework.DX.Services.Hermes
{
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
}