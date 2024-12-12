using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Hermes;
using LazyFramework.DX.Services.Odin;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Models.Consumers
{
    public class OdinConsumer : HermesConsumer
    {
        public Odin _odin;

        public OdinConsumer(IWorkflowDesignApi api, Hermes hermes, Odin odin, string context) : base(api, hermes, context)
        {
            _odin = odin ?? throw new Exception("Odin service doesn't exist.");
        }


    }
}
