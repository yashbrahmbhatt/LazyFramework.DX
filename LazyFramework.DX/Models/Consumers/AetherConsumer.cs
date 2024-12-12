using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Aether;
using LazyFramework.DX.Services.Hermes;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Models.Consumers
{
    public class AetherConsumer : HermesConsumer
    {
        public Aether _aether;
        
        public AetherConsumer(IWorkflowDesignApi api, Hermes hermes, Aether aether, string context) : base(api, hermes, context)
        {
            _aether = aether ?? throw new Exception("Aether service doesn't exist.");
        }
    }
}
