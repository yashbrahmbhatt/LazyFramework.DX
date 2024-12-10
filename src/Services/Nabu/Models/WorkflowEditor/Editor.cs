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

namespace LazyFramework.DX.Services.Nabu.WorkflowEditor
{
    public class Editor
    {
        public string Path { get; set; }
        public XDocument Document { get; set; }
        public List<Expression> Expressions { get; set; }
        public Namespaces Namespaces { get; set; }
        public References References { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Argument> Arguments { get; set; }
        private Hermes.Hermes Hermes { get; }
        private async void Log(string message, LogLevel level = LogLevel.Debug)
        {
            Hermes.Log(message, "Nabu.Editor", level);
        }

        public string Class => XDocumentHelpers.GetAttribute(Document.Root, LocalName.Class)?.Value
                                ?? throw new InvalidOperationException("Invalid XAML");

        public string? Description => XDocumentHelpers.GetAttribute(Document.Root, LocalName.Description)?.Value;

        public List<Variable> Variables { get; set; }



        public Editor(string path, Hermes.Hermes hermes)
        {
            Hermes = hermes ?? throw new ArgumentNullException(nameof(hermes));
            Path = path;
            Document = XDocument.Load(path);
            Load();
        }

        public Editor(XDocument document, Hermes.Hermes hermes)
        {
            Hermes = hermes ?? throw new ArgumentNullException(nameof(hermes));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Load();
        }



        public async void Load()
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



    }

}
