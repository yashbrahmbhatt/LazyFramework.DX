using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LazyFramework.DX.Services.Nabu.Models.AutoDoc
{
    public static class AutoDoc
    {

        public static void DocumentProject(Settings settings, Hermes.Hermes hermes)
        {
            var editors = Directory.GetFiles(settings.ProjectRoot, "*.xaml", SearchOption.AllDirectories)
                .Where(f =>
                    settings.IgnoredDirectories.Length > 0 ?
                    !settings.IgnoredDirectories.Any(
                        d => Path.GetRelativePath(Directory.GetCurrentDirectory(), f).ToLower().StartsWith(d.ToLower())
                    ) :
                    true
                ).Select(f => new AutoDocEditor(f, hermes));

            Project projectJson = new Project(Path.Combine(settings.ProjectRoot, "project.json"));
            string workflowTemplatePath = Path.Combine(settings.TemplatesRoot, "Workflow.md");
            string projectTemplatePath = Path.Combine(settings.TemplatesRoot, "Project.md");
            string folderTemplatePath = Path.Combine(settings.TemplatesRoot, "Folder.md");
            string testTemplatePath = Path.Combine(settings.TemplatesRoot, "Test.md");
            Console.WriteLine("Found " + editors.Count().ToString() + " workflows to document");

            EnumerableRowCollection<DataRow> testWorkflows = projectJson.FileInfoCollection.AsEnumerable().Where(r => r["editingStatus"].ToString() == "Publishable" || r["editingStatus"] == "InProgress");

            Dictionary<string, IEnumerable<string>> workflowsUsedInTests = new Dictionary<string, IEnumerable<string>>();
            Console.WriteLine("Creating test dictionary");
            foreach (var test in testWorkflows)
            {
                var path = test["fileName"].ToString();
                var workflows = new AutoDocEditor(path, hermes);
                workflowsUsedInTests[path] = workflows.WorkflowsUsed;
            }
            // Log.LogThings(new object[1] { workflowsUsedInTests });
            string projectMD = File.ReadAllText(projectTemplatePath)
                .Replace("{Name}", projectJson.Name)
                .Replace("{Description}", projectJson.Description)
                .Replace("{Type}", projectJson.Type)
                .Replace("{Version}", projectJson.ProjectVersion)
                .Replace("{StudioVersion}", projectJson.StudioVersion)
                .Replace("{Dependencies}", editors.First().GenerateMarkdownTable(projectJson.Dependencies))
                .Replace("{EntryPoints}", editors.First().GenerateMarkdownTable(projectJson.EntryPoints))
                .Replace("{Language}", projectJson.Language);

            if (Directory.Exists(settings.OutputRoot)) Directory.Delete(settings.OutputRoot, true);
            Directory.CreateDirectory(settings.OutputRoot);

            File.WriteAllText(Path.Combine(settings.OutputRoot, projectJson.Name + ".md"), projectMD);
            Console.WriteLine("Project md written");
            foreach (var editor in editors)
            {
                Console.WriteLine(string.Format("Documenting workflow '{0}'", editor.Editor.Path));
                var outputFilePath = editor.Editor.Path.Replace(settings.ProjectRoot, settings.OutputRoot).Replace(".xaml", ".md");
                var outputFolderPath = new FileInfo(outputFilePath).Directory.FullName;
                var tests = workflowsUsedInTests.Keys.Where(k => workflowsUsedInTests[k].Contains(Path.GetRelativePath(settings.ProjectRoot, editor.Editor.Path))).Select(x => Path.GetRelativePath(settings.ProjectRoot, x));
                var workflowMD = File.ReadAllText(workflowTemplatePath)
                    .Replace("{Class}", editor.Editor.Class)
                    .Replace("{Description}", editor.Editor.Description)
                    .Replace("{Namespaces}", editor.GenerateMarkdownTable(editor.Editor.Namespaces.Values, "Namespace"))
                    .Replace("{References}", editor.GenerateMarkdownTable(editor.Editor.References.Values, "Reference"))
                    .Replace("{Arguments}", editor.GenerateMarkdownTable(JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(editor.Editor.Arguments))))
                    .Replace("{Outline}", editor.Outline)
                    .Replace("{WorkflowName}", editor.Editor.Class)
                    .Replace("{WorkflowsUsed}", editor.GenerateMarkdownTable(editor.WorkflowsUsed, "Path"))
                    .Replace("{Tests}", editor.GenerateMarkdownTable(JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(
                        tests.Aggregate(
                            new List<Dictionary<string, string>>(),
                            (acc, next) => acc.Concat(
                                new List<Dictionary<string, string>>(){
                                    new Dictionary<string, string>(){
                                        {"Path", next},
                                        {"Description", editors.First(e => e.Editor.Path.Contains(next)).Editor.Description}
                                    }
                                }).ToList())))));
                if (!Directory.Exists(outputFolderPath)) Directory.CreateDirectory(outputFolderPath);

                File.WriteAllText(outputFilePath, workflowMD);
            }

        }
    }
}
