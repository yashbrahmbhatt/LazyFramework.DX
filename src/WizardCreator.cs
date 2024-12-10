using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api.Wizards;
using UiPath.Studio.Activities.Api;
using System.Windows.Input;
using System.Activities;
using Activity = System.Activities.Activity;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using LazyFramework.DX.Services.Hermes;
using LazyFramework.DX.Services.Odin;
using LazyFramework.DX.Services.Athena;
using Newtonsoft.Json;

namespace LazyFramework
{
    public static class WizardCreator
    {
        private static IServiceCollection _services;
        private static ServiceProvider _provider;
        private static Hermes _hermes;
        private static Odin _odin;
        private static Athena _athena;
        private static async void Log(string message, LogLevel level = LogLevel.Info) => _hermes.Log(message, "LazyFramework.WizardCreator", level);
        //private static Nabu _nabu;

        private static void InitializeServices(IWorkflowDesignApi api)
        {
            try
            {
                _services = new ServiceCollection();
                _services.AddSingleton<IWorkflowDesignApi>(api);
                _hermes = new Hermes(api);
                _services.AddSingleton(_hermes);
                _services.AddSingleton<Odin>(provider =>
                {
                    var odin = new Odin(provider) ?? throw new Exception("Failed to initialize Odin.");
                    return odin;
                });
                _services.AddSingleton<Athena>(provider =>
                {
                    var athena = new Athena(provider) ?? throw new Exception("Failed to initialize Athena.");
                    return athena;
                });
                //_services.AddSingleton<Nabu>(provider =>
                //{
                //    var nabu = new Nabu(provider) ?? throw new Exception("Failed to initialize Nabu.");
                //    return nabu;
                //});

                // Build service provider
                _provider = _services.BuildServiceProvider(new ServiceProviderOptions()
                {
                    ValidateOnBuild = true,
                });

                _odin = _provider.GetService<Odin>() ?? throw new Exception("Odin failed to initialize.");
                _athena = _provider.GetService<Athena>() ?? throw new Exception("Athena failed to initialize.");
                //_nabu = _provider.GetService<Nabu>() ?? throw new Exception("Nabu failed to initialize.");


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ConfigureServices: {ex.Message}, ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        public static void CreateWizards(IWorkflowDesignApi workflowDesignApi)
        {
            InitializeServices(workflowDesignApi);
            Wizard.CreateWizard(workflowDesignApi, _hermes);
            //Log(JsonConvert.SerializeObject(Application.Current.MainWindow, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
        }

    }
}
