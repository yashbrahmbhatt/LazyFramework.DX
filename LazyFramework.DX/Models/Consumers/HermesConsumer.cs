using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Hermes;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Models.Consumers
{
    public class HermesConsumer : BaseConsumer
    {
        public Hermes _hermes;
        public string _context;

        public HermesConsumer(IWorkflowDesignApi api, Hermes hermes, string context) : base(api)
        {
            _context = context;
            _hermes = hermes ?? throw new Exception("Hermes service doesn't exist.");
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            _hermes.Log(message, _context, level); // Assuming Hermes has an async Log method
        }
    }
}
