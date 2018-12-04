// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xaml;
using MS.Internal.Xaml.Context;
using System.Xaml.Schema;
using System.Xaml.MS.Impl;

namespace MS.Internal.Xaml.Parser
{
    class XamlScanner
    {
        XmlReader _xmlReader;
        IXmlLineInfo _xmlLineInfo;

        // XamlParserContext vs. XamlScannerStack
        // The XamlScannerStack belongs to the Scanner (aka XamlScanner) exclusively.
        // It has its own stack because it necessarily will read ahead sometimes.
        //
        // The XamlParserContext belongs to the Parser (aka XamlParser).
        // Except the scanner loads namespaces into the Parser's XamlParserContext,
        // and reads from it to resolve type names and namespace prefixes.
        //
        XamlScannerStack _scannerStack;
        XamlParserContext _parserContext;

        XamlText _accumulatedText = null;
        List<XamlAttribute> _attributes;
        int _nextAttribute;
        XamlScannerNode _currentNode;
        Queue<XamlScannerNode> _readNodesQueue;
        XamlXmlReaderSettings _settings;
        XamlAttribute _typeArgumentAttribute;
        bool _hasKeyAttribute = false;

        internal XamlScanner(XamlParserContext context, XmlReader xmlReader, XamlXmlReaderSettings settings)
        {
            _xmlReader = xmlReader;
            _xmlLineInfo = settings.ProvideLineInfo ? (xmlReader as IXmlLineInfo) : null;  //consider removing the "settings" check

            _parserContext = context;

            _scannerStack = new XamlScannerStack();
            _readNodesQueue = new Queue<XamlScannerNode>();

            _settings = settings;
            if (settings.XmlSpacePreserve)
            {
                _scannerStack.CurrentXmlSpacePreserve = true;
            }
        }

        public void Read()
        {
            LoadQueue();
            _currentNode = _readNodesQueue.Dequeue();
        }

        public ScannerNodeType PeekNodeType
        {
            get
            {
                LoadQueue();
                return _readNodesQueue.Peek().NodeType;
            }
        }

        public XamlType PeekType
        {
            get
            {
                LoadQueue();
                return _readNodesQueue.Peek().Type;
            }
        }

        public ScannerNodeType NodeType
        {
            get { return _currentNode.NodeType; }
        }

        public XamlType Type
        {
            get { return _currentNode.Type; }
        }

        public XamlMember PropertyAttribute
        {
            get { return _currentNode.PropertyAttribute; }
        }

        public XamlText PropertyAttributeText
        {
            get { return _currentNode.PropertyAttributeText; }
        }

        public bool IsCtorForcingMember
        {
            get { return _currentNode.IsCtorForcingMember; }
        }

        public XamlMember PropertyElement
        {
            get { return _currentNode.PropertyElement; }
        }

        public XamlText TextContent
        {
            get { return _currentNode.TextContent; }
        }

        public bool IsXDataText
        {
            get { return _currentNode.IsXDataText; }
        }

        public bool HasKeyAttribute
        {
            get
            {
                return _hasKeyAttribute;
            }
        }

        public string Prefix
        {
            get { return _currentNode.Prefix; }
        }

        public string Namespace
        {
            get { return _currentNode.TypeNamespace; }
        }

        public int LineNumber
        {
            get { return _currentNode.LineNumber; }
        }

        public int LinePosition
        {
            get { return _currentNode.LinePosition; }
        }

        // ===================================================================

        private void LoadQueue()
        {
            if (_readNodesQueue.Count == 0)
            {
                DoXmlRead();
            }
        }

        private void DoXmlRead()
        {
            while (_readNodesQueue.Count == 0)
            {
                if (_xmlReader.Read())
                {
                    ProcessCurrentXmlNode();
                }
                else
                {
                    // false from XML Read() is the same as reading .None.
                    ReadNone();
                }
            }
        }

        private void ProcessCurrentXmlNode()
        {
            XmlNodeType xmlNodeType = _xmlReader.NodeType;
            switch (xmlNodeType)
            {
            case XmlNodeType.Element:
                ReadElement();
                break;

            case XmlNodeType.EndElement:
                ReadEndElement();
                break;

            case XmlNodeType.Text:
            case XmlNodeType.CDATA:
                ReadText();
                break;

            case XmlNodeType.SignificantWhitespace:
            case XmlNodeType.Whitespace:
                ReadWhitespace();
                break;

            case XmlNodeType.None:
                ReadNone();
                break;

            default:
                break;
            }
        }
        // ============= Private ==================================

