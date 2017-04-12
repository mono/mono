//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
// PERF, Microsoft, Microsoft: Make LookupNamespace do something smarter when lots of names
// PERF, Microsoft, Microsoft: Make Attribute lookup smarter when lots of attributes
namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    // Large numbers of attributes
    // Use delimiter on node for figuring out Element/EndElement?
    // Optimize StringHandle.CompareTo
    // Fix FixXmlAttribute - Temporary until we actually write an XmlAttribute node

    abstract class XmlBaseReader : XmlDictionaryReader
    {
        XmlBufferReader bufferReader;
        XmlNode node;
        NamespaceManager nsMgr;
        XmlElementNode[] elementNodes;
        XmlAttributeNode[] attributeNodes;
        XmlAtomicTextNode atomicTextNode;
        int depth;
        int attributeCount;
        int attributeStart;    // Starting index for searching
        XmlDictionaryReaderQuotas quotas;

        XmlNameTable nameTable;
        XmlDeclarationNode declarationNode;
        XmlComplexTextNode complexTextNode;
        XmlWhitespaceTextNode whitespaceTextNode;
        XmlCDataNode cdataNode;
        XmlCommentNode commentNode;
        XmlElementNode rootElementNode;
        int attributeIndex;    // Index for iteration
        char[] chars;
        string prefix;
        string localName;
        string ns;
        string value;
        int trailCharCount;
        int trailByteCount;
        char[] trailChars;
        byte[] trailBytes;
        bool rootElement;
        bool readingElement;
        XmlSigningNodeWriter signingWriter;
        bool signing;
        AttributeSorter attributeSorter;

        static XmlInitialNode initialNode = new XmlInitialNode(XmlBufferReader.Empty);
        static XmlEndOfFileNode endOfFileNode = new XmlEndOfFileNode(XmlBufferReader.Empty);
        static XmlClosedNode closedNode = new XmlClosedNode(XmlBufferReader.Empty);
        static BinHexEncoding binhexEncoding;
        static Base64Encoding base64Encoding;

        const string xmlns = "xmlns";
        const string xml = "xml";
        const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";
        const string xmlNamespace = "http://www.w3.org/XML/1998/namespace";

        protected XmlBaseReader()
        {
            this.bufferReader = new XmlBufferReader(this);
            this.nsMgr = new NamespaceManager(bufferReader);
            this.quotas = new XmlDictionaryReaderQuotas();
            this.rootElementNode = new XmlElementNode(bufferReader);
            this.atomicTextNode = new XmlAtomicTextNode(bufferReader);
            this.node = closedNode;
        }

        static BinHexEncoding BinHexEncoding
        {
            get
            {
                if (binhexEncoding == null)
                    binhexEncoding = new BinHexEncoding();
                return binhexEncoding;
            }
        }

        static Base64Encoding Base64Encoding
        {
            get
            {
                if (base64Encoding == null)
                    base64Encoding = new Base64Encoding();
                return base64Encoding;
            }
        }

        protected XmlBufferReader BufferReader
        {
            get
            {
                return bufferReader;
            }
        }

        public override XmlDictionaryReaderQuotas Quotas
        {
            get
            {
                return quotas;
            }
        }

        protected XmlNode Node
        {
            get
            {
                return node;
            }
        }

        protected void MoveToNode(XmlNode node)
        {
            this.node = node;
            this.ns = null;
            this.localName = null;
            this.prefix = null;
            this.value = null;
        }

        protected void MoveToInitial(XmlDictionaryReaderQuotas quotas)
        {
            if (quotas == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("quotas");

            quotas.InternalCopyTo(this.quotas);
            this.quotas.MakeReadOnly();
            this.nsMgr.Clear();
            this.depth = 0;
            this.attributeCount = 0;
            this.attributeStart = -1;
            this.attributeIndex = -1;
            this.rootElement = false;
            this.readingElement = false;
            this.signing = false;
            MoveToNode(initialNode);
        }

        protected XmlDeclarationNode MoveToDeclaration()
        {
            if (attributeCount < 1)
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlDeclMissingVersion)));

            if (attributeCount > 3)
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlMalformedDecl)));

            // version
            if (!CheckDeclAttribute(0, "version", "1.0", false, SR.XmlInvalidVersion))
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlDeclMissingVersion)));

            // encoding/standalone
            // We only validate that they are the only attributes that exist.  Encoding can have any value.
            if (attributeCount > 1)
            {
                if (CheckDeclAttribute(1, "encoding", null, true, SR.XmlInvalidEncoding))
                {
                    if (attributeCount == 3 && !CheckStandalone(2))
                        XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlMalformedDecl)));
                }
                else if (!CheckStandalone(1) || attributeCount > 2)
                {
                    XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlMalformedDecl)));
                }
            }

            if (declarationNode == null)
            {
                declarationNode = new XmlDeclarationNode(bufferReader);
            }
            MoveToNode(declarationNode);
            return declarationNode;
        }

        bool CheckStandalone(int attr)
        {
            XmlAttributeNode node = attributeNodes[attr];
            if (!node.Prefix.IsEmpty)
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlMalformedDecl)));

            if (node.LocalName != "standalone")
                return false;

            if (!node.Value.Equals2("yes", false) && !node.Value.Equals2("no", false))
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlInvalidStandalone)));

            return true;
        }

        bool CheckDeclAttribute(int index, string localName, string value, bool checkLower, string valueSR)
        {
            XmlAttributeNode node = attributeNodes[index];
            if (!node.Prefix.IsEmpty)
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlMalformedDecl)));

            if (node.LocalName != localName)
                return false;

            if (value != null && !node.Value.Equals2(value, checkLower))
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(valueSR)));

            return true;
        }

        protected XmlCommentNode MoveToComment()
        {
            if (commentNode == null)
            {
                commentNode = new XmlCommentNode(bufferReader);
            }
            MoveToNode(commentNode);
            return commentNode;
        }

        protected XmlCDataNode MoveToCData()
        {
            if (cdataNode == null)
            {
                this.cdataNode = new XmlCDataNode(bufferReader);
            }
            MoveToNode(cdataNode);
            return cdataNode;
        }

        protected XmlAtomicTextNode MoveToAtomicText()
        {
            XmlAtomicTextNode textNode = this.atomicTextNode;
            MoveToNode(textNode);
            return textNode;
        }

        protected XmlComplexTextNode MoveToComplexText()
        {
            if (complexTextNode == null)
            {
                complexTextNode = new XmlComplexTextNode(bufferReader);
            }
            MoveToNode(complexTextNode);
            return complexTextNode;
        }

        protected XmlTextNode MoveToWhitespaceText()
        {
            if (whitespaceTextNode == null)
            {
                whitespaceTextNode = new XmlWhitespaceTextNode(bufferReader);
            }
            if (nsMgr.XmlSpace == XmlSpace.Preserve)
                whitespaceTextNode.NodeType = XmlNodeType.SignificantWhitespace;
            else
                whitespaceTextNode.NodeType = XmlNodeType.Whitespace;
            MoveToNode(whitespaceTextNode);
            return whitespaceTextNode;
        }

        protected XmlElementNode ElementNode
        {
            get
            {
                if (depth == 0)
                    return rootElementNode;
                else
                    return elementNodes[depth];
            }
        }

        protected void MoveToEndElement()
        {
            if (depth == 0)
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            XmlElementNode elementNode = elementNodes[depth];
            XmlEndElementNode endElementNode = elementNode.EndElement;
            endElementNode.Namespace = elementNode.Namespace;
            MoveToNode(endElementNode);
        }

        protected void MoveToEndOfFile()
        {
            if (depth != 0)
                XmlExceptionHelper.ThrowUnexpectedEndOfFile(this);
            MoveToNode(endOfFileNode);
        }

        protected XmlElementNode EnterScope()
        {
            if (depth == 0)
            {
                if (rootElement)
                    XmlExceptionHelper.ThrowMultipleRootElements(this);
                rootElement = true;
            }
            nsMgr.EnterScope();
            depth++;
            if (depth > quotas.MaxDepth)
                XmlExceptionHelper.ThrowMaxDepthExceeded(this, quotas.MaxDepth);
            if (elementNodes == null)
            {
                elementNodes = new XmlElementNode[4];
            }
            else if (elementNodes.Length == depth)
            {
                XmlElementNode[] newElementNodes = new XmlElementNode[depth * 2];
                Array.Copy(elementNodes, newElementNodes, depth);
                elementNodes = newElementNodes;
            }
            XmlElementNode elementNode = elementNodes[depth];
            if (elementNode == null)
            {
                elementNode = new XmlElementNode(bufferReader);
                elementNodes[depth] = elementNode;
            }
            this.attributeCount = 0;
            this.attributeStart = -1;
            this.attributeIndex = -1;
            MoveToNode(elementNode);
            return elementNode;
        }

        protected void ExitScope()
        {
            if (depth == 0)
                XmlExceptionHelper.ThrowUnexpectedEndElement(this);
            depth--;
            nsMgr.ExitScope();
        }

        XmlAttributeNode AddAttribute(QNameType qnameType, bool isAtomicValue)
        {
            int attributeIndex = this.attributeCount;
            if (attributeNodes == null)
            {
                attributeNodes = new XmlAttributeNode[4];
            }
            else if (attributeNodes.Length == attributeIndex)
            {
                XmlAttributeNode[] newAttributeNodes = new XmlAttributeNode[attributeIndex * 2];
                Array.Copy(attributeNodes, newAttributeNodes, attributeIndex);
                attributeNodes = newAttributeNodes;
            }
            XmlAttributeNode attributeNode = attributeNodes[attributeIndex];
            if (attributeNode == null)
            {
                attributeNode = new XmlAttributeNode(bufferReader);
                attributeNodes[attributeIndex] = attributeNode;
            }
            attributeNode.QNameType = qnameType;
            attributeNode.IsAtomicValue = isAtomicValue;
            attributeNode.AttributeText.QNameType = qnameType;
            attributeNode.AttributeText.IsAtomicValue = isAtomicValue;
            this.attributeCount++;
            return attributeNode;
        }

        protected Namespace AddNamespace()
        {
            return nsMgr.AddNamespace();
        }

        protected XmlAttributeNode AddAttribute()
        {
            return AddAttribute(QNameType.Normal, true);
        }

        protected XmlAttributeNode AddXmlAttribute()
        {
            return AddAttribute(QNameType.Normal, true);
        }

        protected XmlAttributeNode AddXmlnsAttribute(Namespace ns)
        {
            if (!ns.Prefix.IsEmpty && ns.Uri.IsEmpty)
                XmlExceptionHelper.ThrowEmptyNamespace(this);

            // Some prefixes can only be bound to a particular namespace
            if (ns.Prefix.IsXml && ns.Uri != xmlNamespace)
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlSpecificBindingPrefix, "xml", xmlNamespace)));
            }
            else if (ns.Prefix.IsXmlns && ns.Uri != xmlnsNamespace)
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString(SR.XmlSpecificBindingPrefix, "xmlns", xmlnsNamespace)));
            }

            nsMgr.Register(ns);
            XmlAttributeNode attributeNode = AddAttribute(QNameType.Xmlns, false);
            attributeNode.Namespace = ns;
            attributeNode.AttributeText.Namespace = ns;
            return attributeNode;
        }

        protected void FixXmlAttribute(XmlAttributeNode attributeNode)
        {
            if (attributeNode.Prefix == xml)
            {
                if (attributeNode.LocalName == "lang")
                {
                    nsMgr.AddLangAttribute(attributeNode.Value.GetString());
                }
                else if (attributeNode.LocalName == "space")
                {
                    string value = attributeNode.Value.GetString();
                    if (value == "preserve")
                    {
                        nsMgr.AddSpaceAttribute(XmlSpace.Preserve);
                    }
                    else if (value == "default")
                    {
                        nsMgr.AddSpaceAttribute(XmlSpace.Default);
                    }
                }
            }
        }

        protected bool OutsideRootElement
        {
            get
            {
                return depth == 0;
            }
        }

        public override bool CanReadBinaryContent
        {
            get { return true; }
        }

        public override bool CanReadValueChunk
        {
            get { return true; }
        }

        public override string BaseURI
        {
            get
            {
                return string.Empty;
            }
        }

        public override bool HasValue
        {
            get
            {
                return node.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public override string this[int index]
        {
            get
            {
                return GetAttribute(index);
            }
        }

        public override string this[string name]
        {
            get
            {
                return GetAttribute(name);
            }
        }

        public override string this[string localName, string namespaceUri]
        {
            get
            {
                return GetAttribute(localName, namespaceUri);
            }
        }

        public override int AttributeCount
        {
            get
            {
                if (node.CanGetAttribute)
                    return attributeCount;
                return 0;
            }
        }

        public override void Close()
        {
            MoveToNode(closedNode);
            nameTable = null;
            if (attributeNodes != null && attributeNodes.Length > 16)
                attributeNodes = null;
            if (elementNodes != null && elementNodes.Length > 16)
                elementNodes = null;
            nsMgr.Close();
            bufferReader.Close();
            if (signingWriter != null)
                signingWriter.Close();
            if (attributeSorter != null)
                attributeSorter.Close();
        }

        public sealed override int Depth
        {
            get
            {
                // Internally, depth is simply measured by Element/EndElement.  What XmlReader exposes is a little different
                // so we need to account for this with some minor adjustments.

                // We increment depth immediately when we see an element, but XmlTextReader waits until its consumed
                // We decrement depth when its consumed, but XmlTextReader decrements depth immediately

                // If we're on Attribute Text (i.e. ReadAttributeValue), then its considered a level deeper
                return this.depth + node.DepthDelta;
            }
        }

        public override bool EOF
        {
            get
            {
                return node.ReadState == ReadState.EndOfFile;
            }
        }

        XmlAttributeNode GetAttributeNode(int index)
        {
            if (!node.CanGetAttribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", SR.GetString(SR.XmlElementAttributes)));
            if (index < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeNonNegative)));
            if (index >= attributeCount)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", SR.GetString(SR.OffsetExceedsBufferSize, attributeCount)));
            return attributeNodes[index];
        }

        XmlAttributeNode GetAttributeNode(string name)
        {
            if (name == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            if (!node.CanGetAttribute)
                return null;
            int index = name.IndexOf(':');
            string prefix;
            string localName;
            if (index == -1)
            {
                if (name == xmlns)
                {
                    prefix = xmlns;
                    localName = string.Empty;
                }
                else
                {
                    prefix = string.Empty;
                    localName = name;
                }
            }
            else
            {
                // If this function becomes a performance issue because of the allocated strings then we can
                // make a version of Equals that takes an offset and count into the string.
                prefix = name.Substring(0, index);
                localName = name.Substring(index + 1);
            }
            XmlAttributeNode[] attributeNodes = this.attributeNodes;
            int attributeCount = this.attributeCount;
            int attributeIndex = this.attributeStart;
            for (int i = 0; i < attributeCount; i++)
            {
                if (++attributeIndex >= attributeCount)
                {
                    attributeIndex = 0;
                }
                XmlAttributeNode attributeNode = attributeNodes[attributeIndex];
                if (attributeNode.IsPrefixAndLocalName(prefix, localName))
                {
                    this.attributeStart = attributeIndex;
                    return attributeNode;
                }
            }
            return null;
        }

        XmlAttributeNode GetAttributeNode(string localName, string namespaceUri)
        {
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            if (namespaceUri == null)
                namespaceUri = string.Empty;
            if (!node.CanGetAttribute)
                return null;
            XmlAttributeNode[] attributeNodes = this.attributeNodes;
            int attributeCount = this.attributeCount;
            int attributeIndex = this.attributeStart;
            for (int i = 0; i < attributeCount; i++)
            {
                if (++attributeIndex >= attributeCount)
                {
                    attributeIndex = 0;
                }
                XmlAttributeNode attributeNode = attributeNodes[attributeIndex];
                if (attributeNode.IsLocalNameAndNamespaceUri(localName, namespaceUri))
                {
                    this.attributeStart = attributeIndex;
                    return attributeNode;
                }
            }
            return null;
        }

        XmlAttributeNode GetAttributeNode(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            if (namespaceUri == null)
                namespaceUri = XmlDictionaryString.Empty;
            if (!node.CanGetAttribute)
                return null;
            XmlAttributeNode[] attributeNodes = this.attributeNodes;
            int attributeCount = this.attributeCount;
            int attributeIndex = this.attributeStart;
            for (int i = 0; i < attributeCount; i++)
            {
                if (++attributeIndex >= attributeCount)
                {
                    attributeIndex = 0;
                }
                XmlAttributeNode attributeNode = attributeNodes[attributeIndex];
                if (attributeNode.IsLocalNameAndNamespaceUri(localName, namespaceUri))
                {
                    this.attributeStart = attributeIndex;
                    return attributeNode;
                }
            }
            return null;
        }

        public override string GetAttribute(int index)
        {
            return GetAttributeNode(index).ValueAsString;
        }

        public override string GetAttribute(string name)
        {
            XmlAttributeNode attributeNode = GetAttributeNode(name);
            if (attributeNode == null)
                return null;
            return attributeNode.ValueAsString;
        }

        public override string GetAttribute(string localName, string namespaceUri)
        {
            XmlAttributeNode attributeNode = GetAttributeNode(localName, namespaceUri);
            if (attributeNode == null)
                return null;
            return attributeNode.ValueAsString;
        }

        public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            XmlAttributeNode attributeNode = GetAttributeNode(localName, namespaceUri);
            if (attributeNode == null)
                return null;
            return attributeNode.ValueAsString;
        }

        public sealed override bool IsEmptyElement
        {
            get
            {
                return node.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                if (this.localName == null)
                {
                    this.localName = GetLocalName(true);
                }

                return this.localName;
            }
        }

        public override string LookupNamespace(string prefix)
        {
            Namespace ns = nsMgr.LookupNamespace(prefix);
            if (ns != null)
                return ns.Uri.GetString(NameTable);
            if (prefix == xmlns)
                return xmlnsNamespace;
            return null;
        }

        protected Namespace LookupNamespace(PrefixHandleType prefix)
        {
            Namespace ns = nsMgr.LookupNamespace(prefix);
            if (ns == null)
                XmlExceptionHelper.ThrowUndefinedPrefix(this, PrefixHandle.GetString(prefix));
            return ns;
        }

        protected Namespace LookupNamespace(PrefixHandle prefix)
        {
            Namespace ns = nsMgr.LookupNamespace(prefix);
            if (ns == null)
                XmlExceptionHelper.ThrowUndefinedPrefix(this, prefix.GetString());
            return ns;
        }

        protected void ProcessAttributes()
        {
            if (attributeCount > 0)
            {
                ProcessAttributes(attributeNodes, attributeCount);
            }
        }

        void ProcessAttributes(XmlAttributeNode[] attributeNodes, int attributeCount)
        {
            for (int i = 0; i < attributeCount; i++)
            {
                XmlAttributeNode attributeNode = attributeNodes[i];
                if (attributeNode.QNameType == QNameType.Normal)
                {
                    PrefixHandle prefix = attributeNode.Prefix;
                    if (!prefix.IsEmpty)
                    {
                        attributeNode.Namespace = LookupNamespace(prefix);
                    }
                    else
                    {
                        attributeNode.Namespace = NamespaceManager.EmptyNamespace;
                    }
                    attributeNode.AttributeText.Namespace = attributeNode.Namespace;
                }
            }

            if (attributeCount > 1)
            {
                if (attributeCount < 12)
                {
                    // For small numbers of attributes, a naive n * (n - 1) / 2 comparisons to check for uniqueness is faster
                    for (int i = 0; i < attributeCount - 1; i++)
                    {
                        XmlAttributeNode attributeNode1 = attributeNodes[i];
                        QNameType qnameType = attributeNode1.QNameType;
                        if (qnameType == QNameType.Normal)
                        {
                            for (int j = i + 1; j < attributeCount; j++)
                            {
                                XmlAttributeNode attributeNode2 = attributeNodes[j];
                                if (attributeNode2.QNameType == QNameType.Normal && attributeNode1.LocalName == attributeNode2.LocalName && attributeNode1.Namespace.Uri == attributeNode2.Namespace.Uri)
                                {
                                    XmlExceptionHelper.ThrowDuplicateAttribute(this, attributeNode1.Prefix.GetString(), attributeNode2.Prefix.GetString(), attributeNode1.LocalName.GetString(), attributeNode1.Namespace.Uri.GetString());
                                }
                            }
                        }
                        else
                        {
                            Fx.Assert(qnameType == QNameType.Xmlns, "");
                            for (int j = i + 1; j < attributeCount; j++)
                            {
                                XmlAttributeNode attributeNode2 = attributeNodes[j];
                                if (attributeNode2.QNameType == QNameType.Xmlns && attributeNode1.Namespace.Prefix == attributeNode2.Namespace.Prefix)
                                    XmlExceptionHelper.ThrowDuplicateAttribute(this, xmlns, xmlns, attributeNode1.Namespace.Prefix.GetString(), xmlnsNamespace);
                            }
                        }
                    }
                }
                else
                {
                    CheckAttributes(attributeNodes, attributeCount);
                }
            }
        }

        void CheckAttributes(XmlAttributeNode[] attributeNodes, int attributeCount)
        {
            // For large numbers of attributes, sorting the attributes (n * lg(n)) is faster
            if (attributeSorter == null)
                attributeSorter = new AttributeSorter();

            if (!attributeSorter.Sort(attributeNodes, attributeCount))
            {
                int attribute1, attribute2;
                attributeSorter.GetIndeces(out attribute1, out attribute2);
                if (attributeNodes[attribute1].QNameType == QNameType.Xmlns)
                    XmlExceptionHelper.ThrowDuplicateXmlnsAttribute(this, attributeNodes[attribute1].Namespace.Prefix.GetString(), xmlnsNamespace);
                else
                    XmlExceptionHelper.ThrowDuplicateAttribute(this, attributeNodes[attribute1].Prefix.GetString(), attributeNodes[attribute2].Prefix.GetString(), attributeNodes[attribute1].LocalName.GetString(), attributeNodes[attribute1].Namespace.Uri.GetString());
            }
        }

        public override void MoveToAttribute(int index)
        {
            MoveToNode(GetAttributeNode(index));
            this.attributeIndex = index;
        }

        public override bool MoveToAttribute(string name)
        {
            XmlNode attributeNode = GetAttributeNode(name);
            if (attributeNode == null)
                return false;
            MoveToNode(attributeNode);
            this.attributeIndex = this.attributeStart;
            return true;
        }

        public override bool MoveToAttribute(string localName, string namespaceUri)
        {
            XmlNode attributeNode = GetAttributeNode(localName, namespaceUri);
            if (attributeNode == null)
                return false;
            MoveToNode(attributeNode);
            this.attributeIndex = this.attributeStart;
            return true;
        }

        public override bool MoveToElement()
        {
            if (!node.CanMoveToElement)
                return false;
            if (depth == 0)
                MoveToDeclaration();
            else
                MoveToNode(elementNodes[depth]);
            this.attributeIndex = -1;
            return true;
        }

        public override XmlNodeType MoveToContent()
        {
            do
            {
                if (node.HasContent)
                {
                    if (node.NodeType != XmlNodeType.Text && node.NodeType != XmlNodeType.CDATA)
                        break;

                    if (trailByteCount > 0)
                    {
                        break;
                    }

                    if (this.value == null)
                    {
                        if (!node.Value.IsWhitespace())
                            break;
                    }
                    else
                    {
                        if (!XmlConverter.IsWhitespace(this.value))
                            break;
                    }
                }
                else
                {
                    if (node.NodeType == XmlNodeType.Attribute)
                    {
                        MoveToElement();
                        break;
                    }
                }
            }
            while (Read());
            return node.NodeType;
        }

        public override bool MoveToFirstAttribute()
        {
            if (!node.CanGetAttribute || attributeCount == 0)
                return false;
            MoveToNode(GetAttributeNode(0));
            this.attributeIndex = 0;
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if (!node.CanGetAttribute)
                return false;
            int attributeIndex = this.attributeIndex + 1;
            if (attributeIndex >= attributeCount)
                return false;
            MoveToNode(GetAttributeNode(attributeIndex));
            this.attributeIndex = attributeIndex;
            return true;
        }

        public override string NamespaceURI
        {
            get
            {
                if (this.ns == null)
                {
                    this.ns = GetNamespaceUri(true);
                }
                return this.ns;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                if (nameTable == null)
                {
                    nameTable = new QuotaNameTable(this, quotas.MaxNameTableCharCount);
                    nameTable.Add(xml);
                    nameTable.Add(xmlns);
                    nameTable.Add(xmlnsNamespace);
                    nameTable.Add(xmlNamespace);
                    for (PrefixHandleType i = PrefixHandleType.A; i <= PrefixHandleType.Z; i++)
                    {
                        nameTable.Add(PrefixHandle.GetString(i));
                    }
                }

                return nameTable;
            }
        }

        public sealed override XmlNodeType NodeType
        {
            get
            {
                return node.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                if (prefix == null)
                {
                    QNameType qnameType = node.QNameType;
                    if (qnameType == QNameType.Normal)
                    {
                        prefix = node.Prefix.GetString(NameTable);
                    }
                    else if (qnameType == QNameType.Xmlns)
                    {
                        if (node.Namespace.Prefix.IsEmpty)
                            prefix = string.Empty;
                        else
                            prefix = xmlns;
                    }
                    else
                    {
                        prefix = xml;
                    }
                }

                return prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return node.QuoteChar;
            }
        }

        string GetLocalName(bool enforceAtomization)
        {
            if (this.localName != null)
            {
                return this.localName;
            }

            QNameType qnameType = node.QNameType;
            if (qnameType == QNameType.Normal)
            {
                if (enforceAtomization || nameTable != null)
                {
                    return node.LocalName.GetString(NameTable);
                }
                else
                {
                    return node.LocalName.GetString();
                }
            }
            else
            {
                Fx.Assert(qnameType == QNameType.Xmlns, "");
                if (node.Namespace.Prefix.IsEmpty)
                {
                    return xmlns;
                }
                else
                {
                    if (enforceAtomization || nameTable != null)
                    {
                        return node.Namespace.Prefix.GetString(NameTable);
                    }
                    else
                    {
                        return node.Namespace.Prefix.GetString();
                    }
                }
            }
        }

        string GetNamespaceUri(bool enforceAtomization)
        {
            if (this.ns != null)
            {
                return this.ns;
            }

            QNameType qnameType = node.QNameType;
            if (qnameType == QNameType.Normal)
            {
                if (enforceAtomization || nameTable != null)
                {
                    return node.Namespace.Uri.GetString(NameTable);
                }
                else
                {
                    return node.Namespace.Uri.GetString();
                }
            }
            else
            {
                Fx.Assert(qnameType == QNameType.Xmlns, "");
                return xmlnsNamespace;
            }
        }

        // In a XmlReader names of elements and attributes should be returned atomized 
        // (see XmlReader.NameTable on MSDN for more information)
        // to allow a comparison based on object instance comparison instead of string comparison
        // This method allows to receive the localName and namespaceUri without enforcing atomization
        // to avoid the cost of atomization if this is not necessary.
        public override void GetNonAtomizedNames(out string localName, out string namespaceUri)
        {
            localName = GetLocalName(false);
            namespaceUri = GetNamespaceUri(false);
        }

        public override bool IsLocalName(string localName)
        {
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            return node.IsLocalName(localName);
        }

        public override bool IsLocalName(XmlDictionaryString localName)
        {
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            return node.IsLocalName(localName);
        }

        public override bool IsNamespaceUri(string namespaceUri)
        {
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            return node.IsNamespaceUri(namespaceUri);
        }

        public override bool IsNamespaceUri(XmlDictionaryString namespaceUri)
        {
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            return node.IsNamespaceUri(namespaceUri);
        }

        public override sealed bool IsStartElement()
        {
            XmlNodeType nodeType = node.NodeType;
            if (nodeType == XmlNodeType.Element)
                return true;
            if (nodeType == XmlNodeType.EndElement)
                return false;
            if (nodeType == XmlNodeType.None)
            {
                Read();
                if (node.NodeType == XmlNodeType.Element)
                    return true;
            }
            return (MoveToContent() == XmlNodeType.Element);
        }

        public override bool IsStartElement(string name)
        {
            if (name == null)
                return false;
            int index = name.IndexOf(':');
            string prefix;
            string localName;
            if (index == -1)
            {
                prefix = string.Empty;
                localName = name;
            }
            else
            {
                prefix = name.Substring(0, index);
                localName = name.Substring(index + 1);
            }
            return (node.NodeType == XmlNodeType.Element || IsStartElement()) && node.Prefix == prefix && node.LocalName == localName;
        }

        public override bool IsStartElement(string localName, string namespaceUri)
        {
            if (localName == null)
                return false;
            if (namespaceUri == null)
                return false;
            return (node.NodeType == XmlNodeType.Element || IsStartElement()) && node.LocalName == localName && node.IsNamespaceUri(namespaceUri);
        }

        public override bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            return (node.NodeType == XmlNodeType.Element || IsStartElement()) && node.LocalName == localName && node.IsNamespaceUri(namespaceUri);
        }

        public override int IndexOfLocalName(string[] localNames, string namespaceUri)
        {
            if (localNames == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            QNameType qnameType = node.QNameType;
            if (node.IsNamespaceUri(namespaceUri))
            {
                if (qnameType == QNameType.Normal)
                {
                    StringHandle localName = node.LocalName;
                    for (int i = 0; i < localNames.Length; i++)
                    {
                        string value = localNames[i];
                        if (value == null)
                            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
                        if (localName == value)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    PrefixHandle prefix = node.Namespace.Prefix;
                    for (int i = 0; i < localNames.Length; i++)
                    {
                        string value = localNames[i];
                        if (value == null)
                            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
                        if (prefix == value)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public override int IndexOfLocalName(XmlDictionaryString[] localNames, XmlDictionaryString namespaceUri)
        {
            if (localNames == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            QNameType qnameType = node.QNameType;
            if (node.IsNamespaceUri(namespaceUri))
            {
                if (qnameType == QNameType.Normal)
                {
                    StringHandle localName = node.LocalName;
                    for (int i = 0; i < localNames.Length; i++)
                    {
                        XmlDictionaryString value = localNames[i];
                        if (value == null)
                            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
                        if (localName == value)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    PrefixHandle prefix = node.Namespace.Prefix;
                    for (int i = 0; i < localNames.Length; i++)
                    {
                        XmlDictionaryString value = localNames[i];
                        if (value == null)
                            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
                        if (prefix == value)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public override int ReadValueChunk(char[] chars, int offset, int count)
        {
            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > chars.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, chars.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > chars.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - offset)));
            int actual;

            if (this.value == null)
            {
                if (node.QNameType == QNameType.Normal)
                {
                    if (node.Value.TryReadChars(chars, offset, count, out actual))
                        return actual;
                }
            }

            string value = this.Value;
            actual = Math.Min(count, value.Length);
            value.CopyTo(0, chars, offset, actual);
            this.value = value.Substring(actual);
            return actual;
        }

        public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > buffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > buffer.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));
            if (count == 0)
                return 0;
            int actual;
            if (this.value == null)
            {
                if (trailByteCount == 0 && trailCharCount == 0)
                {
                    if (node.QNameType == QNameType.Normal)
                    {
                        if (node.Value.TryReadBase64(buffer, offset, count, out actual))
                            return actual;
                    }
                }
            }
            return ReadBytes(Base64Encoding, 3, 4, buffer, offset, Math.Min(count, 512), false);
        }

        public override string ReadElementContentAsString()
        {
            if (node.NodeType != XmlNodeType.Element)
                MoveToStartElement();

            if (node.IsEmptyElement)
            {
                Read();
                return string.Empty;
            }
            else
            {
                Read();
                string s = ReadContentAsString();
                ReadEndElement();
                return s;
            }
        }

        public override string ReadElementString()
        {
            MoveToStartElement();
            if (IsEmptyElement)
            {
                Read();
                return string.Empty;
            }
            else
            {
                Read();
                string s = ReadString();
                ReadEndElement();
                return s;
            }
        }

        public override string ReadElementString(string name)
        {
            MoveToStartElement(name);
            return ReadElementString();
        }

        public override string ReadElementString(string localName, string namespaceUri)
        {
            MoveToStartElement(localName, namespaceUri);
            return ReadElementString();
        }

        public override void ReadStartElement()
        {
            if (node.NodeType != XmlNodeType.Element)
                MoveToStartElement();
            Read();
        }

        public override void ReadStartElement(string name)
        {
            MoveToStartElement(name);
            Read();
        }

        public override void ReadStartElement(string localName, string namespaceUri)
        {
            MoveToStartElement(localName, namespaceUri);
            Read();
        }

        public override void ReadEndElement()
        {
            if (node.NodeType != XmlNodeType.EndElement && MoveToContent() != XmlNodeType.EndElement)
            {
                int nodeDepth = node.NodeType == XmlNodeType.Element ? this.depth - 1 : this.depth;
                if (nodeDepth == 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlEndElementNoOpenNodes)));
                // If depth is non-zero, then the document isn't what was expected
                XmlElementNode elementNode = elementNodes[nodeDepth];
                XmlExceptionHelper.ThrowEndElementExpected(this, elementNode.LocalName.GetString(), elementNode.Namespace.Uri.GetString());
            }
            Read();
        }

        public override bool ReadAttributeValue()
        {
            XmlAttributeTextNode attributeTextNode = node.AttributeText;
            if (attributeTextNode == null)
                return false;
            MoveToNode(attributeTextNode);
            return true;
        }

        public override ReadState ReadState
        {
            get
            {
                return node.ReadState;
            }
        }

        void SkipValue(XmlNode node)
        {
            if (node.SkipValue)
                Read();
        }

        public override bool TryGetBase64ContentLength(out int length)
        {
            if (trailByteCount == 0 && trailCharCount == 0 && this.value == null)
            {
                XmlNode node = this.Node;
                if (node.IsAtomicValue)
                    return node.Value.TryGetByteArrayLength(out length);
            }
            return base.TryGetBase64ContentLength(out length);
        }

        public override byte[] ReadContentAsBase64()
        {
            if (trailByteCount == 0 && trailCharCount == 0 && this.value == null)
            {
                XmlNode node = this.Node;
                if (node.IsAtomicValue)
                {
                    byte[] value = node.Value.ToByteArray();
                    if (value.Length > quotas.MaxArrayLength)
                        XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, quotas.MaxArrayLength);
                    SkipValue(node);
                    return value;
                }
            }

            if (!bufferReader.IsStreamed)
                return ReadContentAsBase64(quotas.MaxArrayLength, bufferReader.Buffer.Length);

            return ReadContentAsBase64(quotas.MaxArrayLength, XmlDictionaryReader.MaxInitialArrayLength);  // Initial count will get ignored
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
        {
            if (!readingElement)
            {
                if (IsEmptyElement)
                {
                    Read();
                    return 0;
                }

                ReadStartElement();
                readingElement = true;
            }

            int i = ReadContentAsBase64(buffer, offset, count);

            if (i == 0)
            {
                ReadEndElement();
                readingElement = false;
            }

            return i;
        }

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > buffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > buffer.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));
            if (count == 0)
                return 0;
            int actual;
            if (trailByteCount == 0 && trailCharCount == 0 && this.value == null)
            {
                if (node.QNameType == QNameType.Normal)
                {
                    while (node.NodeType != XmlNodeType.Comment && node.Value.TryReadBase64(buffer, offset, count, out actual))
                    {
                        if (actual != 0)
                            return actual;
                        Read();
                    }
                }
            }
            XmlNodeType nodeType = node.NodeType;
            if (nodeType == XmlNodeType.Element || nodeType == XmlNodeType.EndElement)
                return 0;
            return ReadBytes(Base64Encoding, 3, 4, buffer, offset, Math.Min(count, 512), true);
        }

        public override byte[] ReadContentAsBinHex()
        {
            return ReadContentAsBinHex(quotas.MaxArrayLength);
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
        {
            if (!readingElement)
            {
                if (IsEmptyElement)
                {
                    Read();
                    return 0;
                }

                ReadStartElement();
                readingElement = true;
            }

            int i = ReadContentAsBinHex(buffer, offset, count);

            if (i == 0)
            {
                ReadEndElement();
                readingElement = false;
            }

            return i;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > buffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > buffer.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));
            if (count == 0)
                return 0;
            return ReadBytes(BinHexEncoding, 1, 2, buffer, offset, Math.Min(count, 512), true);
        }

        int ReadBytes(Encoding encoding, int byteBlock, int charBlock, byte[] buffer, int offset, int byteCount, bool readContent)
        {
            // If there are any trailing buffer return them.
            if (trailByteCount > 0)
            {
                int actual = Math.Min(trailByteCount, byteCount);
                Array.Copy(trailBytes, 0, buffer, offset, actual);
                trailByteCount -= actual;
                Array.Copy(trailBytes, actual, trailBytes, 0, trailByteCount);
                return actual;
            }
            XmlNodeType nodeType = node.NodeType;
            if (nodeType == XmlNodeType.Element || nodeType == XmlNodeType.EndElement)
                return 0;
            int maxCharCount;
            if (byteCount < byteBlock)
            {
                // Convert at least charBlock chars
                maxCharCount = charBlock;
            }
            else
            {
                // Round down to the nearest multiple of charBlock
                maxCharCount = byteCount / byteBlock * charBlock;
            }
            char[] chars = GetCharBuffer(maxCharCount);
            int charCount = 0;
            while (true)
            {
                // If we didn't align on the boundary, then we might have some remaining characters
                if (trailCharCount > 0)
                {
                    Array.Copy(trailChars, 0, chars, charCount, trailCharCount);
                    charCount += trailCharCount;
                    trailCharCount = 0;
                }
                // Read until we at least get a charBlock
                while (charCount < charBlock)
                {
                    int actualCharCount;
                    if (readContent)
                    {
                        actualCharCount = ReadContentAsChars(chars, charCount, maxCharCount - charCount);
                        // When deserializing base64 content which contains new line chars (CR, LF) chars from ReadObject, the reader reads in chunks of base64 content, LF char, base64 content, LF char and so on
                        // Relying on encoding.GetBytes' exception to handle LF char would result in performance degradation so skipping LF char here
                        if (actualCharCount == 1 && chars[charCount] == '\n')
                            continue;
                    }
                    else
                        actualCharCount = ReadValueChunk(chars, charCount, maxCharCount - charCount);
                    if (actualCharCount == 0)
                        break;
                    charCount += actualCharCount;
                }
                // Trim so its a multiple of charBlock
                if (charCount >= charBlock)
                {
                    trailCharCount = (charCount % charBlock);
                    if (trailCharCount > 0)
                    {
                        if (trailChars == null)
                            trailChars = new char[4];
                        charCount = charCount - trailCharCount;
                        Array.Copy(chars, charCount, trailChars, 0, trailCharCount);
                    }
                }
                try
                {
                    if (byteCount < byteBlock)
                    {
                        if (trailBytes == null)
                            trailBytes = new byte[3];
                        trailByteCount = encoding.GetBytes(chars, 0, charCount, trailBytes, 0);
                        int actual = Math.Min(trailByteCount, byteCount);
                        Array.Copy(trailBytes, 0, buffer, offset, actual);
                        trailByteCount -= actual;
                        Array.Copy(trailBytes, actual, trailBytes, 0, trailByteCount);
                        return actual;
                    }
                    else
                    {
                        // charCount is a multiple of charBlock and we have enough room to convert everything
                        return encoding.GetBytes(chars, 0, charCount, buffer, offset);
                    }
                }
                catch (FormatException exception)
                {
                    // Something was wrong with the format, see if we can strip the spaces
                    int i = 0;
                    int j = 0;
                    while (true)
                    {
                        while (j < charCount && XmlConverter.IsWhitespace(chars[j]))
                            j++;
                        if (j == charCount)
                            break;
                        chars[i++] = chars[j++];
                    }
                    // No spaces, so don't try again
                    if (i == charCount)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(exception.Message, exception.InnerException));
                    charCount = i;
                }
            }
        }

        public override string ReadContentAsString()
        {
            string value;
            XmlNode node = this.Node;
            if (node.IsAtomicValue)
            {
                if (this.value != null)
                {
                    value = this.value;
                    if (node.AttributeText == null)
                        this.value = string.Empty;
                }
                else
                {
                    value = node.Value.GetString();
                    SkipValue(node);
                    if (value.Length > quotas.MaxStringContentLength)
                        XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, quotas.MaxStringContentLength);
                }
                return value;
            }
            return base.ReadContentAsString(quotas.MaxStringContentLength);
        }

        public override Boolean ReadContentAsBoolean()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                bool value = node.Value.ToBoolean();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToBoolean(ReadContentAsString());
        }

        public override Int64 ReadContentAsLong()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                Int64 value = node.Value.ToLong();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToInt64(ReadContentAsString());
        }

        public override Int32 ReadContentAsInt()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                Int32 value = node.Value.ToInt();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToInt32(ReadContentAsString());
        }

        public override DateTime ReadContentAsDateTime()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                DateTime value = node.Value.ToDateTime();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToDateTime(ReadContentAsString());
        }

        public override Double ReadContentAsDouble()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                double value = node.Value.ToDouble();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToDouble(ReadContentAsString());
        }

        public override Single ReadContentAsFloat()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                float value = node.Value.ToSingle();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToSingle(ReadContentAsString());
        }

        public override Decimal ReadContentAsDecimal()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                decimal value = node.Value.ToDecimal();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToDecimal(ReadContentAsString());
        }

        public override UniqueId ReadContentAsUniqueId()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                UniqueId value = node.Value.ToUniqueId();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToUniqueId(ReadContentAsString());
        }

        public override TimeSpan ReadContentAsTimeSpan()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                TimeSpan value = node.Value.ToTimeSpan();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToTimeSpan(ReadContentAsString());
        }

        public override Guid ReadContentAsGuid()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                Guid value = node.Value.ToGuid();
                SkipValue(node);
                return value;
            }
            return XmlConverter.ToGuid(ReadContentAsString());
        }

        public override object ReadContentAsObject()
        {
            XmlNode node = this.Node;
            if (this.value == null && node.IsAtomicValue)
            {
                object obj = node.Value.ToObject();
                SkipValue(node);
                return obj;
            }
            return ReadContentAsString();
        }

        public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
        {
            if (type == typeof(ulong))
            {
                if (this.value == null && node.IsAtomicValue)
                {
                    ulong value = node.Value.ToULong();
                    SkipValue(node);
                    return value;
                }
                else
                {
                    return XmlConverter.ToUInt64(ReadContentAsString());
                }
            }
            else if (type == typeof(bool))
                return ReadContentAsBoolean();
            else if (type == typeof(int))
                return ReadContentAsInt();
            else if (type == typeof(long))
                return ReadContentAsLong();
            else if (type == typeof(float))
                return ReadContentAsFloat();
            else if (type == typeof(double))
                return ReadContentAsDouble();
            else if (type == typeof(decimal))
                return ReadContentAsDecimal();
            else if (type == typeof(DateTime))
                return ReadContentAsDateTime();
            else if (type == typeof(UniqueId))
                return ReadContentAsUniqueId();
            else if (type == typeof(Guid))
                return ReadContentAsGuid();
            else if (type == typeof(TimeSpan))
                return ReadContentAsTimeSpan();
            else if (type == typeof(object))
                return ReadContentAsObject();
            else
                return base.ReadContentAs(type, namespaceResolver);
        }

        public override void ResolveEntity()
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidOperation)));
        }

        public override void Skip()
        {
            if (node.ReadState != ReadState.Interactive)
                return;
            if ((node.NodeType == XmlNodeType.Element || MoveToElement()) && !IsEmptyElement)
            {
                int depth = Depth;
                while (Read() && depth < Depth)
                {
                    // Nothing, just read on
                }
                // consume end tag
                if (node.NodeType == XmlNodeType.EndElement)
                    Read();
            }
            else
            {
                Read();
            }
        }

        public override string Value
        {
            get
            {
                if (value == null)
                {
                    value = node.ValueAsString;
                }

                return value;
            }
        }

        public override Type ValueType
        {
            get
            {
                if (this.value == null && node.QNameType == QNameType.Normal)
                {
                    Type type = node.Value.ToType();
                    if (node.IsAtomicValue)
                        return type;
                    if (type == typeof(byte[]))
                        return type;
                }
                return typeof(string);
            }
        }

        public override string XmlLang
        {
            get
            {
                return nsMgr.XmlLang;
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                return nsMgr.XmlSpace;
            }
        }

        public override bool TryGetLocalNameAsDictionaryString(out XmlDictionaryString localName)
        {
            return node.TryGetLocalNameAsDictionaryString(out localName);
        }

        public override bool TryGetNamespaceUriAsDictionaryString(out XmlDictionaryString localName)
        {
            return node.TryGetNamespaceUriAsDictionaryString(out localName);
        }

        public override bool TryGetValueAsDictionaryString(out XmlDictionaryString value)
        {
            return node.TryGetValueAsDictionaryString(out value);
        }

        public override Int16[] ReadInt16Array(string localName, string namespaceUri)
        {
            return Int16ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override Int16[] ReadInt16Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int16ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override Int32[] ReadInt32Array(string localName, string namespaceUri)
        {
            return Int32ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override Int32[] ReadInt32Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int32ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override Int64[] ReadInt64Array(string localName, string namespaceUri)
        {
            return Int64ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override Int64[] ReadInt64Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int64ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override float[] ReadSingleArray(string localName, string namespaceUri)
        {
            return SingleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override float[] ReadSingleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return SingleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override double[] ReadDoubleArray(string localName, string namespaceUri)
        {
            return DoubleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override double[] ReadDoubleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DoubleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override decimal[] ReadDecimalArray(string localName, string namespaceUri)
        {
            return DecimalArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override decimal[] ReadDecimalArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DecimalArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override DateTime[] ReadDateTimeArray(string localName, string namespaceUri)
        {
            return DateTimeArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override DateTime[] ReadDateTimeArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DateTimeArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override Guid[] ReadGuidArray(string localName, string namespaceUri)
        {
            return GuidArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override Guid[] ReadGuidArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return GuidArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override TimeSpan[] ReadTimeSpanArray(string localName, string namespaceUri)
        {
            return TimeSpanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public override TimeSpan[] ReadTimeSpanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return TimeSpanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, quotas.MaxArrayLength);
        }

        public string GetOpenElements()
        {
            string s = string.Empty;
            for (int i = depth; i > 0; i--)
            {
                string localName = elementNodes[i].LocalName.GetString();
                if (i != depth)
                    s += ", ";
                s += localName;
            }
            return s;
        }

        char[] GetCharBuffer(int count)
        {
            if (count > 1024)
                return new char[count];

            if (chars == null || chars.Length < count)
                chars = new char[count];

            return chars;
        }

        void SignStartElement(XmlSigningNodeWriter writer)
        {
            int prefixOffset, prefixLength;
            byte[] prefixBuffer = node.Prefix.GetString(out prefixOffset, out prefixLength);
            int localNameOffset, localNameLength;
            byte[] localNameBuffer = node.LocalName.GetString(out localNameOffset, out localNameLength);
            writer.WriteStartElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
        }

        void SignAttribute(XmlSigningNodeWriter writer, XmlAttributeNode attributeNode)
        {
            QNameType qnameType = attributeNode.QNameType;
            if (qnameType == QNameType.Normal)
            {
                int prefixOffset, prefixLength;
                byte[] prefixBuffer = attributeNode.Prefix.GetString(out prefixOffset, out prefixLength);
                int localNameOffset, localNameLength;
                byte[] localNameBuffer = attributeNode.LocalName.GetString(out localNameOffset, out localNameLength);
                writer.WriteStartAttribute(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
                attributeNode.Value.Sign(writer);
                writer.WriteEndAttribute();
            }
            else
            {
                Fx.Assert(qnameType == QNameType.Xmlns, "");
                int prefixOffset, prefixLength;
                byte[] prefixBuffer = attributeNode.Namespace.Prefix.GetString(out prefixOffset, out prefixLength);
                int nsOffset, nsLength;
                byte[] nsBuffer = attributeNode.Namespace.Uri.GetString(out nsOffset, out nsLength);
                writer.WriteXmlnsAttribute(prefixBuffer, prefixOffset, prefixLength, nsBuffer, nsOffset, nsLength);
            }
        }

        void SignEndElement(XmlSigningNodeWriter writer)
        {
            int prefixOffset, prefixLength;
            byte[] prefixBuffer = node.Prefix.GetString(out prefixOffset, out prefixLength);
            int localNameOffset, localNameLength;
            byte[] localNameBuffer = node.LocalName.GetString(out localNameOffset, out localNameLength);
            writer.WriteEndElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
        }

        void SignNode(XmlSigningNodeWriter writer)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.None:
                    break;
                case XmlNodeType.Element:
                    SignStartElement(writer);
                    for (int i = 0; i < attributeCount; i++)
                        SignAttribute(writer, attributeNodes[i]);
                    writer.WriteEndStartElement(node.IsEmptyElement);
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.CDATA:
                    node.Value.Sign(writer);
                    break;
                case XmlNodeType.XmlDeclaration:
                    writer.WriteDeclaration();
                    break;
                case XmlNodeType.Comment:
                    writer.WriteComment(node.Value.GetString());
                    break;
                case XmlNodeType.EndElement:
                    SignEndElement(writer);
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
        }

        public override bool CanCanonicalize
        {
            get
            {
                return true;
            }
        }

        protected bool Signing
        {
            get
            {
                return signing;
            }
        }

        protected void SignNode()
        {
            if (signing)
            {
                SignNode(signingWriter);
            }
        }

        public override void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            if (signing)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlCanonicalizationStarted)));

            if (signingWriter == null)
                signingWriter = CreateSigningNodeWriter();

            signingWriter.SetOutput(XmlNodeWriter.Null, stream, includeComments, inclusivePrefixes);
            nsMgr.Sign(signingWriter);
            signing = true;
        }

        public override void EndCanonicalization()
        {
            if (!signing)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlCanonicalizationNotStarted)));

            signingWriter.Flush();
            signingWriter.Close();
            signing = false;
        }

        protected abstract XmlSigningNodeWriter CreateSigningNodeWriter();

        protected enum QNameType
        {
            Normal,
            Xmlns,
        }

        protected class XmlNode
        {
            XmlNodeType nodeType;
            PrefixHandle prefix;
            StringHandle localName;
            ValueHandle value;
            Namespace ns;
            bool hasValue;
            bool canGetAttribute;
            bool canMoveToElement;
            ReadState readState;
            XmlAttributeTextNode attributeTextNode;
            bool exitScope;
            int depthDelta;
            bool isAtomicValue;
            bool skipValue;
            QNameType qnameType;
            bool hasContent;
            bool isEmptyElement;
            char quoteChar;

            protected enum XmlNodeFlags
            {
                None = 0x00,
                CanGetAttribute = 0x01,
                CanMoveToElement = 0x02,
                HasValue = 0x04,
                AtomicValue = 0x08,
                SkipValue = 0x10,
                HasContent = 0x20
            }

            protected XmlNode(XmlNodeType nodeType,
                              PrefixHandle prefix,
                              StringHandle localName,
                              ValueHandle value,
                              XmlNodeFlags nodeFlags,
                              ReadState readState,
                              XmlAttributeTextNode attributeTextNode,
                              int depthDelta)
            {
                this.nodeType = nodeType;
                this.prefix = prefix;
                this.localName = localName;
                this.value = value;
                this.ns = NamespaceManager.EmptyNamespace;
                this.hasValue = ((nodeFlags & XmlNodeFlags.HasValue) != 0);
                this.canGetAttribute = ((nodeFlags & XmlNodeFlags.CanGetAttribute) != 0);
                this.canMoveToElement = ((nodeFlags & XmlNodeFlags.CanMoveToElement) != 0);
                this.isAtomicValue = ((nodeFlags & XmlNodeFlags.AtomicValue) != 0);
                this.skipValue = ((nodeFlags & XmlNodeFlags.SkipValue) != 0);
                this.hasContent = ((nodeFlags & XmlNodeFlags.HasContent) != 0);
                this.readState = readState;
                this.attributeTextNode = attributeTextNode;
                this.exitScope = (nodeType == XmlNodeType.EndElement);
                this.depthDelta = depthDelta;
                this.isEmptyElement = false;
                this.quoteChar = '"';
                this.qnameType = QNameType.Normal;
            }

            // Most nodes are read-only and fixed for the particular node type, but a few need to be tweaked
            // QNameType needs to get set for all nodes with a qname (Element/Attribute)
            // NodeType gets set for WhiteSpace vs. SignificantWhitespace
            // ExitScope/IsEmptyElement is only updated by text for empty elements
            // QuoteChar is only updated by text for attributes
            // IsAtomicValue is set to false for XmlnsAttributes so we don't have to check QNameType

            public bool HasValue { get { return hasValue; } }
            public ReadState ReadState { get { return readState; } }
            public StringHandle LocalName { get { Fx.Assert(qnameType != QNameType.Xmlns, ""); return localName; } }
            public PrefixHandle Prefix { get { Fx.Assert(qnameType != QNameType.Xmlns, ""); return prefix; } }
            public bool CanGetAttribute { get { return canGetAttribute; } }
            public bool CanMoveToElement { get { return canMoveToElement; } }
            public XmlAttributeTextNode AttributeText { get { return attributeTextNode; } }
            public bool SkipValue { get { return skipValue; } }
            public ValueHandle Value { get { Fx.Assert(qnameType != QNameType.Xmlns, ""); return value; } }
            public int DepthDelta { get { return depthDelta; } }
            public bool HasContent { get { return hasContent; } }

            public XmlNodeType NodeType
            {
                get
                {
                    return nodeType;
                }
                set
                {
                    nodeType = value;
                }
            }

            public QNameType QNameType
            {
                get
                {
                    return qnameType;
                }
                set
                {
                    qnameType = value;
                }
            }

            public Namespace Namespace
            {
                get
                {
                    return ns;
                }
                set
                {
                    ns = value;
                }
            }

            public bool IsAtomicValue
            {
                get
                {
                    return isAtomicValue;
                }
                set
                {
                    isAtomicValue = value;
                }
            }

            public bool ExitScope
            {
                get
                {
                    return exitScope;
                }
                set
                {
                    exitScope = value;
                }
            }

            public bool IsEmptyElement
            {
                get
                {
                    return isEmptyElement;
                }
                set
                {
                    isEmptyElement = value;
                }
            }

            public char QuoteChar
            {
                get
                {
                    return quoteChar;
                }
                set
                {
                    quoteChar = value;
                }
            }

            public bool IsLocalName(string localName)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.LocalName == localName;
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    return this.Namespace.Prefix == localName;
                }
            }

            public bool IsLocalName(XmlDictionaryString localName)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.LocalName == localName;
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    return this.Namespace.Prefix == localName;
                }
            }

            public bool IsNamespaceUri(string ns)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.Namespace.IsUri(ns);
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    return ns == xmlnsNamespace;
                }
            }

            public bool IsNamespaceUri(XmlDictionaryString ns)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.Namespace.IsUri(ns);
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    return ns.Value == xmlnsNamespace;
                }
            }

            public bool IsLocalNameAndNamespaceUri(string localName, string ns)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.LocalName == localName && this.Namespace.IsUri(ns);
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    return this.Namespace.Prefix == localName && ns == xmlnsNamespace;
                }
            }

            public bool IsLocalNameAndNamespaceUri(XmlDictionaryString localName, XmlDictionaryString ns)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.LocalName == localName && this.Namespace.IsUri(ns);
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    return this.Namespace.Prefix == localName && ns.Value == xmlnsNamespace;
                }
            }

            public bool IsPrefixAndLocalName(string prefix, string localName)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.Prefix == prefix && this.LocalName == localName;
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    return prefix == xmlns && this.Namespace.Prefix == localName;
                }
            }

            public bool TryGetLocalNameAsDictionaryString(out XmlDictionaryString localName)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.LocalName.TryGetDictionaryString(out localName);
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    localName = null;
                    return false;
                }
            }

            public bool TryGetNamespaceUriAsDictionaryString(out XmlDictionaryString ns)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.Namespace.Uri.TryGetDictionaryString(out ns);
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    ns = null;
                    return false;
                }
            }

            public bool TryGetValueAsDictionaryString(out XmlDictionaryString value)
            {
                if (qnameType == QNameType.Normal)
                {
                    return this.Value.TryGetDictionaryString(out value);
                }
                else
                {
                    Fx.Assert(qnameType == QNameType.Xmlns, "");
                    value = null;
                    return false;
                }
            }

            public string ValueAsString
            {
                get
                {
                    if (qnameType == QNameType.Normal)
                    {
                        return Value.GetString();
                    }
                    else
                    {
                        Fx.Assert(qnameType == QNameType.Xmlns, "");
                        return Namespace.Uri.GetString();
                    }
                }
            }
        }

        protected class XmlElementNode : XmlNode
        {
            XmlEndElementNode endElementNode;
            int bufferOffset;

            public XmlElementNode(XmlBufferReader bufferReader)
                : this(new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader))
            {
            }

            XmlElementNode(PrefixHandle prefix, StringHandle localName, ValueHandle value)
                : base(XmlNodeType.Element,
                       prefix,
                       localName,
                       value,
                       XmlNodeFlags.CanGetAttribute | XmlNodeFlags.HasContent,
                       ReadState.Interactive,
                       null,
                       -1)
            {
                this.endElementNode = new XmlEndElementNode(prefix, localName, value);
            }

            public XmlEndElementNode EndElement
            {
                get
                {
                    return endElementNode;
                }
            }

            public int BufferOffset
            {
                get
                {
                    return bufferOffset;
                }
                set
                {
                    bufferOffset = value;
                }
            }

            public int NameOffset;
            public int NameLength;
        }

        protected class XmlAttributeNode : XmlNode
        {
            public XmlAttributeNode(XmlBufferReader bufferReader)
                : this(new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader))
            {
            }

            XmlAttributeNode(PrefixHandle prefix, StringHandle localName, ValueHandle value)
                : base(XmlNodeType.Attribute,
                       prefix,
                       localName,
                       value,
                       XmlNodeFlags.CanGetAttribute | XmlNodeFlags.CanMoveToElement | XmlNodeFlags.HasValue | XmlNodeFlags.AtomicValue,
                       ReadState.Interactive,
                       new XmlAttributeTextNode(prefix, localName, value),
                       0)
            {
            }
        }

        protected class XmlEndElementNode : XmlNode
        {
            public XmlEndElementNode(PrefixHandle prefix, StringHandle localName, ValueHandle value)
                : base(XmlNodeType.EndElement,
                       prefix,
                       localName,
                       value,
                       XmlNodeFlags.HasContent,
                       ReadState.Interactive,
                       null,
                       -1)
            {
            }
        }

        protected class XmlTextNode : XmlNode
        {
            protected XmlTextNode(XmlNodeType nodeType,
                              PrefixHandle prefix,
                              StringHandle localName,
                              ValueHandle value,
                              XmlNodeFlags nodeFlags,
                              ReadState readState,
                              XmlAttributeTextNode attributeTextNode,
                              int depthDelta)
                :
                base(nodeType, prefix, localName, value, nodeFlags, readState, attributeTextNode, depthDelta)
            {
            }
        }

        protected class XmlAtomicTextNode : XmlTextNode
        {
            public XmlAtomicTextNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.Text,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.HasValue | XmlNodeFlags.AtomicValue | XmlNodeFlags.SkipValue | XmlNodeFlags.HasContent,
                       ReadState.Interactive,
                       null,
                       0)
            {
            }
        }

        protected class XmlComplexTextNode : XmlTextNode
        {
            public XmlComplexTextNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.Text,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.HasValue | XmlNodeFlags.HasContent,
                       ReadState.Interactive,
                       null,
                       0)
            {
            }
        }

        protected class XmlWhitespaceTextNode : XmlTextNode
        {
            public XmlWhitespaceTextNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.Whitespace,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.HasValue,
                       ReadState.Interactive,
                       null,
                       0)
            {
            }
        }

        protected class XmlCDataNode : XmlTextNode
        {
            public XmlCDataNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.CDATA,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.HasValue | XmlNodeFlags.HasContent,
                       ReadState.Interactive,
                       null,
                       0)
            {
            }
        }

        protected class XmlAttributeTextNode : XmlTextNode
        {
            public XmlAttributeTextNode(PrefixHandle prefix, StringHandle localName, ValueHandle value)
                : base(XmlNodeType.Text,
                       prefix,
                       localName,
                       value,
                       XmlNodeFlags.HasValue | XmlNodeFlags.CanGetAttribute | XmlNodeFlags.CanMoveToElement | XmlNodeFlags.AtomicValue | XmlNodeFlags.HasContent,
                       ReadState.Interactive,
                       null,
                       1)
            {
            }
        }

        protected class XmlInitialNode : XmlNode
        {
            public XmlInitialNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.None,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.None,
                       ReadState.Initial,
                       null,
                       0)
            {
            }
        }

        protected class XmlDeclarationNode : XmlNode
        {
            public XmlDeclarationNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.XmlDeclaration,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.CanGetAttribute,
                       ReadState.Interactive,
                       null,
                       0)
            {
            }
        }

        protected class XmlCommentNode : XmlNode
        {
            public XmlCommentNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.Comment,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.HasValue,
                       ReadState.Interactive,
                       null,
                       0)
            {
            }
        }

        protected class XmlEndOfFileNode : XmlNode
        {
            public XmlEndOfFileNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.None,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.None,
                       ReadState.EndOfFile,
                       null,
                       0)
            {
            }
        }

        protected class XmlClosedNode : XmlNode
        {
            public XmlClosedNode(XmlBufferReader bufferReader)
                : base(XmlNodeType.None,
                       new PrefixHandle(bufferReader),
                       new StringHandle(bufferReader),
                       new ValueHandle(bufferReader),
                       XmlNodeFlags.None,
                       ReadState.Closed,
                       null,
                       0)
            {
            }
        }

        class AttributeSorter : IComparer
        {
            object[] indeces;
            XmlAttributeNode[] attributeNodes;
            int attributeCount;
            int attributeIndex1;
            int attributeIndex2;

            public bool Sort(XmlAttributeNode[] attributeNodes, int attributeCount)
            {
                this.attributeIndex1 = -1;
                this.attributeIndex2 = -1;
                this.attributeNodes = attributeNodes;
                this.attributeCount = attributeCount;
                bool sorted = Sort();
                this.attributeNodes = null;
                this.attributeCount = 0;
                return sorted;
            }

            public void GetIndeces(out int attributeIndex1, out int attributeIndex2)
            {
                attributeIndex1 = this.attributeIndex1;
                attributeIndex2 = this.attributeIndex2;
            }

            public void Close()
            {
                if (indeces != null && indeces.Length > 32)
                {
                    indeces = null;
                }
            }

            bool Sort()
            {
                // Optimistically use the last sort order and check to see if that works.  This helps the case
                // where elements with large numbers of attributes are repeated.
                if (indeces != null && indeces.Length == attributeCount && IsSorted())
                    return true;

                object[] newIndeces = new object[attributeCount];
                for (int i = 0; i < newIndeces.Length; i++)
                    newIndeces[i] = i;
                this.indeces = newIndeces;
                Array.Sort(indeces, 0, attributeCount, this);
                return IsSorted();
            }

            bool IsSorted()
            {
                for (int i = 0; i < indeces.Length - 1; i++)
                {
                    if (Compare(indeces[i], indeces[i + 1]) >= 0)
                    {
                        attributeIndex1 = (int)indeces[i];
                        attributeIndex2 = (int)indeces[i + 1];
                        return false;
                    }
                }
                return true;
            }

            public int Compare(object obj1, object obj2)
            {
                int index1 = (int)obj1;
                int index2 = (int)obj2;
                XmlAttributeNode attribute1 = attributeNodes[index1];
                XmlAttributeNode attribute2 = attributeNodes[index2];

                int i = CompareQNameType(attribute1.QNameType, attribute2.QNameType);
                if (i == 0)
                {
                    QNameType qnameType = attribute1.QNameType;
                    if (qnameType == QNameType.Normal)
                    {
                        i = attribute1.LocalName.CompareTo(attribute2.LocalName);
                        if (i == 0)
                        {
                            i = attribute1.Namespace.Uri.CompareTo(attribute2.Namespace.Uri);
                        }
                    }
                    else
                    {
                        Fx.Assert(qnameType == QNameType.Xmlns, "");
                        i = attribute1.Namespace.Prefix.CompareTo(attribute2.Namespace.Prefix);
                    }
                }

                return i;
            }

            public int CompareQNameType(QNameType type1, QNameType type2)
            {
                return (int)type1 - (int)type2;
            }
        }

        class NamespaceManager
        {
            XmlBufferReader bufferReader;
            Namespace[] namespaces;
            int nsCount;
            int depth;
            Namespace[] shortPrefixUri;
            static Namespace emptyNamespace = new Namespace(XmlBufferReader.Empty);
            static Namespace xmlNamespace;
            XmlAttribute[] attributes;
            int attributeCount;
            XmlSpace space;
            string lang;

            public NamespaceManager(XmlBufferReader bufferReader)
            {
                this.bufferReader = bufferReader;
                this.shortPrefixUri = new Namespace[(int)PrefixHandleType.Max];
                this.shortPrefixUri[(int)PrefixHandleType.Empty] = emptyNamespace;
                this.namespaces = null;
                this.nsCount = 0;
                this.attributes = null;
                this.attributeCount = 0;
                this.space = XmlSpace.None;
                this.lang = string.Empty;
                this.depth = 0;
            }

            public void Close()
            {
                if (namespaces != null && namespaces.Length > 32)
                    namespaces = null;
                if (attributes != null && attributes.Length > 4)
                    attributes = null;
                lang = string.Empty;
            }

            static public Namespace XmlNamespace
            {
                get
                {
                    if (xmlNamespace == null)
                    {
                        byte[] xmlBuffer = 
                            {
                                (byte)'x', (byte)'m', (byte)'l',
                                (byte)'h', (byte)'t', (byte)'t', (byte)'p', (byte)':', (byte)'/', (byte)'/', (byte)'w',
                                (byte)'w', (byte)'w', (byte)'.', (byte)'w', (byte)'3', (byte)'.', (byte)'o', (byte)'r',
                                (byte)'g', (byte)'/', (byte)'X', (byte)'M', (byte)'L', (byte)'/', (byte)'1', (byte)'9',
                                (byte)'9', (byte)'8', (byte)'/', (byte)'n', (byte)'a', (byte)'m', (byte)'e', (byte)'s',
                                (byte)'p', (byte)'a', (byte)'c', (byte)'e'
                            };
                        Namespace nameSpace = new Namespace(new XmlBufferReader(xmlBuffer));
                        nameSpace.Prefix.SetValue(0, 3);
                        nameSpace.Uri.SetValue(3, xmlBuffer.Length - 3);
                        xmlNamespace = nameSpace;
                    }
                    return xmlNamespace;
                }
            }

            static public Namespace EmptyNamespace
            {
                get
                {
                    return emptyNamespace;
                }
            }

            public string XmlLang
            {
                get
                {
                    return lang;
                }
            }

            public XmlSpace XmlSpace
            {
                get
                {
                    return space;
                }
            }

            public void Clear()
            {
                if (nsCount != 0)
                {
                    if (shortPrefixUri != null)
                    {
                        for (int i = 0; i < shortPrefixUri.Length; i++)
                        {
                            shortPrefixUri[i] = null;
                        }
                    }
                    shortPrefixUri[(int)PrefixHandleType.Empty] = emptyNamespace;
                    nsCount = 0;
                }
                this.attributeCount = 0;
                this.space = XmlSpace.None;
                this.lang = string.Empty;
                this.depth = 0;
            }

            public void EnterScope()
            {
                depth++;
            }

            public void ExitScope()
            {
                while (nsCount > 0)
                {
                    Namespace nameSpace = namespaces[nsCount - 1];
                    if (nameSpace.Depth != depth)
                        break;
                    PrefixHandleType shortPrefix;
                    if (nameSpace.Prefix.TryGetShortPrefix(out shortPrefix))
                    {
                        shortPrefixUri[(int)shortPrefix] = nameSpace.OuterUri;
                    }
                    nsCount--;
                }
                while (attributeCount > 0)
                {
                    XmlAttribute attribute = attributes[attributeCount - 1];
                    if (attribute.Depth != depth)
                        break;
                    space = attribute.XmlSpace;
                    lang = attribute.XmlLang;
                    attributeCount--;
                }
                depth--;
            }

            public void Sign(XmlSigningNodeWriter writer)
            {
                for (int i = 0; i < nsCount; i++)
                {
                    PrefixHandle prefix = namespaces[i].Prefix;
                    bool found = false;
                    for (int j = i + 1; j < nsCount; j++)
                    {
                        if (Equals(prefix, namespaces[j].Prefix))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        int prefixOffset, prefixLength;
                        byte[] prefixBuffer = prefix.GetString(out prefixOffset, out prefixLength);
                        int nsOffset, nsLength;
                        byte[] nsBuffer = namespaces[i].Uri.GetString(out nsOffset, out nsLength);
                        writer.WriteXmlnsAttribute(prefixBuffer, prefixOffset, prefixLength, nsBuffer, nsOffset, nsLength);
                    }
                }
            }

            public void AddLangAttribute(string lang)
            {
                AddAttribute();
                this.lang = lang;
            }

            public void AddSpaceAttribute(XmlSpace space)
            {
                AddAttribute();
                this.space = space;
            }

            void AddAttribute()
            {
                if (attributes == null)
                {
                    attributes = new XmlAttribute[1];
                }
                else if (attributes.Length == attributeCount)
                {
                    XmlAttribute[] newAttributes = new XmlAttribute[attributeCount * 2];
                    Array.Copy(attributes, newAttributes, attributeCount);
                    attributes = newAttributes;
                }
                XmlAttribute attribute = attributes[attributeCount];
                if (attribute == null)
                {
                    attribute = new XmlAttribute();
                    attributes[attributeCount] = attribute;
                }
                attribute.XmlLang = this.lang;
                attribute.XmlSpace = this.space;
                attribute.Depth = depth;
                attributeCount++;
            }

            public void Register(Namespace nameSpace)
            {
                PrefixHandleType shortPrefix;
                if (nameSpace.Prefix.TryGetShortPrefix(out shortPrefix))
                {
                    nameSpace.OuterUri = shortPrefixUri[(int)shortPrefix];
                    shortPrefixUri[(int)shortPrefix] = nameSpace;
                }
                else
                {
                    nameSpace.OuterUri = null;
                }
            }

            public Namespace AddNamespace()
            {
                if (namespaces == null)
                {
                    namespaces = new Namespace[4];
                }
                else if (namespaces.Length == nsCount)
                {
                    Namespace[] newNamespaces = new Namespace[nsCount * 2];
                    Array.Copy(namespaces, newNamespaces, nsCount);
                    namespaces = newNamespaces;
                }
                Namespace nameSpace = namespaces[nsCount];
                if (nameSpace == null)
                {
                    nameSpace = new Namespace(bufferReader);
                    namespaces[nsCount] = nameSpace;
                }
                nameSpace.Clear();
                nameSpace.Depth = depth;
                nsCount++;
                return nameSpace;
            }

            public Namespace LookupNamespace(PrefixHandleType prefix)
            {
                return shortPrefixUri[(int)prefix];
            }

            public Namespace LookupNamespace(PrefixHandle prefix)
            {
                PrefixHandleType shortPrefix;
                if (prefix.TryGetShortPrefix(out shortPrefix))
                    return LookupNamespace(shortPrefix);
                for (int i = nsCount - 1; i >= 0; i--)
                {
                    Namespace nameSpace = namespaces[i];
                    if (nameSpace.Prefix == prefix)
                        return nameSpace;
                }
                if (prefix.IsXml)
                    return XmlNamespace;
                return null;
            }

            public Namespace LookupNamespace(string prefix)
            {
                PrefixHandleType shortPrefix;
                if (TryGetShortPrefix(prefix, out shortPrefix))
                    return LookupNamespace(shortPrefix);
                for (int i = nsCount - 1; i >= 0; i--)
                {
                    Namespace nameSpace = namespaces[i];
                    if (nameSpace.Prefix == prefix)
                        return nameSpace;
                }
                if (prefix == "xml")
                    return XmlNamespace;
                return null;
            }

            bool TryGetShortPrefix(string s, out PrefixHandleType shortPrefix)
            {
                int length = s.Length;
                if (length == 0)
                {
                    shortPrefix = PrefixHandleType.Empty;
                    return true;
                }
                if (length == 1)
                {
                    char ch = s[0];
                    if (ch >= 'a' && ch <= 'z')
                    {
                        shortPrefix = PrefixHandle.GetAlphaPrefix(ch - 'a');
                        return true;
                    }
                }
                shortPrefix = PrefixHandleType.Empty;
                return false;
            }

            class XmlAttribute
            {
                XmlSpace space;
                string lang;
                int depth;

                public XmlAttribute()
                {
                }

                public int Depth
                {
                    get
                    {
                        return depth;
                    }
                    set
                    {
                        depth = value;
                    }
                }

                public string XmlLang
                {
                    get
                    {
                        return lang;
                    }
                    set
                    {
                        lang = value;
                    }
                }

                public XmlSpace XmlSpace
                {
                    get
                    {
                        return space;
                    }
                    set
                    {
                        space = value;
                    }
                }
            }
        }

        protected class Namespace
        {
            PrefixHandle prefix;
            StringHandle uri;
            int depth;
            Namespace outerUri;
            string uriString;

            public Namespace(XmlBufferReader bufferReader)
            {
                this.prefix = new PrefixHandle(bufferReader);
                this.uri = new StringHandle(bufferReader);
                this.outerUri = null;
                this.uriString = null;
            }

            public void Clear()
            {
                this.uriString = null;
            }

            public int Depth
            {
                get
                {
                    return depth;
                }
                set
                {
                    depth = value;
                }
            }

            public PrefixHandle Prefix
            {
                get
                {
                    return prefix;
                }
            }

            public bool IsUri(string s)
            {
                Fx.Assert(s != null, "");
                if (object.ReferenceEquals(s, uriString))
                    return true;
                if (uri == s)
                {
                    uriString = s;
                    return true;
                }
                return false;
            }

            public bool IsUri(XmlDictionaryString s)
            {
                if (object.ReferenceEquals(s.Value, uriString))
                    return true;
                if (uri == s)
                {
                    uriString = s.Value;
                    return true;
                }
                return false;
            }

            public StringHandle Uri
            {
                get
                {
                    return uri;
                }
            }

            public Namespace OuterUri
            {
                get
                {
                    return outerUri;
                }
                set
                {
                    outerUri = value;
                }
            }
        }

        class QuotaNameTable : XmlNameTable
        {
            XmlDictionaryReader reader;
            XmlNameTable nameTable;
            int maxCharCount;
            int charCount;

            public QuotaNameTable(XmlDictionaryReader reader, int maxCharCount)
            {
                this.reader = reader;
                this.nameTable = new NameTable();
                this.maxCharCount = maxCharCount;
                this.charCount = 0;
            }

            public override string Get(char[] chars, int offset, int count)
            {
                return nameTable.Get(chars, offset, count);
            }

            public override string Get(string value)
            {
                return nameTable.Get(value);
            }

            void Add(int charCount)
            {
                if (charCount > this.maxCharCount - this.charCount)
                    XmlExceptionHelper.ThrowMaxNameTableCharCountExceeded(reader, maxCharCount);
                this.charCount += charCount;
            }

            public override string Add(char[] chars, int offset, int count)
            {
                string s = nameTable.Get(chars, offset, count);
                if (s != null)
                    return s;
                Add(count);
                return nameTable.Add(chars, offset, count);
            }

            public override string Add(string value)
            {
                string s = nameTable.Get(value);
                if (s != null)
                    return s;
                Add(value.Length);
                return nameTable.Add(value);
            }
        }
    }
}
