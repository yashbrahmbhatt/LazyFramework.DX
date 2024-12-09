using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LazyFramework.Models;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Settings;


namespace LazyFramework.DX.Services.Odin
{
    public static class HermesSettingsKeys
    {
        public static string RootKey = "LazyFramework.Hermes";
        public static string FilesSectionKey = RootKey + ".Files";
        public static string FilesIgnoreSettingKey = FilesSectionKey + ".Ignore";
        public static string MiscSectionKey = RootKey + ".Misc";
    }

    public class Settings
    {
        private IWorkflowDesignApi Api;
        public IEnumerable<string> FilesToIgnore
        {
            get
            {
                Api.Settings.TryGetValue<string>(HermesSettingsKeys.FilesIgnoreSettingKey, out var files);
                return GitignoreParser.GeneratePaths(files, Api.ProjectPropertiesService.GetProjectDirectory());
            }
        }

        public Settings(IWorkflowDesignApi api)
        {
            Api = api;
        }
    }


    public class SettingsCategory : UiPath.Studio.Activities.Api.Settings.SettingsCategory
    {
        public SettingsSection FilesSection = new SettingsSection()
        {
            RequiresPackageReload = true,
            Key = HermesSettingsKeys.FilesSectionKey,
            Title = "Files",
            Description = "Adjust the files that Hermes is looking at.",
            IsDesignTime = true,
            IsExpanded = true
        };
        public SingleValueEditorDescription<string> FilesIgnoreSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Ignore",
            Description = "A way to filter what files to ignore. Uses .gitignore notation",
            Key = HermesSettingsKeys.FilesIgnoreSettingKey,
            DefaultValue = ".",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = true,
            IsReadOnly = false
        };

        public SettingsSection MiscSection = new SettingsSection()
        {
            RequiresPackageReload = true,
            Key = HermesSettingsKeys.MiscSectionKey,
            Title = "Miscellaneous",
            Description = "I'll organize these settings later.",
            IsDesignTime = true,
            IsExpanded = true

        };


        public SettingsCategory()
        {
            RequiresPackageReload = true;
            Key = HermesSettingsKeys.RootKey;
            Header = "Hermes";
            Description = "Witness the messages between gods.";
            IsDesignTime = true;
            IsHidden = false;

        }

        public void Initialize(IActivitiesSettingsService settingsApi)
        {
            settingsApi.AddCategory(this);
            settingsApi.AddSection(this, FilesSection);
            settingsApi.AddSection(this, MiscSection);
            settingsApi.AddSetting(FilesSection, FilesIgnoreSetting);
        }
    }




}
