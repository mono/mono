//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Diagnostics;

#if NO
    public class XmlDelegatedReader : XmlDictionaryReader, IXmlLineInfo
    {
        XmlDictionaryReader reader;

        public XmlDelegatedReader(XmlDictionaryReader reader)
        {
            if (reader == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            this.reader = reader;
        }

        protected XmlDictionaryReader Reader
        {
            get
            {
                return reader;
            }
        }

        public override int AttributeCount
        {
            get
            {
                return reader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return reader.BaseURI;
            }
        }

        public override void Close()
        {
            reader.Close();
        }

        public override int Depth
        {
            get
            {
                return reader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return reader.EOF;
            }
        }

        public override string GetAttribute(int index)
        {
            return reader.GetAttribute(index);
        }

        public override string GetAttribute(string name)
        {
            return reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string ns)
        {
            return reader.GetAttribute(name, ns);
        }

        public override bool HasValue
        {
            get
            {
                return reader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return reader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return reader.IsEmptyElement;
            }
        }

        public override bool IsLocalName(string localName) 
        {
            return reader.IsLocalName(localName);
        }

        public override bool IsNamespaceUri(string ns) 
        {
            return reader.IsNamespaceUri(ns);
        }

        public override bool IsStartElement(string localName) 
        {
            return reader.IsStartElement(localName);
        }

        public override bool IsStartElement(string localName, string ns) 
        {
            return reader.IsStartElement(localName, ns);
        }

        public override string LocalName
        {
            get 
            {
                return reader.LocalName;
            }
        }

        public override string LookupNamespace(string ns)
        {
            return reader.LookupNamespace(ns);
        }

        public override void MoveToAttribute(int index)
        {
            reader.MoveToAttribute(index);
        }

        public override bool MoveToAttribute(string name)
        {
            return reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return reader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            return reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return reader.MoveToNextAttribute();
        }

        public override string Name
        {
            get
            {
                return reader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return reader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return reader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return reader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return reader.QuoteChar;
            }
        }

        public override bool Read()
        {
            return reader.Read();
        }

        public override bool ReadAttributeValue()
        {
            return reader.ReadAttributeValue();
        }

        public override string ReadElementString(string localName)
        {
            return reader.ReadElementString(localName);
        }

        public override string ReadElementString(string localName, string ns)
        {
            return reader.ReadElementString(localName, ns);
        }

        public override string ReadInnerXml()
        {
            return reader.ReadInnerXml();
        }

        public override string ReadOuterXml()
        {
            return reader.ReadOuterXml();
        }

        public override void ReadStartElement(string localName)
        {
            reader.ReadStartElement(localName);
        }

        public override void ReadStartElement(string localName, string ns)
        {
            reader.ReadStartElement(localName, ns);
        }

        public override void ReadEndElement()
        {
            reader.ReadEndElement();
        }

        public override string ReadString()
        {
            return reader.ReadString();
        }

        public override ReadState ReadState
        {
            get
            {
                return reader.ReadState;
            }
        }

        public override void ResolveEntity()
        {
            reader.ResolveEntity();
        }

        public override string this[int index]
        {
            get 
            {
                return reader[index];
            }
        }

        public override string this[string name]
        {
            get
            {
                return reader[name];
            }
        }

        public override string this[string name, string ns]
        {
            get
            {
                return reader[name, ns];
            }
        }

        public override string Value
        {
            get
            {
                return reader.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return reader.XmlLang;
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                return reader.XmlSpace;
            }
        }

        public override int ReadBase64(byte[] buffer, int offset, int count)
        {
            return reader.ReadBase64(buffer, offset, count);
        }

        public override int ReadBinHex(byte[] buffer, int offset, int count)
        {
            return reader.ReadBinHex(buffer, offset, count);
        }

        public override int ReadChars(char[] chars, int offset, int count)
        {
            return reader.ReadChars(chars, offset, count);
        }

        public override int ReadValueAsChars(char[] chars, int offset, int count)
        {
            return reader.ReadValueAsChars(chars, offset, count);
        }

        public override Type ValueType
        {
            get
            {
                return reader.ValueType;
            }
        }
        
        public override Boolean ReadValueAsBoolean()
        {
            return reader.ReadValueAsBoolean();
        }

        public override DateTime ReadValueAsDateTime()
        {
            return reader.ReadValueAsDateTime();
        }

        public override Decimal ReadValueAsDecimal()
        {
            return reader.ReadValueAsDecimal();
        }

        public override Double ReadValueAsDouble()
        {
            return reader.ReadValueAsDouble();
        }

        public override Int32 ReadValueAsInt32()
        {
            return reader.ReadValueAsInt32();
        }

        public override Int64 ReadValueAsInt64()
        {
            return reader.ReadValueAsInt64();
        }

        public override Single ReadValueAsSingle()
        {
            return reader.ReadValueAsSingle();
        }

        public override string ReadValueAsString()
        {
            return reader.ReadValueAsString();
        }

        public override object ReadValueAs(Type type)
        {
            return reader.ReadValueAs(type);
        }
        
        public override bool IsStartSubsetElement()
        {
            return reader.IsStartSubsetElement();
        }

        public override ArraySegment<byte> GetSubset(bool advance)
        {
            return reader.GetSubset(advance);
        }

        public bool HasLineInfo()
        {
            IXmlLineInfo lineInfo = reader as IXmlLineInfo;
            
            if (lineInfo == null)
                return false;

            return lineInfo.HasLineInfo();
        }

        public int LineNumber
        {
            get
            {
                IXmlLineInfo lineInfo = reader as IXmlLineInfo;
                
                if (lineInfo == null)
                    return 1;
                    
                return lineInfo.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                IXmlLineInfo lineInfo = reader as IXmlLineInfo;
                
                if (lineInfo == null)
                    return 1;
                    
                return lineInfo.LinePosition;
            }
        }
    }
#endif
}
