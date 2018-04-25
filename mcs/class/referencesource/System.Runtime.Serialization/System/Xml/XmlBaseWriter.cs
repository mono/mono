//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    abstract class XmlBaseWriter : XmlDictionaryWriter, IFragmentCapableXmlDictionaryWriter
    {
        XmlNodeWriter writer;
        NamespaceManager nsMgr;
        Element[] elements;
        int depth;
        string attributeLocalName;
        string attributeValue;
        bool isXmlAttribute;
        bool isXmlnsAttribute;
        WriteState writeState;
        DocumentState documentState;
        byte[] trailBytes;
        int trailByteCount;
        XmlStreamNodeWriter nodeWriter;
        XmlSigningNodeWriter signingWriter;
        XmlUTF8NodeWriter textFragmentWriter;
        XmlNodeWriter oldWriter;
        Stream oldStream;
        int oldNamespaceBoundary;
        bool inList;
        const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";
        const string xmlNamespace = "http://www.w3.org/XML/1998/namespace";
        static BinHexEncoding binhexEncoding;
        static string[] prefixes = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        XmlBaseWriterNodeWriterAsyncHelper nodeWriterAsyncHelper;

        protected XmlBaseWriter()
        {
            this.nsMgr = new NamespaceManager();
            this.writeState = WriteState.Start;
            this.documentState = DocumentState.None;
        }

        protected void SetOutput(XmlStreamNodeWriter writer)
        {
            this.inList = false;
            this.writer = writer;
            this.nodeWriter = writer;
            this.writeState = WriteState.Start;
            this.documentState = DocumentState.None;
            this.nsMgr.Clear();
            if (this.depth != 0)
            {
                this.elements = null;
                this.depth = 0;
            }
            this.attributeLocalName = null;
            this.attributeValue = null;
            this.oldWriter = null;
            this.oldStream = null;
        }

        public override void Flush()
        {
            if (IsClosed)
                ThrowClosed();

            writer.Flush();
        }

        public override void Close()
        {
            if (IsClosed)
                return;

            try
            {
                FinishDocument();
                AutoComplete(WriteState.Closed);
                writer.Flush();
            }
            finally
            {
                nsMgr.Close();
                if (depth != 0)
                {
                    elements = null;
                    depth = 0;
                }
                attributeValue = null;
                attributeLocalName = null;
                nodeWriter.Close();
                if (signingWriter != null)
                {
                    signingWriter.Close();
                }
                if (textFragmentWriter != null)
                {
                    textFragmentWriter.Close();
                }
                oldWriter = null;
                oldStream = null;
            }
        }

        protected bool IsClosed
        {
            get { return writeState == WriteState.Closed; }
        }

        protected void ThrowClosed()
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlWriterClosed)));
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

        public override WriteState WriteState
        {
            get
            {
                return writeState;
            }
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            if (IsClosed)
                ThrowClosed();

            if (ns == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");

            if (writeState != WriteState.Element)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteXmlnsAttribute", WriteState.ToString())));

            if (prefix == null)
            {
                prefix = nsMgr.LookupPrefix(ns);
                if (prefix == null)
                {
                    GeneratePrefix(ns, null);
                }
            }
            else
            {
                nsMgr.AddNamespaceIfNotDeclared(prefix, ns, null);
            }
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            if (IsClosed)
                ThrowClosed();

            if (ns == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");

            if (writeState != WriteState.Element)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteXmlnsAttribute", WriteState.ToString())));

            if (prefix == null)
            {
                prefix = nsMgr.LookupPrefix(ns.Value);
                if (prefix == null)
                {
                    GeneratePrefix(ns.Value, ns);
                }
            }
            else
            {
                nsMgr.AddNamespaceIfNotDeclared(prefix, ns.Value, ns);
            }
        }

        void StartAttribute(ref string prefix, string localName, string ns, XmlDictionaryString xNs)
        {
            if (IsClosed)
                ThrowClosed();

            if (writeState == WriteState.Attribute)
                WriteEndAttribute();

            if (localName == null || (localName.Length == 0 && prefix != "xmlns"))
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));

            if (writeState != WriteState.Element)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteStartAttribute", WriteState.ToString())));

            if (prefix == null)
            {
                if (ns == xmlnsNamespace && localName != "xmlns")
                    prefix = "xmlns";
                else if (ns == xmlNamespace)
                    prefix = "xml";
                else
                    prefix = string.Empty;
            }

            // Normalize a (prefix,localName) of (null, "xmlns") to ("xmlns", string.Empty).
            if (prefix.Length == 0 && localName == "xmlns")
            {
                prefix = "xmlns";
                localName = string.Empty;
            }

            isXmlnsAttribute = false;
            isXmlAttribute = false;
            if (prefix == "xml")
            {
                if (ns != null && ns != xmlNamespace)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlPrefixBoundToNamespace, "xml", xmlNamespace, ns), "ns"));
                isXmlAttribute = true;
                attributeValue = string.Empty;
                attributeLocalName = localName;
            }
            else if (prefix == "xmlns")
            {
                if (ns != null && ns != xmlnsNamespace)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlPrefixBoundToNamespace, "xmlns", xmlnsNamespace, ns), "ns"));
                isXmlnsAttribute = true;
                attributeValue = string.Empty;
                attributeLocalName = localName;
            }
            else if (ns == null)
            {
                // A null namespace means the namespace of the given prefix.
                if (prefix.Length == 0)
                {
                    // An empty prefix on an attribute means no namespace (not the default namespace)
                    ns = string.Empty;
                }
                else
                {
                    ns = nsMgr.LookupNamespace(prefix);

                    if (ns == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlUndefinedPrefix, prefix), "prefix"));
                }
            }
            else if (ns.Length == 0)
            {
                // An empty namespace means no namespace; prefix must be empty
                if (prefix.Length != 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlEmptyNamespaceRequiresNullPrefix), "prefix"));
            }
            else if (prefix.Length == 0)
            {
                // No prefix specified - try to find a prefix corresponding to the given namespace
                prefix = nsMgr.LookupAttributePrefix(ns);

                // If we didn't find anything with the right namespace, generate one.
                if (prefix == null)
                {
                    // Watch for special values
                    if (ns.Length == xmlnsNamespace.Length && ns == xmlnsNamespace)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlSpecificBindingNamespace, "xmlns", ns)));
                    if (ns.Length == xmlNamespace.Length && ns == xmlNamespace)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlSpecificBindingNamespace, "xml", ns)));

                    prefix = GeneratePrefix(ns, xNs);
                }
            }
            else
            {
                nsMgr.AddNamespaceIfNotDeclared(prefix, ns, xNs);
            }
            writeState = WriteState.Attribute;
        }

        public override void WriteStartAttribute(string prefix, string localName, string namespaceUri)
        {
            StartAttribute(ref prefix, localName, namespaceUri, null);
            if (!isXmlnsAttribute)
            {
                writer.WriteStartAttribute(prefix, localName);
            }
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            StartAttribute(ref prefix, (localName != null ? localName.Value : null), (namespaceUri != null ? namespaceUri.Value : null), namespaceUri);
            if (!isXmlnsAttribute)
            {
                writer.WriteStartAttribute(prefix, localName);
            }
        }

        public override void WriteEndAttribute()
        {
            if (IsClosed)
                ThrowClosed();

            if (writeState != WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteEndAttribute", WriteState.ToString())));

            FlushBase64();
            try
            {
                if (isXmlAttribute)
                {
                    if (attributeLocalName == "lang")
                    {
                        nsMgr.AddLangAttribute(attributeValue);
                    }
                    else if (attributeLocalName == "space")
                    {
                        if (attributeValue == "preserve")
                        {
                            nsMgr.AddSpaceAttribute(XmlSpace.Preserve);
                        }
                        else if (attributeValue == "default")
                        {
                            nsMgr.AddSpaceAttribute(XmlSpace.Default);
                        }
                        else
                        {
                            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlInvalidXmlSpace, attributeValue)));
                        }
                    }
                    else
                    {
                        // XmlTextWriter specifically allows for other localNames
                    }
                    isXmlAttribute = false;
                    attributeLocalName = null;
                    attributeValue = null;
                }

                if (isXmlnsAttribute)
                {
                    nsMgr.AddNamespaceIfNotDeclared(attributeLocalName, attributeValue, null);
                    isXmlnsAttribute = false;
                    attributeLocalName = null;
                    attributeValue = null;
                }
                else
                {
                    writer.WriteEndAttribute();
                }
            }
            finally
            {
                writeState = WriteState.Element;
            }
        }

        public override void WriteComment(string text)
        {
            if (IsClosed)
                ThrowClosed();

            if (writeState == WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteComment", WriteState.ToString())));

            if (text == null)
            {
                text = string.Empty;
            }
            else if (text.IndexOf("--", StringComparison.Ordinal) != -1 || (text.Length > 0 && text[text.Length - 1] == '-'))
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlInvalidCommentChars), "text"));
            }

            StartComment();
            FlushBase64();
            writer.WriteComment(text);
            EndComment();
        }

        public override void WriteFullEndElement()
        {
            if (IsClosed)
                ThrowClosed();

            if (writeState == WriteState.Attribute)
                WriteEndAttribute();

            if (writeState != WriteState.Element && writeState != WriteState.Content)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteFullEndElement", WriteState.ToString())));

            AutoComplete(WriteState.Content);
            WriteEndElement();
        }

        public override void WriteCData(string text)
        {
            if (IsClosed)
                ThrowClosed();

            if (writeState == WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteCData", WriteState.ToString())));

            if (text == null)
                text = string.Empty;

            if (text.Length > 0)
            {
                StartContent();
                FlushBase64();
                writer.WriteCData(text);
                EndContent();
            }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.XmlMethodNotSupported, "WriteDocType")));
        }

        void StartElement(ref string prefix, string localName, string ns, XmlDictionaryString xNs)
        {
            if (IsClosed)
                ThrowClosed();

            if (this.documentState == DocumentState.Epilog)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlOnlyOneRoot)));
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            if (localName.Length == 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidLocalNameEmpty), "localName"));
            if (writeState == WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteStartElement", WriteState.ToString())));

            FlushBase64();
            AutoComplete(WriteState.Element);
            Element element = EnterScope();
            if (ns == null)
            {
                if (prefix == null)
                    prefix = string.Empty;

                ns = nsMgr.LookupNamespace(prefix);

                if (ns == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlUndefinedPrefix, prefix), "prefix"));
            }
            else if (prefix == null)
            {
                prefix = nsMgr.LookupPrefix(ns);

                if (prefix == null)
                {
                    prefix = string.Empty;
                    nsMgr.AddNamespace(string.Empty, ns, xNs);
                }
            }
            else
            {
                nsMgr.AddNamespaceIfNotDeclared(prefix, ns, xNs);
            }
            element.Prefix = prefix;
            element.LocalName = localName;
        }

        public override void WriteStartElement(string prefix, string localName, string namespaceUri)
        {
            StartElement(ref prefix, localName, namespaceUri, null);
            writer.WriteStartElement(prefix, localName);
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            StartElement(ref prefix, (localName != null ? localName.Value : null), (namespaceUri != null ? namespaceUri.Value : null), namespaceUri);
            writer.WriteStartElement(prefix, localName);
        }

        public override void WriteEndElement()
        {
            if (IsClosed)
                ThrowClosed();

            if (depth == 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidDepth, "WriteEndElement", depth.ToString(CultureInfo.InvariantCulture))));

            if (writeState == WriteState.Attribute)
                WriteEndAttribute();

            FlushBase64();
            if (writeState == WriteState.Element)
            {
                nsMgr.DeclareNamespaces(writer);
                writer.WriteEndStartElement(true);
            }
            else
            {
                Element element = elements[depth];
                writer.WriteEndElement(element.Prefix, element.LocalName);
            }

            ExitScope();
            writeState = WriteState.Content;
        }

        Element EnterScope()
        {
            nsMgr.EnterScope();
            depth++;
            if (elements == null)
            {
                elements = new Element[4];
            }
            else if (elements.Length == depth)
            {
                Element[] newElementNodes = new Element[depth * 2];
                Array.Copy(elements, newElementNodes, depth);
                elements = newElementNodes;
            }
            Element element = elements[depth];
            if (element == null)
            {
                element = new Element();
                elements[depth] = element;
            }
            return element;
        }

        void ExitScope()
        {
            elements[depth].Clear();
            depth--;
            if (depth == 0 && documentState == DocumentState.Document)
                this.documentState = DocumentState.Epilog;
            nsMgr.ExitScope();
        }

        protected void FlushElement()
        {
            if (this.writeState == WriteState.Element)
            {
                AutoComplete(WriteState.Content);
            }
        }

        protected void StartComment()
        {
            FlushElement();
        }

        protected void EndComment()
        {
        }

        protected void StartContent()
        {
            FlushElement();
            if (depth == 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlIllegalOutsideRoot)));
        }

        protected void StartContent(char ch)
        {
            FlushElement();
            if (depth == 0)
                VerifyWhitespace(ch);
        }

        protected void StartContent(string s)
        {
            FlushElement();
            if (depth == 0)
                VerifyWhitespace(s);
        }

        protected void StartContent(char[] chars, int offset, int count)
        {
            FlushElement();
            if (depth == 0)
                VerifyWhitespace(chars, offset, count);
        }

        void VerifyWhitespace(char ch)
        {
            if (!IsWhitespace(ch))
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlIllegalOutsideRoot)));
        }

        void VerifyWhitespace(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (!IsWhitespace(s[i]))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlIllegalOutsideRoot)));
        }

        void VerifyWhitespace(char[] chars, int offset, int count)
        {
            for (int i = 0; i < count; i++)
                if (!IsWhitespace(chars[offset + i]))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlIllegalOutsideRoot)));
        }

        bool IsWhitespace(char ch)
        {
            return (ch == ' ' || ch == '\n' || ch == '\r' || ch == 't');
        }

        protected void EndContent()
        {
        }

        void AutoComplete(WriteState writeState)
        {
            if (this.writeState == WriteState.Element)
            {
                EndStartElement();
            }
            this.writeState = writeState;
        }

        void EndStartElement()
        {
            nsMgr.DeclareNamespaces(writer);
            writer.WriteEndStartElement(false);
        }

        public override string LookupPrefix(string ns)
        {
            if (ns == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));

            if (IsClosed)
                ThrowClosed();

            return nsMgr.LookupPrefix(ns);
        }

        internal string LookupNamespace(string prefix)
        {
            if (prefix == null)
                return null;
            return nsMgr.LookupNamespace(prefix);
        }

        string GetQualifiedNamePrefix(string namespaceUri, XmlDictionaryString xNs)
        {
            string prefix = nsMgr.LookupPrefix(namespaceUri);
            if (prefix == null)
            {
                if (writeState != WriteState.Attribute)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlNamespaceNotFound, namespaceUri), "namespaceUri"));

                prefix = GeneratePrefix(namespaceUri, xNs);
            }
            return prefix;
        }

        public override void WriteQualifiedName(string localName, string namespaceUri)
        {
            if (IsClosed)
                ThrowClosed();
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            if (localName.Length == 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidLocalNameEmpty), "localName"));
            if (namespaceUri == null)
                namespaceUri = string.Empty;
            string prefix = GetQualifiedNamePrefix(namespaceUri, null);
            if (prefix.Length != 0)
            {
                WriteString(prefix);
                WriteString(":");
            }
            WriteString(localName);
        }

        public override void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (IsClosed)
                ThrowClosed();
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            if (localName.Value.Length == 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidLocalNameEmpty), "localName"));
            if (namespaceUri == null)
                namespaceUri = XmlDictionaryString.Empty;
            string prefix = GetQualifiedNamePrefix(namespaceUri.Value, namespaceUri);

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(string.Concat(prefix, ":", namespaceUri.Value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteQualifiedName(prefix, localName);
                EndContent();
            }
        }

        public override void WriteStartDocument()
        {
            if (IsClosed)
                ThrowClosed();

            if (writeState != WriteState.Start)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteStartDocument", WriteState.ToString())));

            writeState = WriteState.Prolog;
            documentState = DocumentState.Document;
            writer.WriteDeclaration();
        }

        public override void WriteStartDocument(bool standalone)
        {
            if (IsClosed)
                ThrowClosed();

            WriteStartDocument();
        }


        public override void WriteProcessingInstruction(string name, string text)
        {
            if (IsClosed)
                ThrowClosed();

            if (name != "xml")
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlProcessingInstructionNotSupported), "name"));

            if (writeState != WriteState.Start)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidDeclaration)));

            // The only thing the text can legitimately contain is version, encoding, and standalone.
            // We only support version 1.0, we can only write whatever encoding we were supplied, 
            // and we don't support DTDs, so whatever values are supplied in the text argument are irrelevant.
            writer.WriteDeclaration();
        }

        void FinishDocument()
        {
            if (this.writeState == WriteState.Attribute)
            {
                WriteEndAttribute();
            }

            while (this.depth > 0)
            {
                WriteEndElement();
            }
        }

        public override void WriteEndDocument()
        {
            if (IsClosed)
                ThrowClosed();

            if (writeState == WriteState.Start || writeState == WriteState.Prolog)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlNoRootElement)));

            FinishDocument();
            writeState = WriteState.Start;
            documentState = DocumentState.End;
        }

        protected int NamespaceBoundary
        {
            get
            {
                return nsMgr.NamespaceBoundary;
            }
            set
            {
                nsMgr.NamespaceBoundary = value;
            }
        }

        public override void WriteEntityRef(string name)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.XmlMethodNotSupported, "WriteEntityRef")));
        }

        public override void WriteName(string name)
        {
            if (IsClosed)
                ThrowClosed();

            WriteString(name);
        }

        public override void WriteNmToken(string name)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.XmlMethodNotSupported, "WriteNmToken")));
        }

        public override void WriteWhitespace(string whitespace)
        {
            if (IsClosed)
                ThrowClosed();

            if (whitespace == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("whitespace");

            for (int i = 0; i < whitespace.Length; ++i)
            {
                char c = whitespace[i];
                if (c != ' ' &&
                    c != '\t' &&
                    c != '\n' &&
                    c != '\r')
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlOnlyWhitespace), "whitespace"));
            }

            WriteString(whitespace);
        }

        public override void WriteString(string value)
        {
            if (IsClosed)
                ThrowClosed();

            if (value == null)
                value = string.Empty;

            if (value.Length > 0 || this.inList)
            {
                FlushBase64();

                if (attributeValue != null)
                    WriteAttributeText(value);

                if (!isXmlnsAttribute)
                {
                    StartContent(value);
                    writer.WriteEscapedText(value);
                    EndContent();
                }
            }
        }

        public override void WriteString(XmlDictionaryString value)
        {
            if (IsClosed)
                ThrowClosed();

            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            if (value.Value.Length > 0)
            {
                FlushBase64();

                if (attributeValue != null)
                    WriteAttributeText(value.Value);

                if (!isXmlnsAttribute)
                {
                    StartContent(value.Value);
                    writer.WriteEscapedText(value);
                    EndContent();
                }
            }
        }

        public override void WriteChars(char[] chars, int offset, int count)
        {
            if (IsClosed)
                ThrowClosed();

            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));

            // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));

            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > chars.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - offset)));

            if (count > 0)
            {
                FlushBase64();

                if (attributeValue != null)
                    WriteAttributeText(new string(chars, offset, count));

                if (!isXmlnsAttribute)
                {
                    StartContent(chars, offset, count);
                    writer.WriteEscapedText(chars, offset, count);
                    EndContent();
                }
            }
        }

        public override void WriteRaw(string value)
        {
            if (IsClosed)
                ThrowClosed();

            if (value == null)
                value = string.Empty;

            if (value.Length > 0)
            {
                FlushBase64();

                if (attributeValue != null)
                    WriteAttributeText(value);

                if (!isXmlnsAttribute)
                {
                    StartContent(value);
                    writer.WriteText(value);
                    EndContent();
                }
            }
        }

        public override void WriteRaw(char[] chars, int offset, int count)
        {
            if (IsClosed)
                ThrowClosed();

            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));

            // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));

            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > chars.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - offset)));

            if (count > 0)
            {
                FlushBase64();

                if (attributeValue != null)
                    WriteAttributeText(new string(chars, offset, count));

                if (!isXmlnsAttribute)
                {
                    StartContent(chars, offset, count);
                    writer.WriteText(chars, offset, count);
                    EndContent();
                }
            }
        }

        public override void WriteCharEntity(char ch)
        {
            if (IsClosed)
                ThrowClosed();

            if (ch >= 0xd800 && ch <= 0xdfff)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlMissingLowSurrogate), "ch"));

            if (attributeValue != null)
                WriteAttributeText(ch.ToString());

            if (!isXmlnsAttribute)
            {
                StartContent(ch);
                FlushBase64();
                writer.WriteCharEntity(ch);
                EndContent();
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            if (IsClosed)
                ThrowClosed();

            SurrogateChar ch = new SurrogateChar(lowChar, highChar);

            if (attributeValue != null)
            {
                char[] chars = new char[2] { highChar, lowChar };
                WriteAttributeText(new string(chars));
            }

            if (!isXmlnsAttribute)
            {
                StartContent();
                FlushBase64();
                writer.WriteCharEntity(ch.Char);
                EndContent();
            }
        }

        public override void WriteValue(object value)
        {
            if (IsClosed)
                ThrowClosed();

            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

            else if (value is object[])
            {
                WriteValue((object[])value);
            }
            else if (value is Array)
            {
                WriteValue((Array)value);
            }
            else if (value is IStreamProvider)
            {
                WriteValue((IStreamProvider)value);
            }
            else
            {
                WritePrimitiveValue(value);
            }
        }

        protected void WritePrimitiveValue(object value)
        {
            if (IsClosed)
                ThrowClosed();

            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

            if (value is ulong)
            {
                WriteValue((ulong)value);
            }
            else if (value is string)
            {
                WriteValue((string)value);
            }
            else if (value is int)
            {
                WriteValue((int)value);
            }
            else if (value is long)
            {
                WriteValue((long)value);
            }
            else if (value is bool)
            {
                WriteValue((bool)value);
            }
            else if (value is double)
            {
                WriteValue((double)value);
            }
            else if (value is DateTime)
            {
                WriteValue((DateTime)value);
            }
            else if (value is float)
            {
                WriteValue((float)value);
            }
            else if (value is decimal)
            {
                WriteValue((decimal)value);
            }
            else if (value is XmlDictionaryString)
            {
                WriteValue((XmlDictionaryString)value);
            }
            else if (value is UniqueId)
            {
                WriteValue((UniqueId)value);
            }
            else if (value is Guid)
            {
                WriteValue((Guid)value);
            }
            else if (value is TimeSpan)
            {
                WriteValue((TimeSpan)value);
            }
            else if (value.GetType().IsArray)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlNestedArraysNotSupported), "value"));
            }
            else
            {
                base.WriteValue(value);
            }
        }

        public override void WriteValue(string value)
        {
            if (IsClosed)
                ThrowClosed();

            WriteString(value);
        }

        public override void WriteValue(int value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteInt32Text(value);
                EndContent();
            }
        }

        public override void WriteValue(long value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteInt64Text(value);
                EndContent();
            }
        }

        void WriteValue(ulong value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteUInt64Text(value);
                EndContent();
            }
        }

        public override void WriteValue(bool value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteBoolText(value);
                EndContent();
            }
        }

        public override void WriteValue(decimal value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteDecimalText(value);
                EndContent();
            }
        }

        public override void WriteValue(float value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteFloatText(value);
                EndContent();
            }
        }

        public override void WriteValue(double value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteDoubleText(value);
                EndContent();
            }
        }

        public override void WriteValue(XmlDictionaryString value)
        {
            WriteString(value);
        }

        public override void WriteValue(DateTime value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteDateTimeText(value);
                EndContent();
            }
        }

        public override void WriteValue(UniqueId value)
        {
            if (IsClosed)
                ThrowClosed();

            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteUniqueIdText(value);
                EndContent();
            }
        }

        public override void WriteValue(Guid value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteGuidText(value);
                EndContent();
            }
        }

        public override void WriteValue(TimeSpan value)
        {
            if (IsClosed)
                ThrowClosed();

            FlushBase64();
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.ToString(value));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteTimeSpanText(value);
                EndContent();
            }
        }

        public override void WriteBase64(byte[] buffer, int offset, int count)
        {
            if (IsClosed)
                ThrowClosed();

            EnsureBufferBounds(buffer, offset, count);
            if (count > 0)
            {
                if (trailByteCount > 0)
                {
                    while (trailByteCount < 3 && count > 0)
                    {
                        trailBytes[trailByteCount++] = buffer[offset++];
                        count--;
                    }
                }

                int totalByteCount = trailByteCount + count;
                int actualByteCount = totalByteCount - (totalByteCount % 3);

                if (trailBytes == null)
                {
                    trailBytes = new byte[3];
                }

                if (actualByteCount >= 3)
                {
                    if (attributeValue != null)
                    {
                        WriteAttributeText(XmlConverter.Base64Encoding.GetString(trailBytes, 0, trailByteCount));
                        WriteAttributeText(XmlConverter.Base64Encoding.GetString(buffer, offset, actualByteCount - trailByteCount));
                    }

                    if (!isXmlnsAttribute)
                    {
                        StartContent();
                        writer.WriteBase64Text(trailBytes, trailByteCount, buffer, offset, actualByteCount - trailByteCount);
                        EndContent();
                    }
                    trailByteCount = (totalByteCount - actualByteCount);

                    if (trailByteCount > 0)
                    {
                        int trailOffset = offset + count - trailByteCount;
                        for (int i = 0; i < trailByteCount; i++)
                            trailBytes[i] = buffer[trailOffset++];
                    }
                }
                else
                {
                    Buffer.BlockCopy(buffer, offset, trailBytes, trailByteCount, count);
                    trailByteCount += count;
                }
            }
        }

        internal override IAsyncResult BeginWriteBase64(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (IsClosed)
                ThrowClosed();

            EnsureBufferBounds(buffer, offset, count);

            return new WriteBase64AsyncResult(buffer, offset, count, this, callback, state);
        }

        internal override void EndWriteBase64(IAsyncResult result)
        {
            WriteBase64AsyncResult.End(result);
        }

        internal override AsyncCompletionResult WriteBase64Async(AsyncEventArgs<XmlWriteBase64AsyncArguments> state)
        {
            if (this.nodeWriterAsyncHelper == null)
            {
                this.nodeWriterAsyncHelper = new XmlBaseWriterNodeWriterAsyncHelper(this);
            }

            this.nodeWriterAsyncHelper.SetArguments(state);

            if (this.nodeWriterAsyncHelper.StartAsync() == AsyncCompletionResult.Completed)
            {                
                return AsyncCompletionResult.Completed;
            }

            return AsyncCompletionResult.Queued;
        }

        class WriteBase64AsyncResult : AsyncResult
        {
            static AsyncCompletion onComplete = new AsyncCompletion(OnComplete);
            XmlBaseWriter writer;
            byte[] buffer;
            int offset;
            int count;
            int actualByteCount;
            int totalByteCount;

            public WriteBase64AsyncResult(byte[] buffer, int offset, int count, XmlBaseWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.writer = writer;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;

                bool completeSelf = true;

                if (this.count > 0)
                {
                    if (writer.trailByteCount > 0)
                    {
                        while (writer.trailByteCount < 3 && this.count > 0)
                        {
                            writer.trailBytes[writer.trailByteCount++] = buffer[this.offset++];
                            this.count--;
                        }
                    }

                    this.totalByteCount = writer.trailByteCount + this.count;
                    this.actualByteCount = totalByteCount - (totalByteCount % 3);

                    if (writer.trailBytes == null)
                    {
                        writer.trailBytes = new byte[3];
                    }

                    if (actualByteCount >= 3)
                    {
                        if (writer.attributeValue != null)
                        {
                            writer.WriteAttributeText(XmlConverter.Base64Encoding.GetString(writer.trailBytes, 0, writer.trailByteCount));
                            writer.WriteAttributeText(XmlConverter.Base64Encoding.GetString(buffer, this.offset, actualByteCount - writer.trailByteCount));
                        }

                        // StartContent/WriteBase64Text/EndContent will be called from HandleWriteBase64 as appropriate
                        completeSelf = HandleWriteBase64Text(null);
                    }
                    else
                    {
                        Buffer.BlockCopy(buffer, this.offset, writer.trailBytes, writer.trailByteCount, this.count);
                        writer.trailByteCount += this.count;
                    }
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            static bool OnComplete(IAsyncResult result)
            {
                WriteBase64AsyncResult thisPtr = (WriteBase64AsyncResult)result.AsyncState;
                return thisPtr.HandleWriteBase64Text(result);
            }

            bool HandleWriteBase64Text(IAsyncResult result)
            {
                // in this code block if count > 0 && actualByteCount >= 3
                if (!writer.isXmlnsAttribute)
                {
                    if (result == null)
                    {
                        this.writer.StartContent();
                        result = this.writer.writer.BeginWriteBase64Text(this.writer.trailBytes,
                            this.writer.trailByteCount,
                            this.buffer,
                            this.offset,
                            this.actualByteCount - this.writer.trailByteCount,
                            PrepareAsyncCompletion(onComplete),
                            this);

                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                    }
                    this.writer.writer.EndWriteBase64Text(result);
                    this.writer.EndContent();
                }

                this.writer.trailByteCount = (totalByteCount - actualByteCount);
                if (this.writer.trailByteCount > 0)
                {
                    int trailOffset = offset + count - this.writer.trailByteCount;
                    for (int i = 0; i < this.writer.trailByteCount; i++)
                        this.writer.trailBytes[i] = this.buffer[trailOffset++];
                }

                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteBase64AsyncResult>(result);
            }
        }

        public override void WriteBinHex(byte[] buffer, int offset, int count)
        {
            if (IsClosed)
                ThrowClosed();

            EnsureBufferBounds(buffer, offset, count);

            WriteRaw(BinHexEncoding.GetString(buffer, offset, count));
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
                return writer == signingWriter;
            }
        }

        public override void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            if (IsClosed)
                ThrowClosed();
            if (Signing)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlCanonicalizationStarted)));
            FlushElement();
            if (signingWriter == null)
                signingWriter = CreateSigningNodeWriter();
            signingWriter.SetOutput(writer, stream, includeComments, inclusivePrefixes);
            writer = signingWriter;
            SignScope(signingWriter.CanonicalWriter);
        }

        public override void EndCanonicalization()
        {
            if (IsClosed)
                ThrowClosed();
            if (!Signing)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlCanonicalizationNotStarted)));
            signingWriter.Flush();
            writer = signingWriter.NodeWriter;
        }

        protected abstract XmlSigningNodeWriter CreateSigningNodeWriter();

        public virtual bool CanFragment
        {
            get
            {
                return true;
            }
        }

        public void StartFragment(Stream stream, bool generateSelfContainedTextFragment)
        {
            if (!CanFragment)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            if (IsClosed)
                ThrowClosed();
            if (stream == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
            if (oldStream != null || oldWriter != null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            if (WriteState == WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "StartFragment", WriteState.ToString())));
            FlushElement();
            writer.Flush();

            oldNamespaceBoundary = NamespaceBoundary;

            XmlStreamNodeWriter fragmentWriter = null;
            if (generateSelfContainedTextFragment)
            {
                this.NamespaceBoundary = depth + 1;
                if (textFragmentWriter == null)
                    textFragmentWriter = new XmlUTF8NodeWriter();
                textFragmentWriter.SetOutput(stream, false, Encoding.UTF8);
                fragmentWriter = textFragmentWriter;
            }

            if (Signing)
            {
                if (fragmentWriter != null)
                {
                    oldWriter = signingWriter.NodeWriter;
                    signingWriter.NodeWriter = fragmentWriter;
                }
                else
                {
                    oldStream = ((XmlStreamNodeWriter)signingWriter.NodeWriter).Stream;
                    ((XmlStreamNodeWriter)signingWriter.NodeWriter).Stream = stream;
                }
            }
            else
            {
                if (fragmentWriter != null)
                {
                    oldWriter = writer;
                    writer = fragmentWriter;
                }
                else
                {
                    oldStream = nodeWriter.Stream;
                    nodeWriter.Stream = stream;
                }
            }
        }

        public void EndFragment()
        {
            if (IsClosed)
                ThrowClosed();
            if (oldStream == null && oldWriter == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            if (WriteState == WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "EndFragment", WriteState.ToString())));

            FlushElement();
            writer.Flush();

            if (Signing)
            {
                if (oldWriter != null)
                    signingWriter.NodeWriter = oldWriter;
                else
                    ((XmlStreamNodeWriter)signingWriter.NodeWriter).Stream = oldStream;
            }
            else
            {
                if (oldWriter != null)
                    writer = oldWriter;
                else
                    nodeWriter.Stream = oldStream;
            }
            NamespaceBoundary = oldNamespaceBoundary;
            oldWriter = null;
            oldStream = null;
        }

        public void WriteFragment(byte[] buffer, int offset, int count)
        {
            if (!CanFragment)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            if (IsClosed)
                ThrowClosed();
            if (buffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > buffer.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));
            if (WriteState == WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteFragment", WriteState.ToString())));
            if (writer != nodeWriter)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            FlushElement();
            FlushBase64();
            nodeWriter.Flush();
            nodeWriter.Stream.Write(buffer, offset, count);
        }

        void FlushBase64()
        {
            if (trailByteCount > 0)
            {
                FlushTrailBytes();
            }
        }

        void FlushTrailBytes()
        {
            if (attributeValue != null)
                WriteAttributeText(XmlConverter.Base64Encoding.GetString(trailBytes, 0, trailByteCount));

            if (!isXmlnsAttribute)
            {
                StartContent();
                writer.WriteBase64Text(trailBytes, trailByteCount, trailBytes, 0, 0);
                EndContent();
            }
            trailByteCount = 0;
        }

        void WriteValue(object[] array)
        {
            FlushBase64();
            StartContent();
            writer.WriteStartListText();
            this.inList = true;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != 0)
                {
                    writer.WriteListSeparator();
                }
                WritePrimitiveValue(array[i]);
            }
            this.inList = false;
            writer.WriteEndListText();
            EndContent();
        }

        void WriteValue(Array array)
        {
            FlushBase64();
            StartContent();
            writer.WriteStartListText();
            this.inList = true;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != 0)
                {
                    writer.WriteListSeparator();
                }
                WritePrimitiveValue(array.GetValue(i));
            }
            this.inList = false;
            writer.WriteEndListText();
            EndContent();
        }

        protected void StartArray(int count)
        {
            FlushBase64();
            if (this.documentState == DocumentState.Epilog)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlOnlyOneRoot)));
            if (this.documentState == DocumentState.Document && count > 1 && depth == 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlOnlyOneRoot)));
            if (writeState == WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidWriteState, "WriteStartElement", WriteState.ToString())));
            AutoComplete(WriteState.Content);
        }

        protected void EndArray()
        {
        }

        void EnsureBufferBounds(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");

            // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));

            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > buffer.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));
        }

        string GeneratePrefix(string ns, XmlDictionaryString xNs)
        {
            if (writeState != WriteState.Element && writeState != WriteState.Attribute)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidPrefixState, WriteState.ToString())));

            string prefix = nsMgr.AddNamespace(ns, xNs);

            if (prefix != null)
                return prefix;

            while (true)
            {
                int prefixId = elements[depth].PrefixId++;
                prefix = string.Concat("d", depth.ToString(CultureInfo.InvariantCulture), "p", prefixId.ToString(CultureInfo.InvariantCulture));

                if (nsMgr.LookupNamespace(prefix) == null)
                {
                    nsMgr.AddNamespace(prefix, ns, xNs);
                    return prefix;
                }
            }
        }

        protected void SignScope(XmlCanonicalWriter signingWriter)
        {
            nsMgr.Sign(signingWriter);
        }

        void WriteAttributeText(string value)
        {
            if (attributeValue.Length == 0)
                attributeValue = value;
            else
                attributeValue += value;
        }

        class Element
        {
            string prefix;
            string localName;
            int prefixId;

            public string Prefix
            {
                get
                {
                    return prefix;
                }
                set
                {
                    prefix = value;
                }
            }

            public string LocalName
            {
                get
                {
                    return localName;
                }
                set
                {
                    localName = value;
                }
            }

            public int PrefixId
            {
                get
                {
                    return prefixId;
                }
                set
                {
                    prefixId = value;
                }
            }

            public void Clear()
            {
                this.prefix = null;
                this.localName = null;
                this.prefixId = 0;
            }
        }

        enum DocumentState : byte
        {
            None,       // Not inside StartDocument/EndDocument - Allows multiple root elemnts
            Document,   // Inside StartDocument/EndDocument
            Epilog,     // EndDocument must be called
            End         // Nothing further to write
        }

        class NamespaceManager
        {
            Namespace[] namespaces;
            Namespace lastNameSpace;
            int nsCount;
            int depth;
            XmlAttribute[] attributes;
            int attributeCount;
            XmlSpace space;
            string lang;
            int namespaceBoundary;
            int nsTop;
            Namespace defaultNamespace;

            public NamespaceManager()
            {
                defaultNamespace = new Namespace();
                defaultNamespace.Depth = 0;
                defaultNamespace.Prefix = string.Empty;
                defaultNamespace.Uri = string.Empty;
                defaultNamespace.UriDictionaryString = null;
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
                if (this.namespaces == null)
                {
                    this.namespaces = new Namespace[4];
                    this.namespaces[0] = defaultNamespace;
                }
                this.nsCount = 1;
                this.nsTop = 0;
                this.depth = 0;
                this.attributeCount = 0;
                this.space = XmlSpace.None;
                this.lang = null;
                this.lastNameSpace = null;
                this.namespaceBoundary = 0;
            }

            public int NamespaceBoundary
            {
                get
                {
                    return namespaceBoundary;
                }
                set
                {
                    int i;
                    for (i = 0; i < nsCount; i++)
                        if (namespaces[i].Depth >= value)
                            break;

                    nsTop = i;
                    namespaceBoundary = value;
                    lastNameSpace = null;
                }
            }

            public void Close()
            {
                if (depth == 0)
                {
                    if (namespaces != null && namespaces.Length > 32)
                        namespaces = null;
                    if (attributes != null && attributes.Length > 4)
                        attributes = null;
                }
                else
                {
                    namespaces = null;
                    attributes = null;
                }
                lang = null;
            }

            public void DeclareNamespaces(XmlNodeWriter writer)
            {
                int i = this.nsCount;
                while (i > 0)
                {
                    Namespace nameSpace = namespaces[i - 1];
                    if (nameSpace.Depth != depth)
                        break;
                    i--;
                }
                while (i < this.nsCount)
                {
                    Namespace nameSpace = namespaces[i];
                    if (nameSpace.UriDictionaryString != null)
                        writer.WriteXmlnsAttribute(nameSpace.Prefix, nameSpace.UriDictionaryString);
                    else
                        writer.WriteXmlnsAttribute(nameSpace.Prefix, nameSpace.Uri);
                    i++;
                }
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
                    if (lastNameSpace == nameSpace)
                        lastNameSpace = null;
                    nameSpace.Clear();
                    nsCount--;
                }
                while (attributeCount > 0)
                {
                    XmlAttribute attribute = attributes[attributeCount - 1];
                    if (attribute.Depth != depth)
                        break;
                    space = attribute.XmlSpace;
                    lang = attribute.XmlLang;
                    attribute.Clear();
                    attributeCount--;
                }
                depth--;
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

            public string AddNamespace(string uri, XmlDictionaryString uriDictionaryString)
            {
                if (uri.Length == 0)
                {
                    // Empty namespace can only be bound to the empty prefix
                    AddNamespaceIfNotDeclared(string.Empty, uri, uriDictionaryString);
                    return string.Empty;
                }
                else
                {
                    for (int i = 0; i < prefixes.Length; i++)
                    {
                        string prefix = prefixes[i];
                        bool declared = false;
                        for (int j = nsCount - 1; j >= nsTop; j--)
                        {
                            Namespace nameSpace = namespaces[j];
                            if (nameSpace.Prefix == prefix)
                            {
                                declared = true;
                                break;
                            }
                        }
                        if (!declared)
                        {
                            AddNamespace(prefix, uri, uriDictionaryString);
                            return prefix;
                        }
                    }
                }
                return null;
            }

            public void AddNamespaceIfNotDeclared(string prefix, string uri, XmlDictionaryString uriDictionaryString)
            {
                if (LookupNamespace(prefix) != uri)
                {
                    AddNamespace(prefix, uri, uriDictionaryString);
                }
            }

            public void AddNamespace(string prefix, string uri, XmlDictionaryString uriDictionaryString)
            {
                if (prefix.Length >= 3)
                {
                    // Upper and lower case letter differ by a bit.
                    if ((prefix[0] & ~32) == 'X' && (prefix[1] & ~32) == 'M' && (prefix[2] & ~32) == 'L')
                    {
                        if (prefix == "xml" && uri == xmlNamespace)
                            return;
                        if (prefix == "xmlns" && uri == xmlnsNamespace)
                            return;
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlReservedPrefix), "prefix"));
                    }
                }
                Namespace nameSpace;
                for (int i = nsCount - 1; i >= 0; i--)
                {
                    nameSpace = namespaces[i];
                    if (nameSpace.Depth != depth)
                        break;
                    if (nameSpace.Prefix == prefix)
                    {
                        if (nameSpace.Uri == uri)
                            return;
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlPrefixBoundToNamespace, prefix, nameSpace.Uri, uri), "prefix"));
                    }
                }
                if (prefix.Length != 0 && uri.Length == 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlEmptyNamespaceRequiresNullPrefix), "prefix"));
                if (uri.Length == xmlnsNamespace.Length && uri == xmlnsNamespace)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlSpecificBindingNamespace, "xmlns", uri)));
                // The addressing namespace and the xmlNamespace are the same length, so add a quick check to try to disambiguate
                if (uri.Length == xmlNamespace.Length && uri[18] == 'X' && uri == xmlNamespace)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlSpecificBindingNamespace, "xml", uri)));

                if (namespaces.Length == nsCount)
                {
                    Namespace[] newNamespaces = new Namespace[nsCount * 2];
                    Array.Copy(namespaces, newNamespaces, nsCount);
                    namespaces = newNamespaces;
                }
                nameSpace = namespaces[nsCount];
                if (nameSpace == null)
                {
                    nameSpace = new Namespace();
                    namespaces[nsCount] = nameSpace;
                }
                nameSpace.Depth = depth;
                nameSpace.Prefix = prefix;
                nameSpace.Uri = uri;
                nameSpace.UriDictionaryString = uriDictionaryString;
                nsCount++;
                lastNameSpace = null;
            }

            public string LookupPrefix(string ns)
            {
                if (lastNameSpace != null && lastNameSpace.Uri == ns)
                    return lastNameSpace.Prefix;
                int nsCount = this.nsCount;
                for (int i = nsCount - 1; i >= nsTop; i--)
                {
                    Namespace nameSpace = namespaces[i];
                    if (object.ReferenceEquals(nameSpace.Uri, ns))
                    {
                        string prefix = nameSpace.Prefix;
                        // Make sure that the prefix refers to the namespace in scope
                        bool declared = false;
                        for (int j = i + 1; j < nsCount; j++)
                        {
                            if (namespaces[j].Prefix == prefix)
                            {
                                declared = true;
                                break;
                            }
                        }
                        if (!declared)
                        {
                            lastNameSpace = nameSpace;
                            return prefix;
                        }
                    }
                }
                for (int i = nsCount - 1; i >= nsTop; i--)
                {
                    Namespace nameSpace = namespaces[i];
                    if (nameSpace.Uri == ns)
                    {
                        string prefix = nameSpace.Prefix;
                        // Make sure that the prefix refers to the namespace in scope
                        bool declared = false;
                        for (int j = i + 1; j < nsCount; j++)
                        {
                            if (namespaces[j].Prefix == prefix)
                            {
                                declared = true;
                                break;
                            }
                        }
                        if (!declared)
                        {
                            lastNameSpace = nameSpace;
                            return prefix;
                        }
                    }
                }

                if (ns.Length == 0)
                {
                    // Make sure the default binding is still valid
                    bool emptyPrefixUnassigned = true;
                    for (int i = nsCount - 1; i >= nsTop; i--)
                    {
                        if (namespaces[i].Prefix.Length == 0)
                        {
                            emptyPrefixUnassigned = false;
                            break;
                        }
                    }
                    if (emptyPrefixUnassigned)
                        return string.Empty;
                }

                if (ns == xmlnsNamespace)
                    return "xmlns";
                if (ns == xmlNamespace)
                    return "xml";
                return null;
            }

            public string LookupAttributePrefix(string ns)
            {
                if (lastNameSpace != null && lastNameSpace.Uri == ns && lastNameSpace.Prefix.Length != 0)
                    return lastNameSpace.Prefix;

                int nsCount = this.nsCount;
                for (int i = nsCount - 1; i >= nsTop; i--)
                {
                    Namespace nameSpace = namespaces[i];

                    if (object.ReferenceEquals(nameSpace.Uri, ns))
                    {
                        string prefix = nameSpace.Prefix;
                        if (prefix.Length != 0)
                        {
                            // Make sure that the prefix refers to the namespace in scope
                            bool declared = false;
                            for (int j = i + 1; j < nsCount; j++)
                            {
                                if (namespaces[j].Prefix == prefix)
                                {
                                    declared = true;
                                    break;
                                }
                            }
                            if (!declared)
                            {
                                lastNameSpace = nameSpace;
                                return prefix;
                            }
                        }
                    }
                }
                for (int i = nsCount - 1; i >= nsTop; i--)
                {
                    Namespace nameSpace = namespaces[i];
                    if (nameSpace.Uri == ns)
                    {
                        string prefix = nameSpace.Prefix;
                        if (prefix.Length != 0)
                        {
                            // Make sure that the prefix refers to the namespace in scope
                            bool declared = false;
                            for (int j = i + 1; j < nsCount; j++)
                            {
                                if (namespaces[j].Prefix == prefix)
                                {
                                    declared = true;
                                    break;
                                }
                            }
                            if (!declared)
                            {
                                lastNameSpace = nameSpace;
                                return prefix;
                            }
                        }
                    }
                }
                if (ns.Length == 0)
                    return string.Empty;
                return null;
            }

            public string LookupNamespace(string prefix)
            {
                int nsCount = this.nsCount;
                if (prefix.Length == 0)
                {
                    for (int i = nsCount - 1; i >= nsTop; i--)
                    {
                        Namespace nameSpace = namespaces[i];
                        if (nameSpace.Prefix.Length == 0)
                            return nameSpace.Uri;
                    }
                    return string.Empty;
                }
                if (prefix.Length == 1)
                {
                    char prefixChar = prefix[0];
                    for (int i = nsCount - 1; i >= nsTop; i--)
                    {
                        Namespace nameSpace = namespaces[i];
                        if (nameSpace.PrefixChar == prefixChar)
                            return nameSpace.Uri;
                    }
                    return null;
                }
                for (int i = nsCount - 1; i >= nsTop; i--)
                {
                    Namespace nameSpace = namespaces[i];
                    if (nameSpace.Prefix == prefix)
                        return nameSpace.Uri;
                }
                if (prefix == "xmlns")
                    return xmlnsNamespace;
                if (prefix == "xml")
                    return xmlNamespace;
                return null;
            }

            public void Sign(XmlCanonicalWriter signingWriter)
            {
                int nsCount = this.nsCount;
                Fx.Assert(nsCount >= 1 && namespaces[0].Prefix.Length == 0 && namespaces[0].Uri.Length == 0, "");
                for (int i = 1; i < nsCount; i++)
                {
                    Namespace nameSpace = namespaces[i];

                    bool found = false;
                    for (int j = i + 1; j < nsCount && !found; j++)
                    {
                        found = (nameSpace.Prefix == namespaces[j].Prefix);
                    }

                    if (!found)
                    {
                        signingWriter.WriteXmlnsAttribute(nameSpace.Prefix, nameSpace.Uri);
                    }
                }
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

                public void Clear()
                {
                    this.lang = null;
                }
            }

            class Namespace
            {
                string prefix;
                string ns;
                XmlDictionaryString xNs;
                int depth;
                char prefixChar;

                public Namespace()
                {
                }

                public void Clear()
                {
                    this.prefix = null;
                    this.prefixChar = (char)0;
                    this.ns = null;
                    this.xNs = null;
                    this.depth = 0;
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

                public char PrefixChar
                {
                    get
                    {
                        return prefixChar;
                    }
                }

                public string Prefix
                {
                    get
                    {
                        return prefix;
                    }
                    set
                    {
                        if (value.Length == 1)
                            prefixChar = value[0];
                        else
                            prefixChar = (char)0;
                        prefix = value;
                    }
                }

                public string Uri
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

                public XmlDictionaryString UriDictionaryString
                {
                    get
                    {
                        return xNs;
                    }
                    set
                    {
                        xNs = value;
                    }
                }
            }
        }

        class XmlBaseWriterNodeWriterAsyncHelper
        {
            static AsyncEventArgsCallback onWriteComplete;

            XmlBaseWriter writer;
            byte[] buffer;
            int offset;
            int count;
            int actualByteCount;
            int totalByteCount;
            AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs> nodeWriterAsyncState;
            XmlNodeWriterWriteBase64TextArgs nodeWriterArgs;
            AsyncEventArgs<XmlWriteBase64AsyncArguments> inputState;

            public XmlBaseWriterNodeWriterAsyncHelper(XmlBaseWriter writer)
            {
                this.writer = writer;
            }

            public void SetArguments(AsyncEventArgs<XmlWriteBase64AsyncArguments> inputState)
            {
                Fx.Assert(inputState != null, "InputState cannot be null.");
                this.inputState = inputState;
                this.buffer = inputState.Arguments.Buffer;
                this.offset = inputState.Arguments.Offset;
                this.count = inputState.Arguments.Count;
            }

            public AsyncCompletionResult StartAsync()
            {
                bool completeSelf = true;

                if (this.count > 0)
                {
                    // Bytes that have been already been read.
                    if (this.writer.trailByteCount > 0)
                    {
                        // Copy over up to 3 trailing bytes into the trailBytes buffer.
                        while (this.writer.trailByteCount < 3 && this.count > 0)
                        {
                            this.writer.trailBytes[this.writer.trailByteCount++] = this.buffer[this.offset++];
                            this.count--;
                        }
                    }

                    this.totalByteCount = this.writer.trailByteCount + this.count;
                    this.actualByteCount = this.totalByteCount - (this.totalByteCount % 3);

                    if (this.writer.trailBytes == null)
                    {
                        this.writer.trailBytes = new byte[3];
                    }

                    if (actualByteCount >= 3)
                    {
                        if (this.writer.attributeValue != null)
                        {
                            this.writer.WriteAttributeText(XmlConverter.Base64Encoding.GetString(this.writer.trailBytes, 0, this.writer.trailByteCount));
                            this.writer.WriteAttributeText(XmlConverter.Base64Encoding.GetString(this.buffer, this.offset, actualByteCount - this.writer.trailByteCount));
                        }

                        // StartContent/WriteBase64Text/EndContent will be called from HandleWriteBase64 as appropriate
                        completeSelf = HandleWriteBase64Text(false);
                    }
                    else
                    {
                        Buffer.BlockCopy(this.buffer, this.offset, this.writer.trailBytes, this.writer.trailByteCount, this.count);
                        this.writer.trailByteCount += this.count;
                    }
                }

                if (completeSelf)
                {
                    this.Clear();
                    return AsyncCompletionResult.Completed;
                }

                return AsyncCompletionResult.Queued;
            }

            static void OnWriteComplete(IAsyncEventArgs asyncEventArgs)
            {
                bool completeSelf = false;
                Exception completionException = null;
                XmlBaseWriterNodeWriterAsyncHelper thisPtr = (XmlBaseWriterNodeWriterAsyncHelper)asyncEventArgs.AsyncState;
                AsyncEventArgs<XmlWriteBase64AsyncArguments> inputState = thisPtr.inputState;

                try
                {
                    if (asyncEventArgs.Exception != null)
                    {
                        completionException = asyncEventArgs.Exception;
                        completeSelf = true;
                    }
                    else
                    {
                        completeSelf = thisPtr.HandleWriteBase64Text(true);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    completionException = exception;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.Clear();
                    inputState.Complete(false, completionException);
                }
            }

            bool HandleWriteBase64Text(bool isAsyncCallback)
            {
                Fx.Assert(this.count > 0 && this.actualByteCount >= 3, "HandleWriteBase64Text cannot be invoked with less than 3 bytes.");
                if (!writer.isXmlnsAttribute)
                {
                    if (!isAsyncCallback)
                    {
                        if (this.nodeWriterAsyncState == null)
                        {
                            this.nodeWriterAsyncState = new AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs>();
                            this.nodeWriterArgs = new XmlNodeWriterWriteBase64TextArgs();
                        }
                        if (onWriteComplete == null)
                        {
                            onWriteComplete = new AsyncEventArgsCallback(OnWriteComplete);
                        }

                        this.writer.StartContent();
                        this.nodeWriterArgs.TrailBuffer = this.writer.trailBytes;
                        this.nodeWriterArgs.TrailCount = this.writer.trailByteCount;
                        this.nodeWriterArgs.Buffer = this.buffer;
                        this.nodeWriterArgs.Offset = this.offset;
                        this.nodeWriterArgs.Count = this.actualByteCount - this.writer.trailByteCount;

                        this.nodeWriterAsyncState.Set(onWriteComplete, this.nodeWriterArgs, this);
                        if (this.writer.writer.WriteBase64TextAsync(this.nodeWriterAsyncState) != AsyncCompletionResult.Completed)
                        {
                            return false;
                        }

                        this.nodeWriterAsyncState.Complete(true);
                    }

                    this.writer.EndContent();
                }

                this.writer.trailByteCount = (this.totalByteCount - this.actualByteCount);
                if (this.writer.trailByteCount > 0)
                {
                    int trailOffset = offset + count - this.writer.trailByteCount;
                    for (int i = 0; i < this.writer.trailByteCount; i++)
                        this.writer.trailBytes[i] = this.buffer[trailOffset++];
                }

                return true;
            }

            void Clear()
            {
                this.inputState = null;
                this.buffer = null;
                this.offset = 0;
                this.count = 0;
                this.actualByteCount = 0;
                this.totalByteCount = 0;
            }
        }
    }
}
