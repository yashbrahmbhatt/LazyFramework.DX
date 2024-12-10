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
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Activities;
using System.Windows.Media.Imaging;
using LazyFramework.DX.Icons;

namespace LazyFramework.DX.Services.Hermes
{
    public static class Wizard
    {

        private static Hermes _hermes;

        private static async void Log(string message, LogLevel level = LogLevel.Info) => _hermes.Log(message, "Hermes.Wizard", level);

 

        public static void CreateWizard(IWorkflowDesignApi workflowDesignApi, Hermes hermes)
        {
            try
            {
                _hermes = hermes;
                //Log($"HERE: {JsonConvert.SerializeObject(Application.Current.Resources, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
                //Helpers.WriteIconToFile("LazyFramework.DX.Icons.Hermes.jpg");

                var hermesWizard = new WizardDefinition()
                {
                    // You can add other definitions here to create a dropdown.
                    //ChildrenDefinitions.Add()
                    Wizard = new WizardBase()
                    {
                        RunWizard = RunWizard
                    },
                    DisplayName = "Hermes",
                    Shortcut = new KeyGesture(Key.F9, ModifierKeys.Control | ModifierKeys.Shift),
                    IconUri = "Icons/RecordIcon",
                    Tooltip = "Open the window for Hermes, letting you see the logs for the LazyFramework.DX modules."
                };

                var collection = new WizardCollection(); //Use a collection to group all of your wizards.
                collection.WizardDefinitions.Add(hermesWizard);

                workflowDesignApi.Wizards.Register(collection);
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

                Log("Running wizard", LogLevel.Debug);

                _hermes.InitializeWindow();
                _hermes.ShowWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in RunWizard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }


    }
}
