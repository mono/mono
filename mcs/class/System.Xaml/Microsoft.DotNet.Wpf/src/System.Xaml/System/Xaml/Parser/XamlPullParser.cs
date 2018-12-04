// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Xaml;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using MS.Internal.Xaml.Context;
using System.Xaml.Schema;
using System.Xaml.MS.Impl;
using System.Collections;

namespace MS.Internal.Xaml.Parser
{
    internal class XamlPullParser
    {
        XamlParserContext _context;
        XamlScanner _xamlScanner;
        XamlXmlReaderSettings _settings;

        public XamlPullParser(XamlParserContext context, XamlScanner scanner, XamlXmlReaderSettings settings)
        {
            _context = context;
            _xamlScanner = scanner;
            _settings = settings;
        }

        // =====================================================

        //  Document ::= PREFIXDEFINITION* Element
        //  Element ::= EmptyElement | (StartElement ElementBody)
        //  EmptyElement ::= EMPTYELEMENT DIRECTIVE* ATTRIBUTE*
        //  StartElement ::= ELEMENT DIRECTIVE*
        //  ElementBody ::= ATTRIBUTE* ( PropertyElement | ElementContent+ )* ENDTAG
        //  PropertyElement ::= EmptyPropertyElement | NonemptyPropertyElement
        //  EmptyPropertyElement ::= EMPTYPROPERTYELEMENT
        //  NonemptyPropertyElement ::= PROPERTYELEMENT PropertyContent* ENDTAG
        //  ElementContent ::= ( PREFIXDEFINITION* Element ) | TEXT
        //  PropertyContent ::= ( PREFIXDEFINITION* Element ) | TEXT
        //
        // Attribute and Directive values can be markup extensions.


        ///////////////////////////
        //  Document::= PREFIXDEFINITION* Element
        //
        public IEnumerable<XamlNode> Parse()
        {
            _xamlScanner.Read();
            if (ProvideLineInfo)
            {
                yield return Logic_LineInfo();
            }

            ScannerNodeType nodeType = _xamlScanner.NodeType;
            while (nodeType == ScannerNodeType.PREFIXDEFINITION)
            {
                yield return Logic_PrefixDefinition();
                _xamlScanner.Read();
                if (ProvideLineInfo)
                {
                    yield return Logic_LineInfo();
                }
                nodeType = _xamlScanner.NodeType;
            }

            foreach (XamlNode node in P_Element())
            {
                yield return node;
            }
        }

        // =====================================================

        ///////////////////////////
        //  Element ::= EmptyElement | (StartElement ElementBody)
        //
        public IEnumerable<XamlNode> P_Element()
        {
            ScannerNodeType nodeType = _xamlScanner.NodeType;

            switch (nodeType)
            {
            case ScannerNodeType.EMPTYELEMENT:
                foreach (XamlNode node in P_EmptyElement())
                {
                    yield return node;
                }
                break;

            case ScannerNodeType.ELEMENT:
                foreach (XamlNode node in P_StartElement())
                {
                    yield return node;
                }
                foreach (XamlNode node in P_ElementBody())
                {
                    yield return node;
                }
                break;
            default:
                throw new XamlUnexpectedParseException(_xamlScanner, nodeType,
                    SR.Get(SRID.ElementRuleException));
            }
        }

        ///////////////////////////
        //  EmptyElement ::= EMPTYELEMENT DIRECTIVE* ATTRIBUTE*
        //
        public IEnumerable<XamlNode> P_EmptyElement()
        {
            if (_xamlScanner.NodeType != ScannerNodeType.EMPTYELEMENT)
            {
                throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType,
                    SR.Get(SRID.EmptyElementRuleException));
            }
            yield return Logic_StartObject(_xamlScanner.Type, _xamlScanner.Namespace);
            _xamlScanner.Read();
            if (ProvideLineInfo)
            {
                yield return Logic_LineInfo();
            }

            while (_xamlScanner.NodeType == ScannerNodeType.DIRECTIVE)
            {
                // Directives are processed exactly like Attributes.
                foreach (XamlNode node in LogicStream_Attribute())
                {
                    yield return node;
                }
                _xamlScanner.Read();
                if (ProvideLineInfo)
                {
                    yield return Logic_LineInfo();
                }
            }
            while (_xamlScanner.NodeType == ScannerNodeType.ATTRIBUTE)
            {
                foreach (XamlNode node in LogicStream_Attribute())
                {
                    yield return node;
                }
                _xamlScanner.Read();
                if (ProvideLineInfo)
                {
                    yield return Logic_LineInfo();
                }
            }

            yield return Logic_EndOfAttributes();
            yield return Logic_EndObject();
        }

