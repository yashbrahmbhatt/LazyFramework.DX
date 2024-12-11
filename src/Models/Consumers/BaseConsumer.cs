using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Models.Consumers
{
    public class BaseConsumer
    {
        public IWorkflowDesignApi _api;

        public BaseConsumer(IWorkflowDesignApi api)
        {
            _api = api ?? throw new Exception("Workflow design api is not initialized.");
        }
    }
}
