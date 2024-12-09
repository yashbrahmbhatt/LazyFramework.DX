using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Models;
using LazyFramework.DX.Services.Hermes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ReflectionMagic;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Analyzer.Rules;

namespace LazyFramework.DX.Services.Nabu
{
    public class Nabu
    {
        public Dictionary<string, WorkflowInspector> Workflows;
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

            LoadWorkflows();

            Log("Initialized Nabu.");
        }



        public async void LoadWorkflows()
        {
            Log($"Loading workflows...");
            var workflows = await _api.WorkflowOperationsService.GetWorkflowsFilePathsAsync();
            foreach(var workflow in workflows)
            {
                //var path = Models.Helpers.PathExtensions.GetRelativePath(ProjectPath, workflow);
                try
                {
                    var wi = new WorkflowInspector(workflow, LogAction);
                    Workflows.Add(workflow, wi);
                }
                catch (Exception ex)
                {
                    Log($"Failed to load workflow '{workflow}'. {ex.Message}", LogLevel.Error);
                }

            }
            Log($"Workflows loaded. Found {Workflows.Count} workflows.");
            
        }
    }
}