        ///////////////////////////
        //  StartElement ::= ELEMENT DIRECTIVE*
        //
        public IEnumerable<XamlNode> P_StartElement()
        {
            if (_xamlScanner.NodeType != ScannerNodeType.ELEMENT)
            {
                throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType,
                    SR.Get(SRID.StartElementRuleException));
            }
            yield return Logic_StartObject(_xamlScanner.Type, _xamlScanner.Namespace);
            _xamlScanner.Read();
            if (ProvideLineInfo)
            {
                yield return Logic_LineInfo();
            }

            while (_xamlScanner.NodeType == ScannerNodeType.DIRECTIVE)
            {
                // Directives are processed exactly like Attributes.
                foreach (XamlNode node in LogicStream_Attribute())
                {
                    yield return node;
                }
                _xamlScanner.Read();
                if (ProvideLineInfo)
                {
                    yield return Logic_LineInfo();
                }
            }
        }

        ///////////////////////////
        //  ElementBody ::= ATTRIBUTE* ( PropertyElement | ElementContent+ )* ENDTAG
        //
        public IEnumerable<XamlNode> P_ElementBody()
        {
            while (_xamlScanner.NodeType == ScannerNodeType.ATTRIBUTE)
            {
                foreach (XamlNode node in LogicStream_Attribute())
                {
                    yield return node;
                }
                _xamlScanner.Read();
                if (ProvideLineInfo)
                {
                    yield return Logic_LineInfo();
                }
            }

            yield return Logic_EndOfAttributes();

            bool doneWithElementContent = false;
            bool hasContent = false;
            do
            {
                ScannerNodeType nodeType = _xamlScanner.NodeType;
                switch (nodeType)
                {
                case ScannerNodeType.PROPERTYELEMENT:
                case ScannerNodeType.EMPTYPROPERTYELEMENT:
                    hasContent = true;
                    foreach (XamlNode node in P_PropertyElement())
                    {
                        yield return node;
                    }
                    break;

                case ScannerNodeType.PREFIXDEFINITION:
                case ScannerNodeType.ELEMENT:
                case ScannerNodeType.EMPTYELEMENT:
                case ScannerNodeType.TEXT:
                    hasContent = true;
                    do
                    {
                        foreach (XamlNode node in P_ElementContent())
                        {
                            yield return node;
                        }
                        nodeType = _xamlScanner.NodeType;
                    } while (nodeType == ScannerNodeType.PREFIXDEFINITION
                            || nodeType == ScannerNodeType.ELEMENT
                            || nodeType == ScannerNodeType.EMPTYELEMENT
                            || nodeType == ScannerNodeType.TEXT);

                    // If the above started a container directive or an unknown content property, then end the collection.
                    if (_context.CurrentInItemsProperty || _context.CurrentInInitProperty || _context.CurrentInUnknownContent)
                    {
                        yield return Logic_EndMember(); // Container or unknown content property.

                        if (_context.CurrentInCollectionFromMember)
                        {
                            yield return Logic_EndObject();    // Getter pseudo Object
                            yield return Logic_EndMember();   // Content Property
                            _context.CurrentInCollectionFromMember = false;
                            if (_context.CurrentInImplicitArray)
                            {
                                _context.CurrentInImplicitArray = false;
                                yield return Logic_EndObject();
                                yield return Logic_EndMember();
                            }
                        }
                    }
                    break;
                case ScannerNodeType.ENDTAG:
                    // <Foo></Foo> if foo has no default constructor we need to output SM _Initialization V "" EM
                    XamlType currentType = _context.CurrentType;
                    bool hasTypeConverter = currentType.TypeConverter != null;
                    bool isConstructable = currentType.IsConstructible && !currentType.ConstructionRequiresArguments;
                    if (!hasContent && hasTypeConverter && !isConstructable)
                    {
                        yield return Logic_StartInitProperty(currentType);
                        yield return new XamlNode(XamlNodeType.Value, string.Empty);
                        yield return Logic_EndMember();
                    }
                    doneWithElementContent = true;
                    break;
                default:
                    doneWithElementContent = true;
                    break;
                }
            } while (!doneWithElementContent);

            if (_xamlScanner.NodeType != ScannerNodeType.ENDTAG)
            {
                throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType,
                    SR.Get(SRID.ElementBodyRuleException));
            }
            yield return Logic_EndObject();
            _xamlScanner.Read();
            if (ProvideLineInfo)
            {
                yield return Logic_LineInfo();
            }
        }

        ///////////////////////////
        //  PropertyElement ::= EmptyPropertyElement | NonemptyPropertyElement
        //
        public IEnumerable<XamlNode> P_PropertyElement()
        {
            ScannerNodeType nodeType = _xamlScanner.NodeType;

            switch (nodeType)
            {
                case ScannerNodeType.EMPTYPROPERTYELEMENT:
                    foreach (XamlNode node in P_EmptyPropertyElement())
                    {
                        yield return node;
                    }
                    break;
                case ScannerNodeType.PROPERTYELEMENT:
                    foreach (XamlNode node in P_NonemptyPropertyElement())
                    {
                        yield return node;
                    }
                    break;
                default:
                    throw new XamlUnexpectedParseException(_xamlScanner, nodeType,
                        SR.Get(SRID.PropertyElementRuleException));
            }
        }

        ///////////////////////////
        //  EmptyPropertyElement ::= EMPTYPROPERTYELEMENT
        //
        public IEnumerable<XamlNode> P_EmptyPropertyElement()
        {
            if (_xamlScanner.NodeType != ScannerNodeType.EMPTYPROPERTYELEMENT)
            {
                throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType,
                    SR.Get(SRID.EmptyPropertyElementRuleException));
            }
            yield return Logic_StartMember(_xamlScanner.PropertyElement);
            yield return Logic_EndMember();
            _xamlScanner.Read();
            if (ProvideLineInfo)
            {
                yield return Logic_LineInfo();
            }
        }

        ///////////////////////////
        //  NonemptyPropertyElement ::= PROPERTYELEMENT PropertyContent* ENDTAG
        //
        public IEnumerable<XamlNode> P_NonemptyPropertyElement()
        {
            if (_xamlScanner.NodeType != ScannerNodeType.PROPERTYELEMENT)
            {
                throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType,
                    SR.Get(SRID.NonemptyPropertyElementRuleException));
            }
            yield return Logic_StartMember(_xamlScanner.PropertyElement);
            _xamlScanner.Read();
            if (ProvideLineInfo)
            {
                yield return Logic_LineInfo();
            }

            bool doingPropertyContent = true;
            do
            {
                ScannerNodeType nodeType = _xamlScanner.NodeType;
                switch (nodeType)
                {
                case ScannerNodeType.PREFIXDEFINITION:
                case ScannerNodeType.ELEMENT:
                case ScannerNodeType.EMPTYELEMENT:
                case ScannerNodeType.TEXT:
                    do
                    {
                        foreach (XamlNode node in P_PropertyContent())
                        {
                            yield return node;
                        }
                        nodeType = _xamlScanner.NodeType;
                    } while (nodeType == ScannerNodeType.PREFIXDEFINITION
                            || nodeType == ScannerNodeType.ELEMENT
                            || nodeType == ScannerNodeType.EMPTYELEMENT
                            || nodeType == ScannerNodeType.TEXT);
                    // If the above started a container directive, end the collection.
                    if (_context.CurrentInItemsProperty || _context.CurrentInInitProperty)
                    {
                        yield return Logic_EndMember();   // Pseudo container property.

                        if (_context.CurrentInCollectionFromMember)
                        {
                            yield return Logic_EndObject();    // Getter pseudo Object
                            _context.CurrentInCollectionFromMember = false;
                            if (_context.CurrentInImplicitArray)
                            {
                                _context.CurrentInImplicitArray = false;
                                yield return Logic_EndMember();
                                yield return Logic_EndObject();
                            }
                        }
                    }
                    break;
                default:
                    doingPropertyContent = false;
                    break;
                }
            } while (doingPropertyContent);


            if (_xamlScanner.NodeType != ScannerNodeType.ENDTAG)
            {
                throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType,
                    SR.Get(SRID.NonemptyPropertyElementRuleException));
            }
            yield return Logic_EndMember();
            _xamlScanner.Read();
            if (ProvideLineInfo)
            {
                yield return Logic_LineInfo();
            }
        }

        ///////////////////////////
        //  ElementContent ::= ( PREFIXDEFINITION* Element ) | TEXT
        //
        public IEnumerable<XamlNode> P_ElementContent()
        {
            XamlType currentType = _context.CurrentType;

            List<XamlNode> savedPrefixDefinitions = null;
            ScannerNodeType nodeType = _xamlScanner.NodeType;
            switch (nodeType)
            {
            case ScannerNodeType.PREFIXDEFINITION:
            case ScannerNodeType.ELEMENT:
            case ScannerNodeType.EMPTYELEMENT:
            case ScannerNodeType.TEXT:
                if (nodeType == ScannerNodeType.TEXT)
                {
                    XamlText text = _xamlScanner.TextContent;

                    if (Logic_IsDiscardableWhitespace(text))
                    {
                        _xamlScanner.Read();
                        if (ProvideLineInfo)
                        {
                            yield return Logic_LineInfo();
                        }
                        break;
                    }
                }

                // Don't immediately emit the prefix Definitions.
                // buffer them for moment because if this is the first object
                // in a collection, we may need to jam an implicit _Items property
                // on Content Property in before the PrefixDef's and then the ObjectType.
                while (nodeType == ScannerNodeType.PREFIXDEFINITION)
                {
                    if (savedPrefixDefinitions == null)
                    {
                        savedPrefixDefinitions = new List<XamlNode>();
                    }
                    if (ProvideLineInfo)
                    {
                        savedPrefixDefinitions.Add(Logic_LineInfo());
                    }
                    savedPrefixDefinitions.Add(Logic_PrefixDefinition());
                    _xamlScanner.Read();
                    if (ProvideLineInfo)
                    {
                        yield return Logic_LineInfo();
                    }
                    nodeType = _xamlScanner.NodeType;
                }

                // Check for any preambles we need to emit before the
                // emitting the actual element or Text.
                bool isTextInitialization = false;
                if (!_context.CurrentInItemsProperty && !_context.CurrentInUnknownContent)
                {
                    bool isContentProperty = false;
                    // In case of text, we look first for a string or object content property,
                    // then a TypeConverter
                    if (nodeType == ScannerNodeType.TEXT)
                    {
                        if (currentType.ContentProperty != null && CanAcceptString(currentType.ContentProperty))
                        {
                            isContentProperty = true;
                        }
                        // If there have been "real" properties then we are forced to use the
                        // Constructor.  Otherwise we can consider a TypeConverter on the TEXT.
                        else if (!_context.CurrentForcedToUseConstructor
                                && !_xamlScanner.TextContent.IsEmpty
                                && currentType.TypeConverter != null)
                        {
                            isTextInitialization = true;
                        }
                    }
                    // Otherwise, we look first for a collection, and then fall back to content property
                    if (!isTextInitialization && !isContentProperty)
                    {
                        // If we are first in a collection
                        if (currentType.IsCollection || currentType.IsDictionary)
                        {
                            yield return Logic_StartItemsProperty(currentType);
                        }
                        else  // Back to ContentProperty (either element or unknown content)
                        {
                            isContentProperty = true;
                        }
                    }
                    // Don't yield more than one unknown content property for multiple,
                    // contiguous content objects and values.
                    if (isContentProperty && !_context.CurrentInUnknownContent)
                    {
                        XamlMember contentProperty = currentType.ContentProperty;
                        if (contentProperty != null)
                        {
                            bool isVisible = _context.IsVisible(
                                contentProperty, _context.CurrentTypeIsRoot ? _context.CurrentType : null);
                            // Visible content properties produce known members.
                            // Invisible content properties produce unknown members.
                            // Protected content properties of root instances and internal
                            // content properties can be visible, depending on the reader settings.
                            if (!isVisible)
                            {
                                // We use the current type, not the actual declaring type of the non-visible property,
                                // for consistency with how non-visible PEs and Attribute Properties are handled.
                                contentProperty = new XamlMember(contentProperty.Name, currentType, false);
                            }
                        }
                        // A null argument produces an unknown content member.
                        yield return Logic_StartContentProperty(contentProperty);

                        // Check for and emit the get collection from member.
                        foreach (XamlNode node in LogicStream_CheckForStartGetCollectionFromMember())
                        {
                            yield return node;
                        }
                    }
                }

                // Now we are ready for the given element.
                // so now emit the saved prefix definitions.
                if (savedPrefixDefinitions != null)
                {
                    for (int i = 0; i < savedPrefixDefinitions.Count; i++)
                    {
                        yield return savedPrefixDefinitions[i];
                    }
                    if (ProvideLineInfo)
                    {
                        yield return Logic_LineInfo();
                    }
                }

                if (nodeType == ScannerNodeType.TEXT)
                {
                    XamlText text = _xamlScanner.TextContent;
                    string trimmed = Logic_ApplyFinalTextTrimming(text);
                    bool isXDataText = _xamlScanner.IsXDataText;
                    _xamlScanner.Read();
                    if (ProvideLineInfo)
                    {
                        yield return Logic_LineInfo();
                    }

                    if (trimmed == String.Empty)
                    {
                        break;
                    }

                    if (isTextInitialization)
                    {
                        yield return Logic_StartInitProperty(currentType);
                    }

                    if (isXDataText)
                    {
                        yield return Logic_StartObject(XamlLanguage.XData, null);
                        XamlMember xDataTextProperty = XamlLanguage.XData.GetMember("Text");
                        yield return Logic_EndOfAttributes();
                        yield return Logic_StartMember(xDataTextProperty);
                    }

                    yield return new XamlNode(XamlNodeType.Value, trimmed);

                    if (isXDataText)
                    {
                        yield return Logic_EndMember();
                        yield return Logic_EndObject();
                    }
                }
                else
                {
                    foreach (XamlNode node in P_Element())
                    {
                        yield return node;
                    }
                }

                // If we are not in an items or unknown content property, then
                // there cannot be more objects or values that follow this content
                // (a singular property), and thus we can end this property now.
                if (!_context.CurrentInItemsProperty && !_context.CurrentInUnknownContent)
                {
                    yield return Logic_EndMember();
                }
                break;
            } // end switch
        }

        ///////////////////////////
        //  PropertyContent ::= ( PREFIXDEFINITION* Element ) | TEXT
        //
        public IEnumerable<XamlNode> P_PropertyContent()
        {
            ScannerNodeType nodeType = _xamlScanner.NodeType;
            List<XamlNode> _savedPrefixDefinitions = null;
            string trimmed = String.Empty;
            bool isTextXML = false;

            switch (nodeType)
            {
            case ScannerNodeType.PREFIXDEFINITION:
            case ScannerNodeType.ELEMENT:
            case ScannerNodeType.EMPTYELEMENT:
            case ScannerNodeType.TEXT:
                if (nodeType == ScannerNodeType.TEXT)
                {
                    XamlText text = _xamlScanner.TextContent;

                    if (Logic_IsDiscardableWhitespace(text))
                    {
                        trimmed = String.Empty;
                    }
                    else
                    {
                        trimmed = Logic_ApplyFinalTextTrimming(text);
                    }

                    isTextXML = _xamlScanner.IsXDataText;
                    _xamlScanner.Read();
                    if (ProvideLineInfo)
                    {
                        yield return Logic_LineInfo();
                    }
                    if (trimmed == String.Empty)
                    {
                        break;
                    }
                }

                // Don't immediately emit the prefix Definitions.
                // buffer them for moment because if this is the first object
                // in a collection, we may need to jam an implicit _Items property
                // in before the PrefixDef's and then the ObjectType.
                while (nodeType == ScannerNodeType.PREFIXDEFINITION)
                {
                    if (_savedPrefixDefinitions == null)
                    {
                        _savedPrefixDefinitions = new List<XamlNode>();
                    }
                    _savedPrefixDefinitions.Add(Logic_PrefixDefinition());
                    if (ProvideLineInfo)
                    {
                        _savedPrefixDefinitions.Add(Logic_LineInfo());
                    }
                    _xamlScanner.Read();
                    if (ProvideLineInfo)
                    {
                        yield return Logic_LineInfo();
                    }
                    nodeType = _xamlScanner.NodeType;
                }

                // If this is TEXT and the current Property has a TypeConverter
                // Then emit the TEXT now.
                if (nodeType == ScannerNodeType.TEXT
                    && _context.CurrentMember.TypeConverter != null)
                {
                    yield return new XamlNode(XamlNodeType.Value, trimmed);
                }
                else
                {
                    // Check for any preambles we need to emit before the
                    // emitting the actual element or Text.
                    if (!_context.CurrentInCollectionFromMember)
                    {
                        // Check for and emit the get collection from member.
                        foreach (XamlNode node in LogicStream_CheckForStartGetCollectionFromMember())
                        {
                            yield return node;
                        }
                    }

                    // We couldn't emit text in the code above (directly under the property).
                    // We have now (possibly) started a get collection from member.  This TEXT might go
                    // under the _items.
                    // This might be <XDATA>.
                    // It might still be an error, ie. Unknown Content.
                    // This is the last chance to emit the TEXT.
                    if (nodeType == ScannerNodeType.TEXT)
                    {
                        if (isTextXML)
                        {
                            yield return Logic_StartObject(XamlLanguage.XData, null);
                            XamlMember xDataTextProperty = XamlLanguage.XData.GetMember("Text");
                            yield return Logic_EndOfAttributes();
                            yield return Logic_StartMember(xDataTextProperty);
                        }

                        yield return new XamlNode(XamlNodeType.Value, trimmed);

                        if (isTextXML)
                        {
                            yield return Logic_EndMember();
                            yield return Logic_EndObject();
                        }
                    }
                    else
                    {
                        // Now we are ready for the given element.
                        // now emit the saved prefix definitions.
                        if (_savedPrefixDefinitions != null)
                        {
                            for (int i = 0; i < _savedPrefixDefinitions.Count; i++)
                            {
                                yield return _savedPrefixDefinitions[i];
                            }
                        }

                        foreach (XamlNode node in P_Element())
                        {
                            yield return node;
                        }
                    }
                }
                break;
            }
        }

        // ---------- Private properties  ---------------

        private int LineNumber
        {
            get { return _xamlScanner.LineNumber; }
        }

        private int LinePosition
        {
            get { return _xamlScanner.LinePosition; }
        }

        private bool ProvideLineInfo
        {
            get { return _settings.ProvideLineInfo; }
        }

        // =================== Logic Functions ========================

        private XamlNode Logic_LineInfo()
        {
            LineInfo lineInfo = new LineInfo(LineNumber, LinePosition);
            XamlNode lineInfoNode = new XamlNode(lineInfo);
            return lineInfoNode;
        }

        private XamlNode Logic_PrefixDefinition()
        {
            string prefix = _xamlScanner.Prefix;
            string xamlNs = _xamlScanner.Namespace;
            XamlNode addNs = new XamlNode(XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration(xamlNs, prefix));
            return addNs;
        }

        private XamlNode Logic_StartObject(XamlType xamlType, string xamlNamespace)
        {
            _context.PushScope();
            _context.CurrentType = xamlType;
            _context.CurrentTypeNamespace = xamlNamespace;

            XamlNode startObj = new XamlNode(XamlNodeType.StartObject, xamlType);
            return startObj;
        }

        private XamlNode Logic_EndObject()
        {
            XamlType xamlType = _context.CurrentType;

            _context.PopScope();
            _context.CurrentPreviousChildType = xamlType;

            XamlNode endObj = new XamlNode(XamlNodeType.EndObject);
            return endObj;
        }

        private IEnumerable<XamlNode> LogicStream_Attribute()
        {
            XamlMember property = _xamlScanner.PropertyAttribute;
            XamlText text = _xamlScanner.PropertyAttributeText;

            if (_xamlScanner.IsCtorForcingMember)
            {
                _context.CurrentForcedToUseConstructor = true;
            }

            XamlNode startProperty = new XamlNode(XamlNodeType.StartMember, property);
            yield return startProperty;

            if (text.LooksLikeAMarkupExtension)
            {
                MePullParser me = new MePullParser(_context);
                foreach (XamlNode node in me.Parse(text.Text, LineNumber, LinePosition))
                {
                    yield return node;
                }
            }
            else
            {
                XamlNode textNode = new XamlNode(XamlNodeType.Value, text.AttributeText);
                yield return textNode;
            }
            yield return new XamlNode(XamlNodeType.EndMember);
        }

        private XamlNode Logic_EndOfAttributes()
        {
            var endOfAttributes = new XamlNode(XamlNode.InternalNodeType.EndOfAttributes);
            return endOfAttributes;
        }

        private XamlNode Logic_StartMember(XamlMember member)
        {
            _context.CurrentMember = member;
            if (_xamlScanner.IsCtorForcingMember)
            {
                _context.CurrentForcedToUseConstructor = true;
            }
            XamlType memberXamlType = member.Type;
            _context.CurrentInContainerDirective = member.IsDirective && (memberXamlType != null && (memberXamlType.IsCollection || memberXamlType.IsDictionary));

            var startMember = new XamlNode(XamlNodeType.StartMember, member);
            return startMember;
        }

        private XamlNode Logic_EndMember()
        {
            _context.CurrentMember = null;
            _context.CurrentPreviousChildType = null;
            _context.CurrentInContainerDirective = false;
            return new XamlNode(XamlNodeType.EndMember);
        }

        private XamlNode Logic_StartContentProperty(XamlMember property)
        {
            if (property == null)
            {
                property = XamlLanguage.UnknownContent;
            }
            _context.CurrentMember = property;
            var startProperty = new XamlNode(XamlNodeType.StartMember, property);
            // SetLineInfo(startProperty);  // No line number info for objects from members.
            return startProperty;
        }

        private XamlNode Logic_StartInitProperty(XamlType ownerType)
        {
            var initProperty = XamlLanguage.Initialization;

            _context.CurrentMember = initProperty;

            var startProperty = new XamlNode(XamlNodeType.StartMember, initProperty);
            // SetLineInfo(startProperty);  // No line number info for implicit properties.
            return startProperty;
        }

        private string Logic_ApplyFinalTextTrimming(XamlText text)
        {
            ScannerNodeType nextNodeType = _xamlScanner.PeekNodeType;
            string trimmed = text.Text;

            if (!text.IsSpacePreserved)
            {
                // Trim trailing space from text if it is the last bit of content.
                // End Element and End Property Element and Start of PE all end "content"
                if (nextNodeType == ScannerNodeType.ENDTAG || nextNodeType == ScannerNodeType.PROPERTYELEMENT || nextNodeType == ScannerNodeType.EMPTYPROPERTYELEMENT)
                {
                    trimmed = XamlText.TrimTrailingWhitespace(trimmed);
                }

                // If the text is the first thing, ie. before any element
                // OR the previous element was "TrimSurroundingWhitespace"
                // then trim leading Whitespace.
                XamlType previousObject = _context.CurrentPreviousChildType;
                if (previousObject == null || previousObject.TrimSurroundingWhitespace)
                {
                    trimmed = XamlText.TrimLeadingWhitespace(trimmed);
                }

                // If next element is "TrimSurroundingWhitespace", trim trailing WS.
                if (nextNodeType == ScannerNodeType.ELEMENT
                    || nextNodeType == ScannerNodeType.EMPTYELEMENT)
                {
                    XamlType nextXamlType = _xamlScanner.PeekType;
                    if (nextXamlType.TrimSurroundingWhitespace)
                    {
                        trimmed = XamlText.TrimTrailingWhitespace(trimmed);
                    }
                }
            }
            return trimmed;
        }

        private XamlNode Logic_StartGetObjectFromMember(XamlType realType)
        {
            _context.PushScope();
            _context.CurrentType = realType;
            _context.CurrentInCollectionFromMember = true;

            var startObj = new XamlNode(XamlNodeType.GetObject);
            return startObj;
        }

        private XamlNode Logic_StartItemsProperty(XamlType collectionType)
        {
            _context.CurrentMember = XamlLanguage.Items;
            _context.CurrentInContainerDirective = true;

            var startProperty = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items);
            //SetLineInfo(startProperty);  // No line number info for implicit properties.
            return startProperty;
        }

        #region Optimizations
        private readonly XamlTypeName arrayType = new XamlTypeName(@"http://schemas.microsoft.com/winfx/2006/xaml", "Array");
        private XamlType _arrayExtensionType = null;
        private XamlType ArrayExtensionType
        {
            get
            {
                if (_arrayExtensionType == null)
                {
                    _arrayExtensionType = _context.GetXamlType(arrayType);
                }
                return _arrayExtensionType;
            }
        }

        private XamlMember _arrayTypeMember = null;
        private XamlMember ArrayTypeMember
        {
            get
            {
                if (_arrayTypeMember == null)
                {
                    _arrayTypeMember = _context.GetXamlProperty(ArrayExtensionType, @"Type", null);
                }
                return _arrayTypeMember;
            }
        }

        private XamlMember _itemsTypeMember = null;
        private XamlMember ItemsTypeMember
        {
            get
            {
                if (_itemsTypeMember == null)
                {
                    _itemsTypeMember = _context.GetXamlProperty(ArrayExtensionType, @"Items", null);
                }
                return _itemsTypeMember;
            }
        }
        #endregion

        private IEnumerable<XamlNode> LogicStream_CheckForStartGetCollectionFromMember()
        {
            XamlType currentType = _context.CurrentType;
            XamlMember currentProperty = _context.CurrentMember;

            XamlType propertyType = currentProperty.Type;

            XamlType valueElementType = (_xamlScanner.NodeType == ScannerNodeType.TEXT)
                            ? XamlLanguage.String
                            : _xamlScanner.Type;

            if (propertyType.IsArray && _xamlScanner.Type != ArrayExtensionType)
            {
                IEnumerable<NamespaceDeclaration> newNamespaces = null;
                XamlTypeName typeName = new XamlTypeName(propertyType.ItemType);
                INamespacePrefixLookup prefixResolver = new NamespacePrefixLookup(out newNamespaces, _context.FindNamespaceByPrefix);
                string typeNameString = typeName.ToString(prefixResolver);    // SideEffects!!! prefixResolver will populate newNamespaces

                foreach (NamespaceDeclaration nsDecl in newNamespaces)
                {
                    yield return new XamlNode(XamlNodeType.NamespaceDeclaration, nsDecl);
                }
                yield return Logic_StartObject(ArrayExtensionType, null);
                _context.CurrentInImplicitArray = true;
                yield return Logic_StartMember(ArrayTypeMember);

                yield return new XamlNode(XamlNodeType.Value, typeNameString);
                yield return Logic_EndMember();
                yield return Logic_EndOfAttributes();
                yield return Logic_StartMember(ItemsTypeMember);

                currentType = _context.CurrentType;
                currentProperty = _context.CurrentMember;
                propertyType = currentProperty.Type;
            }

            // Now Consider inserting special preamble to "Get" the collection:
            //   . GO
            //   . . SM _items
            if (!currentProperty.IsDirective && (propertyType.IsCollection || propertyType.IsDictionary))
            {
                bool emitPreamble = false;

                // If the collection property is Readonly then "Get" the collection.
                if (currentProperty.IsReadOnly || !_context.CurrentMemberIsWriteVisible())
                {
                    emitPreamble = true;
                }
                // If the collection is R/W and there is a type converter and we have Text
                // use the type converter rather than the GO; SM _items;
                else if (propertyType.TypeConverter != null && !currentProperty.IsReadOnly
                    && _xamlScanner.NodeType == ScannerNodeType.TEXT)
                {
                    emitPreamble = false;
                }
                // Or if the Value (this is the first value in the collection)
                // isn't assignable to the Collection then "Get" the collection.
                else if (valueElementType == null || !valueElementType.CanAssignTo(propertyType))
                {
                    if (valueElementType != null)
                    {
                        // Unless: the Value is a Markup extension, in which case it is
                        // assumed that the ProvideValue() type will be AssignableFrom
                        // or If the next object has an x:Key in which case it must be
                        // a dictionary entry.
                        // so Don't "Get" the collection.
                        if (!valueElementType.IsMarkupExtension || _xamlScanner.HasKeyAttribute)
                        {
                            emitPreamble = true;
                        }
                        // Except: the Array Extension can never return a dictionary
                        // so for Array Extension do "Get" the collection.
                        // Note Array Extension would be suitable for List Collections
                        // Note: a fully validating parser should look at MarkupExtensionReturnType
                        // for this choice, there might be other MarkupExtensions that fit this.
                        else if (valueElementType == XamlLanguage.Array)
                        {
                            emitPreamble = true;
                        }
                    }
                }
                if (emitPreamble)
                {
                    yield return Logic_StartGetObjectFromMember(propertyType);
                    yield return Logic_StartItemsProperty(propertyType);
                }
            }
        }

        /// <summary>
        /// Returns true if whitespace is discardable at this phase in
        /// the parsing.  Here we discard whitespace between property elements
        /// but keep it between object elements for collections that accept it.
        /// Discarding trailing whitespace in collections cannot be decided here.
        /// [see: Logic_ReadAhead_ApplyFinalTextTrimming
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool Logic_IsDiscardableWhitespace(XamlText text)
        {
            if (!text.IsWhiteSpaceOnly)
            {
                return false;
            }
            else
            {
                // Force unknown members to behave as whitespace significant collections in order to preserve as much information as possible.
                if (_context.CurrentMember != null && _context.CurrentMember.IsUnknown)
                {
                    return false;
                }
                else if (_context.CurrentInContainerDirective)
                {
                    XamlType collectionType = _context.CurrentMember == XamlLanguage.Items ? _context.CurrentType : _context.CurrentMember.Type;
                    if (collectionType.IsWhitespaceSignificantCollection)
                    {
                        return false;
                    }
                }
                else
                {
                    // Whitespace, by itself does not start content.  Eg. The WS between
                    // the Start Element and the first Property Element is not content, but
                    // the WS between the Start Element and the first child Element (ie. other content)
                    // is content.
                    XamlMember prop = _context.CurrentMember;
                    if (_xamlScanner.PeekNodeType == ScannerNodeType.ELEMENT)
                    {
                        if (prop == null)
                        {
                            prop = _context.CurrentType.ContentProperty;
                        }
                        if (prop != null && prop.Type != null && prop.Type.IsWhitespaceSignificantCollection)
                        {
                            return false;
                        }
                        if (prop == null && _context.CurrentType.IsWhitespaceSignificantCollection)
                        {
                            return false;
                        }
                    }
                    // Whitespace can also start content if space is preserved and it's at the end of an element and...
                    else if (text.IsSpacePreserved && _xamlScanner.PeekNodeType == ScannerNodeType.ENDTAG)
                    {
                        // ...it's by itself in a PE with no other children
                        if (prop != null)
                        {
                            if (_context.CurrentPreviousChildType == null)
                            {
                                return false;
                            }
                        }
                        // ...it's in an element with a string content property
                        else if (_context.CurrentType.ContentProperty != null)
                        {
                            prop = _context.CurrentType.ContentProperty;
                            // For backcompat we need to support CPs of type object here.
                            // Theoretically we'd also like to support all type-convertible CPs.
                            // However, for non-string CPs, 3.0 only surfaced whitespace as text if
                            // the CP hadn't already been set. For string, it surfaced it in all cases.
                            // So to avoid a breaking change, we only surface string right now.
                            if (prop.Type == XamlLanguage.String)
                            {
                                return false;
                            }
                            if (prop.Type.IsWhitespaceSignificantCollection)
                            {
                                return false;
                            }
                        }
                        // ...it's in a type-convertible element
                        else if (_context.CurrentType.TypeConverter != null && !_context.CurrentForcedToUseConstructor)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static bool CanAcceptString(XamlMember property)
        {
            if (property == null)
            {
                return false;
            }
            if (property.TypeConverter == BuiltInValueConverter.String)
            {
                return true;
            }
            if (property.TypeConverter == BuiltInValueConverter.Object)
            {
                return true;
            }
            XamlType propertyType = property.Type;
            if (propertyType.IsCollection)
            {
                foreach (XamlType allowedType in propertyType.AllowedContentTypes)
                {
                    if (allowedType == XamlLanguage.String || allowedType == XamlLanguage.Object)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    [Serializable]  // FxCop advised this be Serializable.
    class XamlUnexpectedParseException : XamlParseException
    {
        public XamlUnexpectedParseException() { }

        // FxCop says this is never called
        //public XamlUnexpectedParseException(string message)
        //    : base(message) { }

        // FxCop says this is never called
        //public XamlUnexpectedParseException(string message, Exception innerException)
        //    : base(message, innerException) { }

        public XamlUnexpectedParseException(XamlScanner xamlScanner, ScannerNodeType nodetype, string parseRule)
            : base(xamlScanner, SR.Get(SRID.UnexpectedNodeType, nodetype.ToString(), parseRule)) { }

        protected XamlUnexpectedParseException(System.Runtime.Serialization.SerializationInfo info,
                                               System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