        private XamlText AccumulatedText
        {
            get
            {
                if (_accumulatedText == null)
                {
                    _accumulatedText = new XamlText(_scannerStack.CurrentXmlSpacePreserve);
                }
                return _accumulatedText;
            }
        }

        private void ClearAccumulatedText()
        {
            _accumulatedText = null;
        }

        private bool HaveAccumulatedText
        {
            get { return _accumulatedText != null && !_accumulatedText.IsEmpty; }
        }

        // ============= Element Processing ==================================

        private void ReadElement()
        {
            // Accumulated text is enqueued a piece of content before this element
            EnqueueAnyText();
            _hasKeyAttribute = false;
            // Empty Elements are by definition leaf elements and they
            // don't nest.  So we don't need to stack this state.
            bool isEmptyTag = _xmlReader.IsEmptyElement;

            string prefix = _xmlReader.Prefix;
            string strippedName = _xmlReader.LocalName;
            if (XamlName.ContainsDot(strippedName))
            {
                Debug.Assert(_xmlReader.NodeType == XmlNodeType.Element);
                XamlPropertyName name = XamlPropertyName.Parse(_xmlReader.Name, _xmlReader.NamespaceURI);
                if (_scannerStack.CurrentType == null)
                {
                    throw LineInfo(new XamlParseException(SR.Get(SRID.ParentlessPropertyElement, _xmlReader.Name)));
                }
                ReadPropertyElement(name, _scannerStack.CurrentType, _scannerStack.CurrentTypeNamespace, isEmptyTag);
            }
            else
            {
                XamlName name = new XamlQualifiedName(prefix, strippedName);
                ReadObjectElement(name, isEmptyTag);
            }
        }

        // ReadObjectElement: reads the entire start tag.  This may result in
        // more than one Scanner Nodes.   The results are enqueued onto a list
        // rather than returned directly, then the caller can de-queue the nodes
        // one at a time.
        private void ReadObjectElement(XamlName name, bool isEmptyTag)
        {
            _typeArgumentAttribute = null;
            XamlScannerNode node = new XamlScannerNode(_xmlLineInfo);

            // Scan for xmlns(s) before attempting to resolve the type.
            // So while we are there, collect up all the attributes.
            // Enqueue the xmlns attributes first.
            // PostProcess and Enqueue the other attributes after.
            PreprocessAttributes();

            node.Prefix = name.Prefix;
            node.IsEmptyTag = isEmptyTag;

            // It is possible for an application to provide XML nodes via XmlNodeReader
            // where the URI is defined but there was no xmlns attribute for use to resolve against.
            // See app Paperboy
            Debug.Assert(_xmlReader.NodeType == XmlNodeType.Element);
            string xamlNs = _xmlReader.NamespaceURI;
            if (xamlNs == null)
            {
                ReadObjectElement_NoNamespace(name, node);
            }
            else  // if (xamlNs != null)
            {
                node.TypeNamespace = xamlNs;

                // First check if the XML element is a
                // Directive Property  <x:Key>
                //
                XamlSchemaContext schemaContext = _parserContext.SchemaContext;
                XamlMember dirProperty = schemaContext.GetXamlDirective(xamlNs, name.Name);
                if (dirProperty != null)
                {
                    ReadObjectElement_DirectiveProperty(dirProperty, node);
                }
                else  // normal Element Case.
                {
                    bool sawXData = ReadObjectElement_Object(xamlNs, name.Name, node);
                    if (sawXData)
                    {
                        return;
                    }
                }
            }

            _readNodesQueue.Enqueue(node);

            // Now add the processed attributes from the rest of the start tag.
            while (HaveUnprocessedAttributes)
            {
                EnqueueAnotherAttribute(isEmptyTag);
            }
        }

