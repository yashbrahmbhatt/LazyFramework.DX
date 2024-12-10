using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;
using LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives;

namespace LazyFramework.DX.Services.Nabu.Models.AutoDoc
{
    public class AutoDocEditor
    {
        public XDocument Document;
        public string Outline;
        public IEnumerable<string> WorkflowsUsed
        {
            get
            {
                return Editor.Document.Descendants().Where(d => d.Name.LocalName == "InvokeWorkflowFile").Select(d => d.Attributes().First(a => a.Name.LocalName == "WorkflowFileName").Value.Replace("\\\\", "\\")).Distinct();
            }
        }
        public Editor Editor;

        public AutoDocEditor(string filePath, Hermes.Hermes hermes)
        {
            Editor = new Editor(filePath, hermes);
            if (Editor.Activities.Count == 0) return;
            Outline = @":::mermaid
stateDiagram-v2
";
            string prev = "";
            var res = TraverseWorkflow(Editor.Activities.First(), Outline, prev);
            Outline = res.markdown + "\n:::";

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
            foreach (Activity activity in Editor.Activities)
            {
                int distance = GetDistance(activity, element);
                map[activity.Id] = distance;
                if (distance > closest && distance < 0) closest = distance;
            }
            // Log.LogThings(new object[] { map });
            return Editor.Activities.Where(d => GetDistance(d, element) == closest);
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
