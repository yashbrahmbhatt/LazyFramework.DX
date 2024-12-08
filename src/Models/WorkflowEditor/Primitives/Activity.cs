using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.Models.WorkflowEditor;

namespace LazyFramework.Models.WorkflowEditor.Primitives
{
    public class Activity
    {
        private XAttribute? IdElement
        {
            get
            {
                return ActivityElement.Attributes().Where(a => a.Name.LocalName == "WorkflowViewState.IdRef").First();
            }
        }
        public string Id
        {
            get
            {
                return IdElement.Value.ToString().Replace("`", "");
            }
            set
            {
                IdElement.Value = value;
            }
        }
        public string Name
        {
            get
            {
                return NameElement.Value;
            }
            set
            {
                NameElement.Value = value;
            }
        }
        public string? Description
        {
            get
            {
                return DescriptionElement != null ? DescriptionElement.Value : "";
            }
            set
            {
                DescriptionElement.Value = value;
            }
        }
        public XElement ActivityElement { get; set; }
        public string Type
        {
            get
            {
                return ActivityElement.Name.LocalName;
            }
        }


        private XAttribute DescriptionElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ActivityElement, LocalName.Description);
            }
        }
        private XAttribute NameElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ActivityElement, LocalName.DisplayName);
            }
        }
        public Activity(XElement activity)
        {
            ActivityElement = activity;
        }
    }
}
