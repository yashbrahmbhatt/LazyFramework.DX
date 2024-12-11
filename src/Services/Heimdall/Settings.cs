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


namespace LazyFramework.DX.Services.Heimdall
{
    public static class HeimdallSettingsKeys
    {
        public static string HeimdallSectionKey = SettingsCreator.RootKey + ".Heimdall";
        public static string FilesIgnoreSettingKey = HeimdallSectionKey + ".Ignore";
    }

    public class Settings
    {
        private IWorkflowDesignApi Api;
        public IEnumerable<string> FilesToIgnore
        {
            get
            {
                Api.Settings.TryGetValue<string>(HeimdallSettingsKeys.FilesIgnoreSettingKey, out var files);
                return GitignoreParser.GeneratePaths(files, Api.ProjectPropertiesService.GetProjectDirectory());
            }
        }

        public Settings(IWorkflowDesignApi api)
        {
            Api = api;
        }
    }


    public class HeimdallSettingsSection : SettingsSection
    {

        public SingleValueEditorDescription<string> FilesIgnoreSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Ignore",
            Description = "A way to filter what files to ignore. Uses .gitignore notation",
            Key = HeimdallSettingsKeys.FilesIgnoreSettingKey,
            DefaultValue = ".",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = true,
            IsReadOnly = false
        };

        public HeimdallSettingsSection() : base()
        {
            RequiresPackageReload = true;
            Key = HeimdallSettingsKeys.HeimdallSectionKey;
            Title = "Heimdall";
            Description = "Witness the messages between gods.";
            IsDesignTime = true;
            RequiresPackageReload = true;
            IsExpanded = false;
            

        }

        public void Initialize(IActivitiesSettingsService settingsApi, SettingsCategory root)
        {
            settingsApi.AddSection(root, this);
            settingsApi.AddSetting(this, FilesIgnoreSetting);
        }
    }
}
