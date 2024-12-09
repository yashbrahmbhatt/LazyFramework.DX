using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using References = LazyFramework.Models.WorkflowEditor.Primitives.References;
using Activity = LazyFramework.Models.WorkflowEditor.Primitives.Activity;
using Variable = LazyFramework.Models.WorkflowEditor.Primitives.Variable;
using LazyFramework.Models.WorkflowEditor;
using LazyFramework.Models.WorkflowEditor.Primitives;
using Argument = LazyFramework.Models.WorkflowEditor.Primitives.Argument;

namespace LazyFramework.WorkflowEditor 
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
        public string Class
        {
            get
            {
                var classElement = XDocumentHelpers.GetAttribute(Document.Root, LocalName.Class);
                if (classElement == null) throw new InvalidOperationException("Invalid XAML");
                return classElement.Value;
            }
        }
        public string? Description
        {
            get
            {
                var descriptionElement = XDocumentHelpers.GetAttribute(Document.Root, LocalName.Description);
                if (descriptionElement == null) return null;
                return descriptionElement.Value;
            }
        }
        public List<Variable> Variables { get; set; }



        public Editor(string path)
        {
            Path = path;
            Document = XDocument.Load(path);
            Load();
        }
        public Editor(XDocument document)
        {
            Document = new XDocument(document);
            Load();
        }



        public void Load()
        {
            Namespaces = GetAllNamespaces();
            References = GetAllReferences();
            Expressions = GetAllExpressions();
            Variables = GetAllVariables();
            Activities = GetAllActivities();
            Arguments = GetAllArguments();
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
            var Expressions = Document.Descendants().Where(d => LocalName.Expressions.Contains(d.Name.LocalName) && XDocumentHelpers.GetClosestParentWithAttribute(d, LocalName.DisplayName) != null && d.Parent.Name.LocalName != "Variable.Default")
            .Aggregate(new List<Expression>(), (acc, e) =>
            {
                acc.Add(new Expression(e));
                return acc;
            });

            return Expressions;
        }
        public List<Variable> GetAllVariables()
        {
            var Variables = Document.Descendants().Where(d => d.Name.LocalName == LocalName.Variable)
            .Aggregate(new List<Variable>(), (acc, v) =>
            {
                acc.Add(new Variable(v));
                return acc;
            });

            return Variables;
        }
        public List<Activity> GetAllActivities()
        {
            var Activities = Document.Descendants()
            .Where(d => d.Attributes().Any(a => a.Name.LocalName == LocalName.DisplayName))
            .Aggregate(new List<Activity>(), (acc, d) =>
            {
                acc.Add(new Activity(d));
                return acc;
            });

            return Activities;
        }
        public List<Argument> GetAllArguments()
        {
            var Arguments = Document.Descendants().Where(d => d.Name.LocalName == LocalName.ArgumentDefinition)
            .Aggregate(new List<Argument>(), (acc, ad) =>
            {

                acc.Add(new Argument(ad, Class));
                return acc;
            });

            return Arguments;
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
