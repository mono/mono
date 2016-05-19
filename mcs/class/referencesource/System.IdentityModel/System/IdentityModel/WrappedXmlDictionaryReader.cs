//-----------------------------------------------------------------------
// <copyright file="WrappedXmlDictionaryReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Xml;

    /// <summary>
    /// This class wraps a given _reader and delegates all calls to it. 
    /// XmlDictionaryReader class does not provide a way to set the _reader
    /// Quotas on the XmlDictionaryReader.CreateDictionaryReader(XmlReader)
    /// API. This class overrides XmlDictionaryReader.Quotas property and 
    /// hence custom quotas can be specified.
    /// </summary>
    internal class WrappedXmlDictionaryReader : XmlDictionaryReader, IXmlLineInfo
    {
        private XmlReader reader;
        private XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas;

        public WrappedXmlDictionaryReader(
            XmlReader reader,
            XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (xmlDictionaryReaderQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlDictionaryReaderQuotas");
            }

            this.reader = reader;
            this.xmlDictionaryReaderQuotas = xmlDictionaryReaderQuotas;
        }

        public override int AttributeCount
        {
            get
            {
                return this.reader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.reader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get { return this.reader.CanReadBinaryContent; }
        }

        public override bool CanReadValueChunk
        {
            get { return this.reader.CanReadValueChunk; }
        }

        public override int Depth
        {
            get
            {
                return this.reader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.reader.EOF;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.reader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.reader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.reader.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.reader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.reader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.reader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.reader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.reader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.reader.QuoteChar;
            }
        }

        public override ReadState ReadState
        {
            get
            {
                return this.reader.ReadState;
            }
        }

        public override string Value
        {
            get
            {
                return this.reader.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.reader.XmlLang;
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                return this.reader.XmlSpace;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.reader.ValueType;
            }
        }

        public int LineNumber
        {
            get
            {
                IXmlLineInfo lineInfo = this.reader as IXmlLineInfo;

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
                IXmlLineInfo lineInfo = this.reader as IXmlLineInfo;

                if (lineInfo == null)
                {
                    return 1;
                }

                return lineInfo.LinePosition;
            }
        }

        public override XmlDictionaryReaderQuotas Quotas
        {
            get
            {
                return this.xmlDictionaryReaderQuotas;
            }
        }

        public override string this[int index]
        {
            get
            {
                return this.reader[index];
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.reader[name];
            }
        }

        public override string this[string name, string namespaceUri]
        {
            get
            {
                return this.reader[name, namespaceUri];
            }
        }

        public override void Close()
        {
            this.reader.Close();
        }

        public override string GetAttribute(int index)
        {
            return this.reader.GetAttribute(index);
        }

        public override string GetAttribute(string name)
        {
            return this.reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceUri)
        {
            return this.reader.GetAttribute(name, namespaceUri);
        }

        public override bool IsStartElement(string name)
        {
            return this.reader.IsStartElement(name);
        }

        public override bool IsStartElement(string localName, string namespaceUri)
        {
            return this.reader.IsStartElement(localName, namespaceUri);
        }

        public override string LookupNamespace(string namespaceUri)
        {
            return this.reader.LookupNamespace(namespaceUri);
        }

        public override void MoveToAttribute(int index)
        {
            this.reader.MoveToAttribute(index);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string namespaceUri)
        {
            return this.reader.MoveToAttribute(name, namespaceUri);
        }

        public override bool MoveToElement()
        {
            return this.reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.reader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            return this.reader.Read();
        }

        public override bool ReadAttributeValue()
        {
            return this.reader.ReadAttributeValue();
        }

        public override string ReadElementString(string name)
        {
            return this.reader.ReadElementString(name);
        }

        public override string ReadElementString(string localName, string namespaceUri)
        {
            return this.reader.ReadElementString(localName, namespaceUri);
        }

        public override string ReadInnerXml()
        {
            return this.reader.ReadInnerXml();
        }

        public override string ReadOuterXml()
        {
            return this.reader.ReadOuterXml();
        }

        public override void ReadStartElement(string name)
        {
            this.reader.ReadStartElement(name);
        }

        public override void ReadStartElement(string localName, string namespaceUri)
        {
            this.reader.ReadStartElement(localName, namespaceUri);
        }

        public override void ReadEndElement()
        {
            this.reader.ReadEndElement();
        }

        public override string ReadString()
        {
            return this.reader.ReadString();
        }

        public override void ResolveEntity()
        {
            this.reader.ResolveEntity();
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
        {
            return this.reader.ReadElementContentAsBase64(buffer, offset, count);
        }

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
        {
            return this.reader.ReadContentAsBase64(buffer, offset, count);
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
        {
            return this.reader.ReadElementContentAsBinHex(buffer, offset, count);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
        {
            return this.reader.ReadContentAsBinHex(buffer, offset, count);
        }

        public override int ReadValueChunk(char[] chars, int offset, int count)
        {
            return this.reader.ReadValueChunk(chars, offset, count);
        }

        public override bool ReadContentAsBoolean()
        {
            return this.reader.ReadContentAsBoolean();
        }

        public override DateTime ReadContentAsDateTime()
        {
            return this.reader.ReadContentAsDateTime();
        }

        public override decimal ReadContentAsDecimal()
        {
            return (decimal)this.reader.ReadContentAs(typeof(decimal), null);
        }

        public override double ReadContentAsDouble()
        {
            return this.reader.ReadContentAsDouble();
        }

        public override int ReadContentAsInt()
        {
            return this.reader.ReadContentAsInt();
        }

        public override long ReadContentAsLong()
        {
            return this.reader.ReadContentAsLong();
        }

        public override float ReadContentAsFloat()
        {
            return this.reader.ReadContentAsFloat();
        }

        public override string ReadContentAsString()
        {
            return this.reader.ReadContentAsString();
        }

        public override object ReadContentAs(Type valueType, IXmlNamespaceResolver namespaceResolver)
        {
            return this.reader.ReadContentAs(valueType, namespaceResolver);
        }

        public bool HasLineInfo()
        {
            IXmlLineInfo lineInfo = this.reader as IXmlLineInfo;

            if (lineInfo == null)
            {
                return false;
            }

            return lineInfo.HasLineInfo();
        }
    }
}
