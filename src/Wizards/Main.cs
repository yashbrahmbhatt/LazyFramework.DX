using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UiPath.Studio.Activities.Api.Wizards;
using UiPath.Studio.Activities.Api;
using Newtonsoft.Json;
using LazyFramework.Models.Config;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using LazyFramework.Services.Hermes;
using LazyFramework.Services.Odin;
using LazyFramework.Services.Athena;
using System.Activities;

namespace LazyFramework.Wizards
{
    public class Main
    {
        private static IServiceCollection _services;
        private static ServiceProvider _provider;
        private static Logger _logger;
        private class Logger : LoggerConsumer
        {
            public Logger(Hermes hermes)
            {
                Logger = hermes;
                LoggerContext = "App";
            }
        }
        private static void InitializeServices(IWorkflowDesignApi api)
        {
            try
            {
                _services = new ServiceCollection();
                _services.AddSingleton<IWorkflowDesignApi>(api);
                _services.AddSingleton<Hermes>();
                _services.AddSingleton<Odin>(provider =>
                {
                    var odin = new Odin(provider) ?? throw new InvalidOperationException("Failed to initialize Odin.");
                    return odin;
                });
                _services.AddSingleton<Athena>(provider =>
                {
                    var athena = new Athena(provider) ?? throw new InvalidOperationException("Failed to initialize Athena.");
                    return athena;
                });
                // Build service provider
                _provider = _services.BuildServiceProvider(new ServiceProviderOptions()
                {
                    ValidateOnBuild = true,
                    
                });
                
                // Test services
                var hermes = _provider.GetService<Hermes>();
                var odin = _provider.GetService<Odin>();
                var athena = _provider.GetService<Athena>();
                if (hermes == null || odin == null || athena == null)
                {
                    throw new InvalidOperationException("One or more services failed to initialize.");
                }
                _logger = new Logger(hermes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ConfigureServices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static void CreateWizard(IWorkflowDesignApi workflowDesignApi)
        {
            try
            {
                InitializeServices(workflowDesignApi);
                var wizardApi = workflowDesignApi.Wizards;
                var wizardDefinition = new WizardDefinition()
                {
                    // You can add other definitions here to create a dropdown.
                    //ChildrenDefinitions.Add()
                    Wizard = new WizardBase()
                    {
                        RunWizard = RunWizard
                    },
                    DisplayName = "LazyFramework",
                    Shortcut = new KeyGesture(Key.F9, ModifierKeys.Control | ModifierKeys.Shift),
                    IconUri = "Icons/RecordIcon",
                    Tooltip = "Be lazy and leverage design time tools to make development easier."
                };
                var collection = new WizardCollection(); //Use a collection to group all of your wizards.
                collection.WizardDefinitions.Add(wizardDefinition);

                wizardApi.Register(collection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static Activity RunWizard()
        {
            try
            {

                _logger.Log("Running wizard", LogLevel.Debug);
                if (_provider == null) throw new InvalidOperationException("Services not initialized.");
                var hermes = _provider.GetService<Hermes>();
                hermes.ShowWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in RunWizard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }


    }
}
