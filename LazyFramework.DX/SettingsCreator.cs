using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Settings;
using LazyFramework.DX.Services;
using System.Reflection;

namespace LazyFramework
{
    public static class SettingsCreator
    {
        public static string RootKey = "LazyFramework";
        public static SettingsCategory Root = new SettingsCategory()
        {
            Key = RootKey,
            Header = "LazyFramework",
            Description = "Settings for the Lazy Framework",
            IsHidden = false,
            IsDesignTime = true,
            RequiresPackageReload = false
        };

        public static string?[] GetAllSettingKeys()
        {
            // Get all types in the current assembly (you can narrow it down if needed)
            var types = Assembly.GetExecutingAssembly().GetTypes();

            // Find all classes that have static fields and name ends with "SettingsKeys"
            var keyValues = types
                .Where(t => t.IsClass && t.GetFields(BindingFlags.Public | BindingFlags.Static).Any())
                .Where(t => t.Name.EndsWith("SettingsKeys"))
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static)
                                  .Where(f => f.FieldType == typeof(string))
                                  .Select(f => (string)f.GetValue(null)))
                .ToArray();

            return keyValues;
        }

        public static string?[] GetAllSettingKeysForClass(Type type)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();

            var keyValues = types
                .Where(t => t.IsClass && t.GetFields(BindingFlags.Public | BindingFlags.Static).Any())
                .Where(t => t.Name == type.Name)
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static)
                                  .Where(f => f.FieldType == typeof(string))
                                  .Select(f => (string)f.GetValue(null)))
                .ToArray();
            return keyValues;
        }
        public static void CreateSettings(IWorkflowDesignApi workflowDesignApi)
        {
            var api = workflowDesignApi.Settings;
            api.AddCategory(Root);
            new DX.Services.Nabu.NabuSettingsSection().Initialize(workflowDesignApi.Settings, Root);
            new DX.Services.Heimdall.HeimdallSettingsSection().Initialize(workflowDesignApi.Settings, Root);
            new DX.Services.Athena.AthenaSettingsSection().Initialize(workflowDesignApi, Root);
        }
    }
}
