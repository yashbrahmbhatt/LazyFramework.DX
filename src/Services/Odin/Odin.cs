using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LazyFramework.Models;
using LazyFramework.DX.Services.Hermes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using LazyFramework.DX.Models;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Heimdall;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api.Settings;



namespace LazyFramework.DX.Services.Odin
{
    public class Odin : HermesConsumer, IMediator
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        public Odin(IWorkflowDesignApi api, Hermes.Hermes hermes): base(api, hermes, "Odin")
        {
            try
            {
                Log("Initializing Odin."); // Error here 'No key exists for 'Hermes' ???????
                _api.Settings.RegisterValueChangedObserver(new UiPath.Studio.Activities.Api.Settings.SettingsObserver()
                {
                    Keys = SettingsCreator.GetAllSettingKeys(),
                    ValueChanged = async (e) => await OnSettingChanges(e)
                });
                Log($"Odin intialized.");
            }
            catch (Exception ex)
            {
                Log($"Failed to initialize Odin: {ex.Message}", LogLevel.Error);
                throw;

            }
        }

        public async Task OnSettingChanges(SettingsValueChangedArgs e)
        {
            Log($"{e.ChangedSettings.Count} settings changed.");
            foreach (var setting in e.ChangedSettings)
            {
                await Notify<SettingChangedEvent>(new SettingChangedEvent(setting));
            }
            Log($"Settings change event handled.");
        }

        public async Task Register<TEvent>(Action<TEvent> handler)
        {
            Log($"Registering handler for event {typeof(TEvent).Name}");
            if (!_handlers.ContainsKey(typeof(TEvent)))
            {
                _handlers[typeof(TEvent)] = new List<Delegate>();
            }
            _handlers[typeof(TEvent)].Add(handler);
            Log($"Handler registered for event {typeof(TEvent).Name}");
        }

        public async Task Notify<TEvent>(TEvent eventArgs)
        {
            Log($"Notifying event {typeof(TEvent).Name}", LogLevel.Debug);
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                foreach (var handler in handlers.Cast<Action<TEvent>>())
                {
                    handler.Invoke(eventArgs);
                }
                Log($"Event {typeof(TEvent).Name} handled by {handlers.Count} handlers");
            }
            else
            {
                //Log($"No handlers registered for event {typeof(TEvent).Name}", LogLevel.Warning);
            }

        }
    }

}
