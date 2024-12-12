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
using UiPath.Studio.Api.Theme;

namespace LazyFramework.DX.Services.Hermes
{
    public static class Wizard
    {

        private static Hermes _hermes;

        public static void CreateWizard(IWorkflowDesignApi workflowDesignApi, Hermes hermes)
        {
            try
            {
                _hermes = hermes;
                var theme = workflowDesignApi.Theme.GetThemeType();
                var hermesWizard = new WizardDefinition()
                {
                    // You can add other definitions here to create a dropdown.
                    //ChildrenDefinitions.Add()
                    Wizard = new WizardBase(),
                    DisplayName = "Hermes",
                    Shortcut = new KeyGesture(Key.F9, ModifierKeys.Control | ModifierKeys.Shift),
                    IconUri = $"pack://application:,,,/YourExtension;component/Icons/" +
          (theme == (int)ThemeType.Light ? "Hermes.jpg" : "Hermes_Contrast.jpg"),
                    Tooltip = "A set of wizards for interacting with the Hermes module",
                };
                hermesWizard.ChildrenDefinitions.Add(new WizardDefinition()
                {
                    Wizard = new WizardBase()
                    {
                        RunWizard = OpenWindow
                    },
                    DisplayName = "Open Hermes Window",
                    IconUri = $"pack://application:,,,/YourExtension;component/Icons/" +
          (theme == (int)ThemeType.Light ? "Hermes.jpg" : "Hermes_Contrast.jpg"),
                    Tooltip = "Open the Hermes window",
                    Shortcut = new KeyGesture(Key.F10, ModifierKeys.Control | ModifierKeys.Shift)
                });

                var collection = new WizardCollection(); //Use a collection to group all of your wizards.
                collection.WizardDefinitions.Add(hermesWizard);

                workflowDesignApi.Wizards.Register(collection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static Activity OpenWindow()
        {
            try
            {
                if (_hermes == null) return null;
                _hermes.Log("Running open window wizard", "Hermes.Wizards.OpenWindow", LogLevel.Debug);
                _hermes.ShowWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in RunWizard: {ex.Message} {ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }


    }
}
