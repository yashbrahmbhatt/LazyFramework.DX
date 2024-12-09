using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LazyFramework.DX.Models.WorkflowEditor.Primitives
{
    public class Namespace
    {
        public XElement _element;
        public string Value
        {
            get
            {
                return _element.Value;
            }
            set
            {
                _element.Value = value;
            }
        }

        public Namespace(XElement element)
        {

        }
    }
}
