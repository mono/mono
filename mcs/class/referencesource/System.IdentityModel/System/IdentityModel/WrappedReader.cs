//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.IdentityModel
{
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Diagnostics;
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;

    sealed class WrappedReader : DelegatingXmlDictionaryReader, IXmlLineInfo
    {
        XmlTokenStream xmlTokens;
        MemoryStream contentStream;
        TextReader contentReader;
        bool recordDone;
        int depth;
        bool disposed;

        public WrappedReader(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (!reader.IsStartElement())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InnerReaderMustBeAtElement)));
            }
            this.xmlTokens = new XmlTokenStream(32);
            base.InitializeInnerReader(reader);
            Record();
        }

        public int LineNumber
        {
            get
            {
                IXmlLineInfo lineInfo = base.InnerReader as IXmlLineInfo;
                if (lineInfo == null)
                {
                    return 1;
                }
                return lineInfo.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                IXmlLineInfo lineInfo = base.InnerReader as IXmlLineInfo;
                if (lineInfo == null)
                {
                    return 1;
                }
                return lineInfo.LinePosition;
            }
        }

        public XmlTokenStream XmlTokens
        {
            get { return this.xmlTokens; }
        }

        public override void Close()
        {
            OnEndOfContent();
            base.InnerReader.Close();
        }

        public bool HasLineInfo()
        {
            IXmlLineInfo lineInfo = base.InnerReader as IXmlLineInfo;
            return lineInfo != null && lineInfo.HasLineInfo();
        }

        public override void MoveToAttribute(int index)
        {
            OnEndOfContent();
            base.InnerReader.MoveToAttribute(index);
        }

        public override bool MoveToAttribute(string name)
        {
            OnEndOfContent();
            return base.InnerReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            OnEndOfContent();
            return base.InnerReader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            OnEndOfContent();
            return base.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            OnEndOfContent();
            return base.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            OnEndOfContent();
            return base.MoveToNextAttribute();
        }

        void OnEndOfContent()
        {
            if (this.contentReader != null)
            {
                this.contentReader.Close();
                this.contentReader = null;
            }
            if (this.contentStream != null)
            {
                this.contentStream.Close();
                this.contentStream = null;
            }
        }

        public override bool Read()
        {
            OnEndOfContent();
            if (!base.Read())
            {
                return false;
            }
            if (!this.recordDone)
            {
                Record();
            }
            return true;
        }

        int ReadBinaryContent(byte[] buffer, int offset, int count, bool isBase64)
        {
            CryptoHelper.ValidateBufferBounds(buffer, offset, count);

            //
            // Concatentate text nodes to get entire element value before attempting to convert
            // XmlDictionaryReader.CreateDictionaryReader( XmlReader ) creates a reader that returns base64 in a single text node
            // XmlDictionaryReader.CreateTextReader( Stream ) creates a reader that produces multiple text and whitespace nodes
            // Attribute nodes consist of only a single value
            //
            if (this.contentStream == null)
            {
                string encodedValue;
                if (NodeType == XmlNodeType.Attribute)
                {
                    encodedValue = Value;
                }
                else
                {
                    StringBuilder fullText = new StringBuilder(1000);
                    while (NodeType != XmlNodeType.Element && NodeType != XmlNodeType.EndElement)
                    {
                        switch (NodeType)
                        {
                            // concatenate text nodes
                            case XmlNodeType.Text:
                                fullText.Append(Value);
                                break;

                            // skip whitespace
                            case XmlNodeType.Whitespace:
                                break;
                        }

                        Read();
                    }

                    encodedValue = fullText.ToString();
                }

                byte[] value = isBase64 ? Convert.FromBase64String(encodedValue) : HexBinary.Parse(encodedValue).Value;
                this.contentStream = new MemoryStream(value);
            }

            int read = this.contentStream.Read(buffer, offset, count);
            if (read == 0)
            {
                this.contentStream.Close();
                this.contentStream = null;
            }

            return read;
        }

        // CodeReviewQ: The commented method was original System.IdentityModel method to ReadBinaryContent. The one in Microsoft,IdentityModel is coded differently.  See above.
        // I have done primitive conceptual level validation and have kept the M.IM's method and have commented the S.IM's method. But we need prescriptive guidance
        // here as we may be potentailly breaking WCF customers with this change. To avoid breaking WCF's existing customers we need to uncommented the method below and comment the one above.

        //int ReadBinaryContent(byte[] buffer, int offset, int count, bool isBase64)
        //{
        //    CryptoHelper.ValidateBufferBounds(buffer, offset, count);
        //    int read = 0;
        //    while (count > 0 && this.NodeType != XmlNodeType.Element && this.NodeType != XmlNodeType.EndElement)
        //    {
        //        if (this.contentStream == null)
        //        {
        //            byte[] value = isBase64 ? Convert.FromBase64String(this.Value) : HexBinary.Parse(this.Value).Value;
        //            this.contentStream = new MemoryStream(value);
        //        }
        //        int actual = this.contentStream.Read(buffer, offset, count);
        //        if (actual == 0)
        //        {
        //            if (this.NodeType == XmlNodeType.Attribute)
        //            {
        //                break;
        //            }
        //            if (!Read())
        //            {
        //                break;
        //            }
        //        }
        //        read += actual;
        //        offset += actual;
        //        count -= actual;
        //    }
        //    return read;
        //}

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
        {
            return ReadBinaryContent(buffer, offset, count, true);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
        {
            return ReadBinaryContent(buffer, offset, count, false);
        }

        public override int ReadValueChunk(char[] chars, int offset, int count)
        {
            if (this.contentReader == null)
            {
                this.contentReader = new StringReader(Value);
            }
            return this.contentReader.Read(chars, offset, count);
        }

        void Record()
        {
            switch (NodeType)
            {
                case XmlNodeType.Element:
                    {
                        bool isEmpty = base.InnerReader.IsEmptyElement;
                        this.xmlTokens.AddElement(base.InnerReader.Prefix, base.InnerReader.LocalName, base.InnerReader.NamespaceURI, isEmpty);
                        if (base.InnerReader.MoveToFirstAttribute())
                        {
                            do
                            {
                                this.xmlTokens.AddAttribute(base.InnerReader.Prefix, base.InnerReader.LocalName, base.InnerReader.NamespaceURI, base.InnerReader.Value);
                            }
                            while (base.InnerReader.MoveToNextAttribute());
                            base.InnerReader.MoveToElement();
                        }
                        if (!isEmpty)
                        {
                            this.depth++;
                        }
                        else if (this.depth == 0)
                        {
                            this.recordDone = true;
                        }
                        break;
                    }
                case XmlNodeType.CDATA:
                case XmlNodeType.Comment:
                case XmlNodeType.Text:
                case XmlNodeType.EntityReference:
                case XmlNodeType.EndEntity:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                    {
                        this.xmlTokens.Add(NodeType, Value);
                        break;
                    }
                case XmlNodeType.EndElement:
                    {
                        this.xmlTokens.Add(NodeType, Value);
                        if (--this.depth == 0)
                        {
                            this.recordDone = true;
                        }
                        break;
                    }
                case XmlNodeType.DocumentType:
                case XmlNodeType.XmlDeclaration:
                    {
                        break;
                    }
                default:
                    {
                       
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnsupportedNodeTypeInReader,
                     base.InnerReader.NodeType, base.InnerReader.Name)));
                            
                    }

            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                //
                // Free all of our managed resources
                //
                if (this.contentReader != null)
                {
                    this.contentReader.Dispose();
                    this.contentReader = null;
                }

                if (this.contentStream != null)
                {
                    this.contentStream.Dispose();
                    this.contentStream = null;
                }
            }

            // Free native resources, if any.

            this.disposed = true;
        }
    }

    sealed internal class XmlTokenStream : ISecurityElement
    {
        int count;
        XmlTokenEntry[] entries;
        string excludedElement;
        int? excludedElementDepth;
        string excludedElementNamespace;

        public XmlTokenStream(int initialSize)
        {
            if (initialSize < 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("initialSize", SR.GetString(SR.ValueMustBeGreaterThanZero)));
            }
            this.entries = new XmlTokenEntry[initialSize];
        }
        
        // This constructor is used by the Trim method to reduce the size of the XmlTokenEntry array to the minimum required.
        public XmlTokenStream(XmlTokenStream other)
        {
            this.count = other.count;
            this.excludedElement = other.excludedElement;
            this.excludedElementDepth = other.excludedElementDepth;
            this.excludedElementNamespace = other.excludedElementNamespace;
            this.entries = new XmlTokenEntry[this.count];
            Array.Copy(other.entries, this.entries, this.count);
        }
        
        public void Add(XmlNodeType type, string value)
        {
            EnsureCapacityToAdd();
            this.entries[this.count++].Set(type, value);
        }

        public void AddAttribute(string prefix, string localName, string namespaceUri, string value)
        {
            EnsureCapacityToAdd();
            this.entries[this.count++].SetAttribute(prefix, localName, namespaceUri, value);
        }

        public void AddElement(string prefix, string localName, string namespaceUri, bool isEmptyElement)
        {
            EnsureCapacityToAdd();
            this.entries[this.count++].SetElement(prefix, localName, namespaceUri, isEmptyElement);
        }

        void EnsureCapacityToAdd()
        {
            if (this.count == this.entries.Length)
            {
                XmlTokenEntry[] newBuffer = new XmlTokenEntry[this.entries.Length * 2];
                Array.Copy(this.entries, 0, newBuffer, 0, this.count);
                this.entries = newBuffer;
            }
        }

        public void SetElementExclusion(string excludedElement, string excludedElementNamespace)
        {
            SetElementExclusion(excludedElement, excludedElementNamespace, null);
        }

        public void SetElementExclusion(string excludedElement, string excludedElementNamespace, int? excludedElementDepth)
        {
            this.excludedElement = excludedElement;
            this.excludedElementDepth = excludedElementDepth;
            this.excludedElementNamespace = excludedElementNamespace;
        }

        /// <summary>
        /// Free unneeded entries from array
        /// </summary>
        /// <returns></returns>
        public XmlTokenStream Trim()
        {
            return new XmlTokenStream(this);
        }

        public XmlTokenStreamWriter GetWriter()
        {
            return new XmlTokenStreamWriter( entries, count, excludedElement, excludedElementDepth, excludedElementNamespace );
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.GetWriter().WriteTo(writer, dictionaryManager);
        }

        bool ISecurityElement.HasId
        {
            get { return false; }
        }

        string ISecurityElement.Id
        {
            get { return null; }
        }

        internal class XmlTokenStreamWriter : ISecurityElement
        {
            XmlTokenEntry[] entries;
            int count;
            int position;
            string excludedElement;
            int? excludedElementDepth;
            string excludedElementNamespace;

            public XmlTokenStreamWriter(XmlTokenEntry[] entries,
                                         int count,
                                         string excludedElement,
                                         int? excludedElementDepth,
                                         string excludedElementNamespace)
            {
                if (entries == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entries");
                }
                this.entries = entries;
                this.count = count;
                this.excludedElement = excludedElement;
                this.excludedElementDepth = excludedElementDepth;
                this.excludedElementNamespace = excludedElementNamespace;
            }

            public int Count
            {
                get { return this.count; }
            }

            public int Position
            {
                get { return this.position; }
            }

            public XmlNodeType NodeType
            {
                get { return this.entries[this.position].nodeType; }
            }

            public bool IsEmptyElement
            {
                get { return this.entries[this.position].IsEmptyElement; }
            }

            public string Prefix
            {
                get { return this.entries[this.position].prefix; }
            }

            public string LocalName
            {
                get { return this.entries[this.position].localName; }
            }

            public string NamespaceUri
            {
                get { return this.entries[this.position].namespaceUri; }
            }

            public string Value
            {
                get { return this.entries[this.position].Value; }
            }

            public string ExcludedElement
            {
                get { return this.excludedElement; }
            }

            public string ExcludedElementNamespace
            {
                get { return this.excludedElementNamespace; }
            }
            bool ISecurityElement.HasId
            {
                get { return false; }
            }

            string ISecurityElement.Id
            {
                get { return null; }
            }

            public bool MoveToFirst()
            {
                this.position = 0;
                return this.count > 0;
            }

            public bool MoveToFirstAttribute()
            {
                DiagnosticUtility.DebugAssert(this.NodeType == XmlNodeType.Element, "");
                if (this.position < this.Count - 1 && this.entries[this.position + 1].nodeType == XmlNodeType.Attribute)
                {
                    this.position++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool MoveToNext()
            {
                if (this.position < this.count - 1)
                {
                    this.position++;
                    return true;
                }
                return false;
            }

            public bool MoveToNextAttribute()
            {
                DiagnosticUtility.DebugAssert(this.NodeType == XmlNodeType.Attribute, "");
                if (this.position < this.count - 1 && this.entries[this.position + 1].nodeType == XmlNodeType.Attribute)
                {
                    this.position++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
            {
                if (writer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
                }
                if (!MoveToFirst())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlTokenBufferIsEmpty)));
                }
                int depth = 0;
                int recordedDepth = -1;
                bool include = true;
                do
                {
                    switch (this.NodeType)
                    {
                        case XmlNodeType.Element:
                            bool isEmpty = this.IsEmptyElement;
                            depth++;
                            if (include
                                && (null == excludedElementDepth || excludedElementDepth == (depth - 1))
                                && this.LocalName == this.excludedElement 
                                && this.NamespaceUri == this.excludedElementNamespace)
                            {
                                include = false;
                                recordedDepth = depth;
                            }
                            if (include)
                            {
                                writer.WriteStartElement(this.Prefix, this.LocalName, this.NamespaceUri);
                            }
                            if (MoveToFirstAttribute())
                            {
                                do
                                {
                                    if (include)
                                    {
                                        writer.WriteAttributeString(this.Prefix, this.LocalName, this.NamespaceUri, this.Value);
                                    }
                                }
                                while (MoveToNextAttribute());
                            }
                            if (isEmpty)
                            {
                                goto case XmlNodeType.EndElement;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (include)
                            {
                                writer.WriteEndElement();
                            }
                            else if (recordedDepth == depth)
                            {
                                include = true;
                                recordedDepth = -1;
                            }
                            depth--;
                            break;
                        case XmlNodeType.CDATA:
                            if (include)
                            {
                                writer.WriteCData(this.Value);
                            }
                            break;
                        case XmlNodeType.Comment:
                            if (include)
                            {
                                writer.WriteComment(this.Value);
                            }
                            break;
                        case XmlNodeType.Text:
                            if (include)
                            {
                                writer.WriteString(this.Value);
                            }
                            break;
                        case XmlNodeType.SignificantWhitespace:
                        case XmlNodeType.Whitespace:
                            if (include)
                            {
                                writer.WriteWhitespace(this.Value);
                            }
                            break;
                        case XmlNodeType.DocumentType:
                        case XmlNodeType.XmlDeclaration:
                            break;
                    }
                }
                while (MoveToNext());
            }

        }       
      
        internal struct XmlTokenEntry
        {
            internal XmlNodeType nodeType;
            internal string prefix;
            internal string localName;
            internal string namespaceUri;
            string value;

            public bool IsEmptyElement
            {
                get { return this.value == null; }
                set { this.value = value ? null : ""; }
            }

            public string Value
            {
                get { return this.value; }
            }

            public void Set(XmlNodeType nodeType, string value)
            {
                this.nodeType = nodeType;
                this.value = value;
            }

            public void SetAttribute(string prefix, string localName, string namespaceUri, string value)
            {
                this.nodeType = XmlNodeType.Attribute;
                this.prefix = prefix;
                this.localName = localName;
                this.namespaceUri = namespaceUri;
                this.value = value;
            }

            public void SetElement(string prefix, string localName, string namespaceUri, bool isEmptyElement)
            {
                this.nodeType = XmlNodeType.Element;
                this.prefix = prefix;
                this.localName = localName;
                this.namespaceUri = namespaceUri;
                this.IsEmptyElement = isEmptyElement;
            }
        }
    }
}
