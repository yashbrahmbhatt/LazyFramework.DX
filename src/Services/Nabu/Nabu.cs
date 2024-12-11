using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Hermes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Analyzer.Rules;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;
using System.IO;
using System.Data;
using LazyFramework.DX.Services.Odin;
using LazyFramework.DX.Models;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Heimdall;
using LazyFramework.DX.Services.Nabu.Models;

namespace LazyFramework.DX.Services.Nabu
{
    public class Nabu : OdinConsumer
    {
        public Dictionary<string, Editor> Workflows = new Dictionary<string, Editor>();
        private NabuSettings _settings;
        private Project _project;
        private string _projectRoot => _api.ProjectPropertiesService.GetProjectDirectory();
        public Nabu(IWorkflowDesignApi api, Hermes.Hermes hermes, Odin.Odin odin) : base(api, hermes, odin, "Nabu")
        {
            Log("Initializing Nabu...");
            var projectPath = _api.ProjectPropertiesService.GetProjectDirectory();
            _odin.Register<JsonFileEvent>(async (e) => await HandleJsonFileEvent(e));
            _odin.Register<XamlFileEvent>((e) => HandleXAMLFileEvent(e));
            _settings = new NabuSettings(_api, hermes);
            LoadProject();
            DocumentProject();
            Log("Initialized Nabu.");
        }

        public async Task HandleJsonFileEvent(JsonFileEvent e)
        {
            var path = e.Path;
            var type = e.EventType;
            
            if(path.ToLower() == Path.Combine(_projectRoot, "project.json").ToLower())
            {
                Log($"Project json file event: {type}");
                if(type == WatcherChangeTypes.Created || type == WatcherChangeTypes.Changed)
                {
                    await LoadProject();
                    await DocumentProject();
                }
                Log($"Handled JSON file event.");
            } else
            {
                Log($"Not a project.json file event: {path}");
            }
           
        }

        public List<string> GetWorkflowsUsing(string workflowPath)
        {
            if (string.IsNullOrEmpty(workflowPath))
                throw new ArgumentException("Workflow path cannot be null or empty.", nameof(workflowPath));

            return Workflows
                .Where(kvp => kvp.Value.WorkflowsUsed != null && kvp.Value.WorkflowsUsed.Contains(workflowPath))
                .Select(kvp => kvp.Key)
                .ToList();
        }

        public async Task HandleXAMLFileEvent(XamlFileEvent e)
        {
            var path = e.Path;
            var type = e.EventType;
            Log($"XAML file event: {type}");
            if (_settings.IgnoredDirectories.Contains(path))
            {
                Log($"File is in ignored directory: {path}");
                return;
            }
            var newEditor = new Editor(path, _hermes, _api);
            switch (type)
            {
                case WatcherChangeTypes.Created:
                    Workflows.Add(path, newEditor);
                    await DocumentWorkflow(path);
                    break;
                case WatcherChangeTypes.Changed:
                    Workflows[path] = newEditor;
                    await DocumentWorkflow(path);
                    break;
                case WatcherChangeTypes.Deleted:
                    Workflows.Remove(path);
                    DeleteWorkflowDocumentation(path);
                    newEditor = null;
                    break;
                case WatcherChangeTypes.Renamed:
                    if (e.OldPath != null) Workflows.Remove(e.OldPath);
                    DeleteWorkflowDocumentation(e.OldPath);
                    Workflows.Add(path, newEditor);
                    await DocumentWorkflow(path);
                    break;
            }
            await _odin.Notify<WorkflowChangedEvent>(new WorkflowChangedEvent(path,type,newEditor));
            Log($"Handled XAML file event.");
        }

        public async Task LoadProject()
        {
            Log("Loading project...");
            _project = new Project(Path.Combine(_projectRoot, "project.json"));
            Log("Project loaded.");
        }

