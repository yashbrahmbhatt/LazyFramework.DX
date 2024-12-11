using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Hermes;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Models.Consumers
{
    public class SettingsConsumer : HermesConsumer
    {
        public SettingsConsumer(IWorkflowDesignApi api, Hermes hermes, string context) : base(api, hermes, context)
        {
        }

        public T GetSetting<T>(string key)
        {
            var success = _api.Settings.TryGetValue<T>(key, out var value);
            if(success)
            {
                Log($"Setting {key} has value {value}");
            }
            else
            {
                Log($"Setting {key} not found", LogLevel.Error);
            }
            return (T)value;
        }

        public void SetSetting(string key, string value)
        {
            var success = _api.Settings.TrySetValue(key, value);
            if (success)
            {
                Log($"Setting {key} set to {value}");
            }
            else
            {
                Log($"Setting {key} could not be set", LogLevel.Error);
            }

        }
    }
}
