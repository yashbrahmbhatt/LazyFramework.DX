using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;

namespace LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives
{
    public class Variable
    {
        public string Name
        {
            get
            {
                return Variable_NameElement.Value;
            }
            set
            {
                Variable_NameElement.Value = value;
            }
        }
        public string Type
        {
            get
            {
                return Variable_TypeElement.Value;
            }
            set
            {
                Variable_TypeElement.Value = value;
                if (Variable_Default_Expression_Type != null) Variable_Default_Expression_Type.Value = value;
            }
        }
        public string? Description
        {
            get
            {
                if (Variable_DescriptionElement == null) return null;
                return Variable_DescriptionElement.Value;
            }
            set
            {
                if (value == null)
                {
                    if (Variable_DescriptionElement != null) Variable_DescriptionElement.Remove();
                }
                else
                {
                    if (Variable_DescriptionElement != null)
                    {
                        Variable_DescriptionElement.Value = value;
                    }
                    else
                    {
                        VariableElement.Add(new XAttribute(NamespaceNames.X + LocalName.Description, value));
                    }
                }
            }
        }

        public string ScopeName
        {
            get
            {
                return Scope_NameElement.Value;
            }
        }

        public string? DefaultValue
        {
            get
            {
                if (Variable_Default_ExpressionElement == null) return null;
                if (Variable_Default_ExpressionElement is XAttribute)
                {
                    return "\"" + ((XAttribute)Variable_Default_ExpressionElement).Value + "\"";
                }
                else
                {
                    var element = (XElement)Variable_Default_ExpressionElement;
                    if (element.Name.LocalName == LocalName.Literal)
                    {
                        var ValueAttribute = XDocumentHelpers.GetAttribute(element, LocalName.LiteralValue);
                        if (ValueAttribute == null) return "\"" + element.Value + "\"";
                        return "\"" + ValueAttribute.Value + "\"";
                    }
                    else
                    {
                        return element.Value;
                    }
                }

            }
            set
            {
                if (value == null)
                {
                    if (Variable_DefaultElement is XAttribute) ((XAttribute)Variable_DefaultElement).Remove();
                    if (Variable_DefaultElement is XElement) ((XElement)Variable_DefaultElement).Remove();
                }
                else if (value == "\"\"")
                {
                    if (Variable_Default_ExpressionElement == null || Variable_DefaultElement is XAttribute)
                    {
                        var DefaultElement = new XElement(
                            NamespaceNames.Empty + "Variable.Default",
                            new XElement(
                                NamespaceNames.Empty + LocalName.Literal,
                                new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                new XAttribute(LocalName.LiteralValue, "")
                        ));
                        if (Variable_Default_ExpressionElement is XAttribute) ((XAttribute)Variable_DefaultElement).Remove();

                        VariableElement.Add(DefaultElement);
                    }
                    else
                    {
                        var element = (XElement)Variable_Default_ExpressionElement;
                        switch (element.Name.LocalName)
                        {
                            case "Literal":
                                element.Value = "";
                                break;
                            case "CSharpValue":
                                var newElement = new XElement(
                                    NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                    new XAttribute(LocalName.LiteralValue, "")
                                );
                                element.Remove();
                                ((XElement)Variable_DefaultElement).Add(newElement);

                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                }
                else if (value.StartsWith("\"") && value.EndsWith("\""))
                {

                    if (Variable_Default_ExpressionElement == null)
                    {
                        var DefaultElement = new XElement(
                            NamespaceNames.Empty + "Variable.Default",
                            new XElement(
                                NamespaceNames.Empty + LocalName.Literal,
                                new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                new XAttribute(LocalName.LiteralValue, value.Remove(value.Length - 1).Remove(0, 1))
                        ));
                        VariableElement.Add(DefaultElement);
                    }
                    else if (Variable_Default_ExpressionElement is XAttribute)
                    {
                        ((XAttribute)Variable_Default_ExpressionElement).Value = value.Remove(value.Length - 1).Remove(0, 1);
                    }
                    else
                    {
                        var element = (XElement)Variable_Default_ExpressionElement;
                        switch (element.Name.LocalName)
                        {
                            case "Literal":
                                var valueElement = XDocumentHelpers.GetAttribute(element, LocalName.LiteralValue);
                                valueElement.Value = value.Remove(value.Length - 1).Remove(0, 1);
                                break;
                            case "CSharpValue":
                                var newElement = new XElement(
                                    NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                    new XAttribute(LocalName.LiteralValue, value)
                                );
                                element.Remove();
                                ((XElement)Variable_DefaultElement).Add(newElement);



                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                }
                else
                {
                    if (Variable_Default_ExpressionElement == null || Variable_Default_ExpressionElement is XAttribute)
                    {
                        var DefaultElement = new XElement(
                            NamespaceNames.Empty + "Variable.Default",
                            new XElement(
                                NamespaceNames.Empty + LocalName.CSharpValue,
                                new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                value
                        ));
                        if (Variable_Default_ExpressionElement is XAttribute) ((XAttribute)Variable_Default_ExpressionElement).Remove();
                        VariableElement.Add(DefaultElement);
                    }
                    else
                    {
                        var element = (XElement)Variable_Default_ExpressionElement;
                        switch (element.Name.LocalName)
                        {
                            case "Literal":
                                var newElement = new XElement(
                                    NamespaceNames.Empty + LocalName.CSharpValue,
                                    new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                    value
                                );
                                ((XElement)Variable_DefaultElement).Add(newElement);
                                element.Remove();
                                break;
                            case "CSharpValue":
                                element.Value = value.Remove(value.Length - 1).Remove(0);
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                }

            }
        }
        private XElement VariableElement { get; set; }
        private XAttribute Variable_TypeElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(VariableElement, LocalName.VariableType);
            }
        }
        private XAttribute Variable_NameElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(VariableElement, LocalName.VariableName);
            }
        }

        private XAttribute? Variable_DescriptionElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(VariableElement, LocalName.Description);
            }
        }
        private XElement ScopeElement
        {
            get
            {
                return XDocumentHelpers.GetClosestParentWithAttribute(VariableElement, LocalName.DisplayName);
            }
        }
        private XAttribute Scope_NameElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ScopeElement, LocalName.DisplayName);
            }
        }
        private XObject? Variable_DefaultElement
        {
            get
            {
                var attributeDefault = XDocumentHelpers.GetAttribute(VariableElement, "Default");
                if (VariableElement.HasElements)
                {
                    return VariableElement.Elements().First();
                }
                else if (attributeDefault != null)
                {
                    return attributeDefault;
                }
                else
                {
                    return null;
                }
            }
        }
        private XObject? Variable_Default_ExpressionElement
        {
            get
            {
                if (Variable_DefaultElement == null)
                {
                    var defaultElement = XDocumentHelpers.GetAttribute(VariableElement, "Default");
                    if (defaultElement != null) return defaultElement;
                    return null;
                }
                else if (Variable_DefaultElement is XAttribute)
                {
                    return Variable_DefaultElement;
                }
                else if (Variable_DefaultElement is XElement)
                {
                    return ((XElement)Variable_DefaultElement).Elements().First();
                }
                else
                {
                    throw new Exception("Unexpected Value");
                }
            }
        }
        private XAttribute? Variable_Default_Expression_Type
        {
            get
            {
                if (!(Variable_Default_ExpressionElement is XElement)) return null;
                return XDocumentHelpers.GetAttribute((XElement)Variable_Default_ExpressionElement, LocalName.VariableType);
            }
        }
        public Variable(XElement variable)
        {
            VariableElement = variable;
        }
    }

}