        public async Task DocumentProject()
        {
            Log($"Documenting project '{_project.Name}'");

            // Log.LogThings(new object[1] { workflowsUsedInTests });
            string projectMD = _settings.ProjectTemplateSetting
                .Replace("{Name}", _project.Name)
                .Replace("{Description}", _project.Description)
                .Replace("{Type}", _project.Type)
                .Replace("{Version}", _project.ProjectVersion)
                .Replace("{StudioVersion}", _project.StudioVersion)
                .Replace("{Dependencies}", Workflows.Values.First().GenerateMarkdownTable(_project.Dependencies))
                .Replace("{EntryPoints}", Workflows.Values.First().GenerateMarkdownTable(_project.EntryPoints))
                .Replace("{Language}", _project.Language);

            if (Directory.Exists(_settings.OutputRoot)) Directory.Delete(_settings.OutputRoot, true);
            Directory.CreateDirectory(_settings.OutputRoot);
#if(NET6_0_OR_GREATER)
            await File.WriteAllTextAsync(Path.Combine(_settings.OutputRoot, _project.Name + ".md"), projectMD);
#else
            File.WriteAllText(Path.Combine(_settings.OutputRoot, _project.Name + ".md"), projectMD);
#endif
            Log("Project md written");
        }

        public void DeleteWorkflowDocumentation(string path)
        {
            var outputFilePath = path.Replace(_projectRoot, _settings.OutputRoot).Replace(".xaml", ".md");
            File.Delete(outputFilePath);
        }

        public async Task DocumentWorkflow(string path)
        {
            if (!File.Exists(path))
            {
                Log($"File not found: {path}", LogLevel.Error);
                return;
            }
            if(!Workflows.ContainsKey(path))
            {
                Log($"Workflow editor not found: {path}", LogLevel.Warning);
                Workflows[path] = new Editor(path, _hermes, _api);
            }
            var editor = Workflows[path];
            Log($"Documenting workflow '{editor.Class}'");
            IEnumerable<string?> testWorkflows = _project.FileInfoCollection.Rows.Cast<DataRow>().Where(r => r["editingStatus"].ToString() == "Publishable" || r["editingStatus"].ToString() == "InProgress").Select(r => r["fileName"].ToString());
            var tests = testWorkflows.Where(twf => twf != null && GetWorkflowsUsing(path).Contains(twf));
            var testTable = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(
                    tests.Aggregate(
                        new List<Dictionary<string, string>>(),
                        (acc, next) => acc.Concat(
                            new List<Dictionary<string, string>>(){
                                    new Dictionary<string, string>(){
                                        {"Path", next ?? ""},
                                        {"Description", Workflows.Values.First(e => next != null && e.Path.Contains(next))?.Description ?? ""}
                                    }
                            }).ToList())));
            var outputFilePath = editor.Path.Replace(_projectRoot, _settings.OutputRoot).Replace(".xaml", ".md");
            var outputFolderPath = new FileInfo(outputFilePath).Directory?.FullName;
            if(outputFolderPath == null)
            {
                Log($"Failed to get output folder path for {editor.Class}", LogLevel.Error);
                return;
            }
            var arguments = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(editor.Arguments));
            var workflowMD = _settings.WorkflowTemplateSetting
                .Replace("{Class}", editor.Class)
                .Replace("{Description}", editor.Description)
                .Replace("{Namespaces}", editor.GenerateMarkdownTable(editor.Namespaces.Values, "Namespace"))
                .Replace("{References}", editor.GenerateMarkdownTable(editor.References.Values, "Reference"))
                .Replace("{Arguments}", arguments != null ? editor.GenerateMarkdownTable(arguments) : "No arguments were found in this workflow.")
                .Replace("{Outline}", editor.Outline)
                .Replace("{WorkflowName}", editor.Class)
                .Replace("{WorkflowsUsed}", editor.GenerateMarkdownTable(editor.WorkflowsUsed, "Path"))
                .Replace("{Tests}", editor.GenerateMarkdownTable(testTable));
            if (!Directory.Exists(outputFolderPath)) Directory.CreateDirectory(outputFolderPath);

            File.WriteAllText(outputFilePath, workflowMD);
            Log($"Workflow '{editor.Class}' documented.");


        }

        public void OnDisposed()
        {
            
        }
    }
}
