//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Xml;

    internal class KeyInfo
    {
        SecurityTokenSerializer _keyInfoSerializer;
        SecurityKeyIdentifier _ski;
        string _retrieval;

        public KeyInfo( SecurityTokenSerializer keyInfoSerializer )
        {
            _keyInfoSerializer = keyInfoSerializer;
            _ski = new SecurityKeyIdentifier();
        }

        public string RetrievalMethod
        {
            get { return _retrieval; }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get { return _ski; }
            set
            {
                if ( value == null )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "value" );
                }

                _ski = value;
            }
        }

        public virtual void ReadXml( XmlDictionaryReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            reader.MoveToContent();
            if ( reader.IsStartElement( XD.XmlSignatureDictionary.KeyInfo.Value, XD.XmlSignatureDictionary.Namespace.Value ) )
            {
                // <KeyInfo>
                reader.ReadStartElement();

                while ( reader.IsStartElement() )
                {
                    // <RetrievalMethod>
                    if ( reader.IsStartElement( XmlSignatureConstants.Elements.RetrievalMethod, XD.XmlSignatureDictionary.Namespace.Value ) )
                    {
                        string method = reader.GetAttribute( XD.XmlSignatureDictionary.URI.Value );
                        if ( !string.IsNullOrEmpty( method ) )
                        {
                            _retrieval = method;
                        }
                        reader.Skip();
                    }
                    // check if internal serializer can handle clause
                    else if ( _keyInfoSerializer.CanReadKeyIdentifierClause( reader ) )
                    {
                        _ski.Add( _keyInfoSerializer.ReadKeyIdentifierClause( reader ) );
                    }
                    // trace we skipped over an element
                    else if ( reader.IsStartElement() )
                    {
                        string xml = reader.ReadOuterXml();

                        if ( DiagnosticUtility.ShouldTraceWarning )
                        {
                            TraceUtility.TraceString( System.Diagnostics.TraceEventType.Warning, SR.GetString( SR.ID8023, reader.Name, reader.NamespaceURI, xml ) );
                        }
                    }
                    reader.MoveToContent();
                }

                reader.MoveToContent();
                reader.ReadEndElement();
            }
        }
    }
}
