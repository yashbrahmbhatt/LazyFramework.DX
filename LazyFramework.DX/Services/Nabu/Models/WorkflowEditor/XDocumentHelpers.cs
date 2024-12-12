using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LazyFramework.DX.Services.Nabu.WorkflowEditor
{
    public static class XDocumentHelpers
    {
        public static XElement? GetClosestParentWithAttribute(XElement? node, string attribute)
        {
            while (node?.Parent != null)
            {
                node = node.Parent;
                if (node.Attributes().Any(a => a.Name.LocalName == attribute))
                {
                    return node;
                }
            }
            return null;
        }



        public static XAttribute? GetAttribute(XElement? element, string attribute)
        {
#if (NET6_0_OR_GREATER)
            return element?.Attributes().FirstOrDefault(a => a.Name.LocalName == attribute, null);
#else
            return element?.Attributes().FirstOrDefault(a => a.Name.LocalName == attribute);
#endif

        }

        public static int GetIndexPosition(XObject? xObject)
        {
            if (xObject?.Parent == null) return -1;

            if (xObject is XElement element)
            {
                return element.Parent?.Elements(element.Name).TakeWhile(e => e != element).Count() + 1 ?? -1;
            }
            else if (xObject is XAttribute attribute)
            {
                return attribute.Parent?.Attributes().TakeWhile(a => a != attribute).Count() + 1 ?? -1;
            }

            throw new InvalidOperationException("XObject is not an XElement or XAttribute.");
        }


        public static string? GetRelativeXPath(XObject? xObject)
        {
            if (xObject == null) return null;
            int index = GetIndexPosition(xObject);
            string name = xObject switch
            {
                XElement element => element.Name.LocalName,
                XAttribute attribute => attribute.Name.LocalName,
                _ => throw new InvalidOperationException("XObject is not an XElement or XAttribute.")
            };

            return index == -1 ? $"/{name}" : $"{name}[{index}]";
        }


        public static string GetAbsoluteXPath(XObject? xObject)
        {
            if (xObject == null) return "";

            IEnumerable<string?> ancestors = xObject switch
            {
                XElement element => element.Ancestors().Select(GetRelativeXPath),
                XAttribute attribute => attribute.Parent?.Ancestors().Select(GetRelativeXPath) ?? Enumerable.Empty<string>(),
                _ => throw new InvalidOperationException("XObject is not an XElement or XAttribute.")
            };



            return $"/{string.Join("/", ancestors.Reverse())}{GetRelativeXPath(xObject)}";
        }


    }
}