        private void ReadObjectElement_NoNamespace(XamlName name, XamlScannerNode node)
        {
            XamlType errType = CreateErrorXamlType(name, string.Empty);
            node.Type = errType;

            PostprocessAttributes(node);

            if (!node.IsEmptyTag)
            {
                node.NodeType = ScannerNodeType.ELEMENT;
                _scannerStack.Push(node.Type, node.TypeNamespace);
            }
            else
            {
                node.NodeType = ScannerNodeType.EMPTYELEMENT;
            }
        }

        private void ReadObjectElement_DirectiveProperty(XamlMember dirProperty, XamlScannerNode node)
        {
            node.PropertyElement = dirProperty;

            // node.Type is not set so any found attributes are and error.
            // We don't want to skip them silently.
            PostprocessAttributes(node);

            if (_scannerStack.Depth > 0)
            {
                // A property Element tag will be the end of content.
                _scannerStack.CurrentlyInContent = false;
            }

            if (!node.IsEmptyTag)
            {
                _scannerStack.CurrentProperty = node.PropertyElement;
            }
            node.NodeType = ScannerNodeType.PROPERTYELEMENT;
            node.IsCtorForcingMember = false;
        }

        private bool ReadObjectElement_Object(string xmlns, string name, XamlScannerNode node)
        {
            if (IsXDataElement(xmlns, name))
            {
                // If XData don't Enqueue the <x:XData> node.
                // just queue the InnerXml as TEXT (w/ IsTextXML == true).
                // This will advance the "current" xml node to the </x:XData>
                // which will be skipped when we return and the main loop
                // Read()s to the "next" XmlNode.
                ReadInnerXDataSection();
                return true;
            }

            IList<XamlTypeName> typeArgs = null;
            if (_typeArgumentAttribute != null)
            {
                string error;
                typeArgs = XamlTypeName.ParseListInternal(_typeArgumentAttribute.Value, _parserContext.FindNamespaceByPrefix, out error);
                if (typeArgs == null)
                {
                    throw new XamlParseException(_typeArgumentAttribute.LineNumber, _typeArgumentAttribute.LinePosition, error);
                }
            }
            XamlTypeName typeName = new XamlTypeName(xmlns, name, typeArgs);
            node.Type = _parserContext.GetXamlType(typeName, true);

            // Finish initializing the attributes in the context of the
            // current Element.
            PostprocessAttributes(node);

            if (_scannerStack.Depth > 0)
            {
                // Sub-elements (and Text) are the definition of Content
                _scannerStack.CurrentlyInContent = true;
            }

            if (!node.IsEmptyTag)
            {
                node.NodeType = ScannerNodeType.ELEMENT;
                _scannerStack.Push(node.Type, node.TypeNamespace);
            }
            else
            {
                node.NodeType = ScannerNodeType.EMPTYELEMENT;
            }
            return false;
        }

        private void ReadPropertyElement(XamlPropertyName name, XamlType tagType, string tagNamespace, bool isEmptyTag)
        {
            // <Button>   <== currentElement
            //   <FrameworkElement.Width>   <== FrameworkElement is ownerType

            XamlScannerNode node = new XamlScannerNode(_xmlLineInfo);

            // Attributes aren't allowed on property elements.
            // but if they are there we need to scan them so the
            // XamlParser can error, or whatever.
            // (don't want to skip them w/o error)
            PreprocessAttributes();

            // It is possible for an application to provide XML nodes via XmlNodeReader
            // where the URI is defined but there was no xmlns attribute for use to resolve against.
            // See app Paperboy
            Debug.Assert(_xmlReader.NodeType == XmlNodeType.Element);
            string ownerNamespace = _xmlReader.NamespaceURI;
            XamlMember property = null;

            bool tagIsRoot = _scannerStack.Depth == 1; // PEs are processed after frame is pushed
            property = _parserContext.GetDottedProperty(tagType, tagNamespace, name, tagIsRoot);

            node.Prefix = name.Prefix;
            node.TypeNamespace = ownerNamespace;
            node.IsEmptyTag = isEmptyTag;

            // node.Type is not set (this is a property)
            // so this processing does less.
            PostprocessAttributes(node);

            if (_scannerStack.Depth > 0)
            {
                // A property Element tag will be the end of content.
                // This also allows to to start content again.
                // That is an error, but at least the parser/scanner can
                // understand what is going on.
                _scannerStack.CurrentlyInContent = false;
            }

            node.PropertyElement = property;

            node.IsCtorForcingMember = !property.IsAttachable;

            if (!node.IsEmptyTag)
            {
                _scannerStack.CurrentProperty = node.PropertyElement;
                node.NodeType = ScannerNodeType.PROPERTYELEMENT;
            }
            else
            {
                node.NodeType = ScannerNodeType.EMPTYPROPERTYELEMENT;
            }

            _readNodesQueue.Enqueue(node);

            while (HaveUnprocessedAttributes)
            {
                EnqueueAnotherAttribute(isEmptyTag);
            }
        }

