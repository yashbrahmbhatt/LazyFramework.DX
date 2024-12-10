using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Hermes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ReflectionMagic;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Analyzer.Rules;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;
using LazyFramework.DX.Services.Nabu.Models.AutoDoc;
using System.IO;

namespace LazyFramework.DX.Services.Nabu
{
    public class Nabu
    {
        public Dictionary<string, Editor> Workflows = new Dictionary<string, Editor>();
        private IWorkflowDesignApi _api;
        private string _projectPath;
        private static Hermes.Hermes _hermes;
        private static async void Log(string message, LogLevel level = LogLevel.Info) => _hermes.Log(message, "Nabu", level);
        private static Action<string, LogLevel?> LogAction = (message, level) => Log(message, level ?? LogLevel.Debug);
        public Nabu(IServiceProvider provider)
        {

            _hermes = provider.GetService<Hermes.Hermes>() ?? throw new Exception("Hermes is not initialized");
            _api = provider.GetService<IWorkflowDesignApi>() ?? throw new Exception("IWorkflowDesign is not initalized");
            _projectPath = _api.ProjectPropertiesService.GetProjectDirectory();

            //LoadWorkflows();
            WriteDefaultTemplates();
            AutoDoc.DocumentProject(new Settings()
            {
                IgnoredDirectories = new string[2] { "Design", ".templates" },
                OutputRoot = "Documentation",
                ProjectRoot = _projectPath,
                TemplatesRoot = DX.Helpers.PathExtensions.Combine(_projectPath, ".autodoc")
            }, _hermes);
            Log("Initialized Nabu.");
        }

        public void WriteDefaultTemplates()
        {
            var templatesRoot = DX.Helpers.PathExtensions.Combine(_projectPath, ".autodoc");
            if (!Directory.Exists(templatesRoot)) Directory.CreateDirectory(templatesRoot);
            var projectMD = @"# {Name}
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

</details>";
            var workflowMD = @"# {WorkflowName}
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

{Outline}";
            if(!File.Exists(DX.Helpers.PathExtensions.Combine(templatesRoot, "project.md"))) File.WriteAllText(DX.Helpers.PathExtensions.Combine(templatesRoot, "project.md"), projectMD);
            if (!File.Exists(DX.Helpers.PathExtensions.Combine(templatesRoot, "workflow.md"))) File.WriteAllText(DX.Helpers.PathExtensions.Combine(templatesRoot, "workflow.md"), workflowMD);
        }

        public async void LoadWorkflows()
        {
            Log($"Loading workflows...");
            var workflows = await _api.WorkflowOperationsService.GetWorkflowsFilePathsAsync();
            
            foreach (var workflow in workflows)
            {
                if (workflow.EndsWith(".cs")) continue;
                var path = DX.Helpers.PathExtensions.Combine(_projectPath, workflow);
                try
                {
                    var editor = new Editor(path, _hermes);
                    Workflows.Add(path, editor);
                    Log($"Loaded workflow '{workflow}' with {editor.Activities.Count} activities");
                }
                catch (Exception ex)
                {
                    Log($"Failed to load workflow '{path}'. {ex.Message}", LogLevel.Error);
                }


            }
            Log($"Workflows loaded. Found {Workflows.Count} workflows.");
            
        }
    }
}
