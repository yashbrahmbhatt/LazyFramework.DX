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

namespace LazyFramework.DX.Services.Athena
{
    public static class SettingKeys
    {
        public static string RootKey = "LazyFramework.Athena";
        public static string ConfigFileSectionKey = RootKey + ".ConfigFile";
        public static string ConfigFilePathSettingKey = ConfigFileSectionKey + ".Path";
        public static string ConfigFileTypeSettingKey = ConfigFileSectionKey + ".Type";
        public static string OutputSectionKey = RootKey + ".Output";
        public static string OutputPathSettingKey = OutputSectionKey + ".Path";
        public static string OutputNamespaceSettingKey = OutputSectionKey + ".Namespace";
        public static string MiscSectionKey = RootKey + ".Misc";
    }

    public class ConfigFileType
    {
        private ConfigFileType(string value) { Value = value; }
        public string Value { get; set; }
        public static ConfigFileType Excel { get { return new ConfigFileType("Excel"); } }
        public static ConfigFileType Json
        {
            get
            {
                return new ConfigFileType("Json");
            }
        }
    }
    

    public class Settings
    {
        private IWorkflowDesignApi Api;
        public string ConfigFilePath
        {
            get
            {
                Api.Settings.TryGetValue<string>(SettingKeys.ConfigFilePathSettingKey, out var files);
                return files;
            }
            set
            {
                Api.Settings.TrySetValue(SettingKeys.ConfigFilePathSettingKey, value);
            }
        }
        public string ConfigFileType
        {
            get
            {
                Api.Settings.TryGetValue<string>(SettingKeys.ConfigFileTypeSettingKey, out var files);
                return files;
            }
            set
            {
                Api.Settings.TrySetValue(SettingKeys.ConfigFileTypeSettingKey, value);
            }
        }
        public string OutputPath
        {
            get
            {
                Api.Settings.TryGetValue<string>(SettingKeys.OutputPathSettingKey, out var files);
                return files;
            }
            set
            {
                Api.Settings.TrySetValue(SettingKeys.OutputPathSettingKey, value);
            }
        }
        public string OutputNamespace
        {
            get
            {
                Api.Settings.TryGetValue<string>(SettingKeys.OutputNamespaceSettingKey, out var files);
                return files;
            }
            set
            {
                Api.Settings.TrySetValue(SettingKeys.OutputNamespaceSettingKey, value);
            }
        }

        public Settings(IWorkflowDesignApi api)
        {
            Api = api;
        }
    }


    public class SettingsCategory : UiPath.Studio.Activities.Api.Settings.SettingsCategory
    {
        public SettingsSection section = new SettingsSection()
        {
            RequiresPackageReload = true,
            Key = SettingKeys.ConfigFileSectionKey,
            Title = "Config File",
            Description = "Settings related to the config file for Athena to watch",
            IsDesignTime = true,
            IsExpanded = true
        };
        public SingleValueEditorDescription<string> ConfigFilePathSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Path",
            Description = "The path to your config file.",
            Key = SettingKeys.ConfigFilePathSettingKey,
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
            Key = SettingKeys.ConfigFileTypeSettingKey,
            DefaultValue = ConfigFileType.Excel.Value,
            GetDisplayValue = val => val.ToString(),
            IsDesignTime = true,
            RequiresPackageReload = false,
            IsReadOnly = false,
            Values = new string[2]{ ConfigFileType.Excel.Value, ConfigFileType.Json.Value }

        };

        public SettingsSection OutputSection = new SettingsSection()
        {
            RequiresPackageReload = true,
            Key = SettingKeys.OutputSectionKey,
            Title = "Output",
            Description = "Settings related to the output of Athena",
            IsDesignTime = true,
            IsExpanded = true
        };
        public SingleValueEditorDescription<string> OutputPathSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Path",
            Description = "The path to output the generated classes.",
            Key = SettingKeys.OutputPathSettingKey,
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
            Key = SettingKeys.OutputNamespaceSettingKey,
            DefaultValue = "Generated",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = false,
            IsReadOnly = false
        };

        public SettingsSection MiscSection = new SettingsSection()
        {
            RequiresPackageReload = true,
            Key = SettingKeys.MiscSectionKey,
            Title = "Miscellaneous",
            Description = "I'll organize these settings later.",
            IsDesignTime = true,
            IsExpanded = true

        };

        public SettingsCategory()
        {
            RequiresPackageReload = false;
            Key = SettingKeys.RootKey;
            Header = "Athena";
            Description = "Athena brings order and wisdon to the chaos.\n\nThe Athena module automatically generates classes from your Excel or Json config files so that you don't have to rely on type-unsafe generics.";
            IsDesignTime = true;
            IsHidden = false;

        }

        public void Initialize(IWorkflowDesignApi api)
        {
            var settingsApi = api.Settings;
            OutputNamespaceSetting.DefaultValue = api.ProjectPropertiesService.GetProjectName() + ".ConfigClasses";
            settingsApi.AddCategory(this);
            settingsApi.AddSection(this, section);
            settingsApi.AddSection(this, MiscSection);
            settingsApi.AddSetting(section, ConfigFilePathSetting);
            settingsApi.AddSetting(section, ConfigFileTypeSetting);
            settingsApi.AddSection(this, OutputSection);
            settingsApi.AddSetting(OutputSection, OutputPathSetting);
            settingsApi.AddSetting(OutputSection, OutputNamespaceSetting);
        }
    }
}
