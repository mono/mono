//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

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
    internal class IdentityModelWrappedXmlDictionaryReader : XmlDictionaryReader, IXmlLineInfo
    {
        XmlReader _reader;
        XmlDictionaryReaderQuotas _xmlDictionaryReaderQuotas;

        public IdentityModelWrappedXmlDictionaryReader( 
            XmlReader reader, 
            XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas )
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            if ( xmlDictionaryReaderQuotas == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "xmlDictionaryReaderQuotas" );
            }

            _reader = reader;
            _xmlDictionaryReaderQuotas = xmlDictionaryReaderQuotas;
        }

        public override int AttributeCount
        {
            get
            {
                return _reader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return _reader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get { return _reader.CanReadBinaryContent; }
        }

        public override bool CanReadValueChunk
        {
            get { return _reader.CanReadValueChunk; }
        }

        public override void Close()
        {
            _reader.Close();
        }

        public override int Depth
        {
            get
            {
                return _reader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return _reader.EOF;
            }
        }

        public override string GetAttribute( int index )
        {
            return _reader.GetAttribute( index );
        }

        public override string GetAttribute( string name )
        {
            return _reader.GetAttribute( name );
        }

        public override string GetAttribute( string name, string namespaceUri )
        {
            return _reader.GetAttribute( name, namespaceUri );
        }

        public override bool HasValue
        {
            get
            {
                return _reader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return _reader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return _reader.IsEmptyElement;
            }
        }

        public override bool IsStartElement( string name )
        {
            return _reader.IsStartElement( name );
        }

        public override bool IsStartElement( string localName, string namespaceUri )
        {
            return _reader.IsStartElement( localName, namespaceUri );
        }

        public override string LocalName
        {
            get
            {
                return _reader.LocalName;
            }
        }

        public override string LookupNamespace( string namespaceUri )
        {
            return _reader.LookupNamespace( namespaceUri );
        }

        public override void MoveToAttribute( int index )
        {
            _reader.MoveToAttribute( index );
        }

        public override bool MoveToAttribute( string name )
        {
            return _reader.MoveToAttribute( name );
        }

        public override bool MoveToAttribute( string name, string namespaceUri )
        {
            return _reader.MoveToAttribute( name, namespaceUri );
        }

        public override bool MoveToElement()
        {
            return _reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return _reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return _reader.MoveToNextAttribute();
        }

        public override string Name
        {
            get
            {
                return _reader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return _reader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return _reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return _reader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return _reader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return _reader.QuoteChar;
            }
        }

        public override bool Read()
        {
            return _reader.Read();
        }

        public override bool ReadAttributeValue()
        {
            return _reader.ReadAttributeValue();
        }

        public override string ReadElementString( string name )
        {
            return _reader.ReadElementString( name );
        }

        public override string ReadElementString( string localName, string namespaceUri )
        {
            return _reader.ReadElementString( localName, namespaceUri );
        }

        public override string ReadInnerXml()
        {
            return _reader.ReadInnerXml();
        }

        public override string ReadOuterXml()
        {
            return _reader.ReadOuterXml();
        }

        public override void ReadStartElement( string name )
        {
            _reader.ReadStartElement( name );
        }

        public override void ReadStartElement( string localName, string namespaceUri )
        {
            _reader.ReadStartElement( localName, namespaceUri );
        }

        public override void ReadEndElement()
        {
            _reader.ReadEndElement();
        }

        public override string ReadString()
        {
            return _reader.ReadString();
        }

        public override ReadState ReadState
        {
            get
            {
                return _reader.ReadState;
            }
        }

        public override void ResolveEntity()
        {
            _reader.ResolveEntity();
        }

        public override string this[int index]
        {
            get
            {
                return _reader[index];
            }
        }

        public override string this[string name]
        {
            get
            {
                return _reader[name];
            }
        }

        public override string this[string name, string namespaceUri]
        {
            get
            {
                return _reader[name, namespaceUri];
            }
        }

        public override string Value
        {
            get
            {
                return _reader.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return _reader.XmlLang;
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                return _reader.XmlSpace;
            }
        }

        public override int ReadElementContentAsBase64( byte[] buffer, int offset, int count )
        {
            return _reader.ReadElementContentAsBase64( buffer, offset, count );
        }

        public override int ReadContentAsBase64( byte[] buffer, int offset, int count )
        {
            return _reader.ReadContentAsBase64( buffer, offset, count );
        }

        public override int ReadElementContentAsBinHex( byte[] buffer, int offset, int count )
        {
            return _reader.ReadElementContentAsBinHex( buffer, offset, count );
        }

        public override int ReadContentAsBinHex( byte[] buffer, int offset, int count )
        {
            return _reader.ReadContentAsBinHex( buffer, offset, count );
        }

        public override int ReadValueChunk( char[] chars, int offset, int count )
        {
            return _reader.ReadValueChunk( chars, offset, count );
        }

        public override Type ValueType
        {
            get
            {
                return _reader.ValueType;
            }
        }

        public override Boolean ReadContentAsBoolean()
        {
            return _reader.ReadContentAsBoolean();
        }

        public override DateTime ReadContentAsDateTime()
        {
            return _reader.ReadContentAsDateTime();
        }

        public override Decimal ReadContentAsDecimal()
        {
            return (Decimal)_reader.ReadContentAs( typeof( Decimal ), null );
        }

        public override Double ReadContentAsDouble()
        {
            return _reader.ReadContentAsDouble();
        }

        public override Int32 ReadContentAsInt()
        {
            return _reader.ReadContentAsInt();
        }

        public override Int64 ReadContentAsLong()
        {
            return _reader.ReadContentAsLong();
        }

        public override Single ReadContentAsFloat()
        {
            return _reader.ReadContentAsFloat();
        }

        public override string ReadContentAsString()
        {
            return _reader.ReadContentAsString();
        }

        public override object ReadContentAs( Type valueType, IXmlNamespaceResolver namespaceResolver )
        {
            return _reader.ReadContentAs( valueType, namespaceResolver );
        }

        public bool HasLineInfo()
        {
            IXmlLineInfo lineInfo = _reader as IXmlLineInfo;

            if ( lineInfo == null )
            {
                return false;
            }

            return lineInfo.HasLineInfo();
        }

        public int LineNumber
        {
            get
            {
                IXmlLineInfo lineInfo = _reader as IXmlLineInfo;

                if ( lineInfo == null )
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
                IXmlLineInfo lineInfo = _reader as IXmlLineInfo;

                if ( lineInfo == null )
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
                return _xmlDictionaryReaderQuotas;
            }
        }
    }
}
