using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UiPath.Studio.Activities.Api.Settings;
using UiPath.Studio.Activities.Api;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Hermes;
using LazyFramework.Models;

namespace LazyFramework.DX.Services.Nabu
{
    public static class NabuSettingsKeys
    {
        public static string NabuSectionKey = SettingsCreator.RootKey + ".Nabu";
        public static string ProjectTemplateSettingKey = NabuSectionKey + ".ProjectTemplate";
        public static string WorkflowTemplateSettingKey = NabuSectionKey + ".WorkflowTemplate";
        public static string MermaidPrefixSettingKey = NabuSectionKey + ".MermaidPrefix";
        public static string MermaidSuffixSettingKey = NabuSectionKey + ".MermaidSuffix";
        public static string IgnoredDirectoriesSetting = NabuSectionKey + ".IgnoredDirectories";
        public static string OutputRoot = NabuSectionKey + ".OutputRoot";
    }

    public class NabuSettings : SettingsConsumer
    {
        public string ProjectTemplateSetting
        {
            get => GetSetting<string>(NabuSettingsKeys.ProjectTemplateSettingKey);
            set => SetSetting(NabuSettingsKeys.ProjectTemplateSettingKey, value);
        }

        public string WorkflowTemplateSetting
        {
            get => GetSetting<string>(NabuSettingsKeys.WorkflowTemplateSettingKey);
            set => SetSetting(NabuSettingsKeys.WorkflowTemplateSettingKey, value);
        }
        public string MermaidPrefixSetting
        {
            get => GetSetting<string>(NabuSettingsKeys.MermaidPrefixSettingKey);
            set => SetSetting(NabuSettingsKeys.MermaidPrefixSettingKey, value);
        }
        public string MermainSuffixSetting
        {
            get => GetSetting<string>(NabuSettingsKeys.MermaidSuffixSettingKey);
            set => SetSetting(NabuSettingsKeys.MermaidSuffixSettingKey, value);
        }

        public IEnumerable<string> IgnoredDirectories
        {
            get
            {
                var value = GetSetting<string>(NabuSettingsKeys.IgnoredDirectoriesSetting);
                return GitignoreParser.GeneratePaths(value, _api.ProjectPropertiesService.GetProjectDirectory());
            }
        }
        public string OutputRoot
        {
            get => GetSetting<string>(NabuSettingsKeys.OutputRoot);
            set => SetSetting(NabuSettingsKeys.OutputRoot, value);
        }
        public NabuSettings(IWorkflowDesignApi api, Hermes.Hermes hermes) : base(api, hermes, "Nabu.Settings")
        {

        }
    }


    public class NabuSettingsSection : SettingsSection
    {

        public SingleValueEditorDescription<string> ProjectTemplateSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Project Template",
            Description = $"A template for the markdown to generate for the project scope. Available keys: \r\n" +
            $"EntryPoints - A table with the paths to the entry points\r\n" +
            $"Dependencies - A table with the name and version of the dependencies of the project\r\n" +
            $"Description - The project description\r\n" +
            $"Language - The .NET language used in this project\r\n" +
            $"StudioVersion - The Studio version used for this project\r\n" +
            $"Version - The version of the project\r\n" +
            $"Type - The output type of this project",
            Key = NabuSettingsKeys.ProjectTemplateSettingKey,
            DefaultValue =
@"# {Name}
Type: {Type}
Version: {Version}
Studio Version: {StudioVersion}
Language: {Language}

{Description}

<hr />

## Project Details
<details>
    <summary>
    <b>Dependencies</b>
    </summary>

{Dependencies}

</details>
<details>
    <summary>
    <b>Entry Points</b>
    </summary>

{EntryPoints}

</details>",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = true,
            IsReadOnly = false
        };