        private void ReadEndElement()
        {
            // Accumulated text is enqued at start of tags (element and property)
            // and at end tags.
            EnqueueAnyText();

            // if we are ending a property element tag clear the current property
            // if we are ending an element then pop off the current frame.
            if (_scannerStack.CurrentProperty != null)
            {
                _scannerStack.CurrentProperty = null;
                // List of Content is considered separately for each property.
                _scannerStack.CurrentlyInContent = false;
            }
            else
            {
                _scannerStack.Pop();
            }

            XamlScannerNode node = new XamlScannerNode(_xmlLineInfo);
            node.NodeType = ScannerNodeType.ENDTAG;
            _readNodesQueue.Enqueue(node);
        }

        private void ReadText()
        {
            // Trim the leading whitespace from the text if it is the first bit on content.
            bool isFirstTextInContent = !_scannerStack.CurrentlyInContent;
            AccumulatedText.Paste(_xmlReader.Value, isFirstTextInContent);
            _scannerStack.CurrentlyInContent = true;
        }

        private void ReadWhitespace()
        {
            bool isFirstTextInContent = !_scannerStack.CurrentlyInContent;
            AccumulatedText.Paste(_xmlReader.Value, isFirstTextInContent);
            // Whitespace, by itself, does not change the "InContent" state.
        }

        private void ReadNone()
        {
            XamlScannerNode node = new XamlScannerNode(_xmlLineInfo);
            node.NodeType = ScannerNodeType.NONE;
            _readNodesQueue.Enqueue(node);
        }

        private void ReadInnerXDataSection()
        {
            XamlScannerNode node = new XamlScannerNode(_xmlLineInfo);
            _xmlReader.MoveToContent(); // skip whitespaces
            string xmlData = _xmlReader.ReadInnerXml();
            xmlData = xmlData.Trim();
            node.NodeType = ScannerNodeType.TEXT;
            node.IsXDataText = true;
            XamlText xmlText = new XamlText(true);
            xmlText.Paste(xmlData, false);
            node.TextContent = xmlText;
            _readNodesQueue.Enqueue(node);

            // Read InnerXml will advance over the End xData tag and
            // we need to process the current XML state w/o going back for a Read().
            ProcessCurrentXmlNode();
        }

        private XamlType CreateErrorXamlType(XamlName name, string xmlns)
        {
            return new XamlType(xmlns, name.Name, null, _parserContext.SchemaContext);
        }

        // ======== Attribute Processing =============================

        private void PreprocessAttributes()
        {
            // Collect up all the attributes.
            bool b = _xmlReader.MoveToFirstAttribute();

            if (!b)
            {
                return;
            }

            List<XamlAttribute> list = new List<XamlAttribute>();
            do
            {
                string xmlName = _xmlReader.Name;
                string val = _xmlReader.Value;

                XamlPropertyName propName = XamlPropertyName.Parse(xmlName);

                if (propName == null)
                {
                    throw new XamlParseException(SR.Get(SRID.InvalidXamlMemberName, xmlName));
                }

                XamlAttribute attr = new XamlAttribute(propName, val, _xmlLineInfo);

                if (attr.Kind == ScannerAttributeKind.Namespace)
                {
                    EnqueuePrefixDefinition(attr);
                }
                else
                {
                    list.Add(attr);
                }

                b = _xmlReader.MoveToNextAttribute();
            } while (b);

            PreprocessForTypeArguments(list);

            if (list.Count > 0)
            {
                _attributes = list;
            }
            // Restore the XML reader’s position to the Element after reading the
            // attributes so that the rest of the code can always assume it is on an Element
            _xmlReader.MoveToElement();
        }

