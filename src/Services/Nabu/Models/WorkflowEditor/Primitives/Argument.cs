using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;

namespace LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives
{
    public class Argument
    {
        public string Class { get; private set; }
        public string Name
        {
            get
            {
                return XDocumentHelpers.GetAttribute(PropertyElement, LocalName.ArgumentDefinitionName).Value;
            }
        }
        public string? Description
        {
            get
            {
                var attr = XDocumentHelpers.GetAttribute(PropertyElement, LocalName.Description);
                return attr != null ? attr.Value : "";
            }
        }
        public string? DefaultValue
        {
            get
            {
                if (Argument_ExpressionElement == null) return null;
                if (Argument_ExpressionElement is XAttribute) return "\"" + ((XAttribute)Argument_ExpressionElement).Value + "\"";
                if (Argument_ExpressionElement is XElement)
                {
                    var element = (XElement)Argument_ExpressionElement;
                    switch (element.Name.LocalName)
                    {
                        case "CSharpValue":
                            return element.Value;
                        case "Literal":
                            return "\"" + XDocumentHelpers.GetAttribute(element, LocalName.LiteralValue).Value + "\"";
                        default:
                            throw new NotSupportedException("Unkonwn element type");
                    }
                }
                throw new InvalidOperationException("Unsupported Expression Type");
            }
            set
            {
                // No default value
                if (value == null)
                {
                    if (ArgumentObject is XAttribute) ((XAttribute)ArgumentObject).Remove();
                    if (ArgumentObject is XElement) ((XElement)ArgumentObject).Remove();
                }

                // Value is Empty String (ie. "\"\"")
                // Convert to Literal Child on Root
                // If it was an attribute or null before, we have to create the whole branch
                // Root -> this:Class + Name -> In/Out/InOutArgument -> Literal
                // Otherwise we just replace the CSharpValue with the Literal
                else if (value == "\"\"")
                {
                    if (Type != "x:String") return;
                    if (ArgumentObject == null || ArgumentObject is XAttribute)
                    {
                        var newArgumentElement = new XElement(
                            "{clr-namespace:}" + Class + "." + Name,
                            new XElement(
                                NamespaceNames.Empty + string.Format("{0}Argument", Direction),
                                new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                new XElement(
                                    NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    new XAttribute(LocalName.LiteralValue, "")
                                )
                                )
                            );

                        if (ArgumentObject is XAttribute) ((XAttribute)ArgumentObject).Remove();
                        PropertyElement.Document.Root.Elements().First().AddAfterSelf(newArgumentElement);
                    }
                    if (ArgumentObject is XElement)
                    {
                        var element = (XElement)Argument_ExpressionElement;

                        switch (element.Name.LocalName)
                        {
                            case "CSharpValue":
                                var newExpressionElement = new XElement(
                                     NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    new XAttribute(LocalName.LiteralValue, "")
                                );
                                Argument_DirectionWrapperElement.Add(newExpressionElement);
                                element.Remove();
                                break;
                            case "Literal":
                                var attribute = XDocumentHelpers.GetAttribute((XElement)Argument_ExpressionElement, LocalName.LiteralValue);
                                attribute.Value = "";
                                return;
                            default:
                                throw new NotSupportedException("Unkonwn element type");
                        }
                    }
                }

                // Value is Non-Empty String (ie. \""Hello World\"")
                // Convert to Attribute on Root
                // If Its an attribute, just set value
                // Otherwise we have to create the attribute
                // If it was an Empty String or CSharpValue before, we have to remove that element as well.
                else if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    if (ArgumentObject is XAttribute && value != null)
                    {
                        ((XAttribute)Argument_ExpressionElement).Value = value.Remove(value.Length - 1).Remove(0, 1);
                        return;
                    }
                    else
                    {
                        if (ArgumentObject != null)
                        {
                            ((XElement)ArgumentObject).Remove();
                        }
                        PropertyElement.Document.Root.Add(new XAttribute("{clr-namespace:}" + Class + "." + Name, value.Remove(value.Length - 1).Remove(0, 1)));
                    }

                }

                // Value is Code String (ie. "new DateTime().ToString()"
                // Convert to Child of Root
                // Same thing as Literals but change the local name and mapping of Value
                else
                {

                    if (ArgumentObject == null || ArgumentObject is XAttribute)
                    {
                        var newArgumentElement = new XElement(
                            "{clr-namespace:}" + Class + "." + Name,
                            new XElement(
                                NamespaceNames.Empty + string.Format("{0}Argument", Direction),
                                new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                new XElement(
                                    NamespaceNames.Empty + LocalName.CSharpValue,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    value
                                )
                                )
                            );



                        if (ArgumentObject is XAttribute) ((XAttribute)ArgumentObject).Remove();
                        PropertyElement.Document.Root.Elements().ElementAt(0).AddAfterSelf(newArgumentElement);
                        return;
                    }
                    if (ArgumentObject is XElement)
                    {
                        var element = (XElement)Argument_ExpressionElement;

                        switch (element.Name.LocalName)
                        {
                            case "CSharpValue":
                                element.Value = value;
                                return;
                            case "Literal":
                                var newExpressionElement = new XElement(
                                     NamespaceNames.Empty + LocalName.CSharpValue,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    value
                                );
                                element.Remove();
                                Argument_DirectionWrapperElement.Add(newExpressionElement);
                                return;

                            default:
                                throw new NotSupportedException("Unkonwn element type");
                        }
                    }

                }
            }
        }
        public string Direction
        {
            get
            {
                return Property_TypeAttribute.Value.Split('(').First().Replace("Argument", "");
            }
            set
            {
                var oldSplit = Property_TypeAttribute.Value.Split('(');
                oldSplit[0] = value + "Argument";
                var output = string.Join("(", oldSplit);
                Property_TypeAttribute.Value = output;
            }
        }
        public string Type
        {
            get
            {
                var split = Property_TypeAttribute.Value.Split('(').ToList();
                split.RemoveAt(0);
                split[split.Count - 1] = split[split.Count - 1].Remove(split[split.Count - 1].Length - 1);
                return string.Join("(", split);
            }
            set
            {
                var split = Property_TypeAttribute.Value.Split('(').ToList();
                Property_TypeAttribute.Value = split[0] + '(' + value + ")";

                if (Argument_DirectionWrapper_TypeAttribute != null)
                {
                    Argument_DirectionWrapper_TypeAttribute.Value = value;
                }
                if (Argument_Expression_TypeElement != null)
                {
                    Argument_Expression_TypeElement.Value = value;
                }
            }
        }

