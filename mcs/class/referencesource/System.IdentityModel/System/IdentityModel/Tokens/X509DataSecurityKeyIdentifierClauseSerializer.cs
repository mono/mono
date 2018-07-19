//-----------------------------------------------------------------------
// <copyright file="X509DataSecurityKeyIdentifierClauseSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    /// Implementation of SecurityKeyIdentifierClauseSerializer that handles X.509 Certificate
    /// reference types.
    /// </summary>
    public class X509DataSecurityKeyIdentifierClauseSerializer : SecurityKeyIdentifierClauseSerializer
    {
        /// <summary>
        /// Checks if the given reader is referring to a &lt;ds:X509Data> element.
        /// </summary>
        /// <param name="reader">XmlReader positioned at the SecurityKeyIdentifierClause. </param>
        /// <returns>True if the XmlReader is referring to a &lt;ds:X509Data> element.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        public override bool CanReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return reader.IsStartElement(XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace);
        }

        /// <summary>
        /// Checks if the given SecurityKeyIdentifierClause can be serialized. The
        /// supported SecurityKeyIdentifierClause are,
        /// 1. <see cref="System.IdentityModel.Tokens.X509IssuerSerialKeyIdentifierClause"/>
        /// 2. <see cref="System.IdentityModel.Tokens.X509RawDataKeyIdentifierClause"/>
        /// 3. <see cref="System.IdentityModel.Tokens.X509SubjectKeyIdentifierClause"/>
        /// </summary>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to be serialized.</param>
        /// <returns>True if the 'securityKeyIdentifierClause' is supported.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'securityKeyIdentifierClause' is null.</exception>
        public override bool CanWriteKeyIdentifierClause(SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            if (securityKeyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityKeyIdentifierClause");
            }

            return securityKeyIdentifierClause is X509IssuerSerialKeyIdentifierClause ||
                securityKeyIdentifierClause is X509RawDataKeyIdentifierClause ||
                securityKeyIdentifierClause is X509SubjectKeyIdentifierClause;
        }

        /// <summary>
        /// Deserializes a SecurityKeyIdentifierClause from a given XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader that references a SecurityKeyIdentifierClause.</param>
        /// <returns>Instance of SecurityKeyIdentifierClause</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        /// <exception cref="InvalidOperationException">The XmlReader is not positioned at a valid X.509 SecurityTokenReference.</exception>
        public override SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            if (!this.CanReadKeyIdentifierClause(reader))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(
                    SR.GetString(SR.ID3032, reader.LocalName, reader.NamespaceURI, XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace));
            }

            XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            
            // Read the starting <X509Data> element.
            dictionaryReader.ReadStartElement(XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace);

            List<SecurityKeyIdentifierClause> clauses = new List<SecurityKeyIdentifierClause>();
            while (dictionaryReader.IsStartElement())
            {
                if (dictionaryReader.IsStartElement(XmlSignatureConstants.Elements.X509IssuerSerial, XmlSignatureConstants.Namespace))
                {
                    clauses.Add(CreateIssuerSerialKeyIdentifierClause(dictionaryReader));
                }
                else if (dictionaryReader.IsStartElement(XmlSignatureConstants.Elements.X509SKI, XmlSignatureConstants.Namespace))
                {
                    clauses.Add(CreateSubjectKeyIdentifierClause(dictionaryReader));
                }
                else if (dictionaryReader.IsStartElement(XmlSignatureConstants.Elements.X509Certificate, XmlSignatureConstants.Namespace))
                {
                    clauses.Add(CreateRawDataKeyIdentifierClause(dictionaryReader));
                }
                else
                {
                    // Skip the element since it is not one of <X509IssuerSerial>, <X509SKI> and <X509Certificate>
                    dictionaryReader.Skip();
                }
            }

            // Read the ending </X509Data> element.
            dictionaryReader.ReadEndElement();

            // Return the first identified clause or null if none is identified
            return clauses.Count > 0 ? clauses[0] : null;
        }

        /// <summary>
        /// Serialize a SecurityKeyIdentifierClause to the given XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to which the SecurityKeyIdentifierClause is serialized.</param>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to serialize.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' or 'securityKeyIdentifierClause' is null.</exception>
        /// <exception cref="ArgumentException">The parameter 'securityKeyIdentifierClause' is not a supported clause type.</exception>
        public override void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (securityKeyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityKeyIdentifierClause");
            }

            X509IssuerSerialKeyIdentifierClause issuerSerialClause = securityKeyIdentifierClause as X509IssuerSerialKeyIdentifierClause;
            if (issuerSerialClause != null)
            {
                writer.WriteStartElement(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace);
                writer.WriteStartElement(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509IssuerSerial, XmlSignatureConstants.Namespace);
                writer.WriteElementString(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509IssuerName, XmlSignatureConstants.Namespace, issuerSerialClause.IssuerName);
                writer.WriteElementString(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509SerialNumber, XmlSignatureConstants.Namespace, issuerSerialClause.IssuerSerialNumber);
                writer.WriteEndElement();
                writer.WriteEndElement();
                return;
            }

            X509SubjectKeyIdentifierClause skiClause = securityKeyIdentifierClause as X509SubjectKeyIdentifierClause;
            if (skiClause != null)
            {
                writer.WriteStartElement(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace);
                writer.WriteStartElement(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509SKI, XmlSignatureConstants.Namespace);
                byte[] ski = skiClause.GetX509SubjectKeyIdentifier();
                writer.WriteBase64(ski, 0, ski.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
                return;
            }
#if INCLUDE_CERT_CHAIN
            X509ChainRawDataKeyIdentifierClause x509ChainDataClause = securityKeyIdentifierClause as X509ChainRawDataKeyIdentifierClause;
            if ( x509ChainDataClause != null )
            {
                writer.WriteStartElement( XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace );
                for( int i = 0; i < x509ChainDataClause.CertificateCount; i++ )
                {
                    writer.WriteStartElement( XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509Certificate, XmlSignatureConstants.Namespace );
                    byte[] rawData = x509ChainDataClause.GetX509RawData( i );
                    writer.WriteBase64( rawData, 0, rawData.Length );    
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                return;
            }
#endif

            X509RawDataKeyIdentifierClause rawDataClause = securityKeyIdentifierClause as X509RawDataKeyIdentifierClause;
            if (rawDataClause != null)
            {
                writer.WriteStartElement(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509Data, XmlSignatureConstants.Namespace);
                writer.WriteStartElement(XmlSignatureConstants.Prefix, XmlSignatureConstants.Elements.X509Certificate, XmlSignatureConstants.Namespace);
                byte[] rawData = rawDataClause.GetX509RawData();
                writer.WriteBase64(rawData, 0, rawData.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("securityKeyIdentifierClause", SR.GetString(SR.ID4259, securityKeyIdentifierClause.GetType()));
        }

        /// <summary>
        /// Parses the "X509IssuerSerial" element and generates a corresponding <see cref="X509IssuerSerialKeyIdentifierClause"/> instance.
        /// </summary>
        /// <param name="dictionaryReader">The <see cref="XmlDictionaryReader"/> currently positioning on the "X509IssuerSerial" element. </param>
        /// <returns>An instance of <see cref="X509IssuerSerialKeyIdentifierClause"/> created from the "X509IssuerSerial" element.</returns>
        private static SecurityKeyIdentifierClause CreateIssuerSerialKeyIdentifierClause(XmlDictionaryReader dictionaryReader)
        {
            dictionaryReader.ReadStartElement(XmlSignatureConstants.Elements.X509IssuerSerial, XmlSignatureConstants.Namespace);

            if (!dictionaryReader.IsStartElement(XmlSignatureConstants.Elements.X509IssuerName, XmlSignatureConstants.Namespace))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3032, dictionaryReader.LocalName, dictionaryReader.NamespaceURI, XmlSignatureConstants.Elements.X509IssuerName, XmlSignatureConstants.Namespace));
            }

            string issuerName = dictionaryReader.ReadElementContentAsString(XmlSignatureConstants.Elements.X509IssuerName, XmlSignatureConstants.Namespace);

            if (!dictionaryReader.IsStartElement(XmlSignatureConstants.Elements.X509SerialNumber, XmlSignatureConstants.Namespace))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3032, dictionaryReader.LocalName, dictionaryReader.NamespaceURI, XmlSignatureConstants.Elements.X509SerialNumber, XmlSignatureConstants.Namespace));
            }

            string serialNumber = dictionaryReader.ReadElementContentAsString(XmlSignatureConstants.Elements.X509SerialNumber, XmlSignatureConstants.Namespace);

            dictionaryReader.ReadEndElement(); // Reade the ending </X509IssuerSerial> element.

            return new X509IssuerSerialKeyIdentifierClause(issuerName, serialNumber);
        }

        /// <summary>
        /// Parses the "X509SKI" element and generates a corresponding <see cref="SecurityKeyIdentifierClause"/> instance.
        /// </summary>
        /// <param name="dictionaryReader">The <see cref="XmlDictionaryReader"/> currently positioning on the "X509SKI" element. </param>
        /// <returns>An instance of <see cref="X509SubjectKeyIdentifierClause"/> created from the "X509SKI" element.</returns>
        private static SecurityKeyIdentifierClause CreateSubjectKeyIdentifierClause(XmlDictionaryReader dictionaryReader)
        {
            byte[] ski = dictionaryReader.ReadElementContentAsBase64();
            if ((ski == null) || (ski.Length == 0))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4258, XmlSignatureConstants.Elements.X509SKI, XmlSignatureConstants.Namespace));
            }

            return new X509SubjectKeyIdentifierClause(ski);
        }

        /// <summary>
        /// Parses the "X509Certificate" element and generates a corresponding <see cref="X509RawDataKeyIdentifierClause"/> instance.
        /// </summary>
        /// <param name="dictionaryReader">The <see cref="XmlDictionaryReader"/> currently positioning on the "X509Certificate" element. </param>
        /// <returns>An instance of <see cref="X509RawDataKeyIdentifierClause"/> created from the "X509Certificate" element.</returns>
        private static SecurityKeyIdentifierClause CreateRawDataKeyIdentifierClause(XmlDictionaryReader dictionaryReader)
        {
#if INCLUDE_CERT_CHAIN                
                List<byte[]> rawDatas = new List<byte[]>();
                while (dictionaryReader.IsStartElement(XmlSignatureConstants.Elements.X509Certificate, XmlSignatureConstants.Namespace))
                {
                    byte[] rawBuffer = dictionaryReader.ReadElementContentAsBase64();
                    if (rawBuffer == null || rawBuffer.Length == 0)
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4258, XmlSignatureConstants.Elements.X509Certificate, XmlSignatureConstants.Namespace));
                    }

                    rawDatas.Add(rawBuffer);
                }

                if (rawDatas.Count > 1)
                {
                    return new X509ChainRawDataKeyIdentifierClause(rawDatas);
                }
                else
                {
                    return new X509RawDataKeyIdentifierClause(rawDatas[0]);
                }
#else
            byte[] rawData = null;
            while (dictionaryReader.IsStartElement(XmlSignatureConstants.Elements.X509Certificate, XmlSignatureConstants.Namespace))
            {
                if (rawData == null)
                {
                    rawData = dictionaryReader.ReadElementContentAsBase64();
                    if ((rawData == null) || (rawData.Length == 0))
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4258, XmlSignatureConstants.Elements.X509Certificate, XmlSignatureConstants.Namespace));
                    }
                }
                else
                {
                    // We do not support reading intermediary certs.
                    dictionaryReader.Skip();
                }
            }

            return new X509RawDataKeyIdentifierClause(rawData);
#endif
        }
    }
}