        private void PreprocessForTypeArguments(List<XamlAttribute> attrList)
        {
            int typeArgsIdx = -1;
            for (int i = 0; i < attrList.Count; i++)
            {
                XamlAttribute attr = attrList[i];

                // Find x:TypeArguments if it was present.
                if (KS.Eq(attr.Name.Name, XamlLanguage.TypeArguments.Name))
                {
                    string attrNamespace = _parserContext.FindNamespaceByPrefix(attr.Name.Prefix);
                    XamlMember directiveProperty = _parserContext.ResolveDirectiveProperty(attrNamespace, attr.Name.Name);
                    if (directiveProperty != null)
                    {
                        typeArgsIdx = i;
                        _typeArgumentAttribute = attr;
                        break;
                    }
                }
            }
            if (typeArgsIdx >= 0)
            {
                attrList.RemoveAt(typeArgsIdx);
            }
        }

        private void PostprocessAttributes(XamlScannerNode node)
        {
            if (_attributes == null)
            {
                return;
            }

            _nextAttribute = 0;

            // Attributes on Properties are errors
            // and don't need this detailed processing.
            if (node.Type == null)
            {
                if (_settings.IgnoreUidsOnPropertyElements)
                {
                    StripUidProperty();
                }
                return;
            }


            bool tagIsRoot = _scannerStack.Depth == 0; // Attributes are processed before frame is pushed
            foreach (XamlAttribute attr in _attributes)
            {
                attr.Initialize(_parserContext, node.Type, node.TypeNamespace, tagIsRoot);
            }

            // Sort the Attributes into the order the XAML parser likes.

            List<XamlAttribute> ctorDirectivesList = null;
            List<XamlAttribute> otherDirectivesList = null;
            List<XamlAttribute> otherPropertiesList = null;
            XamlAttribute nameAttribute = null;

            // The Name attribute
            foreach (XamlAttribute attr in _attributes)
            {
                switch(attr.Kind)
                {
                case ScannerAttributeKind.Name:
                    nameAttribute = attr;
                        break;

                case ScannerAttributeKind.CtorDirective:
                    if (ctorDirectivesList == null)
                    {
                        ctorDirectivesList = new List<XamlAttribute>();
                    }
                    ctorDirectivesList.Add(attr);
                        break;

                case ScannerAttributeKind.Directive:
                case ScannerAttributeKind.XmlSpace:
                    if (attr.Property == XamlLanguage.Key)
                    {
                        _hasKeyAttribute = true;
                    }

                    if (otherDirectivesList == null)
                    {
                        otherDirectivesList = new List<XamlAttribute>();
                    }
                    otherDirectivesList.Add(attr);
                    break;

                default:
                    if (otherPropertiesList == null)
                    {
                        otherPropertiesList = new List<XamlAttribute>();
                    }
                    otherPropertiesList.Add(attr);
                    break;
                }
            }

            _attributes = new List<XamlAttribute>();

            // First the Construction Directives
            if (ctorDirectivesList != null)
            {
                _attributes.AddRange(ctorDirectivesList);
            }

            if (otherDirectivesList != null)
            {
                _attributes.AddRange(otherDirectivesList);
            }

            // Next the aliased Name property before any other "real" properties.
            // (this is a WPF template requirement)
            if (nameAttribute != null)
            {
                _attributes.Add(nameAttribute);
            }

            // Then everything else
            if (otherPropertiesList != null)
            {
                _attributes.AddRange(otherPropertiesList);
            }
        }

        private void StripUidProperty()
        {
            for (int i = _attributes.Count - 1; i >= 0; i--)
            {
                if (KS.Eq(_attributes[i].Name.ScopedName, XamlLanguage.Uid.Name))
                {
                    _attributes.RemoveAt(i);
                }
            }
            if (_attributes.Count == 0)
            {
                _attributes = null;
            }
        }

        private bool HaveUnprocessedAttributes
        {
            get { return _attributes != null; }
        }

