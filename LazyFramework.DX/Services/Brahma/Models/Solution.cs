using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Helpers;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Athena.Models;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.ProjectProperties;

namespace LazyFramework.DX.Services.Brahma.Models
{
    public class Solution : HermesConsumer
    {
        public string Name;
        public string Root;
        public string Id;
        public ExpressionLanguage ExpressionLanguage;
        public string TargetFramework;
        public UiPathProject RootProject;
        public Dictionary<string, UiPathProject> Projects = new Dictionary<string, UiPathProject>();
        public Dictionary<string, Editor> ProjectWorkflows = new Dictionary<string, Editor>();
        public Dictionary<string, ConfigObject> ProjectConfigurations = new Dictionary<string, ConfigObject>();
        public ConfigObject SharedConfiguration = new ConfigObject();
        public bool IsSingleProject => Projects.Count == 0;
        public Version StudioVersion;

        public Solution(IWorkflowDesignApi api, Hermes.Hermes hermes) : base(api, hermes, "Brahma.Solution")
        {
            Log("Initializing Brahma");
            Name = _api.ProjectPropertiesService.GetProjectName();
            Id = _api.ProjectPropertiesService.GetProjectId();
            Root = _api.ProjectPropertiesService.GetProjectDirectory();
            ExpressionLanguage = (ExpressionLanguage)_api.ProjectPropertiesService.GetExpressionLanguage();
            TargetFramework = _api.ProjectPropertiesService.GetTargetFramework();
            StudioVersion = _api.StudioDesignSettings.Version;
            LoadRootProject();
            LoadWorkflows(Root);
            Log("Brahma initialized");
        }

        public void LoadRootProject()
        {
            Log($"Loading root project from '{Root}'");
            RootProject = JsonConvert.DeserializeObject<UiPathProject>(File.ReadAllText(Path.Combine(Root, "project.json"))) ?? throw new Exception("Could not load root project");
            Log($"Loaded root project '{RootProject.Name}'");
        }

        public void LoadWorkflows(string root)
        {
            Log($"Loading workflows from '{root}'");
            Directory.GetFiles(root, "*.xaml", SearchOption.AllDirectories).ToList().ForEach(f =>
            {
                var editor = new Editor(f, _hermes, _api);
                ProjectWorkflows.Add(PathExtensions.GetRelativePath(root, f), editor);
            });
            Log($"Loaded {ProjectWorkflows.Count} workflows");
        }

        public void LoadProjects()
        {
            Log($"Loading projects from '{Root}'");
            Directory.GetFiles(Root, "*.project.json", SearchOption.AllDirectories).ToList().ForEach(f =>
            {
                var project = JsonConvert.DeserializeObject<UiPathProject>(File.ReadAllText(f)) ?? throw new Exception($"Could not load project json at '{f}'");
                Projects.Add(PathExtensions.GetRelativePath(Root, f), project);
            });
            Log($"Loaded {Projects.Count} projects");
        }

        public void UpdateProject(string relativeJsonPath)
        {
            if (!Projects.ContainsKey(relativeJsonPath)) throw new Exception($"Project at '{relativeJsonPath}' not found");
            var text = JsonConvert.SerializeObject(Projects[relativeJsonPath]);
            File.WriteAllText(Path.Combine(Root, relativeJsonPath), text);
        }

        public object ToObject()
        {
            return new
            {
                Name,
                Root,
                Id,
                ExpressionLanguage,
                TargetFramework,
                RootProject,
                Projects,
                ProjectWorkflows = ProjectWorkflows.Values.Select(editor =>
                {
                    return new
                    {
                        Path = editor.Path,
                        Class = editor.Class,
                        Namespaces = editor.Namespaces.Values,
                        References = editor.References.Values,
                        Variables = editor.Variables.Select(v =>
                    {
                        return new
                        {
                            Name = v.Name,
                            Type = v.Type,
                            Description = v.Description,
                            DefaultValue = v.DefaultValue
                        };
                    }),
                        Arguments = editor.Arguments.Select(a =>
                    {
                        return new
                        {
                            Name = a.Name,
                            Type = a.Type,
                            Direction = a.Direction,
                            Description = a.Description,
                            DefaultValue = a.DefaultValue
                        };
                    }),
                        Activities = editor.Activities.Select(a =>
                    {
                        return new
                        {
                            Name = a.Name,
                            Description = a.Description,
                            Type = a.Type,
                        };
                    }),
                        Outline = editor.GetOutline("", "").Result,
                        Description = editor.Description,
                        Expressions = editor.Expressions.Select(e =>
                    {
                        return new
                        {
                            Path = e.Path,
                            Value = e.Value,
                            ActivityName = e.ActivityName,
                            ActivityType = e.ActivityType,
                            Type = e.Type
                        };
                    })
                    };
                }),
                ProjectConfigurations,
                SharedConfiguration,
                IsSingleProject,
                StudioVersion
            };
        }
    }
}
