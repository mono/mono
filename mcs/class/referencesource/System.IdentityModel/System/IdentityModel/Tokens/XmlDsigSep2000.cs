//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using System.Xml;
    using KeyIdentifierEntry = System.IdentityModel.Selectors.SecurityTokenSerializer.KeyIdentifierEntry;

    class XmlDsigSep2000 : SecurityTokenSerializer.SerializerEntries
    {
        KeyInfoSerializer securityTokenSerializer;

        public XmlDsigSep2000( KeyInfoSerializer securityTokenSerializer )
        {
            this.securityTokenSerializer = securityTokenSerializer;
        }

        public override void PopulateKeyIdentifierEntries( IList<KeyIdentifierEntry> keyIdentifierEntries )
        {
            keyIdentifierEntries.Add( new KeyInfoEntry( this.securityTokenSerializer ) );
        }

        public override void PopulateKeyIdentifierClauseEntries( IList<SecurityTokenSerializer.KeyIdentifierClauseEntry> keyIdentifierClauseEntries )
        {
            keyIdentifierClauseEntries.Add( new KeyNameClauseEntry() );
            keyIdentifierClauseEntries.Add( new KeyValueClauseEntry() );
            keyIdentifierClauseEntries.Add( new X509CertificateClauseEntry() );
        }

        internal class KeyInfoEntry : KeyIdentifierEntry
        {
            KeyInfoSerializer securityTokenSerializer;

            public KeyInfoEntry( KeyInfoSerializer securityTokenSerializer )
            {
                this.securityTokenSerializer = securityTokenSerializer;
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.KeyInfo;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }

            public override SecurityKeyIdentifier ReadKeyIdentifierCore( XmlDictionaryReader reader )
            {
                reader.ReadStartElement( LocalName, NamespaceUri );
                SecurityKeyIdentifier keyIdentifier = new SecurityKeyIdentifier();
                while ( reader.IsStartElement() )
                {
                    SecurityKeyIdentifierClause clause = this.securityTokenSerializer.ReadKeyIdentifierClause( reader );
                    if ( clause == null )
                    {
                        reader.Skip();
                    }
                    else
                    {
                        keyIdentifier.Add( clause );
                    }
                }
                if ( keyIdentifier.Count == 0 )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new XmlException( SR.GetString( SR.ErrorDeserializingKeyIdentifierClause ) ) );
                }
                reader.ReadEndElement();
                return keyIdentifier;
            }

            public override bool SupportsCore( SecurityKeyIdentifier keyIdentifier )
            {
                return true;
            }

            public override void WriteKeyIdentifierCore( XmlDictionaryWriter writer, SecurityKeyIdentifier keyIdentifier )
            {
                writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, LocalName, NamespaceUri );
                bool clauseWritten = false;
                foreach ( SecurityKeyIdentifierClause clause in keyIdentifier )
                {
                    this.securityTokenSerializer.InnerSecurityTokenSerializer.WriteKeyIdentifierClause( writer, clause );
                    clauseWritten = true;
                }
                writer.WriteEndElement(); // KeyInfo
                if ( !clauseWritten )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new SecurityMessageSerializationException( SR.GetString( SR.NoKeyInfoClausesToWrite ) ) );
                }
            }
        }

        // <ds:KeyName>name</ds:KeyName>
        internal class KeyNameClauseEntry : SecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.KeyName;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore( XmlDictionaryReader reader )
            {
                reader.ReadStartElement( XD.XmlSignatureDictionary.KeyName, NamespaceUri );
                string name = reader.ReadString();
                reader.ReadEndElement();

                return new KeyNameIdentifierClause( name );
            }

            public override bool SupportsCore( SecurityKeyIdentifierClause keyIdentifierClause )
            {
                return keyIdentifierClause is KeyNameIdentifierClause;
            }

            public override void WriteKeyIdentifierClauseCore( XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause )
            {
                KeyNameIdentifierClause nameClause = keyIdentifierClause as KeyNameIdentifierClause;

                writer.WriteElementString( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.KeyName, NamespaceUri, nameClause.KeyName );
            }
        }
        // so far, we only support one type of KeyValue - RSAKeyValue
        //   <ds:KeyValue>
        //     <ds:RSAKeyValue>
        //       <ds:Modulus>xA7SEU+...</ds:Modulus>
        //         <ds:Exponent>AQAB</Exponent>
        //     </ds:RSAKeyValue>
        //   </ds:KeyValue>
        internal class KeyValueClauseEntry : SecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.KeyValue;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }


            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore( XmlDictionaryReader reader )
            {
                reader.ReadStartElement( XD.XmlSignatureDictionary.KeyValue, NamespaceUri );
                reader.ReadStartElement( XD.XmlSignatureDictionary.RsaKeyValue, NamespaceUri );
                reader.ReadStartElement( XD.XmlSignatureDictionary.Modulus, NamespaceUri );

                byte[] modulus = Convert.FromBase64String( reader.ReadString() );

                reader.ReadEndElement();
                reader.ReadStartElement( XD.XmlSignatureDictionary.Exponent, NamespaceUri );
                byte[] exponent = Convert.FromBase64String( reader.ReadString() );
                reader.ReadEndElement();
                reader.ReadEndElement();
                reader.ReadEndElement();

                RSA rsa = new RSACryptoServiceProvider();
                RSAParameters rsaParameters = new RSAParameters();
                rsaParameters.Modulus = modulus;
                rsaParameters.Exponent = exponent;
                rsa.ImportParameters( rsaParameters );

                return new RsaKeyIdentifierClause( rsa );
            }

            public override bool SupportsCore( SecurityKeyIdentifierClause keyIdentifierClause )
            {
                return keyIdentifierClause is RsaKeyIdentifierClause;
            }

            public override void WriteKeyIdentifierClauseCore( XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause )
            {
                RsaKeyIdentifierClause rsaClause = keyIdentifierClause as RsaKeyIdentifierClause;

                writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.KeyValue, NamespaceUri );
                writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.RsaKeyValue, NamespaceUri );
                writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Modulus, NamespaceUri );
                rsaClause.WriteModulusAsBase64( writer );
                writer.WriteEndElement();
                writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Exponent, NamespaceUri );
                rsaClause.WriteExponentAsBase64( writer );
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        // so far, we only support two types of X509Data directly under KeyInfo  - X509Certificate and X509SKI
        //   <ds:X509Data>
        //     <ds:X509Certificate>...</ds:X509Certificate>
        //      or
        //     <X509SKI>... </X509SKI>
        //   </ds:X509Data>
        // only support 1 certificate right now
        internal class X509CertificateClauseEntry : SecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.X509Data;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore( XmlDictionaryReader reader )
            {
                SecurityKeyIdentifierClause ski = null;
                reader.ReadStartElement( XD.XmlSignatureDictionary.X509Data, NamespaceUri );
                while ( reader.IsStartElement() )
                {
                    if ( ski == null && reader.IsStartElement( XD.XmlSignatureDictionary.X509Certificate, NamespaceUri ) )
                    {
                        X509Certificate2 certificate = null;
                        if ( !SecurityUtils.TryCreateX509CertificateFromRawData( reader.ReadElementContentAsBase64(), out certificate ) )
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new SecurityMessageSerializationException( SR.GetString( SR.InvalidX509RawData ) ) );
                        }
                        ski = new X509RawDataKeyIdentifierClause( certificate );
                    }
                    else if ( ski == null && reader.IsStartElement( XmlSignatureStrings.X509Ski, NamespaceUri.ToString() ) )
                    {
                        ski = new X509SubjectKeyIdentifierClause( reader.ReadElementContentAsBase64() );
                    }
                    else if ( ( ski == null ) && reader.IsStartElement( XD.XmlSignatureDictionary.X509IssuerSerial, XD.XmlSignatureDictionary.Namespace ) )
                    {
                        reader.ReadStartElement( XD.XmlSignatureDictionary.X509IssuerSerial, XD.XmlSignatureDictionary.Namespace );
                        reader.ReadStartElement( XD.XmlSignatureDictionary.X509IssuerName, XD.XmlSignatureDictionary.Namespace );
                        string issuerName = reader.ReadContentAsString();
                        reader.ReadEndElement();
                        reader.ReadStartElement( XD.XmlSignatureDictionary.X509SerialNumber, XD.XmlSignatureDictionary.Namespace );
                        string serialNumber = reader.ReadContentAsString();
                        reader.ReadEndElement();
                        reader.ReadEndElement();

                        ski = new X509IssuerSerialKeyIdentifierClause( issuerName, serialNumber );
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
                return ski;
            }

            public override bool SupportsCore( SecurityKeyIdentifierClause keyIdentifierClause )
            {
                return (keyIdentifierClause is X509RawDataKeyIdentifierClause);
                // This method should not write X509IssuerSerialKeyIdentifierClause or X509SubjectKeyIdentifierClause as that should be written by the WSSecurityXXX classes with SecurityTokenReference tag. 
                // The XmlDsig entries are written by the X509SecurityTokenHandler.
            }

            public override void WriteKeyIdentifierClauseCore( XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause )
            {
                X509RawDataKeyIdentifierClause x509Clause = keyIdentifierClause as X509RawDataKeyIdentifierClause;

                if ( x509Clause != null )
                {
                    writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Data, NamespaceUri );

                    writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Certificate, NamespaceUri );
                    byte[] certBytes = x509Clause.GetX509RawData();
                    writer.WriteBase64( certBytes, 0, certBytes.Length );
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

                X509IssuerSerialKeyIdentifierClause issuerSerialClause = keyIdentifierClause as X509IssuerSerialKeyIdentifierClause;
                if ( issuerSerialClause != null )
                {                    
                    writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace );
                    writer.WriteStartElement( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509IssuerSerial, XD.XmlSignatureDictionary.Namespace );
                    writer.WriteElementString( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509IssuerName, XD.XmlSignatureDictionary.Namespace, issuerSerialClause.IssuerName );
                    writer.WriteElementString( XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509SerialNumber, XD.XmlSignatureDictionary.Namespace, issuerSerialClause.IssuerSerialNumber );
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    return;
                }

                X509SubjectKeyIdentifierClause skiClause = keyIdentifierClause as X509SubjectKeyIdentifierClause;
                if ( skiClause != null )
                {
                    writer.WriteStartElement( XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace );
                    writer.WriteStartElement( XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509SKI, XmlSignatureConstants.Namespace );
                    byte[] ski = skiClause.GetX509SubjectKeyIdentifier();
                    writer.WriteBase64( ski, 0, ski.Length );
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    return;
                }
            }
        }

    }
}