        private void EnqueueAnotherAttribute(bool isEmptyTag)
        {
            XamlAttribute attr = _attributes[_nextAttribute++];
            XamlScannerNode node = new XamlScannerNode(attr);

            switch (attr.Kind)
            {
            case ScannerAttributeKind.Directive:
            case ScannerAttributeKind.Name:
            case ScannerAttributeKind.CtorDirective:
                node.NodeType = ScannerNodeType.DIRECTIVE;
                break;

            case ScannerAttributeKind.XmlSpace:
                // Empty tags don't have a stack frame to write on.
                // Empty XML tags don't have content to process spaces.
                if (!isEmptyTag)
                {
                    if (KS.Eq(attr.Value, KnownStrings.Preserve))
                        _scannerStack.CurrentXmlSpacePreserve = true;
                    else
                        _scannerStack.CurrentXmlSpacePreserve = false;
                }
                node.NodeType = ScannerNodeType.DIRECTIVE;
                break;

            case ScannerAttributeKind.Event:
            case ScannerAttributeKind.Property:
                node.IsCtorForcingMember = true;
                node.NodeType = ScannerNodeType.ATTRIBUTE;
                break;

           case ScannerAttributeKind.Unknown:
                XamlMember prop = attr.Property;
                Debug.Assert(prop.IsUnknown);
                // force use of Ctor for unknown simple properties only
                node.IsCtorForcingMember = !prop.IsAttachable && !prop.IsDirective;
                node.NodeType = ScannerNodeType.ATTRIBUTE;
                break;

            case ScannerAttributeKind.AttachableProperty:
                node.NodeType = ScannerNodeType.ATTRIBUTE;
                break;

            default:
                throw new XamlInternalException(SR.Get(SRID.AttributeUnhandledKind));
            }

            // XamlText.Paste normally converts CRLF to LF, even in attribute values.
            // When the property is Glyphs.UnicodeString, disable this (Dev11 796882);
            // the length of the string must correspond to the number of entries in
            // the corresponding Glyphs.Indices property.
            XamlMember attrProperty = attr.Property;
            bool convertCRLFtoLF =
                !(attrProperty != null &&
                  attrProperty.Name == "UnicodeString" &&
                  attrProperty.DeclaringType.Name == "Glyphs");

            node.PropertyAttribute = attrProperty;
            XamlText xamlText = new XamlText(true);  // Don't collapse spaces in attributes
            xamlText.Paste(attr.Value, false, convertCRLFtoLF);
            node.PropertyAttributeText = xamlText;
            node.Prefix = attr.Name.Prefix;

            _readNodesQueue.Enqueue(node);

            if (_nextAttribute >= _attributes.Count)
            {
                _attributes = null;
                _nextAttribute = -1;
            }
        }

        private void EnqueueAnyText()
        {
            if (HaveAccumulatedText)
            {
                // some consideration of _scannerStack.CurrentXmlSpacePreserve
                // it will cause Whitespace here but we only enqueue it if
                // we are In a Whitespace significant collection.
                EnqueueTextNode();
            }
            ClearAccumulatedText();
        }

        private void EnqueueTextNode()
        {
            Debug.Assert(_accumulatedText != null, "Creating unnecessary XamlText objects");

            // Don't send the text if it is Whitespace outside the root tag.
            if (!(_scannerStack.Depth == 0 && AccumulatedText.IsWhiteSpaceOnly))
            {
                XamlScannerNode node = new XamlScannerNode(_xmlLineInfo);
                node.NodeType = ScannerNodeType.TEXT;
                node.TextContent = AccumulatedText;
                _readNodesQueue.Enqueue(node);
            }
        }

        private void EnqueuePrefixDefinition(XamlAttribute attr)
        {
            string prefix = attr.XmlNsPrefixDefined;
            string xamlNamespace = attr.XmlNsUriDefined;
            _parserContext.AddNamespacePrefix(prefix, xamlNamespace);

            XamlScannerNode node = new XamlScannerNode(attr);
            node.NodeType = ScannerNodeType.PREFIXDEFINITION;
            node.Prefix = prefix;
            node.TypeNamespace = xamlNamespace;

            _readNodesQueue.Enqueue(node);
        }

        private bool IsXDataElement(string xmlns, string name)
        {
            return
                XamlLanguage.XamlNamespaces.Contains(xmlns) &&
                KS.Eq(XamlLanguage.XData.Name, name);
        }

        XamlException LineInfo(XamlException e)
        {
            if (_xmlLineInfo != null)
            {
                e.SetLineInfo(_xmlLineInfo.LineNumber, _xmlLineInfo.LinePosition);
            }
            return e;
        }
    }
}