        private XElement PropertyElement { get; set; }
        private XObject? ArgumentObject
        {
            get
            {
                XObject valueElement = PropertyElement.Document.Descendants().FirstOrDefault(d => d.Name.LocalName == Class + "." + Name && d.Parent == PropertyElement.Document.Root);
                if (valueElement == null)
                {
                    valueElement = XDocumentHelpers.GetAttribute(PropertyElement.Document.Root, Class + "." + Name);
                }
                return valueElement;
            }
        }
        private XAttribute Property_TypeAttribute
        {
            get
            {
                return XDocumentHelpers.GetAttribute(PropertyElement, LocalName.ArgumentDefinitionType);
            }
        }
        private XElement? Argument_DirectionWrapperElement
        {
            get
            {
                if (ArgumentObject == null) return null;
                if (ArgumentObject is XAttribute)
                {
                    return null;
                }
                else
                {
                    return ((XElement)ArgumentObject).Elements().First();
                }
            }
        }
        private XAttribute? Argument_DirectionWrapper_TypeAttribute
        {
            get
            {

                if (ArgumentObject == null) return null;
                if (ArgumentObject is XAttribute)
                {
                    return null;
                }
                else
                {
                    return XDocumentHelpers.GetAttribute(Argument_DirectionWrapperElement, LocalName.ArgumentWrapperType);
                }
            }
        }

        private XObject? Argument_ExpressionElement
        {
            get
            {
                if (ArgumentObject == null) return null;
                if (ArgumentObject is XAttribute)
                {
                    return ArgumentObject;
                }
                else
                {
                    return ((XElement)ArgumentObject).Descendants().FirstOrDefault(ved => LocalName.Expressions.Contains(ved.Name.LocalName));
                }
            }
        }

        private XAttribute? Argument_Expression_TypeElement
        {
            get
            {
                if (Argument_ExpressionElement == null) return null;
                if (Argument_ExpressionElement is XAttribute) return null;

                return XDocumentHelpers.GetAttribute((XElement)Argument_ExpressionElement, LocalName.ArgumentValueType);
            }
        }


        public Argument(XElement argumentDefinition, string className)
        {
            PropertyElement = argumentDefinition;
            Class = className;
        }
    }

}
