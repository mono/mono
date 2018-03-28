//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Xml;

    /// <summary>
    /// SecurityTokenHandler for RsaSecurityTokens. 
    /// </summary>
    public class RsaSecurityTokenHandler : SecurityTokenHandler
    {
        static string[] _tokenTypeIdentifiers = new string[] { SecurityTokenTypes.Rsa };

        /// <summary>
        /// Creates an instance of <see cref="RsaSecurityTokenHandler"/>
        /// </summary>
        public RsaSecurityTokenHandler()
        {
        }

        /// <summary>
        /// Checks the reader if this is a representation of an RsaSecurityToken.
        /// </summary>
        /// <param name="reader">XmlReader over the incoming SecurityToken.</param>
        /// <returns>'True' if the reader points to an RsaSecurityToken, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">The input argument 'reader' is null.</exception>
        public override bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return reader.IsStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace);
        }

        /// <summary>
        /// Gets the settings that indicate if the token handler can validate tokens.
        /// Returns true by default.
        /// </summary>
        public override bool CanValidateToken
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a boolean indicating if the handler can write tokens.
        /// Returns true by default.
        /// </summary>
        public override bool CanWriteToken
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the RSA Security Token type as defined in WS-Security Token profile.
        /// </summary>
        public override string[] GetTokenTypeIdentifiers()
        {
            return _tokenTypeIdentifiers;
        }

        /// <summary>
        /// Deserializes an RSA security token from XML.
        /// </summary>
        /// <param name="reader">An XML reader positioned at the start of the token</param>
        /// <returns>An instance of <see cref="RsaSecurityToken"/>.</returns>
        /// <exception cref="ArgumentNullException">The input argument 'reader' is null.</exception>
        /// <exception cref="XmlException">The 'reader' is not positioned at a RSA token. 
        /// or the SecurityContextToken cannot be read.</exception>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            XmlDictionaryReader dicReader = XmlDictionaryReader.CreateDictionaryReader(reader);

            if (!dicReader.IsStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(
                        SR.GetString(
                        SR.ID4065,
                        XmlSignatureConstants.Elements.KeyInfo,
                        XmlSignatureConstants.Namespace,
                        dicReader.LocalName,
                        dicReader.NamespaceURI)));
            }

            dicReader.ReadStartElement();

            if (!dicReader.IsStartElement(XmlSignatureConstants.Elements.KeyValue, XmlSignatureConstants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(
                        SR.GetString(
                        SR.ID4065,
                        XmlSignatureConstants.Elements.KeyValue,
                        XmlSignatureConstants.Namespace,
                        dicReader.LocalName,
                        dicReader.NamespaceURI)));
            }

            dicReader.ReadStartElement();

            RSA rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(dicReader.ReadOuterXml());

            dicReader.ReadEndElement(); //</KeyValue>
            dicReader.ReadEndElement(); //</KeyInfo>

            return new RsaSecurityToken(rsa);
        }

        /// <summary>
        /// Gets the System.Type of the SecurityToken that this token handler handles.
        /// Return type of <see cref="RsaSecurityToken"/> by default.
        /// </summary>
        public override Type TokenType
        {
            get { return typeof(RsaSecurityToken); }
        }

        /// <summary>
        /// Validates a <see cref="RsaSecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="RsaSecurityToken"/> to validate.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not assignable from <see cref="RsaSecurityToken"/>.</exception>
        /// <exception cref="InvalidOperationException">Configuration <see cref="SecurityTokenHandlerConfiguration"/>is null.</exception>        
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            RsaSecurityToken rsaToken = (RsaSecurityToken)token;
            if (rsaToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID0018, typeof(RsaSecurityToken)));
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            try
            {

                // Export the Public Key of the RSA as a Claim. 
                ClaimsIdentity identity = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Rsa, rsaToken.Rsa.ToXmlString(false), ClaimValueTypes.RsaKeyValue, ClaimsIdentity.DefaultIssuer) }, AuthenticationTypes.Signature);

                identity.AddClaim(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated), ClaimValueTypes.DateTime));
                identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Signature));

                if (this.Configuration.SaveBootstrapContext)
                {
                    identity.BootstrapContext = new BootstrapContext(token, this);
                }

                this.TraceTokenValidationSuccess(token);

                List<ClaimsIdentity> identities = new List<ClaimsIdentity>(1);
                identities.Add(identity);
                return identities.AsReadOnly();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.TraceTokenValidationFailure(token, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Serializes an RSA security token to XML.
        /// </summary>
        /// <param name="writer">The XML writer.</param>
        /// <param name="token">An RSA security token.</param>
        /// <exception cref="ArgumentNullException">The input argument 'writer' is null.</exception>
        /// <exception cref="InvalidOperationException">The input argument 'token' is either null or not of type
        /// <see cref="RsaSecurityToken"/>.</exception>
        public override void WriteToken(XmlWriter writer, SecurityToken token)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            RsaSecurityToken rsaToken = token as RsaSecurityToken;

            if (rsaToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID0018, typeof(RsaSecurityToken)));
            }

            RSAParameters rsaParams = rsaToken.Rsa.ExportParameters(false);

            writer.WriteStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace);
            writer.WriteStartElement(XmlSignatureConstants.Elements.KeyValue, XmlSignatureConstants.Namespace);

            //
            // RSA.ToXmlString shouldn't be used here because it doesn't write namespaces.  The modulus and exponent are written manually.
            //
            writer.WriteStartElement(XmlSignatureConstants.Elements.RsaKeyValue, XmlSignatureConstants.Namespace);

            writer.WriteStartElement(XmlSignatureConstants.Elements.Modulus, XmlSignatureConstants.Namespace);

            byte[] modulus = rsaParams.Modulus;
            writer.WriteBase64(modulus, 0, modulus.Length);
            writer.WriteEndElement(); // </modulus>

            writer.WriteStartElement(XmlSignatureConstants.Elements.Exponent, XmlSignatureConstants.Namespace);

            byte[] exponent = rsaParams.Exponent;
            writer.WriteBase64(exponent, 0, exponent.Length);
            writer.WriteEndElement(); // </exponent>

            writer.WriteEndElement(); // </RsaKeyValue>
            writer.WriteEndElement(); // </KeyValue>
            writer.WriteEndElement(); // </KeyInfo>

        }
    }
}
