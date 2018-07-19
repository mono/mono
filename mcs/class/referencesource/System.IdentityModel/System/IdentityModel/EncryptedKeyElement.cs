//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Xml;
    
    /// <summary>
    /// This class implements a deserialization for: EncryptedType as defined in section 3.5.1 of http://www.w3.org/TR/2002/REC-xmlenc-core-2002120
    /// </summary>
    internal class EncryptedKeyElement : EncryptedTypeElement
    {
        string _carriedName;
        string _recipient;

        List<string> _keyReferences;
        List<string> _dataReferences;

        public EncryptedKeyElement( SecurityTokenSerializer keyInfoSerializer )
            : base( keyInfoSerializer )
        {
            _keyReferences = new List<string>();
            _dataReferences = new List<string>();
        }

        public string CarriedName
        {
            get { return _carriedName; }
        }

        public IList<string> DataReferences
        {
            get { return _dataReferences; }
        }

        public IList<string> KeyReferences
        {
            get { return _keyReferences; }
        }

        public override void ReadExtensions( XmlDictionaryReader reader )
        {
            reader.MoveToContent();
            if ( reader.IsStartElement( XmlEncryptionConstants.Elements.ReferenceList, XmlEncryptionConstants.Namespace ) )
            {
                reader.ReadStartElement();

                // could have data or key references.  these are the only two possible elements sec 3.6 xml enc.
                // 3.6 The ReferenceList Element specifies there is a choice. Once one is chosen, it is fixed.
                if ( reader.IsStartElement( XmlEncryptionConstants.Elements.DataReference, XmlEncryptionConstants.Namespace ) )
                {
                    while ( reader.IsStartElement() )
                    {
                        if ( reader.IsStartElement( XmlEncryptionConstants.Elements.DataReference, XmlEncryptionConstants.Namespace ) )
                        {
                            string dataRef = reader.GetAttribute( XmlEncryptionConstants.Attributes.Uri );
                            if ( !string.IsNullOrEmpty( dataRef ) )
                            {
                                _dataReferences.Add( dataRef );
                            }
                            reader.Skip();
                        }
                        else if ( reader.IsStartElement( XmlEncryptionConstants.Elements.KeyReference, XmlEncryptionConstants.Namespace ) )
                        {
                            throw DiagnosticUtility.ThrowHelperXml( reader, SR.GetString( SR.ID4189 ) );
                        }
                        else
                        {
                            string xml = reader.ReadOuterXml();
                            if ( DiagnosticUtility.ShouldTraceWarning )
                            {
                                TraceUtility.TraceString( System.Diagnostics.TraceEventType.Warning, SR.GetString( SR.ID8024, reader.Name, reader.NamespaceURI, xml ) );
                            }
                        }
                    }
                }
                else if ( reader.IsStartElement( XmlEncryptionConstants.Elements.KeyReference, XmlEncryptionConstants.Namespace ) )
                {
                    while ( reader.IsStartElement() )
                    {
                        if ( reader.IsStartElement( XmlEncryptionConstants.Elements.KeyReference, XmlEncryptionConstants.Namespace ) )
                        {
                            string keyRef = reader.GetAttribute( XmlEncryptionConstants.Attributes.Uri );
                            if ( !string.IsNullOrEmpty( keyRef ) )
                            {
                                _keyReferences.Add( keyRef );
                            }
                            reader.Skip();
                        }
                        else if ( reader.IsStartElement( XmlEncryptionConstants.Elements.DataReference, XmlEncryptionConstants.Namespace ) )
                        {
                            throw DiagnosticUtility.ThrowHelperXml( reader, SR.GetString( SR.ID4190 ) );
                        }
                        else
                        {
                            string xml = reader.ReadOuterXml();
                            if ( DiagnosticUtility.ShouldTraceWarning )
                            {
                                TraceUtility.TraceString( System.Diagnostics.TraceEventType.Warning, SR.GetString( SR.ID8024, reader.Name, reader.NamespaceURI, xml ) );
                            }
                        }
                    }
                }
                else
                {
                    // there must be at least one reference.
                    throw DiagnosticUtility.ThrowHelperXml( reader, SR.GetString( SR.ID4191 ) );
                }

                reader.MoveToContent();
                if ( reader.IsStartElement( XmlEncryptionConstants.Elements.CarriedKeyName, XmlEncryptionConstants.Namespace ) )
                {
                    reader.ReadStartElement();
                    _carriedName = reader.ReadString();
                    reader.ReadEndElement();
                }

                // </ReferenceList>
                reader.ReadEndElement();
            }
        }

        public override void ReadXml( XmlDictionaryReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            reader.MoveToContent();
            if ( !reader.IsStartElement( XmlEncryptionConstants.Elements.EncryptedKey, XmlEncryptionConstants.Namespace ) )
            {
                throw DiagnosticUtility.ThrowHelperXml( reader, SR.GetString( SR.ID4187 ) );
            }

            _recipient = reader.GetAttribute( XmlEncryptionConstants.Attributes.Recipient, null );

            //<EncryptedKey> extends <EncryptedType>
            // base will read the start element and end elements
            base.ReadXml( reader );
        }

        public EncryptedKeyIdentifierClause GetClause()
        {
            return new EncryptedKeyIdentifierClause( CipherData.CipherValue, Algorithm, KeyIdentifier );
        }

    }
}
