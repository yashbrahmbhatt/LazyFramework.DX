using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.Services.Hermes
{
    public interface ILoggerService
    {
        void Log(string message, LogLevel level = LogLevel.Info);
    }
}
