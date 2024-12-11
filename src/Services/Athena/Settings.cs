using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api.Settings;
using UiPath.Studio.Activities.Api;
using LazyFramework.Models;
using System.Activities;
using LazyFramework.DX.Services.Athena;
using LazyFramework.DX.Models.Consumers;

namespace LazyFramework.DX.Services.Athena
{
    public static class AthenaSettingKeys
    {
        public static string AthenaSectionKey = SettingsCreator.RootKey + ".Athena";
        public static string ConfigFilePathSettingKey = AthenaSectionKey + ".ConfigPath";
        public static string ConfigFileTypeSettingKey = AthenaSectionKey + ".ConfigType";
        public static string OutputPathSettingKey = AthenaSectionKey + ".OutputPath";
        public static string OutputNamespaceSettingKey = AthenaSectionKey + ".Namespace";
    }

    public class ConfigFileType
    {
        private ConfigFileType(string value) { Value = value; }
        public string Value { get; set; }
        public static ConfigFileType Excel { get { return new ConfigFileType("Excel"); } }
        public static ConfigFileType Json  { get { return new ConfigFileType("Json");  } }
    }
    

    public class AthenaSettings : SettingsConsumer
    {
        public string ConfigFilePath
        {
            get => GetSetting<string>(AthenaSettingKeys.ConfigFilePathSettingKey);
            set => SetSetting(AthenaSettingKeys.ConfigFilePathSettingKey, value);
        }
        public string ConfigFileType
        {
            get => GetSetting<string>(AthenaSettingKeys.ConfigFileTypeSettingKey);
            set => SetSetting(AthenaSettingKeys.ConfigFileTypeSettingKey, value);
        }
        public string OutputPath
        {
            get => GetSetting<string>(AthenaSettingKeys.OutputPathSettingKey);
            set => SetSetting(AthenaSettingKeys.OutputPathSettingKey, value);
        }
        public string OutputNamespace
        {
            get => GetSetting<string>(AthenaSettingKeys.OutputNamespaceSettingKey);
            set => SetSetting(AthenaSettingKeys.OutputNamespaceSettingKey, value);
        }

        public AthenaSettings(IWorkflowDesignApi api, Hermes.Hermes hermes) : base(api, hermes, "Athena.Settings")
        {
        }
    }


    public class AthenaSettingsSection : SettingsSection
    {
        public SingleValueEditorDescription<string> ConfigFilePathSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Path",
            Description = "The path to your config file.",
            Key = AthenaSettingKeys.ConfigFilePathSettingKey,
            DefaultValue = "Data\\Config.xlsx",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = false,
            IsReadOnly = false,
            Validate = val => (val.EndsWith(".xlsx") || val.EndsWith(".json")) ? "" : "Path must end with .xlsx or .json"

        };
        public SingleValueSelectorDescription ConfigFileTypeSetting = new SingleValueSelectorDescription()
        {
            Label = "Type",
            Description = "The type of config file you are using.",
            Key = AthenaSettingKeys.ConfigFileTypeSettingKey,
            DefaultValue = ConfigFileType.Excel.Value,
            GetDisplayValue = val => val.ToString(),
            IsDesignTime = true,
            RequiresPackageReload = false,
            IsReadOnly = false,
            Values = new string[2]{ ConfigFileType.Excel.Value, ConfigFileType.Json.Value }

        };
        public SingleValueEditorDescription<string> OutputPathSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Path",
            Description = "The path to output the generated classes.",
            Key = AthenaSettingKeys.OutputPathSettingKey,
            DefaultValue = "ConfigClasses",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = false,
            IsReadOnly = false,
        };
        public SingleValueEditorDescription<string> OutputNamespaceSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Namespace",
            Description = "The namespace to use for the generated classes.",
            Key = AthenaSettingKeys.OutputNamespaceSettingKey,
            DefaultValue = "Generated",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = false,
            IsReadOnly = false
        };

        public AthenaSettingsSection()
        {
            RequiresPackageReload = false;
            Key = AthenaSettingKeys.AthenaSectionKey;
            Title = "Athena";
            Description = "Athena brings order and wisdon to the chaos.\n\nThe Athena module automatically generates classes from your Excel or Json config files so that you don't have to rely on type-unsafe generics.";
            IsDesignTime = true;
            RequiresPackageReload = false;
            IsExpanded = false;

        }

        public void Initialize(IWorkflowDesignApi api, SettingsCategory root)
        {
            OutputNamespaceSetting.DefaultValue = api.ProjectPropertiesService.GetProjectName() + ".ConfigClasses";
            var settingsApi = api.Settings;
            settingsApi.AddSection(root, this);
            settingsApi.AddSetting(this, ConfigFilePathSetting);
            settingsApi.AddSetting(this, ConfigFileTypeSetting);
            settingsApi.AddSetting(this, OutputPathSetting);
            settingsApi.AddSetting(this, OutputNamespaceSetting);
        }
    }
}
