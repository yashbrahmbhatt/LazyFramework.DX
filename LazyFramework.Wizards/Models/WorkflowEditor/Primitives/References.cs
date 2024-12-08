using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LazyFramework.Models.WorkflowEditor;

namespace LazyFramework.Models.WorkflowEditor.Primitives
{
    public class References
    {
        private List<XElement> ReferencesElementList
        {
            get
            {
                return Document.Root.Descendants().First(d => d.Name.LocalName == LocalName.References).Descendants().Where(d => d.Name.LocalName == "AssemblyReference").ToList();

            }
        }
        public List<string> Values
        {
            get
            {
                return ReferencesElementList.Select(r => r.Value).ToList();
            }
            set
            {
                if (value != null)
                {
                    var ReferenceParent = Document.Root.Elements().First();
                    ReferenceParent.RemoveNodes();
                    foreach (var namespaceVal in value)
                    {
                        var newElement = new XElement(
                            NamespaceNames.Empty + "AssemblyReference",
                            namespaceVal
                        );
                        ReferenceParent.Add(newElement);
                    }
                }
            }
        }

        private XDocument Document { get; set; }
        public References(XDocument element)
        {
            Document = element;
        }
    }
}
