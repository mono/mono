// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Xml;
using System.Xaml;
using MS.Internal.Xaml.Context;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;

namespace MS.Internal.Xaml.Parser
{
    [DebuggerDisplay("{Name.ScopedName}='{Value}'  {Kind}")]
    internal class XamlAttribute
    {
        private string _xmlnsDefinitionPrefix; // set only if it is a XmlNamespace
        private string _xmlnsDefinitionUri;    // set only if it is a XmlNamespace

        public XamlPropertyName Name { get; private set; }
        public string Value { get; private set; }
        public ScannerAttributeKind Kind { get; private set; }
        public XamlMember Property { get; private set; }
        public int LineNumber { get; private set; }
        public int LinePosition { get; private set; }

        public XamlAttribute(XamlPropertyName propName, string val, IXmlLineInfo lineInfo)
        {
            Name = propName;
            Value = val;
            Kind = ScannerAttributeKind.Property;  // non-"namespace" default;
            if (lineInfo != null)
            {
                LineNumber = lineInfo.LineNumber;
                LinePosition = lineInfo.LinePosition;
            }

            // XmlNs like:   xmlns:c="someUristring"
            if (CheckIsXmlNamespaceDefinition(out _xmlnsDefinitionPrefix, out _xmlnsDefinitionUri))
            {
                Kind = ScannerAttributeKind.Namespace;
            }
        }

        // This can only be done after the XmlNs's have be scanned and loaded
        public void Initialize(XamlParserContext context, XamlType tagType, string ownerNamespace, bool tagIsRoot)
        {
            // Namespaces are already flagged (but not the other kinds).
            if (Kind == ScannerAttributeKind.Namespace)
                return;

            Property = GetXamlAttributeProperty(context, Name, tagType, ownerNamespace, tagIsRoot);
            if (Property.IsUnknown)
            {
                Kind = ScannerAttributeKind.Unknown;
            }
            else if (Property.IsEvent)
            {
                Kind = ScannerAttributeKind.Event;
            }
            else if (Property.IsDirective)
            {
                if(Property == XamlLanguage.Space)
                {
                    Kind = ScannerAttributeKind.XmlSpace;
                }
                else if ((Property == XamlLanguage.FactoryMethod)
                    || (Property == XamlLanguage.Arguments)
                    || (Property == XamlLanguage.TypeArguments)
                    || (Property == XamlLanguage.Base)
                    // || (Property == XamlLanguage.Initialization)        // doesn't appear in Xml Text
                    // || (Property == XamlLanguage.PositionalParameters)  // doesn't appear in Xml Text
                    )
                {
                    Kind = ScannerAttributeKind.CtorDirective;
                }
                else
                {
                    Kind = ScannerAttributeKind.Directive;
                }
            }
            else if(Property.IsAttachable)
            {
                Kind = ScannerAttributeKind.AttachableProperty;
            }
            else if (Property == tagType.GetAliasedProperty(XamlLanguage.Name))
            {
                Kind = ScannerAttributeKind.Name;
            }
            else
            {
                Kind = ScannerAttributeKind.Property;
            }
        }


        // FxCop says this is not called 
        //public bool IsXamlNsDefinition
        //{
        //    get { return (!String.IsNullOrEmpty(_xmlnsDefinitionUri)); }
        //}

        // These properties are only defined if this Xml-Attribute is a XmlNs definition.
        public string XmlNsPrefixDefined
        {
            get { return _xmlnsDefinitionPrefix; }
        }

        public string XmlNsUriDefined
        {
            get { return _xmlnsDefinitionUri; }
        }

        //  ========================== internal ================================

        internal bool CheckIsXmlNamespaceDefinition(out string definingPrefix, out string uri)
        {
            uri = String.Empty;
            definingPrefix = String.Empty;

            // case where:  xmlns:pre="ValueUri"
            if (KS.Eq(Name.Prefix, KnownStrings.XmlNsPrefix))
            {
                uri = Value;
                definingPrefix = !Name.IsDotted
                    ? Name.Name
                    : Name.OwnerName + "." + Name.Name;
                return true;
            }
            // case where:  xmlns="ValueUri"
            if (String.IsNullOrEmpty(Name.Prefix) && KS.Eq(Name.Name, KnownStrings.XmlNsPrefix))
            {
                uri = Value;
                definingPrefix = String.Empty;
                return true;
            }
            return false;
        }

        //  ========================== private ================================

        private XamlMember GetXamlAttributeProperty(XamlParserContext context, XamlPropertyName propName,
                                                    XamlType tagType, string tagNamespace, bool tagIsRoot)
        {
            XamlMember prop = null;
            string ns = context.GetAttributeNamespace(propName, tagNamespace);

            // No Namespace, == Unknown Property
            if (ns == null)
            {
                XamlMember unknownProperty;
                if (propName.IsDotted)
                {
                    XamlType attachedOwnerType = new XamlType(string.Empty, propName.OwnerName, null, context.SchemaContext);
                    unknownProperty = new XamlMember(propName.Name, attachedOwnerType, true /*isAttachable*/);
                }
                else
                {
                    unknownProperty = new XamlMember(propName.Name, tagType, false);
                }
                return unknownProperty;
            }

            // Get the property (attached, normal, or directive)
            if (propName.IsDotted)
            {
                prop = context.GetDottedProperty(tagType, tagNamespace, propName, tagIsRoot);
            }
            else
            {
                prop = context.GetNoDotAttributeProperty(tagType, propName, tagNamespace, ns, tagIsRoot);
            }

            return prop;
        }
    }
}