        public SingleValueEditorDescription<string> WorkflowTemplateSetting = new SingleValueEditorDescription<string>()
        {
            Label = "Workflow Template",
            Description = $"A template for the markdown to generate for the workflow scope. Available keys: \r\n" +
                        $"WorkflowName - The name of the workflow\r\n" +
                        $"Class - The class of the workflow\r\n" +
                        $"Description - The description of the workflow\r\n" +
                        $"Namespaces - A list of namespaces used in the workflow\r\n" +
                        $"References - A list of references included in the workflow\r\n" +
                        $"Arguments - A table of arguments used by the workflow\r\n" +
                        $"WorkflowsUsed - A list of workflows used within this workflow\r\n" +
                        $"Tests - A list of tests associated with this workflow\r\n" +
                        $"Outline - The mermaid diagram of the workflow (Beta)\r\n",
            Key = NabuSettingsKeys.WorkflowTemplateSettingKey,
            DefaultValue = 
@"#{WorkflowName}
Class: {Class}

{Description}

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>

{Namespaces}

</details>
<details>
    <summary>
    <b>References</b>
    </summary>

{References}

</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

{Arguments}

</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

{WorkflowsUsed}

</details>
<details>
    <summary>
    <b>Tests</b>
    </summary>

{Tests}

</details>

<hr />

## Outline (Beta)

{Outline}"
,
            GetDisplayValue = val => val,
            IsDesignTime = true,
            RequiresPackageReload = true,
            IsReadOnly = false
        };

        public SingleValueEditorDescription<string> MermaidPrefixSetting = new SingleValueEditorDescription<string>()
        {
            DefaultValue = ":::mermaid\r\n" +
                            "stateDiagram-v2",
            Description = "The prefix to use for the mermaid diagram. Different repository providers have different syntax.",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            Key = NabuSettingsKeys.MermaidPrefixSettingKey,
            Label = "Mermaid Prefix",
            RequiresPackageReload = false,
            IsReadOnly = false,
        };
        public SingleValueEditorDescription<string> MermaidSuffixSetting = new SingleValueEditorDescription<string>()
        {
            DefaultValue = ":::",
            Description = "The suffix to use for the mermaid diagram. Different repository providers have different syntax.",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            Key = NabuSettingsKeys.MermaidSuffixSettingKey,
            Label = "Mermaid Suffix",
            RequiresPackageReload = false,
            IsReadOnly = false,
        };

        public SingleValueEditorDescription<string> IgnoredDirectoriesSetting = new SingleValueEditorDescription<string>()
        {
            DefaultValue = 
@"Design
.templates",
            Description = "The list of directory names to ignore during file processing in .gitignore format.",
            GetDisplayValue = val => string.Join(", ", val),
            IsDesignTime = true,
            Key = NabuSettingsKeys.IgnoredDirectoriesSetting,
            Label = "Ignored Directories",
            RequiresPackageReload = false,
            IsReadOnly = false,
        };

        public SingleValueEditorDescription<string> OutputRootSetting = new SingleValueEditorDescription<string>()
        {
            DefaultValue = "Documentation",
            Description = "The root directory where output files are stored.",
            GetDisplayValue = val => val,
            IsDesignTime = true,
            Key = NabuSettingsKeys.OutputRoot,
            Label = "Output Root",
            RequiresPackageReload = false,
            IsReadOnly = false,
        };

        public NabuSettingsSection() : base()
        {
            RequiresPackageReload = true;
            Key = NabuSettingsKeys.NabuSectionKey;
            Title = "Nabu";
            Description = "The babylonian god of writing blesses you with documentation.";
            IsDesignTime = true;
            RequiresPackageReload = true;
            IsExpanded = false;


        }

        public void Initialize(IActivitiesSettingsService settingsApi, SettingsCategory root)
        {
            settingsApi.AddSection(root, this);
            settingsApi.AddSetting(this, OutputRootSetting);
            settingsApi.AddSetting(this, IgnoredDirectoriesSetting);
            settingsApi.AddSetting(this, ProjectTemplateSetting);
            settingsApi.AddSetting(this, WorkflowTemplateSetting);
            settingsApi.AddSetting(this, MermaidPrefixSetting);
            settingsApi.AddSetting(this, MermaidSuffixSetting);
        }
    }
}