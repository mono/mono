namespace System.Xml.Serialization 
{
    using System.IO;
    using System.Collections;
    using System.Text;
    using System;
    using System.Xml.Schema;

    internal class XmlCountingReader : XmlReader, IXmlTextParser, IXmlLineInfo
    {
        XmlReader innerReader;
        int advanceCount;

        internal XmlCountingReader(XmlReader xmlReader) 
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");
            this.innerReader = xmlReader;
            this.advanceCount = 0;
        }

        internal int AdvanceCount { get { return advanceCount; } }

        void IncrementCount()
        {
            if (advanceCount == Int32.MaxValue)
                advanceCount = 0;
            else
                advanceCount++;
        }

        // Properties (non-advancing)
        public override XmlReaderSettings Settings { get { return innerReader.Settings; } }
        public override XmlNodeType NodeType { get { return innerReader.NodeType; } }
        public override string Name { get { return innerReader.Name; } }
        public override string LocalName { get { return innerReader.LocalName; } }
        public override string NamespaceURI { get { return innerReader.NamespaceURI; } }
        public override string Prefix { get { return innerReader.Prefix; } }
        public override bool HasValue { get { return innerReader.HasValue; } }
        public override string Value { get { return innerReader.Value; } }
        public override int Depth { get { return innerReader.Depth; } }
        public override string BaseURI { get { return innerReader.BaseURI; } }
        public override bool IsEmptyElement { get { return innerReader.IsEmptyElement; } }
        public override bool IsDefault { get { return innerReader.IsDefault; } }
        public override char QuoteChar { get { return innerReader.QuoteChar; } }
        public override XmlSpace XmlSpace { get { return innerReader.XmlSpace; } }
        public override string XmlLang { get { return innerReader.XmlLang; } }
        public override IXmlSchemaInfo SchemaInfo { get { return innerReader.SchemaInfo; } }
        public override Type ValueType { get { return innerReader.ValueType; } }
        public override int AttributeCount { get { return innerReader.AttributeCount; } }
        public override string this[int i] { get { return innerReader[i]; } }
        public override string this[string name] { get { return innerReader[name]; } }
        public override string this[string name, string namespaceURI] { get { return innerReader[name, namespaceURI]; } }
        public override bool EOF { get { return innerReader.EOF; } }
        public override ReadState ReadState { get { return innerReader.ReadState; } }
        public override XmlNameTable NameTable { get { return innerReader.NameTable; } }
        public override bool CanResolveEntity { get { return innerReader.CanResolveEntity; } }
        public override bool CanReadBinaryContent { get { return innerReader.CanReadBinaryContent; } }
        public override bool CanReadValueChunk { get { return innerReader.CanReadValueChunk; } }
        public override bool HasAttributes { get { return innerReader.HasAttributes; } }

        // Methods (non-advancing)
        // Reader tends to under-count rather than over-count 
        public override void Close() { innerReader.Close(); }
        public override string GetAttribute(string name) { return innerReader.GetAttribute(name); }
        public override string GetAttribute(string name, string namespaceURI) { return innerReader.GetAttribute(name, namespaceURI); }
        public override string GetAttribute(int i) { return innerReader.GetAttribute(i); }
        public override bool MoveToAttribute(string name) { return innerReader.MoveToAttribute(name); }
        public override bool MoveToAttribute(string name, string ns) { return innerReader.MoveToAttribute(name, ns); }
        public override void MoveToAttribute(int i) { innerReader.MoveToAttribute(i); }
        public override bool MoveToFirstAttribute() { return innerReader.MoveToFirstAttribute(); }
        public override bool MoveToNextAttribute() { return innerReader.MoveToNextAttribute(); }
        public override bool MoveToElement() { return innerReader.MoveToElement(); }
        public override string LookupNamespace(string prefix) { return innerReader.LookupNamespace(prefix); }
        public override bool ReadAttributeValue() { return innerReader.ReadAttributeValue(); }
        public override void ResolveEntity() { innerReader.ResolveEntity(); }
        public override bool IsStartElement() { return innerReader.IsStartElement(); }
        public override bool IsStartElement(string name) { return innerReader.IsStartElement(name); }
        public override bool IsStartElement(string localname, string ns) { return innerReader.IsStartElement(localname, ns); }
        public override XmlReader ReadSubtree() { return innerReader.ReadSubtree(); }
        public override XmlNodeType MoveToContent() { return innerReader.MoveToContent(); }

        // Methods (advancing)
        public override bool Read()
        {
            IncrementCount();
            return innerReader.Read();
        }

        public override void Skip()
        {
            IncrementCount();
            innerReader.Skip();
        }

        public override string ReadInnerXml()
        {
            if (innerReader.NodeType != XmlNodeType.Attribute)
                IncrementCount();
            return innerReader.ReadInnerXml();
        }
        public override string ReadOuterXml()
        {
            if (innerReader.NodeType != XmlNodeType.Attribute)
                IncrementCount();
            return innerReader.ReadOuterXml();
        }
        public override object ReadContentAsObject() 
        {
            IncrementCount(); 
            return innerReader.ReadContentAsObject();
        }
        public override bool ReadContentAsBoolean() 
        {
            IncrementCount();
            return innerReader.ReadContentAsBoolean();
        }
        public override DateTime ReadContentAsDateTime() 
        {
            IncrementCount();
            return innerReader.ReadContentAsDateTime();
        }
        public override double ReadContentAsDouble() 
        {
            IncrementCount();
            return innerReader.ReadContentAsDouble();
        }
        public override int ReadContentAsInt()
        {
            IncrementCount();
            return innerReader.ReadContentAsInt();
        }
        public override long ReadContentAsLong()
        {
            IncrementCount();
            return innerReader.ReadContentAsLong();
        }
        public override string ReadContentAsString()
        {
            IncrementCount();
            return innerReader.ReadContentAsString();
        }
        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            IncrementCount();
            return innerReader.ReadContentAs(returnType, namespaceResolver);
        }
        public override object ReadElementContentAsObject()
        {
            IncrementCount();
            return innerReader.ReadElementContentAsObject();
        }
        public override object ReadElementContentAsObject(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsObject(localName, namespaceURI);
        }
        public override bool ReadElementContentAsBoolean()
        {
            IncrementCount();
            return innerReader.ReadElementContentAsBoolean();
        }
        public override bool ReadElementContentAsBoolean(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsBoolean(localName, namespaceURI);
        }
        public override DateTime ReadElementContentAsDateTime()
        {
            IncrementCount();
            return innerReader.ReadElementContentAsDateTime();
        }
        public override DateTime ReadElementContentAsDateTime(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsDateTime(localName, namespaceURI);
        }
        public override double ReadElementContentAsDouble()
        {
            IncrementCount();
            return innerReader.ReadElementContentAsDouble();
        }
        public override double ReadElementContentAsDouble(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsDouble(localName, namespaceURI);
        }
        public override int ReadElementContentAsInt()
        {
            IncrementCount();
            return innerReader.ReadElementContentAsInt();
        }
        public override int ReadElementContentAsInt(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsInt(localName, namespaceURI);
        }
        public override long ReadElementContentAsLong()
        {
            IncrementCount();
            return innerReader.ReadElementContentAsLong();
        }
        public override long ReadElementContentAsLong(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsLong(localName, namespaceURI);
        }
        public override string ReadElementContentAsString()
        {
            IncrementCount();
            return innerReader.ReadElementContentAsString();
        }
        public override string ReadElementContentAsString(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsString(localName, namespaceURI);
        }
        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            IncrementCount();
            return innerReader.ReadElementContentAs(returnType, namespaceResolver);
        }
        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI) 
        {
            IncrementCount();
            return innerReader.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI);
        }
        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            IncrementCount();
            return innerReader.ReadContentAsBase64(buffer, index, count);
        }
        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsBase64(buffer, index, count);
        }
        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            IncrementCount();
            return innerReader.ReadContentAsBinHex(buffer, index, count);
        }
        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            IncrementCount();
            return innerReader.ReadElementContentAsBinHex(buffer, index, count);
        }
        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            IncrementCount();
            return innerReader.ReadValueChunk(buffer, index, count);
        }
        public override string ReadString()
        {
            IncrementCount();
            return innerReader.ReadString();
        }
        public override void ReadStartElement()
        {
            IncrementCount();
            innerReader.ReadStartElement();
        }
        public override void ReadStartElement(string name)
        {
            IncrementCount();
            innerReader.ReadStartElement(name);
        }
        public override void ReadStartElement(string localname, string ns)
        {
            IncrementCount();
            innerReader.ReadStartElement(localname, ns);
        }
        public override string ReadElementString()
        {
            IncrementCount();
            return innerReader.ReadElementString();
        }
        public override string ReadElementString(string name)
        {
            IncrementCount();
            return innerReader.ReadElementString(name);
        }
        public override string ReadElementString(string localname, string ns)
        {
            IncrementCount();
            return innerReader.ReadElementString(localname, ns);
        }
        public override void ReadEndElement()
        {
            IncrementCount();
            innerReader.ReadEndElement();
        }
        public override bool ReadToFollowing(string name)
        {
            IncrementCount();
            return ReadToFollowing(name);
        }
        public override bool ReadToFollowing(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadToFollowing(localName, namespaceURI);
        }
        public override bool ReadToDescendant(string name)
        {
            IncrementCount();
            return innerReader.ReadToDescendant(name);
        }
        public override bool ReadToDescendant(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadToDescendant(localName, namespaceURI);
        }
        public override bool ReadToNextSibling(string name)
        {
            IncrementCount();
            return innerReader.ReadToNextSibling(name);
        }
        public override bool ReadToNextSibling(string localName, string namespaceURI)
        {
            IncrementCount();
            return innerReader.ReadToNextSibling(localName, namespaceURI);
        }

        // IDisposable interface
        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing) 
                {
                    IDisposable disposableReader = innerReader as IDisposable;
                    if (disposableReader != null)
                        disposableReader.Dispose();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        // IXmlTextParser members
        bool IXmlTextParser.Normalized
        {
            get
            {
                XmlTextReader xmlTextReader = innerReader as XmlTextReader;
                if (xmlTextReader == null)
                {
                    IXmlTextParser xmlTextParser = innerReader as IXmlTextParser;
                    return (xmlTextParser == null) ? false : xmlTextParser.Normalized;
                }
                else
                    return xmlTextReader.Normalization;
            }
            set
            {
                XmlTextReader xmlTextReader = innerReader as XmlTextReader;
                if (xmlTextReader == null) 
                {
                    IXmlTextParser xmlTextParser = innerReader as IXmlTextParser;
                    if (xmlTextParser != null) 
                        xmlTextParser.Normalized = value;
                }
                else
                    xmlTextReader.Normalization = value;
            }
        }

        WhitespaceHandling IXmlTextParser.WhitespaceHandling 
        {
            get
            {
                XmlTextReader xmlTextReader = innerReader as XmlTextReader;
                if (xmlTextReader == null)
                {
                    IXmlTextParser xmlTextParser = innerReader as IXmlTextParser;
                    return (xmlTextParser == null) ? WhitespaceHandling.None : xmlTextParser.WhitespaceHandling;
                }
                else
                    return xmlTextReader.WhitespaceHandling;
            }
            set 
            {
                XmlTextReader xmlTextReader = innerReader as XmlTextReader;
                if (xmlTextReader == null) 
                {
                    IXmlTextParser xmlTextParser = innerReader as IXmlTextParser;
                    if (xmlTextParser != null) 
                        xmlTextParser.WhitespaceHandling = value;
                }
                else
                    xmlTextReader.WhitespaceHandling = value;
            }
        }
        
        // IXmlLineInfo members
        bool IXmlLineInfo.HasLineInfo()
        {
            IXmlLineInfo iXmlLineInfo = innerReader as IXmlLineInfo;
            return (iXmlLineInfo == null) ? false : iXmlLineInfo.HasLineInfo();
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                IXmlLineInfo iXmlLineInfo = innerReader as IXmlLineInfo;
                return (iXmlLineInfo == null) ? 0 : iXmlLineInfo.LineNumber;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                IXmlLineInfo iXmlLineInfo = innerReader as IXmlLineInfo;
                return (iXmlLineInfo == null) ? 0 : iXmlLineInfo.LinePosition;
            }
        }

    }
}
