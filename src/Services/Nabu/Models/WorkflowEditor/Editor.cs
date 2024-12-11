using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using References = LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives.References;
using Activity = LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives.Activity;
using Variable = LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives.Variable;
using Argument = LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives.Argument;
using Expression = LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives.Expression;
using LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives;
using System.Windows;
using LazyFramework.DX.Services.Hermes;
using System.Data;
using LazyFramework.DX.Models.Consumers;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Services.Nabu.WorkflowEditor 
{
    public class Editor : HermesConsumer
    {
        public string Path { get; set; }
        public XDocument Document { get; set; }
        public List<Expression> Expressions { get; set; }
        public Namespaces Namespaces { get; set; }
        public References References { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Argument> Arguments { get; set; }
        private Hermes.Hermes Hermes { get; }
        public string Outline;
        public IEnumerable<string> WorkflowsUsed
        {
            get
            {
                return Document.Descendants().Where(d => d.Name.LocalName == "InvokeWorkflowFile").Select(d => d.Attributes().First(a => a.Name.LocalName == "WorkflowFileName").Value.Replace("\\\\", "\\")).Distinct();
            }
        }


        public string Class => XDocumentHelpers.GetAttribute(Document.Root, LocalName.Class)?.Value
                                ?? throw new InvalidOperationException("Invalid XAML");

        public string? Description => XDocumentHelpers.GetAttribute(Document.Root, LocalName.Description)?.Value;

        public List<Variable> Variables { get; set; }



        public Editor(string path, Hermes.Hermes hermes, IWorkflowDesignApi api) : base(api, hermes, "Nabu.Editor")
        {
            Hermes = hermes ?? throw new ArgumentNullException(nameof(hermes));
            Path = path;
            Document = XDocument.Load(path);
            Load() ;
        }

        public async Task<string> GetOutline(string prefix, string suffix)
        {
            if (Activities.Count == 0)
            {
                Log($"No activities found in workflow '{Path}'", LogLevel.Warning);
                return "";
            }
            Log($"Generating outline for workflow '{Path}'", LogLevel.Debug);
            await Task.Run(() =>
                Outline = TraverseWorkflow(Activities.First(), prefix, "").markdown + suffix);
            Log($"Outline generated for workflow '{Path}'", LogLevel.Debug);
            return Outline;
        }



        public async Task Load()
        {
            Log($"Loading workflow '{Path}'");
            Namespaces = GetAllNamespaces();
            References = GetAllReferences();
            Expressions = GetAllExpressions();
            Variables = GetAllVariables();
            Activities = GetAllActivities();
            Arguments = GetAllArguments();
            Log($"Workflow loaded with {Activities.Count} activities, {Variables.Count} variables, {Arguments.Count} arguments, and {Expressions.Count} expressions.");
        }

        public void Save(string path)
        {
            Document.Save(path);
        }


        public Namespaces GetAllNamespaces()
        {
            return new Namespaces(Document);
        }
        public References GetAllReferences() { return new References(Document); }
        public List<Expression> GetAllExpressions()
        {
            return Document.Descendants()
                           .Where(d => LocalName.Expressions.Contains(d.Name.LocalName)
                                       && XDocumentHelpers.GetClosestParentWithAttribute(d, LocalName.DisplayName) != null
                                       && d.Parent.Name.LocalName != "Variable.Default")
                           .Select(d => new Expression(d))
                           .ToList();
        }

        public List<Variable> GetAllVariables()
        {
            return Document.Descendants()
                           .Where(d => d.Name.LocalName == LocalName.Variable)
                           .Select(d => new Variable(d))
                           .ToList();
        }

        public List<Activity> GetAllActivities()
        {
            return Document.Descendants()
                           .Where(d => d.Attributes().Any(a => a.Name.LocalName == LocalName.DisplayName))
                           .Select(d => new Activity(d))
                           .ToList();
        }

        public List<Argument> GetAllArguments()
        {
            return Document.Descendants()
                           .Where(d => d.Name.LocalName == LocalName.ArgumentDefinition)
                           .Select(d => new Argument(d, Class))
                           .ToList();
        }

        public Argument GetArgument(string name)
        {
            return Arguments.First(a => a.Name == name);
        }

        public Variable GetVariable(string name)
        {
            return Variables.First(v => v.Name == name);
        }

        public Expression GetExpression(string path)
        {
            return Expressions.First(e => e.Path == path);
        }


        public (string markdown, string prev) TraverseWorkflow(Activity activity, string markdown, string prev)
        {
            var activityChildren = GetClosestChildrenActivities(activity);

            //Console.WriteLine(string.Format("Traversing node of type {0} with a display name of {1}", activity.Type, activity.Name.Replace(":", "")));
            // Log.LogThings(new object[] { activityChildren });
            // No Children
            if (activityChildren.Count() == 0)
            {
                markdown += Environment.NewLine + string.Format("{2}: {0} - {1}", activity.Type, activity.Name.Replace(":", ""), activity.Id) +
                    (prev == "" ? "" : Environment.NewLine + string.Format("{0} --> {1}", prev, activity.Id));
                prev = activity.Id;
            }
            // Yes Children
            else
            {
                switch (activity.Type)
                {
                    case "TryCatch":
                        markdown += Environment.NewLine + (string.IsNullOrEmpty(prev) ? "" : string.Format("{0} --> {1}", prev, activity.Id)) +
                            Environment.NewLine + string.Format("{0}: {1} - {2}", activity.Id, activity.Type, activity.Name.Replace(":", "")) +
                            Environment.NewLine + string.Format("state {0} ", activity.Id) + "{" + Environment.NewLine + "direction TB" +
                            Environment.NewLine + string.Format("{0}_Try: Try", activity.Id) + Environment.NewLine +
                            Environment.NewLine + string.Format("state {0}_Try", activity.Id) + "{" + Environment.NewLine;

                        var tryAct = activityChildren.First();
                        var catchActs = activityChildren.Where(a => a != tryAct);
                        prev = "";
                        var res = TraverseWorkflow(tryAct, "", prev);
                        markdown += res.markdown + "}" + Environment.NewLine;

                        foreach (var descendant in catchActs)
                        {
                            prev = "";
                            res = TraverseWorkflow(descendant, markdown, prev);
                            markdown = res.markdown;

                        }
                        markdown += "}" + Environment.NewLine;
                        prev = activity.Id;
                        break;
                    case "If":
                        markdown += Environment.NewLine + (string.IsNullOrEmpty(prev) ? "" : string.Format("{0} --> {1}", prev, activity.Id)) +
                            Environment.NewLine + string.Format("{0}: {1} - {2}", activity.Id, activity.Type, activity.Name.Replace(":", "")) +
                            Environment.NewLine + string.Format("state {0} <<choice>>", activity.Id) + Environment.NewLine;

                        var then = activityChildren.First();
                        var el = activityChildren.Last();
                        prev = "";
                        res = TraverseWorkflow(then, markdown, prev);
                        markdown = res.markdown;
                        markdown += string.Format("{0} --> {1}: Then", activity.Id, res.prev) + Environment.NewLine;
                        prev = "";
                        res = TraverseWorkflow(el, markdown, prev);
                        markdown += string.Format("{0} --> {1}: Else", activity.Id, res.prev) + Environment.NewLine;
                        prev = activity.Id;
                        break;
                    default:
                        markdown += Environment.NewLine + (string.IsNullOrEmpty(prev) ? "" : string.Format("{0} --> {1}", prev, activity.Id)) +
                            Environment.NewLine + string.Format("{0}: {1} - {2}", activity.Id, activity.Type, activity.Name.Replace(":", "")) +
                            Environment.NewLine + string.Format("state {0} ", activity.Id) + "{" + Environment.NewLine + "direction TB";
                        prev = "";
                        foreach (var descendant in activityChildren)
                        {
                            res = TraverseWorkflow(descendant, markdown, prev);
                            markdown = res.markdown;
                            prev = res.prev;
                        }
                        markdown += Environment.NewLine + "}";
                        prev = activity.Id;
                        break;
                }
            }


            return (markdown, prev);
        }

        public int GetDistance(Activity activity1, Activity activity2)
        {
            int count = 0;
            if (activity1.ActivityElement == activity2.ActivityElement) return 0;
            if (activity1.ActivityElement.Descendants().Contains(activity2.ActivityElement))
            {
                // activity1 --> activity2
                var current = activity2.ActivityElement;
                while (current != activity1.ActivityElement && current != null)
                {
                    count++;
                    current = current.Parent;
                }
                if (current == null) count = int.MaxValue;
            }
            else
            {
                // activity2 --> activity1
                var current = activity1.ActivityElement;
                while (current != activity2.ActivityElement && current != null)
                {
                    count--;
                    current = current.Parent;
                }
                if (current == null) count = int.MaxValue;
            }

            return count;
        }

        public IEnumerable<Activity> GetClosestChildrenActivities(Activity element)
        {
            int closest = int.MinValue;
            Dictionary<string, int> map = new Dictionary<string, int>();
            foreach (Activity activity in Activities)
            {
                int distance = GetDistance(activity, element);
                map[activity.Id] = distance;
                if (distance > closest && distance < 0) closest = distance;
            }
            // Log.LogThings(new object[] { map });
            return Activities.Where(d => GetDistance(d, element) == closest);
        }

        public string GenerateMarkdownLink(string label, string reference)
        {
            return string.Format("[{0}]({1})", label, reference);
        }

        public string GenerateMarkdownTable(DataTable table)
        {
            string output = "| ";
            foreach (DataColumn col in table.Columns)
            {
                output += col.ColumnName + " | ";
            }
            output = output.Trim() + "\n| " + string.Join("|", Enumerable.Range(0, table.Columns.Count).Select(x => " --- ")) + " |\n";
            foreach (DataRow row in table.Rows)
            {
                output += "| ";
                foreach (DataColumn col in table.Columns)
                {
                    output += row[col].ToString() + " | ";
                }
                output = output.Trim() + "\n";
            }
            return output;
        }

        public string GenerateMarkdownTable(IEnumerable<object> list, string listName)
        {
            string output = "| " + listName + " |" + Environment.NewLine +
                "| --- |" + Environment.NewLine;

            foreach (object item in list)
            {
                output += "| " + item.ToString() + " |" + Environment.NewLine;

            }
            return output;
        }
    }

}
