//-----------------------------------------------------------------------
// <copyright file="Saml2SecurityTokenHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Selectors;
    using System.IO;
    using System.Linq;
    using System.Runtime;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using Claim = System.Security.Claims.Claim;
    using SAML2 = System.IdentityModel.Tokens.Saml2Constants;
    using WSC = System.IdentityModel.WSSecureConversationFeb2005Constants;
    using WSC13 = System.IdentityModel.WSSecureConversation13Constants;
    using WSSE = System.IdentityModel.WSSecurity10Constants;
    using WSSE11 = System.IdentityModel.WSSecurity11Constants;

    /// <summary>
    /// Creates SAML2 assertion-based security tokens
    /// </summary>
    public class Saml2SecurityTokenHandler : SecurityTokenHandler
    {
        /// <summary>
        /// The key identifier value type for SAML 2.0 assertion IDs, as defined
        /// by the OASIS Web Services Security SAML Token Profile 1.1. 
        /// </summary>
        public const string TokenProfile11ValueType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID";
        private const string Actor = "Actor";
        private const string Attribute = "Attribute";
        private static string[] tokenTypeIdentifiers = new string[] { SecurityTokenTypes.Saml2TokenProfile11, SecurityTokenTypes.OasisWssSaml2TokenProfile11 };
        private SamlSecurityTokenRequirement samlSecurityTokenRequirement;
        private SecurityTokenSerializer keyInfoSerializer;

        const string ClaimType2009Namespace = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims";
        object syncObject = new object();

        /// <summary>
        /// Creates an instance of <see cref="Saml2SecurityTokenHandler"/>
        /// </summary>
        public Saml2SecurityTokenHandler()
            : this(new SamlSecurityTokenRequirement())
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="Saml2SecurityTokenHandler"/>
        /// </summary>
        /// <param name="samlSecurityTokenRequirement">The SamlSecurityTokenRequirement to be used by the Saml2SecurityTokenHandler instance when validating tokens.</param>
        public Saml2SecurityTokenHandler(SamlSecurityTokenRequirement samlSecurityTokenRequirement)
        {
            if (samlSecurityTokenRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSecurityTokenRequirement");
            }

            this.samlSecurityTokenRequirement = samlSecurityTokenRequirement;
        }

        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="customConfigElements">SAML token authentication requirements.</param>
        /// <exception cref="ArgumentNullException">Input parameter 'customConfigElements' is null.</exception>
        /// <exception cref="InvalidOperationException">Custom configuration specified was invalid.</exception>
        public override void LoadCustomConfiguration(XmlNodeList customConfigElements)
        {
            if (customConfigElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("customConfigElements");
            }

            List<XmlElement> configNodes = XmlUtil.GetXmlElements(customConfigElements);

            bool foundValidConfig = false;

            foreach (XmlElement configElement in configNodes)
            {
                if (configElement.LocalName != ConfigurationStrings.SamlSecurityTokenRequirement)
                {
                    continue;
                }

                if (foundValidConfig)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7026, ConfigurationStrings.SamlSecurityTokenRequirement));
                }

                this.samlSecurityTokenRequirement = new SamlSecurityTokenRequirement(configElement);

                foundValidConfig = true;
            }

            if (!foundValidConfig)
            {
                this.samlSecurityTokenRequirement = new SamlSecurityTokenRequirement();
            }
        }

        /// <summary>
        /// Returns a value that indicates if this handler can validate <see cref="SecurityToken"/>.
        /// </summary>
        /// <returns>'True', indicating this instance can validate <see cref="SecurityToken"/>.</returns>
        public override bool CanValidateToken
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the token type supported by this handler.
        /// </summary>
        public override Type TokenType
        {
            get { return typeof(Saml2SecurityToken); }
        }

        /// <summary>
        /// Gets or sets the <see cref="X509CertificateValidator"/> that is used by the current instance to validate 
        /// certificates that have signed the <see cref="Saml2SecurityToken"/>.
        /// </summary>
        public X509CertificateValidator CertificateValidator
        {
            get
            {
                if (this.samlSecurityTokenRequirement.CertificateValidator == null)
                {
                    if (Configuration != null)
                    {
                        return Configuration.CertificateValidator;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return this.samlSecurityTokenRequirement.CertificateValidator;
                }
            }

            set
            {
                this.samlSecurityTokenRequirement.CertificateValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="SecurityTokenSerializer"/> that will be used to serialize and deserialize
        /// a <see cref="SecurityKeyIdentifier"/>. For example, SamlSubject SecurityKeyIdentifier or Signature 
        /// SecurityKeyIdentifier.
        /// </summary>
        public SecurityTokenSerializer KeyInfoSerializer
        {
            get
            {
                 if ( this.keyInfoSerializer == null )
                 {
                    lock ( this.syncObject )
                    {
                        if ( this.keyInfoSerializer == null )
                        {
                            SecurityTokenHandlerCollection sthc = ( ContainingCollection != null ) ?
                                ContainingCollection : SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
                            this.keyInfoSerializer = new SecurityTokenSerializerAdapter(sthc);
                        }
                    }
                 }

                return this.keyInfoSerializer;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.keyInfoSerializer = value;
            }
        }

        /// <summary>
        /// Gets the value if this instance can write a token.
        /// </summary>
        public override bool CanWriteToken
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SamlSecurityTokenRequirement"/>.
        /// </summary>
        public SamlSecurityTokenRequirement SamlSecurityTokenRequirement
        {
            get
            {
                return this.samlSecurityTokenRequirement;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.samlSecurityTokenRequirement = value;
            }
        }

        /// <summary>
        /// Creates a <see cref="SecurityKeyIdentifierClause"/> to be used as the security token reference when the token is not attached to the message.
        /// </summary>
        /// <param name="token">The saml token.</param>
        /// <param name="attached">Boolean that indicates if a attached or unattached
        /// reference needs to be created.</param>
        /// <returns>A <see cref="SamlAssertionKeyIdentifierClause"/> instance.</returns>
        public override SecurityKeyIdentifierClause CreateSecurityTokenReference(SecurityToken token, bool attached)
        {
            if (null == token)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            return token.CreateKeyIdentifierClause<Saml2AssertionKeyIdentifierClause>();
        }

        /// <summary>
        /// Creates a <see cref="SecurityToken"/> based on a information contained in the <see cref="SecurityTokenDescriptor"/>.
        /// </summary>
        /// <param name="tokenDescriptor">The <see cref="SecurityTokenDescriptor"/> that has creation information.</param>
        /// <returns>A <see cref="SecurityToken"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if 'tokenDescriptor' is null.</exception>
        public override SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            // Assertion/issuer
            Saml2Assertion assertion = new Saml2Assertion(this.CreateIssuerNameIdentifier(tokenDescriptor));

            // Subject
            assertion.Subject = this.CreateSamlSubject(tokenDescriptor);

            // Signature
            assertion.SigningCredentials = this.GetSigningCredentials(tokenDescriptor);

            // Conditions
            assertion.Conditions = this.CreateConditions(tokenDescriptor.Lifetime, tokenDescriptor.AppliesToAddress, tokenDescriptor);

            // Advice
            assertion.Advice = this.CreateAdvice(tokenDescriptor);

            // Statements
            IEnumerable<Saml2Statement> statements = this.CreateStatements(tokenDescriptor);
            if (null != statements)
            {
                foreach (Saml2Statement statement in statements)
                {
                    assertion.Statements.Add(statement);
                }
            }

            // encrypting credentials
            assertion.EncryptingCredentials = this.GetEncryptingCredentials(tokenDescriptor);

            SecurityToken token = new Saml2SecurityToken(assertion);

            return token;
        }

        /// <summary>
        /// Gets the token type identifier(s) supported by this handler.
        /// </summary>
        /// <returns>A collection of strings that identify the tokens this instance can handle.</returns>
        public override string[] GetTokenTypeIdentifiers()
        {
            return tokenTypeIdentifiers;
        }

        /// <summary>
        /// Validates a <see cref="Saml2SecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="Saml2SecurityToken"/> to validate.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not of assignable from <see cref="Saml2SecurityToken"/>.</exception>
        /// <exception cref="InvalidOperationException">Configuration <see cref="SecurityTokenHandlerConfiguration"/>is null.</exception>
        /// <exception cref="SecurityTokenValidationException">Thrown if Saml2SecurityToken.Assertion.IssuerToken is null.</exception>
        /// <exception cref="SecurityTokenValidationException">Thrown if Saml2SecurityToken.Assertion.SigningToken is null.</exception>
        /// <exception cref="InvalidOperationException">Saml2SecurityToken.Assertion is null.</exception>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            Saml2SecurityToken samlToken = token as Saml2SecurityToken;
            if (samlToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID4151));
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            try
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.Diagnostics, SR.GetString(SR.TraceValidateToken), new SecurityTraceRecordHelper.TokenTraceRecord(token), null, null);

                if (samlToken.IssuerToken == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4152)));
                }

                if (samlToken.Assertion == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID1034));
                }

                this.ValidateConditions(
                    samlToken.Assertion.Conditions,
                    SamlSecurityTokenRequirement.ShouldEnforceAudienceRestriction(this.Configuration.AudienceRestriction.AudienceMode, samlToken));

                //
                // We need something like AudienceUriMode and have a setting on Configuration to allow extensibility and custom settings
                // By default we only check bearer tokens
                //
                if (this.Configuration.DetectReplayedTokens)
                {
                    this.DetectReplayedToken(samlToken);
                }

                Saml2SubjectConfirmation subjectConfirmation = samlToken.Assertion.Subject.SubjectConfirmations[0];
                if (subjectConfirmation.SubjectConfirmationData != null)
                {
                    this.ValidateConfirmationData(subjectConfirmation.SubjectConfirmationData);
                }

                // If the backing token is x509, validate trust
                X509SecurityToken issuerToken = samlToken.IssuerToken as X509SecurityToken;
                if (issuerToken != null)
                {
                    this.CertificateValidator.Validate(issuerToken.Certificate);
                }

                ClaimsIdentity claimsIdentity = null;

                if (this.samlSecurityTokenRequirement.MapToWindows)
                {
                    // TFS: 153865, [....] WindowsIdentity does not set Authtype. I don't think that authtype should be set here anyway.
                    // The authtype will be S4U (kerberos) it doesn't really matter that the upn arrived in a SAML token.
                    claimsIdentity = this.CreateWindowsIdentity(this.FindUpn(claimsIdentity));

                    // PARTIAL TRUST: will fail when adding claims, AddClaims is SecurityCritical.
                    claimsIdentity.AddClaims(this.CreateClaims(samlToken).Claims);
                }
                else
                {
                    claimsIdentity = this.CreateClaims(samlToken);
                }

                if (this.Configuration.SaveBootstrapContext)
                {
                    claimsIdentity.BootstrapContext = new BootstrapContext(token, this);
                }

                this.TraceTokenValidationSuccess(token);

                List<ClaimsIdentity> identities = new List<ClaimsIdentity>(1);
                identities.Add(claimsIdentity);
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
        /// Creates a <see cref="WindowsIdentity"/> object using the <paramref name="upn"/> value.
        /// </summary>
        /// <param name="upn">The upn name.</param>
        /// <returns>A <see cref="WindowsIdentity"/> object.</returns>
        /// <exception cref="ArgumentException">If <paramref name="upn"/> is null or empty.</exception>
        protected virtual WindowsIdentity CreateWindowsIdentity(string upn)
        {
            if (string.IsNullOrEmpty(upn))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("upn");
            }

            WindowsIdentity wi = new WindowsIdentity(upn);

            return new WindowsIdentity(wi.Token, AuthenticationTypes.Federation, WindowsAccountType.Normal, true);
        }
        
        /// <summary>
        /// Writes a Saml2 Token using the XmlWriter.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="SecurityToken"/>.</param>
        /// <param name="token">The <see cref="SecurityToken"/> to serialize.</param>
        /// <exception cref="ArgumentNullException">The input argument 'writer' or 'token' is null.</exception>
        /// <exception cref="ArgumentException">The input argument 'token' is not a <see cref="Saml2SecurityToken"/>.</exception>
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

            Saml2SecurityToken samlToken = token as Saml2SecurityToken;

            if (null != samlToken)
            {
                this.WriteAssertion(writer, samlToken.Assertion);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID4160));
            }
        }

        /// <summary>
        /// Indicates whether the current XML element can be read as a token of the type handled by this instance.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> reader positioned at a start element. The reader should not be advanced.</param>
        /// <returns>'True' if the ReadToken method can read the element.</returns>
        public override bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                return false;
            }

            return reader.IsStartElement(SAML2.Elements.Assertion, SAML2.Namespace)
               || reader.IsStartElement(SAML2.Elements.EncryptedAssertion, SAML2.Namespace);
        }

        /// <summary>
        /// Indicates if the current XML element is pointing to a KeyIdentifierClause that
        /// can be serialized by this instance.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> reader positioned at a start element. The reader should not be advanced.</param>
        /// <returns>'True' if the ReadKeyIdentifierClause can read the element. 'False' otherwise.</returns>
        public override bool CanReadKeyIdentifierClause(XmlReader reader)
        {
            return IsSaml2KeyIdentifierClause(reader);
        }

        /// <summary>
        /// Indicates if the given SecurityKeyIdentifierClause can be serialized by this
        /// instance.
        /// </summary>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to be serialized.</param>
        /// <returns>"True' if the given SecurityKeyIdentifierClause can be serialized. 'False' otherwise.</returns>
        public override bool CanWriteKeyIdentifierClause(SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            return (securityKeyIdentifierClause is Saml2AssertionKeyIdentifierClause) ||
                (securityKeyIdentifierClause is WrappedSaml2AssertionKeyIdentifierClause);
        }

        /// <summary>
        /// Reads a SecurityKeyIdentifierClause.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> reader positioned at a <see cref="SecurityKeyIdentifierClause"/> element.</param>
        /// <returns>A <see cref="SecurityKeyIdentifierClause"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'reader' is null.</exception>
        public override SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!IsSaml2KeyIdentifierClause(reader))
            {
                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4161));
            }

            // disallow empty
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, WSSE.Elements.SecurityTokenReference, WSSE.Namespace));
            }

            try
            {
                // @attributes
                string value;
                string id;
                byte[] nonce = null;
                int length = 0;

                // @wsse11:TokenType is checked by IsSaml2KeyIdentifierClause

                // @wsc:Nonce and @wsc:Length, first try WSCFeb2005
                value = reader.GetAttribute(WSC.Attributes.Nonce, WSC.Namespace);
                if (!string.IsNullOrEmpty(value))
                {
                    nonce = Convert.FromBase64String(value);

                    value = reader.GetAttribute(WSC.Attributes.Length, WSC.Namespace);
                    if (!string.IsNullOrEmpty(value))
                    {
                        length = XmlConvert.ToInt32(value);
                    }
                    else
                    {
                        length = WSC.DefaultDerivedKeyLength;
                    }
                }

                // @wsc:Nonce and @wsc:Length, now try WSC13
                if (null == nonce)
                {
                    value = reader.GetAttribute(WSC13.Attributes.Nonce, WSC13.Namespace);
                    if (!string.IsNullOrEmpty(value))
                    {
                        nonce = Convert.FromBase64String(value);

                        value = reader.GetAttribute(WSC13.Attributes.Length, WSC13.Namespace);
                        if (!string.IsNullOrEmpty(value))
                        {
                            length = XmlConvert.ToInt32(value);
                        }
                        else
                        {
                            length = WSC13.DefaultDerivedKeyLength;
                        }
                    }
                }

                // <wsse:SecurityTokenReference> content begins
                reader.Read();

                // <wsse:Reference> - throw exception
                if (reader.IsStartElement(WSSE.Elements.Reference, WSSE.Namespace))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4126));
                }

                // <wsse:KeyIdentifier>
                if (!reader.IsStartElement(WSSE.Elements.KeyIdentifier, WSSE.Namespace))
                {
                    reader.ReadStartElement(WSSE.Elements.KeyIdentifier, WSSE.Namespace);
                }

                // @ValueType - required
                value = reader.GetAttribute(WSSE.Attributes.ValueType);
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, WSSE.Attributes.ValueType, WSSE.Elements.KeyIdentifier));
                }

                if (!StringComparer.Ordinal.Equals(TokenProfile11ValueType, value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4127, value));
                }

                // <wsse:KeyIdentifier> Content is string </wsse:KeyIdentifier>
                id = reader.ReadElementString();

                // </wsse:SecurityTokenReference>
                reader.ReadEndElement();

                return new Saml2AssertionKeyIdentifierClause(id, nonce, length);
            }
            catch (Exception inner)
            {
                // Wrap common data-validation exceptions that may have bubbled up
                if (inner is FormatException
                    || inner is ArgumentException
                    || inner is InvalidOperationException
                    || inner is OverflowException)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4125), inner);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Reads a SAML 2.0 token from the XmlReader.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> reader positioned at a <see cref="Saml2SecurityToken"/> element.</param>
        /// <returns>An instance of <see cref="Saml2SecurityToken"/>.</returns>
        /// <exception cref="InvalidOperationException">Is thrown if 'Configuration', 'Configruation.IssuerTokenResolver' or 'Configuration.ServiceTokenResolver is null.</exception>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            if (Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (Configuration.IssuerTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4275));
            }

            if (Configuration.ServiceTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4276));
            }

            Saml2Assertion assertion = this.ReadAssertion(reader);

            ReadOnlyCollection<SecurityKey> keys = this.ResolveSecurityKeys(assertion, Configuration.ServiceTokenResolver);

            // Resolve signing token if one is present. It may be deferred and signed by reference.
            SecurityToken issuerToken;
            this.TryResolveIssuerToken(assertion, Configuration.IssuerTokenResolver, out issuerToken);

            return new Saml2SecurityToken(assertion, keys, issuerToken);
        }

        /// <summary>
        /// Serializes a Saml2AssertionKeyIdentifierClause to the XmlWriter.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="SecurityKeyIdentifierClause"/>.</param>
        /// <param name="securityKeyIdentifierClause">The <see cref="SecurityKeyIdentifierClause"/> to be serialized.</param>
        /// <exception cref="ArgumentNullException">Input parameter 'writer' or 'securityKeyIdentifierClause' is null.</exception>
        public override void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (securityKeyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }

            Saml2AssertionKeyIdentifierClause samlClause = null;
            WrappedSaml2AssertionKeyIdentifierClause wrappedClause = securityKeyIdentifierClause as WrappedSaml2AssertionKeyIdentifierClause;

            if (wrappedClause != null)
            {
                samlClause = wrappedClause.WrappedClause;
            }
            else
            {
                samlClause = securityKeyIdentifierClause as Saml2AssertionKeyIdentifierClause;
            }

            if (null == samlClause)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("keyIdentifierClause", SR.GetString(SR.ID4162));
            }

            // <wsse:SecurityTokenReference>
            writer.WriteStartElement(WSSE.Elements.SecurityTokenReference, WSSE.Namespace);

            // @wsc:Nonce
            byte[] nonce = samlClause.GetDerivationNonce();
            if (null != nonce)
            {
                writer.WriteAttributeString(WSC.Attributes.Nonce, WSC.Namespace, Convert.ToBase64String(nonce));

                int length = samlClause.DerivationLength;

                // Don't emit @wsc:Length since it's not actually in the spec/schema
                if (length != 0 && length != WSC.DefaultDerivedKeyLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.ID4129)));
                }
            }

            // @wsse11:TokenType
            writer.WriteAttributeString(WSSE11.Attributes.TokenType, WSSE11.Namespace, SecurityTokenTypes.OasisWssSaml2TokenProfile11);

            // <wsse:KeyIdentifier>
            writer.WriteStartElement(WSSE.Elements.KeyIdentifier, WSSE.Namespace);

            // @ValueType
            writer.WriteAttributeString(WSSE.Attributes.ValueType, TokenProfile11ValueType);

            // ID is the string content
            writer.WriteString(samlClause.Id);

            // </wsse:KeyIdentifier>
            writer.WriteEndElement();

            // </wsse:SecurityTokenReference>
            writer.WriteEndElement();
        }

        internal static XmlDictionaryReader CreatePlaintextReaderFromEncryptedData(
                        XmlDictionaryReader reader,
                        SecurityTokenResolver serviceTokenResolver,
                        SecurityTokenSerializer keyInfoSerializer,
                        Collection<EncryptedKeyIdentifierClause> clauses,
                        out EncryptingCredentials encryptingCredentials)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
#pragma warning suppress 56504 // bogus - thinks reader.LocalName, reader.NamespaceURI need validation
                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, reader.LocalName, reader.NamespaceURI));
            }

            encryptingCredentials = null;

            XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.EncryptedElementType, Saml2Constants.Namespace);

            reader.ReadStartElement();
            EncryptedDataElement encryptedData = new EncryptedDataElement(keyInfoSerializer);

            // <xenc:EncryptedData> 1
            encryptedData.ReadXml(reader);

            // <xenc:EncryptedKey> 0-oo
            reader.MoveToContent();
            while (reader.IsStartElement(XmlEncryptionConstants.Elements.EncryptedKey, XmlEncryptionConstants.Namespace))
            {
                SecurityKeyIdentifierClause skic;
                if (keyInfoSerializer.CanReadKeyIdentifierClause(reader))
                {
                    skic = keyInfoSerializer.ReadKeyIdentifierClause(reader);
                }
                else
                {
                    EncryptedKeyElement encryptedKey = new EncryptedKeyElement(keyInfoSerializer);
                    encryptedKey.ReadXml(reader);
                    skic = encryptedKey.GetClause();
                }

                EncryptedKeyIdentifierClause encryptedKeyClause = skic as EncryptedKeyIdentifierClause;
                if (null == encryptedKeyClause)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4172));
                }

                clauses.Add(encryptedKeyClause);
            }

            reader.ReadEndElement();

            // Try to resolve the decryption key from both the embedded 
            // KeyInfo and any external clauses
            SecurityKey decryptionKey = null;
            SecurityKeyIdentifierClause matchingClause = null;

            foreach (SecurityKeyIdentifierClause clause in encryptedData.KeyIdentifier)
            {
                if (serviceTokenResolver.TryResolveSecurityKey(clause, out decryptionKey))
                {
                    matchingClause = clause;
                    break;
                }
            }

            if (null == decryptionKey)
            {
                foreach (SecurityKeyIdentifierClause clause in clauses)
                {
                    if (serviceTokenResolver.TryResolveSecurityKey(clause, out decryptionKey))
                    {
                        matchingClause = clause;
                        break;
                    }
                }
            }

            if (null == decryptionKey)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new EncryptedTokenDecryptionFailedException());
            }

            // Need a symmetric key
            SymmetricSecurityKey symmetricKey = decryptionKey as SymmetricSecurityKey;
            if (null == symmetricKey)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SecurityTokenException(SR.GetString(SR.ID4023)));
            }

            // Do the actual decryption
            SymmetricAlgorithm decryptor = symmetricKey.GetSymmetricAlgorithm(encryptedData.Algorithm);
            byte[] plainText = encryptedData.Decrypt(decryptor);

            // Save off the encrypting credentials for roundtrip
            encryptingCredentials = new ReceivedEncryptingCredentials(decryptionKey, new SecurityKeyIdentifier(matchingClause), encryptedData.Algorithm);

            return XmlDictionaryReader.CreateTextReader(plainText, reader.Quotas);
        }

        // Wraps common data validation exceptions with an XmlException 
        // associated with the failing reader
        internal static Exception TryWrapReadException(XmlReader reader, Exception inner)
        {
            if (inner is FormatException
                || inner is ArgumentException
                || inner is InvalidOperationException
                || inner is OverflowException)
            {
                return DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4125), inner);
            }

            return null;
        }

        /// <summary>
        /// Indicates if the current XML element is pointing to a Saml2SecurityKeyIdentifierClause.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> reader.</param>
        /// <returns>'True' if reader contains a <see cref="Saml2SecurityKeyIdentifierClause"/>. 'False' otherwise.</returns>
        internal static bool IsSaml2KeyIdentifierClause(XmlReader reader)
        {
            if (!reader.IsStartElement(WSSE.Elements.SecurityTokenReference, WSSE.Namespace))
            {
                return false;
            }

            string tokenType = reader.GetAttribute(WSSE11.Attributes.TokenType, WSSE11.Namespace);
            return tokenTypeIdentifiers.Contains(tokenType);
        }

        /// <summary>
        /// Indicates if the current XML element is pointing to a Saml2Assertion.
        /// </summary>
        /// <param name="reader">A reader that may contain a <see cref="Saml2Assertion"/>.</param>
        /// <returns>'True' if reader contains a <see cref="Saml2Assertion"/>. 'False' otherwise.</returns>
        internal static bool IsSaml2Assertion(XmlReader reader)
        {
            return reader.IsStartElement(SAML2.Elements.Assertion, SAML2.Namespace)
               || reader.IsStartElement(SAML2.Elements.EncryptedAssertion, SAML2.Namespace);
        }

        // Read an element that must not contain content.
        internal static void ReadEmptyContentElement(XmlReader reader)
        {
            bool isEmpty = reader.IsEmptyElement;
            reader.Read();
            if (!isEmpty)
            {
                reader.ReadEndElement();
            }
        }

        internal static Saml2Id ReadSimpleNCNameElement(XmlReader reader)
        {
            Fx.Assert(reader.IsStartElement(), "reader is not on start element");

            try
            {
                if (reader.IsEmptyElement)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, reader.LocalName, reader.NamespaceURI));
                }

                XmlUtil.ValidateXsiType(reader, "NCName", XmlSchema.Namespace);

                reader.MoveToElement();
                string value = reader.ReadElementContentAsString();

                return new Saml2Id(value);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        // Reads an element with simple content anyURI. Since this is SAML, 
        // restricts the URI to absolute.
        internal static Uri ReadSimpleUriElement(XmlReader reader)
        {
            return ReadSimpleUriElement(reader, UriKind.Absolute);
        }

        // Reads an element with simple content anyURI where a UriKind can be specified
        internal static Uri ReadSimpleUriElement(XmlReader reader, UriKind kind)
        {
            return ReadSimpleUriElement(reader, kind, false);
        }

        // allow lax reading of relative URIs in some instances for interop
        internal static Uri ReadSimpleUriElement(XmlReader reader, UriKind kind, bool allowLaxReading)
        {
            Fx.Assert(reader.IsStartElement(), "reader is not on start element");

            try
            {
                if (reader.IsEmptyElement)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, reader.LocalName, reader.NamespaceURI));
                }

                XmlUtil.ValidateXsiType(reader, "anyURI", XmlSchema.Namespace);

                reader.MoveToElement();
                string value = reader.ReadElementContentAsString();

                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0022));
                }

                if (!allowLaxReading && !UriUtil.CanCreateValidUri(value, kind))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(kind == UriKind.RelativeOrAbsolute ? SR.ID0019 : SR.ID0013));
                }

                return new Uri(value, kind);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Creates the conditions for the assertion.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Generally, conditions should be included in assertions to limit the 
        /// impact of misuse of the assertion. Specifying the NotBefore and 
        /// NotOnOrAfter conditions can limit the period of vulnerability in 
        /// the case of a compromised assertion. The AudienceRestrictionCondition
        /// can be used to explicitly state the intended relying party or parties
        /// of the assertion, which coupled with appropriate audience restriction
        /// enforcement at relying parties can help to mitigate spoofing attacks
        /// between relying parties.
        /// </para>
        /// <para>
        /// The default implementation creates NotBefore and NotOnOrAfter conditions
        /// based on the tokenDescriptor.Lifetime. It will also generate an 
        /// AudienceRestrictionCondition limiting consumption of the assertion to 
        /// tokenDescriptor.Scope.Address.
        /// </para>
        /// </remarks>
        /// <param name="tokenLifetime">Lifetime of the Token.</param>
        /// <param name="relyingPartyAddress">The endpoint address to who the token is created. The address
        /// is modeled as an AudienceRestriction condition.</param>
        /// <param name="tokenDescriptor">The token descriptor.</param>
        /// <returns>A Saml2Conditions object.</returns>
        protected virtual Saml2Conditions CreateConditions(Lifetime tokenLifetime, string relyingPartyAddress, SecurityTokenDescriptor tokenDescriptor)
        {
            bool hasLifetime = null != tokenLifetime;
            bool hasScope = !string.IsNullOrEmpty(relyingPartyAddress);
            if (!hasLifetime && !hasScope)
            {
                return null;
            }

            Saml2Conditions conditions = new Saml2Conditions();
            if (hasLifetime)
            {
                conditions.NotBefore = tokenLifetime.Created;
                conditions.NotOnOrAfter = tokenLifetime.Expires;
            }

            if (hasScope)
            {
                conditions.AudienceRestrictions.Add(new Saml2AudienceRestriction(new Uri(relyingPartyAddress)));
            }

            return conditions;
        }

        /// <summary>
        /// Creates the advice for the assertion.
        /// </summary>
        /// <remarks>
        /// By default, this method returns null.
        /// </remarks>
        /// <param name="tokenDescriptor">The token descriptor.</param>
        /// <returns>A Saml2Advice object, default is null.</returns>
        protected virtual Saml2Advice CreateAdvice(SecurityTokenDescriptor tokenDescriptor)
        {
            return null;
        }

        /// <summary>
        /// Creates a name identifier that identifies the assertion issuer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// SAML2 assertions must contain a name identifier for the issuer. 
        /// This method may not return null.
        /// </para>
        /// <para>
        /// The default implementation creates a simple name identifier 
        /// from the tokenDescriptor.Issuer. 
        /// </para>
        /// </remarks>
        /// <param name="tokenDescriptor">The token descriptor.</param>
        /// <returns>A <see cref="Saml2NameIdentifier"/> from the tokenDescriptor</returns>
        protected virtual Saml2NameIdentifier CreateIssuerNameIdentifier(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            string issuerName = tokenDescriptor.TokenIssuerName;

            // Must have an issuer
            if (string.IsNullOrEmpty(issuerName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4138)));
            }

            return new Saml2NameIdentifier(issuerName);
        }

        /// <summary>
        /// Generates a Saml2Attribute from a claim.
        /// </summary>
        /// <param name="claim">The <see cref="Claim"/> from which to generate a <see cref="Saml2Attribute"/>.</param>
        /// <param name="tokenDescriptor">Contains all the information that is used in token issuance.</param>
        /// <returns>A <see cref="Saml2Attribute"/> based on the claim.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'claim' is null.</exception>
        protected virtual Saml2Attribute CreateAttribute(Claim claim, SecurityTokenDescriptor tokenDescriptor)
        {
            if (claim == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }

            Saml2Attribute attribute = new Saml2Attribute(claim.Type, claim.Value);
            if (!StringComparer.Ordinal.Equals(ClaimsIdentity.DefaultIssuer, claim.OriginalIssuer))
            {
                attribute.OriginalIssuer = claim.OriginalIssuer;
            }

            attribute.AttributeValueXsiType = claim.ValueType;

            if (claim.Properties.ContainsKey(ClaimProperties.SamlAttributeNameFormat))
            {
                string nameFormat = claim.Properties[ClaimProperties.SamlAttributeNameFormat];
                if (!UriUtil.CanCreateValidUri(nameFormat, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("nameFormat", SR.GetString(SR.ID0013));
                }

                attribute.NameFormat = new Uri(nameFormat);
            }

            if (claim.Properties.ContainsKey(ClaimProperties.SamlAttributeDisplayName))
            {
                attribute.FriendlyName = claim.Properties[ClaimProperties.SamlAttributeDisplayName];
            }

            return attribute;
        }

        /// <summary>
        /// Creates <see cref="Saml2AttributeStatement"/> from a <see cref="SecurityTokenDescriptor"/> and a <see cref="ClaimsIdentity"/>
        /// </summary>
        /// <remarks>This method may return null if the token descriptor does not contain any subject or the subject does not have any claims.
        /// </remarks>
        /// <param name="subject">The <see cref="ClaimsIdentity"/> that contains claims which will be converted to SAML Attributes.</param>
        /// <param name="tokenDescriptor">The <see cref="SecurityTokenDescriptor"/> that contains information on building the <see cref="Saml2AttributeStatement"/>.</param>
        /// <returns>A Saml2AttributeStatement.</returns>
        protected virtual Saml2AttributeStatement CreateAttributeStatement(ClaimsIdentity subject, SecurityTokenDescriptor tokenDescriptor)
        {
            if (subject == null)
            {
                return null;
            }

            // We treat everything else as an Attribute except the nameId claim, which is already processed
            // for saml2subject
            // AuthenticationInstant and AuthenticationType are not converted to Claims
            if (subject.Claims != null)
            {
                List<Saml2Attribute> attributes = new List<Saml2Attribute>();
                foreach (Claim claim in subject.Claims)
                {
                    if (claim != null && claim.Type != ClaimTypes.NameIdentifier)
                    {
                        switch (claim.Type)
                        {
                            case ClaimTypes.AuthenticationInstant:
                            case ClaimTypes.AuthenticationMethod:
                                break;
                            default:
                                attributes.Add(this.CreateAttribute(claim, tokenDescriptor));
                                break;
                        }
                    }
                }

                this.AddDelegateToAttributes(subject, attributes, tokenDescriptor);

                ICollection<Saml2Attribute> collectedAttributes = this.CollectAttributeValues(attributes);
                if (collectedAttributes.Count > 0)
                {
                    return new Saml2AttributeStatement(collectedAttributes);
                }
            }

            return null;
        }

        /// <summary>
        /// Collects attributes with a common claim type, claim value type, and original issuer into a
        /// single attribute with multiple values.
        /// </summary>
        /// <param name="attributes">List of attributes generated from claims.</param>
        /// <returns>A <see cref="ICollection{T}"/> of <see cref="Saml2Attribute"/> with common attributes collected into value lists.</returns>
        protected virtual ICollection<Saml2Attribute> CollectAttributeValues(ICollection<Saml2Attribute> attributes)
        {
            Dictionary<SamlAttributeKeyComparer.AttributeKey, Saml2Attribute> distinctAttributes = new Dictionary<SamlAttributeKeyComparer.AttributeKey, Saml2Attribute>(attributes.Count, new SamlAttributeKeyComparer());

            // Use unique attribute if name, value type, or issuer differ
            foreach (Saml2Attribute saml2Attribute in attributes)
            {
                if (saml2Attribute != null)
                {
                    SamlAttributeKeyComparer.AttributeKey attributeKey = new SamlAttributeKeyComparer.AttributeKey(saml2Attribute);

                    if (distinctAttributes.ContainsKey(attributeKey))
                    {
                        foreach (string attributeValue in saml2Attribute.Values)
                        {
                            distinctAttributes[attributeKey].Values.Add(attributeValue);
                        }
                    }
                    else
                    {
                        distinctAttributes.Add(attributeKey, saml2Attribute);
                    }
                }
            }

            return distinctAttributes.Values;
        }

        /// <summary>
        /// Adds all the delegates associated with the subject into the attribute collection.
        /// </summary>
        /// <param name="subject">The delegate of this <see cref="ClaimsIdentity"/> will be serialized into a <see cref="Saml2Attribute"/>.</param>
        /// <param name="attributes">A <see cref="ICollection{T}"/> of <see cref="Saml2Attribute"/>.</param>
        /// <param name="tokenDescriptor">The <see cref="SecurityTokenDescriptor"/> that contains information on building the delegate.</param>
        protected virtual void AddDelegateToAttributes(ClaimsIdentity subject, ICollection<Saml2Attribute> attributes, SecurityTokenDescriptor tokenDescriptor)
        {
            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }

            if (tokenDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            if (subject.Actor == null)
            {
                return;
            }

            List<Saml2Attribute> actingAsAttributes = new List<Saml2Attribute>();

            foreach (Claim claim in subject.Actor.Claims)
            {
                if (claim != null)
                {
                    actingAsAttributes.Add(this.CreateAttribute(claim, tokenDescriptor));
                }
            }

            this.AddDelegateToAttributes(subject.Actor, actingAsAttributes, tokenDescriptor);

            ICollection<Saml2Attribute> collectedAttributes = this.CollectAttributeValues(actingAsAttributes);
            attributes.Add(this.CreateAttribute(new Claim(ClaimTypes.Actor, this.CreateXmlStringFromAttributes(collectedAttributes), ClaimValueTypes.String), tokenDescriptor));
        }

        /// <summary>
        /// Builds an XML formatted string from a collection of SAML attributes that represent the Actor. 
        /// </summary>
        /// <param name="attributes">An enumeration of Saml2Attributes.</param>
        /// <returns>A well-formed XML string.</returns>
        /// <remarks>The string is of the form ""&lt;Actor&gt;&lt;Attribute name, ns&gt;&lt;AttributeValue&gt;...&lt;/AttributeValue&gt;, ...&lt;/Attribute&gt;...&lt;/Actor&gt;"</remarks>
        protected virtual string CreateXmlStringFromAttributes(IEnumerable<Saml2Attribute> attributes)
        {
            bool actorElementWritten = false;

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlDictionaryWriter dicWriter = XmlDictionaryWriter.CreateTextWriter(ms, Encoding.UTF8, false))
                {
                    foreach (Saml2Attribute samlAttribute in attributes)
                    {
                        if (samlAttribute != null)
                        {
                            if (!actorElementWritten)
                            {
                                dicWriter.WriteStartElement(Actor);
                                actorElementWritten = true;
                            }

                            this.WriteAttribute(dicWriter, samlAttribute);
                        }
                    }

                    if (actorElementWritten)
                    {
                        dicWriter.WriteEndElement();
                    }

                    dicWriter.Flush();
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{T}"/> of <see cref="Saml2Statement"/> to be included in the assertion.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Statements are not required in a SAML2 assertion. This method may
        /// return an empty collection.
        /// </para>
        /// </remarks>
        /// <param name="tokenDescriptor">The <see cref="SecurityTokenDescriptor"/> that contains information on creating the <see cref="Saml2Statement"/>.</param>
        /// <returns>An enumeration of Saml2Statements.</returns>
        protected virtual IEnumerable<Saml2Statement> CreateStatements(SecurityTokenDescriptor tokenDescriptor)
        {
            if (tokenDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            Collection<Saml2Statement> statements = new Collection<Saml2Statement>();

            Saml2AttributeStatement attributeStatement = this.CreateAttributeStatement(tokenDescriptor.Subject, tokenDescriptor);
            if (attributeStatement != null)
            {
                statements.Add(attributeStatement);
            }

            Saml2AuthenticationStatement authenticationStatement = this.CreateAuthenticationStatement(tokenDescriptor.AuthenticationInfo, tokenDescriptor);
            if (authenticationStatement != null)
            {
                statements.Add(authenticationStatement);
            }

            return statements;
        }

        /// <summary>
        /// Given an AuthenticationInformation object, this routine creates a Saml2AuthenticationStatement
        /// to be added to the Saml2Assertion that is produced by the factory.
        /// </summary>
        /// <param name="authInfo">
        /// An AuthenticationInformation object containing the state to be wrapped as a Saml2AuthenticationStatement
        /// object.
        /// </param>
        /// <param name="tokenDescriptor">The token descriptor.</param>
        /// <returns>
        /// The Saml2AuthenticationStatement to add to the assertion being created or null to ignore the AuthenticationInformation
        /// being wrapped as a statement.
        /// </returns>
        protected virtual Saml2AuthenticationStatement CreateAuthenticationStatement(AuthenticationInformation authInfo, SecurityTokenDescriptor tokenDescriptor)
        {
            if (tokenDescriptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            if (tokenDescriptor.Subject == null)
            {
                return null;
            }

            string authenticationMethod = null;
            string authenticationInstant = null;

            // Search for an Authentication Claim.
            IEnumerable<Claim> claimCollection = from c in tokenDescriptor.Subject.Claims where c.Type == ClaimTypes.AuthenticationMethod select c;
            if (claimCollection.Count<Claim>() > 0)
            {
                // We support only one authentication statement and hence we just pick the first authentication type
                // claim found in the claim collection. Since the spec allows multiple Auth Statements, 
                // we do not throw an error.
                authenticationMethod = claimCollection.First<Claim>().Value;
            }

            claimCollection = from c in tokenDescriptor.Subject.Claims where c.Type == ClaimTypes.AuthenticationInstant select c;

            if (claimCollection.Count<Claim>() > 0)
            {
                authenticationInstant = claimCollection.First<Claim>().Value;
            }

            if (authenticationMethod == null && authenticationInstant == null)
            {
                return null;
            }
            else if (authenticationMethod == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4270, "AuthenticationMethod", "SAML2"));
            }
            else if (authenticationInstant == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4270, "AuthenticationInstant", "SAML2"));
            }

            Uri saml2AuthenticationClass;
            if (!UriUtil.TryCreateValidUri(this.DenormalizeAuthenticationType(authenticationMethod), UriKind.Absolute, out saml2AuthenticationClass))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4185, authenticationMethod));
            }

            Saml2AuthenticationContext authCtx = new Saml2AuthenticationContext(saml2AuthenticationClass);
            DateTime authInstantTime = DateTime.ParseExact(authenticationInstant, DateTimeFormats.Accepted, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            Saml2AuthenticationStatement authnStatement = new Saml2AuthenticationStatement(authCtx, authInstantTime);

            if (authInfo != null)
            {
                if (!string.IsNullOrEmpty(authInfo.DnsName)
                    || !string.IsNullOrEmpty(authInfo.Address))
                {
                    authnStatement.SubjectLocality
                        = new Saml2SubjectLocality(authInfo.Address, authInfo.DnsName);
                }

                if (!string.IsNullOrEmpty(authInfo.Session))
                {
                    authnStatement.SessionIndex = authInfo.Session;
                }

                authnStatement.SessionNotOnOrAfter = authInfo.NotOnOrAfter;
            }

            return authnStatement;
        }

        /// <summary>
        /// Creates a SAML2 subject of the assertion.
        /// </summary>
        /// <param name="tokenDescriptor">The security token descriptor to create the subject.</param>
        /// <exception cref="ArgumentNullException">Thrown when 'tokenDescriptor' is null.</exception>
        /// <returns>A Saml2Subject.</returns>
        protected virtual Saml2Subject CreateSamlSubject(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            Saml2Subject saml2Subject = new Saml2Subject();

            // Look for name identifier claims
            string nameIdentifierClaim = null;
            string nameIdentifierFormat = null;
            string nameIdentifierNameQualifier = null;
            string nameIdentifierSpProviderId = null;
            string nameIdentifierSpNameQualifier = null;

            if (tokenDescriptor.Subject != null && tokenDescriptor.Subject.Claims != null)
            {
                foreach (Claim claim in tokenDescriptor.Subject.Claims)
                {
                    if (claim.Type == ClaimTypes.NameIdentifier)
                    {
                        // Do not allow multiple name identifier claim.
                        if (null != nameIdentifierClaim)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4139)));
                        }

                        nameIdentifierClaim = claim.Value;

                        if (claim.Properties.ContainsKey(ClaimProperties.SamlNameIdentifierFormat))
                        {
                            nameIdentifierFormat = claim.Properties[ClaimProperties.SamlNameIdentifierFormat];
                        }

                        if (claim.Properties.ContainsKey(ClaimProperties.SamlNameIdentifierNameQualifier))
                        {
                            nameIdentifierNameQualifier = claim.Properties[ClaimProperties.SamlNameIdentifierNameQualifier];
                        }

                        if (claim.Properties.ContainsKey(ClaimProperties.SamlNameIdentifierSPNameQualifier))
                        {
                            nameIdentifierSpNameQualifier = claim.Properties[ClaimProperties.SamlNameIdentifierSPNameQualifier];
                        }

                        if (claim.Properties.ContainsKey(ClaimProperties.SamlNameIdentifierSPProvidedId))
                        {
                            nameIdentifierSpProviderId = claim.Properties[ClaimProperties.SamlNameIdentifierSPProvidedId];
                        }
                    }
                }
            }

            if (nameIdentifierClaim != null)
            {
                Saml2NameIdentifier nameIdentifier = new Saml2NameIdentifier(nameIdentifierClaim);

                if (nameIdentifierFormat != null && UriUtil.CanCreateValidUri(nameIdentifierFormat, UriKind.Absolute))
                {
                    nameIdentifier.Format = new Uri(nameIdentifierFormat);
                }

                nameIdentifier.NameQualifier = nameIdentifierNameQualifier;
                nameIdentifier.SPNameQualifier = nameIdentifierSpNameQualifier;
                nameIdentifier.SPProvidedId = nameIdentifierSpProviderId;

                saml2Subject.NameId = nameIdentifier;
            }

            // Add subject confirmation data
            Saml2SubjectConfirmation subjectConfirmation;
            if (null == tokenDescriptor.Proof)
            {
                subjectConfirmation = new Saml2SubjectConfirmation(Saml2Constants.ConfirmationMethods.Bearer);
            }
            else
            {
                subjectConfirmation = new Saml2SubjectConfirmation(Saml2Constants.ConfirmationMethods.HolderOfKey, new Saml2SubjectConfirmationData());
                subjectConfirmation.SubjectConfirmationData.KeyIdentifiers.Add(tokenDescriptor.Proof.KeyIdentifier);
            }

            saml2Subject.SubjectConfirmations.Add(subjectConfirmation);

            return saml2Subject;
        }

        /// <summary>
        /// Override this method to change the token encrypting credentials. 
        /// </summary>
        /// <param name="tokenDescriptor">Retrieve some scope encrypting credentials from the Scope object</param>
        /// <returns>the token encrypting credentials</returns>
        /// <exception cref="ArgumentNullException">When the given tokenDescriptor is null</exception>
        protected virtual EncryptingCredentials GetEncryptingCredentials(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            EncryptingCredentials encryptingCredentials = null;

            if (null != tokenDescriptor.EncryptingCredentials)
            {
                encryptingCredentials = tokenDescriptor.EncryptingCredentials;
                if (encryptingCredentials.SecurityKey is AsymmetricSecurityKey)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4178)));
                }
            }

            return encryptingCredentials;
        }

        /// <summary>
        /// Gets the credentials for the signing the assertion.
        /// </summary>
        /// <remarks>
        /// <para>
        /// SAML2 assertions used as security tokens should be signed.
        /// </para>
        /// <para>
        /// The default implementation uses the 
        /// tokenDescriptor.Scope.SigningCredentials.
        /// </para>
        /// </remarks>
        /// <param name="tokenDescriptor">The token descriptor.</param>
        /// <returns>The signing credential.</returns>
        protected virtual SigningCredentials GetSigningCredentials(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            return tokenDescriptor.SigningCredentials;
        }

        /// <summary>
        /// Rejects tokens that are not valid. 
        /// </summary>
        /// <remarks>
        /// The token may not be valid for a number of reasons. For example, the 
        /// current time may not be within the token's validity period, the 
        /// token may contain data that is contradictory or not valid, or the token 
        /// may contain unsupported SAML2 elements.
        /// </remarks>
        /// <param name="conditions">SAML 2.0 condition to be validated.</param>
        /// <param name="enforceAudienceRestriction">True to check for Audience Restriction condition.</param>
        protected virtual void ValidateConditions(Saml2Conditions conditions, bool enforceAudienceRestriction)
        {
            if (conditions != null)
            {
                DateTime now = DateTime.UtcNow;

                if (conditions.NotBefore != null
                    && DateTimeUtil.Add(now, Configuration.MaxClockSkew) < conditions.NotBefore.Value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenNotYetValidException(SR.GetString(SR.ID4147, conditions.NotBefore.Value, now)));
                }

                if (conditions.NotOnOrAfter != null
                    && DateTimeUtil.Add(now, Configuration.MaxClockSkew.Negate()) >= conditions.NotOnOrAfter.Value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenExpiredException(SR.GetString(SR.ID4148, conditions.NotOnOrAfter.Value, now)));
                }

                if (conditions.OneTimeUse)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4149)));
                }

                if (conditions.ProxyRestriction != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4150)));
                }
            }

            if (enforceAudienceRestriction)
            {
                if (this.Configuration == null || this.Configuration.AudienceRestriction.AllowedAudienceUris.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID1032)));
                }

                if (conditions == null || conditions.AudienceRestrictions.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AudienceUriValidationFailedException(SR.GetString(SR.ID1035)));
                }
                else
                {
                    foreach (Saml2AudienceRestriction audienceRestriction in conditions.AudienceRestrictions)
                    {
                        SamlSecurityTokenRequirement.ValidateAudienceRestriction(this.Configuration.AudienceRestriction.AllowedAudienceUris, audienceRestriction.Audiences);
                    }
                }
            }
        }

        /// <summary>
        /// Finds the UPN claim value in the provided <see cref="ClaimsIdentity" /> object for the purpose
        /// of mapping the identity to a <see cref="WindowsIdentity" /> object.
        /// </summary>
        /// <param name="claimsIdentity">The claims identity object containing the desired UPN claim.</param>
        /// <returns>The UPN claim value found.</returns>
        protected virtual string FindUpn(ClaimsIdentity claimsIdentity)
        {
            return ClaimsHelper.FindUpn(claimsIdentity);
        }

        /// <summary>
        /// Returns the Saml2 AuthenticationContext matching a normalized value.
        /// </summary>
        /// <param name="normalizedAuthenticationType">Normalized value.</param>
        /// <returns>A string that represents the denormalized authentication type used to obtain the token.</returns>
        protected virtual string DenormalizeAuthenticationType(string normalizedAuthenticationType)
        {
            return AuthenticationTypeMaps.Denormalize(normalizedAuthenticationType, AuthenticationTypeMaps.Saml2);
        }

        /// <summary>
        /// Throws if a token is detected as being replayed. If the token is not found, it is added to the 
        /// <see cref="TokenReplayCache" />.
        /// </summary>
        /// <param name="token">The token to detect for replay.</param>
        /// <exception cref="ArgumentNullException">The input argument 'token' is null.</exception>
        /// <exception cref="InvalidOperationException">Configuration or Configuration.TokenReplayCache property is null.</exception>
        /// <exception cref="ArgumentException">The input argument 'token' can not be cast as a 'Saml2SecurityToken'.</exception>
        /// <exception cref="SecurityTokenValidationException">The Saml2SecurityToken.Assertion.Id.Value is null or empty.</exception>
        /// <exception cref="SecurityTokenReplayDetectedException">The token is found in the <see cref="TokenReplayCache" />.</exception>
        /// <remarks>The default behavior is to only check tokens bearer tokens (tokens that do not have keys).</remarks>
        protected override void DetectReplayedToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            Saml2SecurityToken samlToken = token as Saml2SecurityToken;
            if (null == samlToken)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID1064, token.GetType().ToString()));
            }

            // by default we only check bearer tokens.
            if (samlToken.SecurityKeys.Count != 0)
            {
                return;
            }

            if (Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (Configuration.Caches.TokenReplayCache == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4278));
            }

            if (string.IsNullOrEmpty(samlToken.Assertion.Id.Value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID1065)));
            }

            StringBuilder stringBuilder = new StringBuilder();
            string key;

            using (HashAlgorithm hashAlgorithm = CryptoHelper.NewSha256HashAlgorithm())
            {
                if (string.IsNullOrEmpty(samlToken.Assertion.Issuer.Value))
                {
                    stringBuilder.AppendFormat("{0}{1}", samlToken.Assertion.Id.Value, tokenTypeIdentifiers[0]);
                }
                else
                {
                    stringBuilder.AppendFormat("{0}{1}{2}", samlToken.Assertion.Id.Value, samlToken.Assertion.Issuer.Value, tokenTypeIdentifiers[0]);
                }

                key = Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString())));
            }

            if (Configuration.Caches.TokenReplayCache.Contains(key))
            {
                string issuer = (samlToken.Assertion.Issuer.Value != null) ? samlToken.Assertion.Issuer.Value : String.Empty;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenReplayDetectedException(SR.GetString(SR.ID1066, typeof(Saml2SecurityToken).ToString(), samlToken.Assertion.Id.Value, issuer)));
            }
            else
            {
                Configuration.Caches.TokenReplayCache.AddOrUpdate(key, token, DateTimeUtil.Add(this.GetTokenReplayCacheEntryExpirationTime(samlToken), Configuration.MaxClockSkew));
            }
        }

        /// <summary>
        /// Returns the time until which the token should be held in the token replay cache.
        /// </summary>
        /// <param name="token">The token to return an expiration time for.</param>
        /// <exception cref="ArgumentNullException">The input argument 'token' is null.</exception>
        /// <exception cref="SecurityTokenValidationException">The Saml2SecurityToken's validity period is greater than the expiration period set to TokenReplayCache.</exception>
        /// <returns>A DateTime representing the expiration time.</returns>
        /// <remarks>By default, this function returns the NotOnOrAfter of the SAML Condition if present.
        /// If that value does not exist, it returns the NotOnOrAfter of the first SubjectConfirmationData.
        /// This function will never return a value further from now than Configuration.TokenReplayCacheExpirationPeriod.</remarks>
        protected virtual DateTime GetTokenReplayCacheEntryExpirationTime(Saml2SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            DateTime? tokenExpiration = null;
            Saml2Assertion assertion = token.Assertion;
            if (assertion != null)
            {
                if (assertion.Conditions != null && assertion.Conditions.NotOnOrAfter.HasValue)
                {
                    // The Condition has a NotOnOrAfter set, use that.
                    tokenExpiration = assertion.Conditions.NotOnOrAfter.Value;
                }
                else if (assertion.Subject != null && assertion.Subject.SubjectConfirmations != null &&
                          assertion.Subject.SubjectConfirmations.Count != 0 &&
                          assertion.Subject.SubjectConfirmations[0].SubjectConfirmationData != null &&
                          assertion.Subject.SubjectConfirmations[0].SubjectConfirmationData.NotOnOrAfter.HasValue)
                {
                    // The condition did not have NotOnOrAfter set, but SCD[0] has a NotOnOrAfter set, use that.
                    tokenExpiration = assertion.Subject.SubjectConfirmations[0].SubjectConfirmationData.NotOnOrAfter.Value;
                }
            }

            // DateTimeUtil handles overflows
            DateTime maximumExpirationTime = DateTimeUtil.Add(DateTime.UtcNow, Configuration.TokenReplayCacheExpirationPeriod);

            // Use DateTime.MaxValue as expiration value for tokens without expiration
            tokenExpiration = tokenExpiration ?? DateTime.MaxValue;

            // If the refined token validity period is greater than the TokenReplayCacheExpirationPeriod, throw
            if (DateTime.Compare(maximumExpirationTime, tokenExpiration.Value) < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SecurityTokenValidationException(SR.GetString(SR.ID1069, tokenExpiration.Value.ToString(), Configuration.TokenReplayCacheExpirationPeriod.ToString())));
            }

            return tokenExpiration.Value;
        }

        /// <summary>
        /// Returns the normalized value matching a SAML2 AuthenticationContext class reference.
        /// </summary>
        /// <param name="saml2AuthenticationContextClassReference">A string representing the <see cref="Saml2Constants.AuthenticationContextClasses"/></param>
        /// <returns>Normalized value.</returns>
        protected virtual string NormalizeAuthenticationContextClassReference(string saml2AuthenticationContextClassReference)
        {
            return AuthenticationTypeMaps.Normalize(saml2AuthenticationContextClassReference, AuthenticationTypeMaps.Saml2);
        }

        /// <summary>
        /// Creates claims from the Saml2Subject.
        /// </summary>
        /// <param name="assertionSubject">The Saml2Subject.</param>
        /// <param name="subject">The ClaimsIdentity subject.</param>
        /// <param name="issuer">The issuer.</param>
        protected virtual void ProcessSamlSubject(Saml2Subject assertionSubject, ClaimsIdentity subject, string issuer)
        {
            if (assertionSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertionSubject");
            }

            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }

            Saml2NameIdentifier nameId = assertionSubject.NameId;

            if (nameId != null)
            {
                Claim claim = new Claim(ClaimTypes.NameIdentifier, nameId.Value, ClaimValueTypes.String, issuer);

                if (nameId.Format != null)
                {
                    claim.Properties[ClaimProperties.SamlNameIdentifierFormat] = nameId.Format.AbsoluteUri;
                }

                if (nameId.NameQualifier != null)
                {
                    claim.Properties[ClaimProperties.SamlNameIdentifierNameQualifier] = nameId.NameQualifier;
                }

                if (nameId.SPNameQualifier != null)
                {
                    claim.Properties[ClaimProperties.SamlNameIdentifierSPNameQualifier] = nameId.SPNameQualifier;
                }

                if (nameId.SPProvidedId != null)
                {
                    claim.Properties[ClaimProperties.SamlNameIdentifierSPProvidedId] = nameId.SPProvidedId;
                }

                subject.AddClaim(claim);
            }
        }

        /// <summary>
        /// Creates claims from a Saml2AttributeStatement.
        /// </summary>
        /// <param name="statement">The Saml2AttributeStatement.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="issuer">The issuer.</param>
        protected virtual void ProcessAttributeStatement(Saml2AttributeStatement statement, ClaimsIdentity subject, string issuer)
        {
            if (statement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("statement");
            }

            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }

            foreach (Saml2Attribute attribute in statement.Attributes)
            {
                if (StringComparer.Ordinal.Equals(attribute.Name, ClaimTypes.Actor))
                {
                    if (subject.Actor != null)
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4218));
                    }

                    this.SetDelegateFromAttribute(attribute, subject, issuer);
                }
                else
                {
                    foreach (string value in attribute.Values)
                    {
                        if (value != null)
                        {
                            string originalIssuer = issuer;
                            if (attribute.OriginalIssuer != null)
                            {
                                originalIssuer = attribute.OriginalIssuer;
                            }

                            Claim claim = new Claim(attribute.Name, value, attribute.AttributeValueXsiType, issuer, originalIssuer);

                            if (attribute.NameFormat != null)
                            {
                                claim.Properties[ClaimProperties.SamlAttributeNameFormat] = attribute.NameFormat.AbsoluteUri;
                            }

                            if (attribute.FriendlyName != null)
                            {
                                claim.Properties[ClaimProperties.SamlAttributeDisplayName] = attribute.FriendlyName;
                            }

                            subject.AddClaim(claim);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method gets called when a special type of Saml2Attribute is detected. The Saml2Attribute passed in 
        /// wraps a Saml2Attribute that contains a collection of AttributeValues, each of which will get mapped to a 
        /// claim.  All of the claims will be returned in an ClaimsIdentity with the specified issuer.
        /// </summary>
        /// <param name="attribute">The <see cref="Saml2Attribute"/> to use.</param>
        /// <param name="subject">The <see cref="ClaimsIdentity"/> that is the subject of this token.</param>
        /// <param name="issuer">The issuer of the claim.</param>
        /// <exception cref="InvalidOperationException">Will be thrown if the Saml2Attribute does not contain any 
        /// valid Saml2AttributeValues.
        /// </exception>
        protected virtual void SetDelegateFromAttribute(Saml2Attribute attribute, ClaimsIdentity subject, string issuer)
        {
            // bail here; nothing to add.
            if (subject == null || attribute == null || attribute.Values == null || attribute.Values.Count < 1)
            {
                return;
            }

            Saml2Attribute actingAsAttribute = null;
            Collection<Claim> claims = new Collection<Claim>();

            foreach (string attributeValue in attribute.Values)
            {
                if (attributeValue != null)
                {
                    using (XmlDictionaryReader dicReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(attributeValue), XmlDictionaryReaderQuotas.Max))
                    {
                        dicReader.MoveToContent();
                        dicReader.ReadStartElement(Actor);

                        while (dicReader.IsStartElement(Attribute))
                        {
                            Saml2Attribute innerAttribute = this.ReadAttribute(dicReader);
                            if (innerAttribute != null)
                            {
                                if (innerAttribute.Name == ClaimTypes.Actor)
                                {
                                    // In this case, we have two delegates acting as an identity: we do not allow this.
                                    if (actingAsAttribute != null)
                                    {
                                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4218));
                                    }

                                    actingAsAttribute = innerAttribute;
                                }
                                else
                                {
                                    string originalIssuer = innerAttribute.OriginalIssuer;
                                    for (int k = 0; k < innerAttribute.Values.Count; ++k)
                                    {
                                        Claim claim = null;
                                        if (string.IsNullOrEmpty(originalIssuer))
                                        {
                                            claim = new Claim(innerAttribute.Name, innerAttribute.Values[k], innerAttribute.AttributeValueXsiType, issuer);
                                        }
                                        else
                                        {
                                            claim = new Claim(innerAttribute.Name, innerAttribute.Values[k], innerAttribute.AttributeValueXsiType, issuer, originalIssuer);
                                        }

                                        if (innerAttribute.NameFormat != null)
                                        {
                                            claim.Properties[ClaimProperties.SamlAttributeNameFormat] = innerAttribute.NameFormat.AbsoluteUri;
                                        }

                                        if (innerAttribute.FriendlyName != null)
                                        {
                                            claim.Properties[ClaimProperties.SamlAttributeDisplayName] = innerAttribute.FriendlyName;
                                        }

                                        claims.Add(claim);
                                    }
                                }
                            }
                        }

                        dicReader.ReadEndElement(); // Actor
                    }
                }
            }

            subject.Actor = new ClaimsIdentity(claims, AuthenticationTypes.Federation);

            this.SetDelegateFromAttribute(actingAsAttribute, subject.Actor, issuer);
        }

        /// <summary>
        /// Creates claims from a Saml2AuthenticationStatement.
        /// </summary>
        /// <param name="statement">The Saml2AuthenticationStatement.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="issuer">The issuer.</param>
        protected virtual void ProcessAuthenticationStatement(Saml2AuthenticationStatement statement, ClaimsIdentity subject, string issuer)
        {
            if (subject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subject");
            }

            if (statement.AuthenticationContext.DeclarationReference != null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4180));
            }

            if (statement.AuthenticationContext.ClassReference != null)
            {
                subject.AddClaim(
                        new Claim(
                            ClaimTypes.AuthenticationMethod,
                            this.NormalizeAuthenticationContextClassReference(statement.AuthenticationContext.ClassReference.AbsoluteUri),
                            ClaimValueTypes.String,
                            issuer));
            }

            subject.AddClaim(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(statement.AuthenticationInstant.ToUniversalTime(), DateTimeFormats.Generated), ClaimValueTypes.DateTime, issuer));
        }

        /// <summary>
        /// Creates claims from a Saml2AuthorizationDecisionStatement.
        /// </summary>
        /// <param name="statement">The Saml2AuthorizationDecisionStatement.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="issuer">The issuer.</param>
        protected virtual void ProcessAuthorizationDecisionStatement(Saml2AuthorizationDecisionStatement statement, ClaimsIdentity subject, string issuer)
        {
        }

        /// <summary>
        /// Processes all statements to generate claims.
        /// </summary>
        /// <param name="statements">A collection of Saml2Statement.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="issuer">The issuer.</param>
        protected virtual void ProcessStatement(Collection<Saml2Statement> statements, ClaimsIdentity subject, string issuer)
        {
            Collection<Saml2AuthenticationStatement> authnStatements = new Collection<Saml2AuthenticationStatement>();

            foreach (Saml2Statement statement in statements)
            {
                Saml2AttributeStatement attrStatement = statement as Saml2AttributeStatement;
                if (attrStatement != null)
                {
                    this.ProcessAttributeStatement(attrStatement, subject, issuer);
                    continue;
                }

                Saml2AuthenticationStatement authnStatement = statement as Saml2AuthenticationStatement;
                if (authnStatement != null)
                {
                    authnStatements.Add(authnStatement);
                    continue;
                }

                Saml2AuthorizationDecisionStatement authzStatement = statement as Saml2AuthorizationDecisionStatement;
                if (authzStatement != null)
                {
                    this.ProcessAuthorizationDecisionStatement(authzStatement, subject, issuer);
                    continue;
                }

                // We don't process custom statements. Just fall through.
            }

            foreach (Saml2AuthenticationStatement authStatement in authnStatements)
            {
                if (authStatement != null)
                {
                    this.ProcessAuthenticationStatement(authStatement, subject, issuer);
                }
            }
        }

        /// <summary>
        /// Creates claims from a Saml2 token.
        /// </summary>
        /// <param name="samlToken">The Saml2SecurityToken.</param>
        /// <returns>An IClaimIdentity.</returns>
        protected virtual ClaimsIdentity CreateClaims(Saml2SecurityToken samlToken)
        {
            if (samlToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlToken");
            }

            ClaimsIdentity subject = new ClaimsIdentity(AuthenticationTypes.Federation, SamlSecurityTokenRequirement.NameClaimType, SamlSecurityTokenRequirement.RoleClaimType);

            Saml2Assertion assertion = samlToken.Assertion;

            if (assertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("samlToken", SR.GetString(SR.ID1034));
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (this.Configuration.IssuerNameRegistry == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4277));
            }

            string issuer = this.Configuration.IssuerNameRegistry.GetIssuerName(samlToken.IssuerToken, assertion.Issuer.Value);

            if (string.IsNullOrEmpty(issuer))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4175)));
            }

            this.ProcessSamlSubject(assertion.Subject, subject, issuer);
            this.ProcessStatement(assertion.Statements, subject, issuer);

            return subject;
        }

        /// <summary>
        /// Validates the Saml2SubjectConfirmation data.
        /// </summary>
        /// <param name="confirmationData">The Saml2 subject confirmation data.</param>
        protected virtual void ValidateConfirmationData(Saml2SubjectConfirmationData confirmationData)
        {
            if (null == confirmationData)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("confirmationData");
            }

            if (null != confirmationData.Address)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4153)));
            }

            if (null != confirmationData.InResponseTo)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4154)));
            }

            if (null != confirmationData.Recipient)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4157)));
            }

            DateTime now = DateTime.UtcNow;

            if (null != confirmationData.NotBefore
                    && DateTimeUtil.Add(now, Configuration.MaxClockSkew) < confirmationData.NotBefore.Value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4176, confirmationData.NotBefore.Value, now)));
            }

            if (null != confirmationData.NotOnOrAfter
                && DateTimeUtil.Add(now, Configuration.MaxClockSkew.Negate()) >= confirmationData.NotOnOrAfter.Value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4177, confirmationData.NotOnOrAfter.Value, now)));
            }
        }

        /// <summary>
        /// Resolves the collection of <see cref="SecurityKey"/> referenced in a <see cref="Saml2Assertion"/>.
        /// </summary>
        /// <param name="assertion"><see cref="Saml2Assertion"/> to process.</param>
        /// <param name="resolver"><see cref="SecurityTokenResolver"/> to use in resolving the <see cref="SecurityKey"/>.</param>
        /// <returns>A read only collection of <see cref="SecurityKey"/> contained in the assertion.</returns>
        protected virtual ReadOnlyCollection<SecurityKey> ResolveSecurityKeys(Saml2Assertion assertion, SecurityTokenResolver resolver)
        {
            if (null == assertion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            // Must have Subject
            Saml2Subject subject = assertion.Subject;
            if (null == subject)
            {
                // No Subject
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4130)));
            }

            // Must have one SubjectConfirmation
            if (0 == subject.SubjectConfirmations.Count)
            {
                // No SubjectConfirmation
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4131)));
            }

            if (subject.SubjectConfirmations.Count > 1)
            {
                // More than one SubjectConfirmation
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4132)));
            }

            // Extract the keys for the given method
            ReadOnlyCollection<SecurityKey> securityKeys;

            Saml2SubjectConfirmation subjectConfirmation = subject.SubjectConfirmations[0];

            // For bearer, ensure there are no keys, set the collection to empty
            // For HolderOfKey, ensure there is at least one key, resolve and create collection
            if (Saml2Constants.ConfirmationMethods.Bearer == subjectConfirmation.Method)
            {
                if (null != subjectConfirmation.SubjectConfirmationData
                    && 0 != subjectConfirmation.SubjectConfirmationData.KeyIdentifiers.Count)
                {
                    // Bearer but has keys
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4133)));
                }

                securityKeys = EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
            else if (Saml2Constants.ConfirmationMethods.HolderOfKey == subjectConfirmation.Method)
            {
                if (null == subjectConfirmation.SubjectConfirmationData
                    || 0 == subjectConfirmation.SubjectConfirmationData.KeyIdentifiers.Count)
                {
                    // Holder-of-key but no keys
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4134)));
                }

                List<SecurityKey> holderKeys = new List<SecurityKey>();
                SecurityKey key;

                foreach (SecurityKeyIdentifier keyIdentifier in subjectConfirmation.SubjectConfirmationData.KeyIdentifiers)
                {
                    key = null;

                    // Try the resolver first
                    foreach (SecurityKeyIdentifierClause clause in keyIdentifier)
                    {
                        if (null != resolver
                            && resolver.TryResolveSecurityKey(clause, out key))
                        {
                            holderKeys.Add(key);
                            break;
                        }
                    }

                    // If that doesn't work, try to create the key (e.g. bare RSA or X509 raw)
                    if (null == key)
                    {
                        if (keyIdentifier.CanCreateKey)
                        {
                            key = keyIdentifier.CreateKey();
                            holderKeys.Add(key);
                        }
                        else
                        {
                            holderKeys.Add(new SecurityKeyElement(keyIdentifier, resolver));
                        }
                    }
                }

                securityKeys = holderKeys.AsReadOnly();
            }
            else
            {
                // SenderVouches, as well as other random things, aren't accepted
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4136, subjectConfirmation.Method)));
            }

            return securityKeys;
        }

        /// <summary>
        /// Resolves the Signing Key Identifier to a SecurityToken.
        /// </summary>
        /// <param name="assertion">The Assertion for which the Issuer token is to be resolved.</param>
        /// <param name="issuerResolver">The current SecurityTokenResolver associated with this handler.</param>
        /// <returns>Instance of SecurityToken</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'assertion' is null.</exception>
        /// <exception cref="SecurityTokenException">Unable to resolve token.</exception>
        protected virtual SecurityToken ResolveIssuerToken(Saml2Assertion assertion, SecurityTokenResolver issuerResolver)
        {
            if (null == assertion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            SecurityToken token;
            if (this.TryResolveIssuerToken(assertion, issuerResolver, out token))
            {
                return token;
            }
            else
            {
                string exceptionMessage = SR.GetString(assertion.SigningCredentials == null ? SR.ID4141 : SR.ID4142);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(exceptionMessage));
            }
        }

        /// <summary>
        /// Resolves the Signing Key Identifier to a SecurityToken.
        /// </summary>
        /// <param name="assertion">The Assertion for which the Issuer token is to be resolved.</param>
        /// <param name="issuerResolver">The current SecurityTokenResolver associated with this handler.</param>
        /// <param name="token">Resolved token.</param>
        /// <returns>True if token is resolved.</returns>
        protected virtual bool TryResolveIssuerToken(Saml2Assertion assertion, SecurityTokenResolver issuerResolver, out SecurityToken token)
        {
            if (null == assertion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            if (assertion.SigningCredentials != null
                && assertion.SigningCredentials.SigningKeyIdentifier != null
                && issuerResolver != null)
            {
                SecurityKeyIdentifier keyIdentifier = assertion.SigningCredentials.SigningKeyIdentifier;
                return issuerResolver.TryResolveToken(keyIdentifier, out token);
            }
            else
            {
                token = null;
                return false;
            }
        }

        /// <summary>
        /// This handles the construct used in &lt;Subject> and &lt;SubjectConfirmation> for ID:
        /// <choice>
        ///     <element ref="saml:BaseID" />
        ///     <element ref="saml:NameID" />
        ///     <element ref="saml:EncryptedID" />
        /// </choice>
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> reader positioned at a <see cref="Saml2NameIdentifier"/> element.</param>
        /// <param name="parentElement">The parent element this SubjectID is part of.</param>
        /// <returns>A <see cref="Saml2NameIdentifier"/> constructed from the XML.</returns>
        protected virtual Saml2NameIdentifier ReadSubjectId(XmlReader reader, string parentElement)
        {
            // <NameID>, <EncryptedID>, <BaseID>
            if (reader.IsStartElement(Saml2Constants.Elements.NameID, Saml2Constants.Namespace))
            {
                return this.ReadNameId(reader);
            }
            else if (reader.IsStartElement(Saml2Constants.Elements.EncryptedID, Saml2Constants.Namespace))
            {
                return this.ReadEncryptedId(reader);
            }
            else if (reader.IsStartElement(Saml2Constants.Elements.BaseID, Saml2Constants.Namespace))
            {
                // Since BaseID is an abstract type, we have to switch off the xsi:type declaration
                XmlQualifiedName declaredType = XmlUtil.GetXsiType(reader);

                // No declaration, or declaring that this is just a "BaseID", is invalid since 
                // statement is abstract
                if (null == declaredType
                    || XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.BaseIDAbstractType, Saml2Constants.Namespace))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4104, reader.LocalName, reader.NamespaceURI));
                }

                // If it's NameID we can handle it
                if (XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.NameIDType, Saml2Constants.Namespace))
                {
                    return this.ReadNameIdType(reader);
                }
                else
                {
                    // Instruct the user to override to handle custom <BaseID>
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4110, parentElement, declaredType.Name, declaredType.Namespace));
                }
            }

            return null;
        }

        /// <summary>
        /// Reads the &lt;saml:Action> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Action"/> element.</param>
        /// <returns>A <see cref="Saml2Action"/> instance.</returns>
        protected virtual Saml2Action ReadAction(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Action, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Action, Saml2Constants.Namespace);
            }

            // disallow empty
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, Saml2Constants.Elements.Action, Saml2Constants.Namespace));
            }

            try
            {
                // Need the content to instantiate, so use locals
                Uri actionNamespace;

                // @attributes
                string attributeValue;

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.ActionType, Saml2Constants.Namespace);

                // @Namespace - required
                attributeValue = reader.GetAttribute(Saml2Constants.Attributes.Namespace);
                if (string.IsNullOrEmpty(attributeValue))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.Namespace, Saml2Constants.Elements.Action));
                }

                if (!UriUtil.CanCreateValidUri(attributeValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0011, Saml2Constants.Attributes.Namespace, Saml2Constants.Elements.Action));
                }

                actionNamespace = new Uri(attributeValue);

                // Content is string
                return new Saml2Action(reader.ReadElementString(), actionNamespace);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:Action> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Action"/>.</param>
        /// <param name="data">The <see cref="Saml2Action"/> to serialize.</param>
        protected virtual void WriteAction(XmlWriter writer, Saml2Action data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            if (null == data.Namespace)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data.Namespace");
            }

            if (string.IsNullOrEmpty(data.Namespace.ToString()))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("data.Namespace");
            }

            // <Action>
            writer.WriteStartElement(Saml2Constants.Elements.Action, Saml2Constants.Namespace);

            // @Namespace - required
            writer.WriteAttributeString(Saml2Constants.Attributes.Namespace, data.Namespace.AbsoluteUri);

            // String content
            writer.WriteString(data.Value);

            // </Action>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:Advice> element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Advice element has an extensibility point to allow XML elements
        /// from non-SAML2 namespaces to be included. By default, because the 
        /// Advice may be ignored without affecting the semantics of the 
        /// assertion, any such elements are ignored. To handle the processing
        /// of those elements, override this method.
        /// </para>
        /// </remarks>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Advice"/> element.</param>
        /// <returns>A <see cref="Saml2Advice"/> instance.</returns>
        protected virtual Saml2Advice ReadAdvice(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Advice, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Advice, Saml2Constants.Namespace);
            }

            try
            {
                Saml2Advice advice = new Saml2Advice();
                bool isEmpty = reader.IsEmptyElement;

                // @attributes

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AdviceType, Saml2Constants.Namespace);

                reader.Read();
                if (!isEmpty)
                {
                    // <AssertionIDRef|AssertionURIRef|Assertion|EncryptedAssertion|other:any> 0-OO
                    while (reader.IsStartElement())
                    {
                        // <AssertionIDRef>, <AssertionURIRef>, <Assertion>, <EncryptedAssertion>
                        if (reader.IsStartElement(Saml2Constants.Elements.AssertionIDRef, Saml2Constants.Namespace))
                        {
                            advice.AssertionIdReferences.Add(ReadSimpleNCNameElement(reader));
                        }
                        else if (reader.IsStartElement(Saml2Constants.Elements.AssertionURIRef, Saml2Constants.Namespace))
                        {
                            advice.AssertionUriReferences.Add(ReadSimpleUriElement(reader));
                        }
                        else if (reader.IsStartElement(Saml2Constants.Elements.Assertion, Saml2Constants.Namespace))
                        {
                            advice.Assertions.Add(this.ReadAssertion(reader));
                        }
                        else if (reader.IsStartElement(Saml2Constants.Elements.EncryptedAssertion, Saml2Constants.Namespace))
                        {
                            advice.Assertions.Add(this.ReadAssertion(reader));
                        }
                        else
                        {
                            TraceUtility.TraceString(TraceEventType.Warning, SR.GetString(SR.ID8006, reader.LocalName, reader.NamespaceURI));
                            reader.Skip();
                        }
                    }

                    reader.ReadEndElement();
                }

                return advice;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:Advice> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Advice"/>.</param>
        /// <param name="data">The <see cref="Saml2Advice"/> to serialize.</param>
        protected virtual void WriteAdvice(XmlWriter writer, Saml2Advice data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // <Advice>
            writer.WriteStartElement(Saml2Constants.Elements.Advice, Saml2Constants.Namespace);

            // <AssertionIDRef> 0-OO
            foreach (Saml2Id id in data.AssertionIdReferences)
            {
                writer.WriteElementString(Saml2Constants.Elements.AssertionIDRef, Saml2Constants.Namespace, id.Value);
            }

            // <AssertionURIRef> 0-OO
            foreach (Uri uri in data.AssertionUriReferences)
            {
                writer.WriteElementString(Saml2Constants.Elements.AssertionURIRef, Saml2Constants.Namespace, uri.AbsoluteUri);
            }

            // <Assertion> 0-OO
            foreach (Saml2Assertion assertion in data.Assertions)
            {
                this.WriteAssertion(writer, assertion);
            }

            // </Advice>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:Assertion> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Assertion"/> element.</param>
        /// <returns>A <see cref="Saml2Assertion"/> instance.</returns>
        protected virtual Saml2Assertion ReadAssertion(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (this.Configuration.IssuerTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4275));
            }

            if (this.Configuration.ServiceTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4276));
            }

            XmlDictionaryReader plaintextReader = XmlDictionaryReader.CreateDictionaryReader(reader);

            Saml2Assertion assertion = new Saml2Assertion(new Saml2NameIdentifier("__TemporaryIssuer__"));

            // If it's an EncryptedAssertion, we need to retrieve the plaintext 
            // and repoint our reader
            if (reader.IsStartElement(Saml2Constants.Elements.EncryptedAssertion, Saml2Constants.Namespace))
            {
                EncryptingCredentials encryptingCredentials = null;
                plaintextReader = CreatePlaintextReaderFromEncryptedData(
                                    plaintextReader,
                                    Configuration.ServiceTokenResolver,
                                    this.KeyInfoSerializer,
                                    assertion.ExternalEncryptedKeys,
                                    out encryptingCredentials);

                assertion.EncryptingCredentials = encryptingCredentials;
            }

            // Throw if wrong element
            if (!plaintextReader.IsStartElement(Saml2Constants.Elements.Assertion, Saml2Constants.Namespace))
            {
                plaintextReader.ReadStartElement(Saml2Constants.Elements.Assertion, Saml2Constants.Namespace);
            }

            // disallow empty
            if (plaintextReader.IsEmptyElement)
            {
#pragma warning suppress 56504 // bogus - thinks plaintextReader.LocalName, plaintextReader.NamespaceURI need validation
                throw DiagnosticUtility.ThrowHelperXml(plaintextReader, SR.GetString(SR.ID3061, plaintextReader.LocalName, plaintextReader.NamespaceURI));
            }

            // Construct a wrapped serializer so that the EnvelopedSignatureReader's 
            // attempt to read the <ds:KeyInfo> will hit our ReadKeyInfo virtual.
            WrappedSerializer wrappedSerializer = new WrappedSerializer(this, assertion);

            // SAML supports enveloped signature, so we need to wrap our reader.
            // We do not dispose this reader, since as a delegating reader it would
            // dispose the inner reader, which we don't properly own.
            EnvelopedSignatureReader realReader = new EnvelopedSignatureReader(plaintextReader, wrappedSerializer, this.Configuration.IssuerTokenResolver, false, false, false);
            try
            {
                // Process @attributes
                string value;

                // @xsi:type
                XmlUtil.ValidateXsiType(realReader, Saml2Constants.Types.AssertionType, Saml2Constants.Namespace);

                // @Version - required - must be "2.0"
                string version = realReader.GetAttribute(Saml2Constants.Attributes.Version);
                if (string.IsNullOrEmpty(version))
                {
                    throw DiagnosticUtility.ThrowHelperXml(realReader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.Version, Saml2Constants.Elements.Assertion));
                }

                if (!StringComparer.Ordinal.Equals(assertion.Version, version))
                {
                    throw DiagnosticUtility.ThrowHelperXml(realReader, SR.GetString(SR.ID4100, version));
                }

                // @ID - required
                value = realReader.GetAttribute(Saml2Constants.Attributes.ID);
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(realReader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.ID, Saml2Constants.Elements.Assertion));
                }

                assertion.Id = new Saml2Id(value);

                // @IssueInstant - required
                value = realReader.GetAttribute(Saml2Constants.Attributes.IssueInstant);
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(realReader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.IssueInstant, Saml2Constants.Elements.Assertion));
                }

                assertion.IssueInstant = XmlConvert.ToDateTime(value, DateTimeFormats.Accepted);

                // Process <elements>
                realReader.Read();

                // <Issuer> 1
                assertion.Issuer = this.ReadIssuer(realReader);

                // <ds:Signature> 0-1
                realReader.TryReadSignature();

                // <Subject> 0-1
                if (realReader.IsStartElement(Saml2Constants.Elements.Subject, Saml2Constants.Namespace))
                {
                    assertion.Subject = this.ReadSubject(realReader);
                }

                // <Conditions> 0-1
                if (realReader.IsStartElement(Saml2Constants.Elements.Conditions, Saml2Constants.Namespace))
                {
                    assertion.Conditions = this.ReadConditions(realReader);
                }

                // <Advice> 0-1
                if (realReader.IsStartElement(Saml2Constants.Elements.Advice, Saml2Constants.Namespace))
                {
                    assertion.Advice = this.ReadAdvice(realReader);
                }

                // <Statement|AuthnStatement|AuthzDecisionStatement|AttributeStatement>, 0-OO
                while (realReader.IsStartElement())
                {
                    Saml2Statement statement;

                    if (realReader.IsStartElement(Saml2Constants.Elements.Statement, Saml2Constants.Namespace))
                    {
                        statement = this.ReadStatement(realReader);
                    }
                    else if (realReader.IsStartElement(Saml2Constants.Elements.AttributeStatement, Saml2Constants.Namespace))
                    {
                        statement = this.ReadAttributeStatement(realReader);
                    }
                    else if (realReader.IsStartElement(Saml2Constants.Elements.AuthnStatement, Saml2Constants.Namespace))
                    {
                        statement = this.ReadAuthenticationStatement(realReader);
                    }
                    else if (realReader.IsStartElement(Saml2Constants.Elements.AuthzDecisionStatement, Saml2Constants.Namespace))
                    {
                        statement = this.ReadAuthorizationDecisionStatement(realReader);
                    }
                    else
                    {
                        break;
                    }

                    assertion.Statements.Add(statement);
                }

                realReader.ReadEndElement();

                if (null == assertion.Subject)
                {
                    // An assertion with no statements MUST contain a <Subject> element. [Saml2Core, line 585]
                    if (0 == assertion.Statements.Count)
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4106));
                    }

                    // Furthermore, the built-in statement types all require the presence of a subject.
                    // [Saml2Core, lines 1050, 1168, 1280]
                    foreach (Saml2Statement statement in assertion.Statements)
                    {
                        if (statement is Saml2AuthenticationStatement
                            || statement is Saml2AttributeStatement
                            || statement is Saml2AuthorizationDecisionStatement)
                        {
                            throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4119));
                        }
                    }
                }

                // Reading the end element will complete the signature; 
                // capture the signing creds
                assertion.SigningCredentials = realReader.SigningCredentials;

                // Save the captured on-the-wire data, which can then be used
                // to re-emit this assertion, preserving the same signature.
                assertion.CaptureSourceData(realReader);

                return assertion;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(realReader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Serializes the provided SamlAssertion to the XmlWriter.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Assertion"/>.</param>
        /// <param name="data">The <see cref="Saml2Assertion"/> to serialize.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> or <paramref name="data"/> parameters are null.</exception>
        /// <exception cref="InvalidOperationException"> The <paramref name="data"/>  has both <see cref="EncryptingCredentials"/> and <see cref="ReceivedEncryptingCredentials"/> properties null.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="data"/> must have a <see cref="Saml2Subject"/> if no <see cref="Saml2Statement"/> are present.</exception>
        /// <exception cref="InvalidOperationException">The SAML2 authentication, attribute, and authorization decision <see cref="Saml2Statement"/> require a <see cref="Saml2Subject"/>.</exception>
        /// <exception cref="CryptographicException">Token encrypting credentials must have a Symmetric Key specified.</exception>
        protected virtual void WriteAssertion(XmlWriter writer, Saml2Assertion data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            XmlWriter originalWriter = writer;
            MemoryStream plaintextStream = null;
            XmlDictionaryWriter plaintextWriter = null;

            // If an EncryptingCredentials is present then check if this is not of type ReceivedEncryptinCredentials.
            // ReceivedEncryptingCredentials mean that it was credentials that were hydrated from a token received
            // on the wire. We should not directly use this while re-serializing a token.
            if ((null != data.EncryptingCredentials) && !(data.EncryptingCredentials is ReceivedEncryptingCredentials))
            {
                plaintextStream = new MemoryStream();
                writer = plaintextWriter = XmlDictionaryWriter.CreateTextWriter(plaintextStream, Encoding.UTF8, false);
            }
            else if (data.ExternalEncryptedKeys == null || data.ExternalEncryptedKeys.Count > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4173)));
            }

            // If we've saved off the token stream, re-emit it.
            if (data.CanWriteSourceData)
            {
                data.WriteSourceData(writer);
            }
            else
            {
                // Wrap the writer if necessary for a signature
                // We do not dispose this writer, since as a delegating writer it would
                // dispose the inner writer, which we don't properly own.
                EnvelopedSignatureWriter signatureWriter = null;
                if (null != data.SigningCredentials)
                {
#pragma warning suppress 56506
                    writer = signatureWriter = new EnvelopedSignatureWriter(writer, data.SigningCredentials, data.Id.Value, new WrappedSerializer(this, data));
                }

                if (null == data.Subject)
                {
                    // An assertion with no statements MUST contain a <Subject> element. [Saml2Core, line 585]
                    if (data.Statements == null || 0 == data.Statements.Count)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4106)));
                    }

                    // Furthermore, the built-in statement types all require the presence of a subject.
                    // [Saml2Core, lines 1050, 1168, 1280]
                    foreach (Saml2Statement statement in data.Statements)
                    {
                        if (statement is Saml2AuthenticationStatement
                            || statement is Saml2AttributeStatement
                            || statement is Saml2AuthorizationDecisionStatement)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new InvalidOperationException(SR.GetString(SR.ID4119)));
                        }
                    }
                }

                // <Assertion>
                writer.WriteStartElement(Saml2Constants.Elements.Assertion, Saml2Constants.Namespace);

                // @ID - required
                writer.WriteAttributeString(Saml2Constants.Attributes.ID, data.Id.Value);

                // @IssueInstant - required
                writer.WriteAttributeString(Saml2Constants.Attributes.IssueInstant, XmlConvert.ToString(data.IssueInstant.ToUniversalTime(), DateTimeFormats.Generated));

                // @Version - required
                writer.WriteAttributeString(Saml2Constants.Attributes.Version, data.Version);

                // <Issuer> 1
                this.WriteIssuer(writer, data.Issuer);

                // <ds:Signature> 0-1
                if (null != signatureWriter)
                {
                    signatureWriter.WriteSignature();
                }

                // <Subject> 0-1
                if (null != data.Subject)
                {
                    this.WriteSubject(writer, data.Subject);
                }

                // <Conditions> 0-1
                if (null != data.Conditions)
                {
                    this.WriteConditions(writer, data.Conditions);
                }

                // <Advice> 0-1
                if (null != data.Advice)
                {
                    this.WriteAdvice(writer, data.Advice);
                }

                // <Statement|AuthnStatement|AuthzDecisionStatement|AttributeStatement>, 0-OO
                foreach (Saml2Statement statement in data.Statements)
                {
                    this.WriteStatement(writer, statement);
                }

                writer.WriteEndElement();
            }

            // Finish off the encryption
            if (null != plaintextWriter)
            {
                ((IDisposable)plaintextWriter).Dispose();
                plaintextWriter = null;

                EncryptedDataElement encryptedData = new EncryptedDataElement();
                encryptedData.Type = XmlEncryptionConstants.EncryptedDataTypes.Element;
                encryptedData.Algorithm = data.EncryptingCredentials.Algorithm;
                encryptedData.KeyIdentifier = data.EncryptingCredentials.SecurityKeyIdentifier;

                // Get the encryption key, which must be symmetric
                SymmetricSecurityKey encryptingKey = data.EncryptingCredentials.SecurityKey as SymmetricSecurityKey;
                if (encryptingKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.ID3064)));
                }

                // Do the actual encryption
                SymmetricAlgorithm symmetricAlgorithm = encryptingKey.GetSymmetricAlgorithm(data.EncryptingCredentials.Algorithm);
                encryptedData.Encrypt(symmetricAlgorithm, plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length);
                ((IDisposable)plaintextStream).Dispose();

                originalWriter.WriteStartElement(Saml2Constants.Elements.EncryptedAssertion, Saml2Constants.Namespace);
                encryptedData.WriteXml(originalWriter, this.KeyInfoSerializer);
                foreach (EncryptedKeyIdentifierClause clause in data.ExternalEncryptedKeys)
                {
                    this.KeyInfoSerializer.WriteKeyIdentifierClause(originalWriter, clause);
                }

                originalWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Reads the &lt;saml:Attribute> element.
        /// </summary>
        /// <remarks>
        /// The default implementation requires that the content of the 
        /// Attribute element be a simple string. To handle complex content
        /// or content of declared simple types other than xs:string, override
        /// this method.
        /// </remarks>
        /// <param name="reader">An <see cref="XmlReader"/> positioned at a <see cref="Saml2Attribute"/> element.</param>
        /// <returns>A <see cref="Saml2Attribute"/> instance.</returns>
        protected virtual Saml2Attribute ReadAttribute(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace);
            }

            try
            {
                Saml2Attribute attribute;
                bool isEmpty = reader.IsEmptyElement;

                // @attributes
                string value;

                // @xsi:type 
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AttributeType, Saml2Constants.Namespace);

                // @Name - required
                value = reader.GetAttribute(Saml2Constants.Attributes.Name);
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.Name, Saml2Constants.Elements.Attribute));
                }

                attribute = new Saml2Attribute(value);

                // @NameFormat - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.NameFormat);
                if (!string.IsNullOrEmpty(value))
                {
                    if (!UriUtil.CanCreateValidUri(value, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0011, Saml2Constants.Attributes.Namespace, Saml2Constants.Elements.Action));
                    }

                    attribute.NameFormat = new Uri(value);
                }

                // @FriendlyName - optional
                attribute.FriendlyName = reader.GetAttribute(Saml2Constants.Attributes.FriendlyName);

                // @OriginalIssuer - optional.
                // We are lax on read here, and will accept the following namespaces for original issuer, in order:
                // http://schemas.xmlsoap.org/ws/2009/09/identity/claims
                // http://schemas.microsoft.com/ws/2008/06/identity
                string originalIssuer = reader.GetAttribute(Saml2Constants.Attributes.OriginalIssuer, ClaimType2009Namespace);

                if (originalIssuer == null)
                {
                    originalIssuer = reader.GetAttribute(Saml2Constants.Attributes.OriginalIssuer, ProductConstants.NamespaceUri);
                }

                if (originalIssuer == String.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4252)));
                }

                attribute.OriginalIssuer = originalIssuer;

                // content
                reader.Read();
                if (!isEmpty)
                {
                    while (reader.IsStartElement(Saml2Constants.Elements.AttributeValue, Saml2Constants.Namespace))
                    {
                        bool isEmptyValue = reader.IsEmptyElement;
                        bool isNil = XmlUtil.IsNil(reader);

                        // FIP 9570 - ENTERPRISE SCENARIO: Saml11SecurityTokenHandler.ReadAttribute is not checking the AttributeValue XSI type correctly.
                        // Lax on receive. If we dont find the AttributeValueXsiType in the format we are looking for in the xml, we default to string.
                        // Read the xsi:type. We are expecting a value of the form "some-non-empty-string" or "some-non-empty-local-prefix:some-non-empty-string".
                        // ":some-non-empty-string" and "some-non-empty-string:" are edge-cases where defaulting to string is reasonable.
                        // For attributeValueXsiTypeSuffix, we want the portion after the local prefix in "some-non-empty-local-prefix:some-non-empty-string"
                        // "some-non-empty-local-prefix:some-non-empty-string" case
                        string attributeValueXsiTypePrefix = null;
                        string attributeValueXsiTypeSuffix = null;
                        string attributeValueXsiTypeSuffixWithLocalPrefix = reader.GetAttribute("type", XmlSchema.InstanceNamespace);
                        if (!string.IsNullOrEmpty(attributeValueXsiTypeSuffixWithLocalPrefix))
                        {
                            // "some-non-empty-string" case
                            if (attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) == -1)
                            {
                                attributeValueXsiTypePrefix = reader.LookupNamespace(String.Empty);
                                attributeValueXsiTypeSuffix = attributeValueXsiTypeSuffixWithLocalPrefix;
                            }
                            else if (attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) > 0 &&
                                      attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) < attributeValueXsiTypeSuffixWithLocalPrefix.Length - 1)
                            {
                                string localPrefix = attributeValueXsiTypeSuffixWithLocalPrefix.Substring(0, attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal));
                                attributeValueXsiTypePrefix = reader.LookupNamespace(localPrefix);
                                attributeValueXsiTypeSuffix = attributeValueXsiTypeSuffixWithLocalPrefix.Substring(attributeValueXsiTypeSuffixWithLocalPrefix.IndexOf(":", StringComparison.Ordinal) + 1);
                            }
                        }

                        if (attributeValueXsiTypePrefix != null && attributeValueXsiTypeSuffix != null)
                        {
                            attribute.AttributeValueXsiType = String.Concat(attributeValueXsiTypePrefix, "#", attributeValueXsiTypeSuffix);
                        }

                        if (isNil)
                        {
                            reader.Read();
                            if (!isEmptyValue)
                            {
                                reader.ReadEndElement();
                            }

                            attribute.Values.Add(null);
                        }
                        else if (isEmptyValue)
                        {
                            reader.Read();
                            attribute.Values.Add(string.Empty);
                        }
                        else
                        {
                            attribute.Values.Add(this.ReadAttributeValue(reader, attribute));
                        }
                    }

                    reader.ReadEndElement();
                }

                return attribute;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Reads an attribute value.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Attribute"/>.</param>
        /// <param name="attribute">The <see cref="Saml2Attribute"/>.</param>
        /// <returns>The attribute value as a string.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected virtual string ReadAttributeValue(XmlReader reader, Saml2Attribute attribute)
        {
            // This code was designed realizing that the writter of the xml controls how our
            // reader will report the NodeType. A completely differnet system (IBM, etc) could write the values. 
            // Considering NodeType is important, because we need to read the entire value, end element and not loose anything significant.
            // 
            // Couple of cases to help understand the design choices.
            //
            // 1. 
            // "<MyElement xmlns=""urn:mynamespace""><another>complex</another></MyElement><sibling>value</sibling>"
            // Could result in the our reader reporting the NodeType as Text OR Element, depending if '<' was entitized to '&lt;'
            //
            // 2. 
            // " <MyElement xmlns=""urn:mynamespace""><another>complex</another></MyElement><sibling>value</sibling>"
            // Could result in the our reader reporting the NodeType as Text OR Whitespace.  Post Whitespace processing, the NodeType could be 
            // reported as Text or Element, depending if '<' was entitized to '&lt;'
            //
            // 3. 
            // "/r/n/t   "
            // Could result in the our reader reporting the NodeType as whitespace.
            //
            // Since an AttributeValue with ONLY Whitespace and a complex Element proceeded by whitespace are reported as the same NodeType (2. and 3.)
            // the whitespace is remembered and discarded if an found is found, otherwise it becomes the value. This is to help users who accidently put a space when adding claims in ADFS
            // If we just skipped the Whitespace, then an AttributeValue that started with Whitespace would loose that part and claims generated from the AttributeValue
            // would be missing that part.
            // 

            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            string result = String.Empty;
            string whiteSpace = String.Empty;

            reader.ReadStartElement(Saml2Constants.Elements.AttributeValue, Saml2Constants.Namespace);

            while (reader.NodeType == XmlNodeType.Whitespace)
            {
                whiteSpace += reader.Value;
                reader.Read();
            }

            reader.MoveToContent();
            if (reader.NodeType == XmlNodeType.Element)
            {
                while (reader.NodeType == XmlNodeType.Element)
                {
                    result += reader.ReadOuterXml();
                    reader.MoveToContent();
                }
            }
            else
            {
                result = whiteSpace;
                result += reader.ReadContentAsString();
            }

            reader.ReadEndElement();
            return result;
        }

        /// <summary>
        /// Writes the &lt;saml:Attribute> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Attribute"/>.</param>
        /// <param name="data">The <see cref="Saml2Attribute"/> to serialize.</param>
        protected virtual void WriteAttribute(XmlWriter writer, Saml2Attribute data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // <Attribute>
            writer.WriteStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace);

            // @Name - required
            writer.WriteAttributeString(Saml2Constants.Attributes.Name, data.Name);

            // @NameFormat - optional
            if (null != data.NameFormat)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.NameFormat, data.NameFormat.AbsoluteUri);
            }

            // @FriendlyName - optional
            if (null != data.FriendlyName)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.FriendlyName, data.FriendlyName);
            }

            // @OriginalIssuer - optional
            if (null != data.OriginalIssuer)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.OriginalIssuer, ClaimType2009Namespace, data.OriginalIssuer);
            }

            string xsiTypePrefix = null;
            string xsiTypeSuffix = null;
            if (!StringComparer.Ordinal.Equals(data.AttributeValueXsiType, ClaimValueTypes.String))
            {
                // ClaimValueTypes are URIs of the form prefix#suffix, while xsi:type should be a QName.
                // Hence, the tokens-to-claims spec requires that ClaimValueTypes be serialized as xmlns:tn="prefix" xsi:type="tn:suffix"
                int indexOfHash = data.AttributeValueXsiType.IndexOf('#');
                xsiTypePrefix = data.AttributeValueXsiType.Substring(0, indexOfHash);
                xsiTypeSuffix = data.AttributeValueXsiType.Substring(indexOfHash + 1);
            }

            // <AttributeValue> 0-OO (nillable)
            foreach (string value in data.Values)
            {
                writer.WriteStartElement(Saml2Constants.Elements.AttributeValue, Saml2Constants.Namespace);

                if (null == value)
                {
                    writer.WriteAttributeString("nil", XmlSchema.InstanceNamespace, XmlConvert.ToString(true));
                }
                else if (value.Length > 0)
                {
                    if ((xsiTypePrefix != null) && (xsiTypeSuffix != null))
                    {
                        writer.WriteAttributeString("xmlns", ProductConstants.ClaimValueTypeSerializationPrefix, null, xsiTypePrefix);
                        writer.WriteAttributeString("type", XmlSchema.InstanceNamespace, String.Concat(ProductConstants.ClaimValueTypeSerializationPrefixWithColon, xsiTypeSuffix));
                    }

                    this.WriteAttributeValue(writer, value, data);
                }

                writer.WriteEndElement();
            }

            // </Attribute>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the saml:Attribute value.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Attribute"/>.</param>
        /// <param name="value">The value of the attribute being serialized.</param>
        /// <param name="attribute">The <see cref="Saml2Attribute"/> to serialize.</param>
        /// <remarks>By default the method writes the value as a string.</remarks>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' is null.</exception>
        protected virtual void WriteAttributeValue(XmlWriter writer, string value, Saml2Attribute attribute)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            writer.WriteString(value);
        }

        /// <summary>
        /// Reads the &lt;saml:AttributeStatement> element, or a 
        /// &lt;saml:Statement element that specifies an xsi:type of
        /// saml:AttributeStatementType.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2AttributeStatement"/> element.</param>
        /// <returns>A <see cref="Saml2AttributeStatement"/> instance.</returns>
        protected virtual Saml2AttributeStatement ReadAttributeStatement(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            bool isStatementElement = false;
            if (reader.IsStartElement(Saml2Constants.Elements.Statement, Saml2Constants.Namespace))
            {
                isStatementElement = true;
            }
            else if (!reader.IsStartElement(Saml2Constants.Elements.AttributeStatement, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.AttributeStatement, Saml2Constants.Namespace);
            }

            try
            {
                // defer disallowing empty element until checking xsi:type
                bool isEmpty = reader.IsEmptyElement;

                // @attributes

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AttributeStatementType, Saml2Constants.Namespace, isStatementElement);

                // disallow empty element, since xsi:type is ok
                if (isEmpty)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, Saml2Constants.Elements.AttributeStatement, Saml2Constants.Namespace));
                }

                // Content
                Saml2AttributeStatement statement = new Saml2AttributeStatement();
                reader.Read();

                // <Attribute|EncryptedAttribute> 1-OO
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Saml2Constants.Elements.EncryptedAttribute, Saml2Constants.Namespace))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4158));
                    }
                    else if (reader.IsStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace))
                    {
                        statement.Attributes.Add(this.ReadAttribute(reader));
                    }
                    else
                    {
                        break;
                    }
                }

                // At least one attribute expected
                if (0 == statement.Attributes.Count)
                {
                    reader.ReadStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace);
                }

                reader.ReadEndElement();

                return statement;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:AttributeStatement> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2AttributeStatement"/>.</param>
        /// <param name="data">The <see cref="Saml2AttributeStatement"/> to serialize.</param>
        protected virtual void WriteAttributeStatement(XmlWriter writer, Saml2AttributeStatement data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            if (data.Attributes == null || 0 == data.Attributes.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4124)));
            }

            // <AttributeStatement>
            writer.WriteStartElement(Saml2Constants.Elements.AttributeStatement, Saml2Constants.Namespace);

            // <Attribute> 1-OO
            foreach (Saml2Attribute attribute in data.Attributes)
            {
                this.WriteAttribute(writer, attribute);
            }

            // </AttributeStatement>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:AudienceRestriction> element or a 
        /// &lt;saml:Condition> element that specifies an xsi:type
        /// of saml:AudienceRestrictionType.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2AudienceRestriction"/> element.</param>
        /// <returns>A <see cref="Saml2AudienceRestriction"/> instance.</returns>
        protected virtual Saml2AudienceRestriction ReadAudienceRestriction(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            bool isConditionElement = false;
            if (reader.IsStartElement(Saml2Constants.Elements.Condition, Saml2Constants.Namespace))
            {
                isConditionElement = true;
            }
            else if (!reader.IsStartElement(Saml2Constants.Elements.AudienceRestriction, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.AudienceRestriction, Saml2Constants.Namespace);
            }

            try
            {
                Saml2AudienceRestriction audienceRestriction;
                bool isEmpty = reader.IsEmptyElement;

                // @attributes

                // @xsi:type -- if we're a <Condition> element, this declaration must be present
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AudienceRestrictionType, Saml2Constants.Namespace, isConditionElement);

                // disallow empty
                if (isEmpty)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, reader.LocalName, reader.NamespaceURI));
                }

                // content
                reader.Read();

                // <Audience> - 1-OO
                if (!reader.IsStartElement(Saml2Constants.Elements.Audience, Saml2Constants.Namespace))
                {
                    reader.ReadStartElement(Saml2Constants.Elements.Audience, Saml2Constants.Namespace);
                }

                // We are now laxing the uri check for audience restriction to support interop partners 
                // This is a specific request from server : Bug 11850
                // ReadSimpleUriElement now has a flag that turns lax reading ON/OFF.
                audienceRestriction = new Saml2AudienceRestriction(ReadSimpleUriElement(reader, UriKind.RelativeOrAbsolute, true));
                while (reader.IsStartElement(Saml2Constants.Elements.Audience, Saml2Constants.Namespace))
                {
                    audienceRestriction.Audiences.Add(ReadSimpleUriElement(reader, UriKind.RelativeOrAbsolute, true));
                }

                reader.ReadEndElement();

                return audienceRestriction;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:AudienceRestriction> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2AudienceRestriction"/>.</param>
        /// <param name="data">The <see cref="Saml2AudienceRestriction"/> to serialize.</param>
        protected virtual void WriteAudienceRestriction(XmlWriter writer, Saml2AudienceRestriction data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // Schema requires at least one audience.
            if (data.Audiences == null || 0 == data.Audiences.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4159)));
            }

            // <AudienceRestriction>
            writer.WriteStartElement(Saml2Constants.Elements.AudienceRestriction, Saml2Constants.Namespace);

            // <Audience> - 1-OO
            foreach (Uri audience in data.Audiences)
            {
                // When writing out the audience uri we use the OriginalString property to preserve the value that was initially passed down during token creation as-is. 
                writer.WriteElementString(Saml2Constants.Elements.Audience, Saml2Constants.Namespace, audience.OriginalString);
            }

            // </AudienceRestriction>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:AuthnContext> element.
        /// </summary>
        /// <remarks>
        /// The default implementation does not handle the optional 
        /// &lt;saml:AuthnContextDecl> element. To handle by-value 
        /// authentication context declarations, override this method.
        /// </remarks>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2AuthenticationContext"/> element.</param>
        /// <returns>A <see cref="Saml2AuthenticationContext"/> instance.</returns>
        protected virtual Saml2AuthenticationContext ReadAuthenticationContext(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.AuthnContext, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.AuthnContext, Saml2Constants.Namespace);
            }

            try
            {
                // Disallow empty
                if (reader.IsEmptyElement)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, Saml2Constants.Elements.AuthnContext, Saml2Constants.Namespace));
                }

                // @attributes

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AuthnContextType, Saml2Constants.Namespace);

                // Content
                reader.ReadStartElement();

                // At least one of ClassRef and ( Decl XOR DeclRef) must be present
                // At this time, we do not support Decl, which is a by-value 
                // authentication context declaration.
                Uri classRef = null;
                Uri declRef = null;

                // <AuthnContextClassRef> - see comment above
                if (reader.IsStartElement(Saml2Constants.Elements.AuthnContextClassRef, Saml2Constants.Namespace))
                {
                    classRef = ReadSimpleUriElement(reader);
                }

                // <AuthnContextDecl> - see comment above
                if (reader.IsStartElement(Saml2Constants.Elements.AuthnContextDecl, Saml2Constants.Namespace))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4118));
                }

                // <AuthnContextDeclRef> - see comment above
                // If there was no ClassRef, there must be a DeclRef
                if (reader.IsStartElement(Saml2Constants.Elements.AuthnContextDeclRef, Saml2Constants.Namespace))
                {
                    declRef = ReadSimpleUriElement(reader);
                }
                else if (null == classRef)
                {
                    reader.ReadStartElement(Saml2Constants.Elements.AuthnContextDeclRef, Saml2Constants.Namespace);
                }

                // Now we have enough data to create the object
                Saml2AuthenticationContext authnContext = new Saml2AuthenticationContext(classRef, declRef);

                // <AuthenticatingAuthority> - 0-OO
                while (reader.IsStartElement(Saml2Constants.Elements.AuthenticatingAuthority, Saml2Constants.Namespace))
                {
                    authnContext.AuthenticatingAuthorities.Add(ReadSimpleUriElement(reader));
                }

                reader.ReadEndElement();

                return authnContext;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:AuthnContext> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2AuthenticationContext"/>.</param>
        /// <param name="data">The <see cref="Saml2AuthenticationContext"/> to serialize.</param>
        protected virtual void WriteAuthenticationContext(XmlWriter writer, Saml2AuthenticationContext data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // One of ClassRef and DeclRef must be present.
            if (null == data.ClassReference && null == data.DeclarationReference)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ID4117)));
            }

            // <AuthnContext>
            writer.WriteStartElement(Saml2Constants.Elements.AuthnContext, Saml2Constants.Namespace);

            // <AuthnContextClassReference> 0-1
            if (null != data.ClassReference)
            {
                writer.WriteElementString(Saml2Constants.Elements.AuthnContextClassRef, Saml2Constants.Namespace, data.ClassReference.AbsoluteUri);
            }

            // <AuthnContextDeclRef> 0-1
            if (null != data.DeclarationReference)
            {
                writer.WriteElementString(Saml2Constants.Elements.AuthnContextDeclRef, Saml2Constants.Namespace, data.DeclarationReference.AbsoluteUri);
            }

            // <AuthenticatingAuthority> 0-OO
            foreach (Uri authority in data.AuthenticatingAuthorities)
            {
                writer.WriteElementString(Saml2Constants.Elements.AuthenticatingAuthority, Saml2Constants.Namespace, authority.AbsoluteUri);
            }

            // </AuthnContext>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:AuthnStatement> element or a &lt;saml:Statement>
        /// element that specifies an xsi:type of saml:AuthnStatementType.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2AuthenticationStatement"/> element.</param>
        /// <returns>A <see cref="Saml2AuthenticationStatement"/> instance.</returns>
        protected virtual Saml2AuthenticationStatement ReadAuthenticationStatement(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            bool isStatementElement = false;
            if (reader.IsStartElement(Saml2Constants.Elements.Statement, Saml2Constants.Namespace))
            {
                isStatementElement = true;
            }
            else if (!reader.IsStartElement(Saml2Constants.Elements.AuthnStatement, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.AuthnStatement, Saml2Constants.Namespace);
            }

            try
            {
                // Must cache the individual data since the required 
                // AuthnContext comes last
                DateTime authnInstant;
                Saml2AuthenticationContext authnContext;
                string sessionIndex;
                DateTime? sessionNotOnOrAfter = null;
                Saml2SubjectLocality subjectLocality = null;

                // defer disallowing empty until after xsi:type
                bool isEmpty = reader.IsEmptyElement;

                // @attributes
                string value;

                // @xsi:type -- if we're a <Statement> element, this declaration must be present
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AuthnStatementType, Saml2Constants.Namespace, isStatementElement);

                // disallow empty, since xsi:type is ok
                if (isEmpty)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, Saml2Constants.Elements.AuthnStatement, Saml2Constants.Namespace));
                }

                // @AuthnInstant - required
                value = reader.GetAttribute(Saml2Constants.Attributes.AuthnInstant);
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.AuthnInstant, Saml2Constants.Elements.AuthnStatement));
                }

                authnInstant = XmlConvert.ToDateTime(value, DateTimeFormats.Accepted);

                // @SessionIndex - optional
                sessionIndex = reader.GetAttribute(Saml2Constants.Attributes.SessionIndex);

                // @SessionNotOnOrAfter - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.SessionNotOnOrAfter);
                if (!string.IsNullOrEmpty(value))
                {
                    sessionNotOnOrAfter = XmlConvert.ToDateTime(value, DateTimeFormats.Accepted);
                }

                // Content
                reader.Read();

                // <SubjectLocality> 0-1
                if (reader.IsStartElement(Saml2Constants.Elements.SubjectLocality, Saml2Constants.Namespace))
                {
                    subjectLocality = this.ReadSubjectLocality(reader);
                }

                // <AuthnContext> 1
                authnContext = this.ReadAuthenticationContext(reader);

                reader.ReadEndElement();

                // Construct the actual object
                Saml2AuthenticationStatement authnStatement = new Saml2AuthenticationStatement(authnContext, authnInstant);
                authnStatement.SessionIndex = sessionIndex;
                authnStatement.SessionNotOnOrAfter = sessionNotOnOrAfter;
                authnStatement.SubjectLocality = subjectLocality;

                return authnStatement;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:AuthnStatement> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2AuthenticationStatement"/>.</param>
        /// <param name="data">The <see cref="Saml2AuthenticationStatement"/> to serialize.</param>
        protected virtual void WriteAuthenticationStatement(XmlWriter writer, Saml2AuthenticationStatement data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // <AuthnStatement>
            writer.WriteStartElement(Saml2Constants.Elements.AuthnStatement, Saml2Constants.Namespace);

            // @AuthnInstant - required
            writer.WriteAttributeString(Saml2Constants.Attributes.AuthnInstant, XmlConvert.ToString(data.AuthenticationInstant.ToUniversalTime(), DateTimeFormats.Generated));

            // @SessionIndex - optional
            if (null != data.SessionIndex)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.SessionIndex, data.SessionIndex);
            }

            // @SessionNotOnOrAfter - optional
            if (null != data.SessionNotOnOrAfter)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.SessionNotOnOrAfter, XmlConvert.ToString(data.SessionNotOnOrAfter.Value.ToUniversalTime(), DateTimeFormats.Generated));
            }

            // <SubjectLocality> 0-1
            if (null != data.SubjectLocality)
            {
                this.WriteSubjectLocality(writer, data.SubjectLocality);
            }

            // <AuthnContext> 1
            this.WriteAuthenticationContext(writer, data.AuthenticationContext);

            // </AuthnStatement>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:AuthzDecisionStatement> element or a 
        /// &lt;saml:Statement element that specifies an xsi:type of
        /// saml:AuthzDecisionStatementType.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2AuthorizationDecisionStatement"/> element.</param>
        /// <returns>A <see cref="Saml2AuthorizationDecisionStatement"/> instance.</returns>
        protected virtual Saml2AuthorizationDecisionStatement ReadAuthorizationDecisionStatement(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            bool isStatementElement = false;
            if (reader.IsStartElement(Saml2Constants.Elements.Statement, Saml2Constants.Namespace))
            {
                isStatementElement = true;
            }
            else if (!reader.IsStartElement(Saml2Constants.Elements.AuthzDecisionStatement, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.AuthzDecisionStatement, Saml2Constants.Namespace);
            }

            try
            {
                // Need the attributes before we can instantiate
                Saml2AuthorizationDecisionStatement statement;
                SamlAccessDecision decision;
                Uri resource;

                // defer rejecting empty until processing xsi:type
                bool isEmpty = reader.IsEmptyElement;

                // @attributes
                string value;

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.AuthzDecisionStatementType, Saml2Constants.Namespace, isStatementElement);

                // disallow empty, since xsi:type is ok
                if (isEmpty)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, Saml2Constants.Elements.AuthzDecisionStatement, Saml2Constants.Namespace));
                }

                // @Decision - required
                value = reader.GetAttribute(Saml2Constants.Attributes.Decision);
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.Decision, Saml2Constants.Elements.AuthzDecisionStatement));
                }
                else if (StringComparer.Ordinal.Equals(SamlAccessDecision.Permit.ToString(), value))
                {
                    decision = SamlAccessDecision.Permit;
                }
                else if (StringComparer.Ordinal.Equals(SamlAccessDecision.Deny.ToString(), value))
                {
                    decision = SamlAccessDecision.Deny;
                }
                else if (StringComparer.Ordinal.Equals(SamlAccessDecision.Indeterminate.ToString(), value))
                {
                    decision = SamlAccessDecision.Indeterminate;
                }
                else
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4123, value));
                }

                // @Resource - required
                value = reader.GetAttribute(Saml2Constants.Attributes.Resource);
                if (null == value)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.Resource, Saml2Constants.Elements.AuthzDecisionStatement));
                }
                else if (0 == value.Length)
                {
                    resource = Saml2AuthorizationDecisionStatement.EmptyResource;
                }
                else
                {
                    if (!UriUtil.CanCreateValidUri(value, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4121));
                    }

                    resource = new Uri(value);
                }

                // Content
                statement = new Saml2AuthorizationDecisionStatement(resource, decision);
                reader.Read();

                // <Action> 1-OO 
                do
                {
                    statement.Actions.Add(this.ReadAction(reader));
                }
                while (reader.IsStartElement(Saml2Constants.Elements.Action, Saml2Constants.Namespace));

                // <Evidence> 0-1
                if (reader.IsStartElement(Saml2Constants.Elements.Evidence, Saml2Constants.Namespace))
                {
                    statement.Evidence = this.ReadEvidence(reader);
                }

                reader.ReadEndElement();

                return statement;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:AuthzDecisionStatement> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2AuthorizationDecisionStatement"/>.</param>
        /// <param name="data">The <see cref="Saml2AuthorizationDecisionStatement"/> to serialize.</param>
        protected virtual void WriteAuthorizationDecisionStatement(XmlWriter writer, Saml2AuthorizationDecisionStatement data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

#pragma warning suppress 56506 // actions are never null
            if (0 == data.Actions.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ID4122)));
            }

            // <AuthzDecisionStatement>
            writer.WriteStartElement(Saml2Constants.Elements.AuthzDecisionStatement, Saml2Constants.Namespace);

            // @Decision - required
            writer.WriteAttributeString(Saml2Constants.Attributes.Decision, data.Decision.ToString());

            // @Resource - required
#pragma warning suppress 56506 // Resource are never null
            writer.WriteAttributeString(Saml2Constants.Attributes.Resource, data.Resource.Equals(Saml2AuthorizationDecisionStatement.EmptyResource) ? data.Resource.ToString() : data.Resource.AbsoluteUri);

            // @Action 1-OO
            foreach (Saml2Action action in data.Actions)
            {
                this.WriteAction(writer, action);
            }

            // Evidence 0-1
            if (null != data.Evidence)
            {
                this.WriteEvidence(writer, data.Evidence);
            }

            // </AuthzDecisionStatement>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:Conditions> element.
        /// </summary>
        /// <remarks>
        /// To handle custom &lt;saml:Condition> elements, override this 
        /// method.
        /// </remarks>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Conditions"/> element.</param>
        /// <returns>A <see cref="Saml2Conditions"/> instance.</returns>
        protected virtual Saml2Conditions ReadConditions(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Conditions, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Conditions, Saml2Constants.Namespace);
            }

            try
            {
                Saml2Conditions conditions = new Saml2Conditions();

                bool isEmpty = reader.IsEmptyElement;

                // @attributes
                string value;

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.ConditionsType, Saml2Constants.Namespace);

                // @NotBefore - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.NotBefore);
                if (!string.IsNullOrEmpty(value))
                {
                    conditions.NotBefore = XmlConvert.ToDateTime(value, DateTimeFormats.Accepted);
                }

                // @NotOnOrAfter - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.NotOnOrAfter);
                if (!string.IsNullOrEmpty(value))
                {
                    conditions.NotOnOrAfter = XmlConvert.ToDateTime(value, DateTimeFormats.Accepted);
                }

                // Content
                reader.ReadStartElement();
                if (!isEmpty)
                {
                    // <Condition|AudienceRestriction|OneTimeUse|ProxyRestriction>, 0-OO
                    while (reader.IsStartElement())
                    {
                        // <Condition> - 0-OO
                        if (reader.IsStartElement(Saml2Constants.Elements.Condition, Saml2Constants.Namespace))
                        {
                            // Since Condition is abstract, must process based on xsi:type
                            XmlQualifiedName declaredType = XmlUtil.GetXsiType(reader);

                            // No type, throw
                            if (null == declaredType
                                || XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.ConditionAbstractType, Saml2Constants.Namespace))
                            {
                                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4104, reader.LocalName, reader.NamespaceURI));
                            }
                            else if (XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.AudienceRestrictionType, Saml2Constants.Namespace))
                            {
                                conditions.AudienceRestrictions.Add(this.ReadAudienceRestriction(reader));
                            }
                            else if (XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.OneTimeUseType, Saml2Constants.Namespace))
                            {
                                if (conditions.OneTimeUse)
                                {
                                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4115, Saml2Constants.Elements.OneTimeUse));
                                }

                                ReadEmptyContentElement(reader);
                                conditions.OneTimeUse = true;
                            }
                            else if (XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.ProxyRestrictionType, Saml2Constants.Namespace))
                            {
                                if (null != conditions.ProxyRestriction)
                                {
                                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4115, Saml2Constants.Elements.ProxyRestricton));
                                }

                                conditions.ProxyRestriction = this.ReadProxyRestriction(reader);
                            }
                            else
                            {
                                // Unknown type - Instruct the user to override to handle custom <Condition>
                                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4113));
                            }
                        }
                        else if (reader.IsStartElement(Saml2Constants.Elements.AudienceRestriction, Saml2Constants.Namespace))
                        {
                            conditions.AudienceRestrictions.Add(this.ReadAudienceRestriction(reader));
                        }
                        else if (reader.IsStartElement(Saml2Constants.Elements.OneTimeUse, Saml2Constants.Namespace))
                        {
                            if (conditions.OneTimeUse)
                            {
                                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4115, Saml2Constants.Elements.OneTimeUse));
                            }

                            ReadEmptyContentElement(reader);
                            conditions.OneTimeUse = true;
                        }
                        else if (reader.IsStartElement(Saml2Constants.Elements.ProxyRestricton, Saml2Constants.Namespace))
                        {
                            if (null != conditions.ProxyRestriction)
                            {
                                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4115, Saml2Constants.Elements.ProxyRestricton));
                            }

                            conditions.ProxyRestriction = this.ReadProxyRestriction(reader);
                        }
                        else
                        {
                            break;
                        }
                    }

                    reader.ReadEndElement();
                }

                return conditions;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:Conditions> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Conditions"/>.</param>
        /// <param name="data">The <see cref="Saml2Conditions"/> to serialize.</param>
        protected virtual void WriteConditions(XmlWriter writer, Saml2Conditions data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // <Conditions>
            writer.WriteStartElement(Saml2Constants.Elements.Conditions, Saml2Constants.Namespace);

            // @NotBefore - optional
            if (null != data.NotBefore)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.NotBefore, XmlConvert.ToString(data.NotBefore.Value.ToUniversalTime(), DateTimeFormats.Generated));
            }

            // @NotOnOrAfter - optional
            if (null != data.NotOnOrAfter)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.NotOnOrAfter, XmlConvert.ToString(data.NotOnOrAfter.Value.ToUniversalTime(), DateTimeFormats.Generated));
            }

            // <AudienceRestriction> 0-OO
            foreach (Saml2AudienceRestriction audienceRestriction in data.AudienceRestrictions)
            {
                this.WriteAudienceRestriction(writer, audienceRestriction);
            }

            // <OneTimeUse> - limited to one in SAML spec
            if (data.OneTimeUse)
            {
                writer.WriteStartElement(Saml2Constants.Elements.OneTimeUse, Saml2Constants.Namespace);
                writer.WriteEndElement();
            }

            // <ProxyRestriction> - limited to one in SAML spec
            if (null != data.ProxyRestriction)
            {
                this.WriteProxyRestriction(writer, data.ProxyRestriction);
            }

            // </Conditions>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:Evidence> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Evidence"/> element.</param>
        /// <returns>A <see cref="Saml2Evidence"/> instance.</returns>
        protected virtual Saml2Evidence ReadEvidence(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Evidence, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Evidence, Saml2Constants.Namespace);
            }

            // disallow empty
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, Saml2Constants.Elements.Evidence, Saml2Constants.Namespace));
            }

            try
            {
                Saml2Evidence evidence = new Saml2Evidence();

                // @attributes

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.EvidenceType, Saml2Constants.Namespace);

                reader.Read();

                // <AssertionIDRef|AssertionURIRef|Assertion|EncryptedAssertion> 0-OO
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(Saml2Constants.Elements.AssertionIDRef, Saml2Constants.Namespace))
                    {
                        evidence.AssertionIdReferences.Add(ReadSimpleNCNameElement(reader));
                    }
                    else if (reader.IsStartElement(Saml2Constants.Elements.AssertionURIRef, Saml2Constants.Namespace))
                    {
                        evidence.AssertionUriReferences.Add(ReadSimpleUriElement(reader));
                    }
                    else if (reader.IsStartElement(Saml2Constants.Elements.Assertion, Saml2Constants.Namespace))
                    {
                        evidence.Assertions.Add(this.ReadAssertion(reader));
                    }
                    else if (reader.IsStartElement(Saml2Constants.Elements.EncryptedAssertion, Saml2Constants.Namespace))
                    {
                        evidence.Assertions.Add(this.ReadAssertion(reader));
                    }
                }

                if (0 == evidence.AssertionIdReferences.Count
                        && 0 == evidence.Assertions.Count
                        && 0 == evidence.AssertionUriReferences.Count)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4120));
                }

                reader.ReadEndElement();

                return evidence;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:Evidence> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Evidence"/>.</param>
        /// <param name="data">The <see cref="Saml2Evidence"/> to serialize.</param>
        protected virtual void WriteEvidence(XmlWriter writer, Saml2Evidence data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            if ((data.AssertionIdReferences == null || 0 == data.AssertionIdReferences.Count)
               && (data.Assertions == null || 0 == data.Assertions.Count)
               && (data.AssertionUriReferences == null || 0 == data.AssertionUriReferences.Count))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ID4120)));
            }

            // <Evidence>
            writer.WriteStartElement(Saml2Constants.Elements.Evidence, Saml2Constants.Namespace);

            // <AssertionIDRef> 0-OO
            foreach (Saml2Id id in data.AssertionIdReferences)
            {
                writer.WriteElementString(Saml2Constants.Elements.AssertionIDRef, Saml2Constants.Namespace, id.Value);
            }

            // <AssertionURIRef> 0-OO
            foreach (Uri uri in data.AssertionUriReferences)
            {
                writer.WriteElementString(Saml2Constants.Elements.AssertionURIRef, Saml2Constants.Namespace, uri.AbsoluteUri);
            }

            // <Assertion> 0-OO
            foreach (Saml2Assertion assertion in data.Assertions)
            {
                this.WriteAssertion(writer, assertion);
            }

            // </Evidence>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:Issuer> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2NameIdentifier"/> element.</param>
        /// <returns>A <see cref="Saml2NameIdentifier"/> instance.</returns>
        protected virtual Saml2NameIdentifier ReadIssuer(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Issuer, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Issuer, Saml2Constants.Namespace);
            }

            return this.ReadNameIdType(reader);
        }

        /// <summary>
        /// Writes the &lt;saml:Issuer> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2NameIdentifier"/>.</param>
        /// <param name="data">The <see cref="Saml2NameIdentifier"/> to serialize.</param>
        protected virtual void WriteIssuer(XmlWriter writer, Saml2NameIdentifier data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            writer.WriteStartElement(Saml2Constants.Elements.Issuer, Saml2Constants.Namespace);
            this.WriteNameIdType(writer, data);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Deserializes the SAML Subject KeyInfo.
        /// </summary>
        /// <param name="reader">XmlReader positioned at a ds:KeyInfo element.</param>
        /// <returns>A <see cref="SecurityKeyIdentifier"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'reader' is null.</exception>
        protected virtual SecurityKeyIdentifier ReadSubjectKeyInfo(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return this.KeyInfoSerializer.ReadKeyIdentifier(reader);
        }

        /// <summary>
        /// Deserializes the SAML Signing KeyInfo
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a than can be positioned at a ds:KeyInfo element.</param>
        /// <param name="assertion">The <see cref="Saml2Assertion"/> that is having the signature checked.</param>
        /// <returns>The <see cref="SecurityKeyIdentifier"/> that defines the key to use to check the signature.</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'reader' is null.</exception>
        protected virtual SecurityKeyIdentifier ReadSigningKeyInfo(XmlReader reader, Saml2Assertion assertion)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            SecurityKeyIdentifier ski;

            if (this.KeyInfoSerializer.CanReadKeyIdentifier(reader))
            {
                ski = this.KeyInfoSerializer.ReadKeyIdentifier(reader);
            }
            else
            {
                KeyInfo keyInfo = new KeyInfo(this.KeyInfoSerializer);
                keyInfo.ReadXml(XmlDictionaryReader.CreateDictionaryReader(reader));
                ski = keyInfo.KeyIdentifier;
            }

            // no key info
            if (ski.Count == 0)
            {
                return new SecurityKeyIdentifier(new Saml2SecurityKeyIdentifierClause(assertion));
            }

            return ski;
        }

        /// <summary>
        /// Serializes the Subject KeyInfo into the given XmlWriter.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="SecurityKeyIdentifier"/>.</param>
        /// <param name="data">The <see cref="SecurityKeyIdentifier"/> to serialize.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'data' is null.</exception>
        protected virtual void WriteSubjectKeyInfo(XmlWriter writer, SecurityKeyIdentifier data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            this.KeyInfoSerializer.WriteKeyIdentifier(writer, data);
        }

        /// <summary>
        /// Serializes the Signing KeyInfo into the given XmlWriter.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="SecurityKeyIdentifier"/>.</param>
        /// <param name="data">The <see cref="SecurityKeyIdentifier"/> to serialize.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'signingKeyIdentifier' is null.</exception>
        protected virtual void WriteSigningKeyInfo(XmlWriter writer, SecurityKeyIdentifier data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            if (this.KeyInfoSerializer.CanWriteKeyIdentifier(data))
            {
                this.KeyInfoSerializer.WriteKeyIdentifier(writer, data);
                return;
            }

            throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4221, data));
        }

        /// <summary>
        /// Reads the &lt;saml:NameID> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2NameIdentifier"/> element.</param>
        /// <returns>An instance of <see cref="Saml2NameIdentifier"/></returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected virtual Saml2NameIdentifier ReadNameId(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.NameID, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.NameID, Saml2Constants.Namespace);
            }

            return this.ReadNameIdType(reader);
        }

        /// <summary>
        /// Writes the &lt;saml:NameID> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2NameIdentifier"/>.</param>
        /// <param name="data">The <see cref="Saml2NameIdentifier"/> to serialize.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'data' is null.</exception>
        /// <exception cref="CryptographicException">Saml2NameIdentifier encrypting credentials must have a Symmetric Key specified.</exception>
        protected virtual void WriteNameId(XmlWriter writer, Saml2NameIdentifier data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // If there are encrypting credentials, then we need to encrypt the name identifier
            if (data.EncryptingCredentials != null)
            {
                EncryptingCredentials encryptingCredentials = data.EncryptingCredentials;

                // Get the encryption key, which must be symmetric
                SymmetricSecurityKey encryptingKey = encryptingCredentials.SecurityKey as SymmetricSecurityKey;
                if (encryptingKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.ID3284)));
                }

                MemoryStream plaintextStream = null;
                try
                {
                    // Serialize an encrypted name ID
                    plaintextStream = new MemoryStream();

                    using (XmlWriter plaintextWriter = XmlDictionaryWriter.CreateTextWriter(plaintextStream, Encoding.UTF8, false))
                    {
                        plaintextWriter.WriteStartElement(Saml2Constants.Elements.NameID, Saml2Constants.Namespace);
                        this.WriteNameIdType(plaintextWriter, data);
                        plaintextWriter.WriteEndElement();
                    }

                    EncryptedDataElement encryptedData = new EncryptedDataElement();
                    encryptedData.Type = XmlEncryptionConstants.EncryptedDataTypes.Element;
                    encryptedData.Algorithm = encryptingCredentials.Algorithm;
                    encryptedData.KeyIdentifier = encryptingCredentials.SecurityKeyIdentifier;

                    // Perform encryption
                    SymmetricAlgorithm symmetricAlgorithm = encryptingKey.GetSymmetricAlgorithm(encryptingCredentials.Algorithm);
                    encryptedData.Encrypt(symmetricAlgorithm, plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length);
                    ((IDisposable)plaintextStream).Dispose();

                    writer.WriteStartElement(Saml2Constants.Elements.EncryptedID, Saml2Constants.Namespace);
                    encryptedData.WriteXml(writer, this.KeyInfoSerializer);

                    foreach (EncryptedKeyIdentifierClause clause in data.ExternalEncryptedKeys)
                    {
                        this.KeyInfoSerializer.WriteKeyIdentifierClause(writer, clause);
                    }

                    writer.WriteEndElement();
                }
                finally
                {
                    if (plaintextStream != null)
                    {
                        plaintextStream.Dispose();
                        plaintextStream = null;
                    }
                }
            }
            else
            {
                writer.WriteStartElement(Saml2Constants.Elements.NameID, Saml2Constants.Namespace);
                this.WriteNameIdType(writer, data);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Both &lt;Issuer> and &lt;NameID> are of NameIDType. This method reads
        /// the content of either one of those elements. 
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2NameIdentifier"/> element.</param>
        /// <returns>An instance of <see cref="Saml2NameIdentifier"/></returns>
        protected virtual Saml2NameIdentifier ReadNameIdType(XmlReader reader)
        {
            try
            {
                reader.MoveToContent();

                Saml2NameIdentifier nameIdentifier = new Saml2NameIdentifier("__TemporaryName__");

                // @attributes
                string value;

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.NameIDType, Saml2Constants.Namespace);

                // @Format - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.Format);
                if (!string.IsNullOrEmpty(value))
                {
                    if (!UriUtil.CanCreateValidUri(value, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0011, Saml2Constants.Attributes.Format, Saml2Constants.Elements.NameID));
                    }

                    nameIdentifier.Format = new Uri(value);
                }

                // @NameQualifier - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.NameQualifier);
                if (!string.IsNullOrEmpty(value))
                {
                    nameIdentifier.NameQualifier = value;
                }

                // @SPNameQualifier - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.SPNameQualifier);
                if (!string.IsNullOrEmpty(value))
                {
                    nameIdentifier.SPNameQualifier = value;
                }

                // @SPProvidedID - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.SPProvidedID);
                if (!string.IsNullOrEmpty(value))
                {
                    nameIdentifier.SPProvidedId = value;
                }

                // Content is string
                nameIdentifier.Value = reader.ReadElementString();

                // According to section 8.3.6, if the name identifier format is of type 'urn:oasis:names:tc:SAML:2.0:nameid-format:entity'
                // the name identifier value must be a uri and name qualifier, spname qualifier, and spproded id must be omitted.
                if (nameIdentifier.Format != null &&
                    StringComparer.Ordinal.Equals(nameIdentifier.Format.AbsoluteUri, Saml2Constants.NameIdentifierFormats.Entity.AbsoluteUri))
                {
                    if (!UriUtil.CanCreateValidUri(nameIdentifier.Value, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4262, nameIdentifier.Value, Saml2Constants.NameIdentifierFormats.Entity.AbsoluteUri));
                    }

                    if (!string.IsNullOrEmpty(nameIdentifier.NameQualifier)
                        || !string.IsNullOrEmpty(nameIdentifier.SPNameQualifier)
                        || !string.IsNullOrEmpty(nameIdentifier.SPProvidedId))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4263, nameIdentifier.Value, Saml2Constants.NameIdentifierFormats.Entity.AbsoluteUri));
                    }
                }

                return nameIdentifier;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Reads the &lt;saml:EncryptedId> element.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> pointing at the XML EncryptedId element</param>
        /// <returns>An instance of <see cref="Saml2NameIdentifier"/> representing the EncryptedId that was read</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">The 'reader' is not positioned at an 'EncryptedID' element.</exception>
        protected virtual Saml2NameIdentifier ReadEncryptedId(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            reader.MoveToContent();

            if (!reader.IsStartElement(Saml2Constants.Elements.EncryptedID, Saml2Constants.Namespace))
            {
                // throw if wrong element
                reader.ReadStartElement(Saml2Constants.Elements.EncryptedID, Saml2Constants.Namespace);
            }

            Collection<EncryptedKeyIdentifierClause> clauses = new Collection<EncryptedKeyIdentifierClause>();
            EncryptingCredentials encryptingCredentials = null;
            Saml2NameIdentifier saml2NameIdentifier = null;

            using (StringReader sr = new StringReader(reader.ReadOuterXml()))
            {
                using (XmlDictionaryReader wrappedReader = new WrappedXmlDictionaryReader(XmlReader.Create(sr), XmlDictionaryReaderQuotas.Max))
                {
                    XmlReader plaintextReader = CreatePlaintextReaderFromEncryptedData(
                                wrappedReader,
                                Configuration.ServiceTokenResolver,
                                this.KeyInfoSerializer,
                                clauses,
                                out encryptingCredentials);

                    saml2NameIdentifier = this.ReadNameIdType(plaintextReader);
                    saml2NameIdentifier.EncryptingCredentials = encryptingCredentials;
                    foreach (EncryptedKeyIdentifierClause clause in clauses)
                    {
                        saml2NameIdentifier.ExternalEncryptedKeys.Add(clause);
                    }
                }
            }

            return saml2NameIdentifier;
        }

        /// <summary>
        /// Both &lt;Issuer> and &lt;NameID> are of NameIDType. This method writes
        /// the content of either one of those elements. 
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2NameIdentifier"/>.</param>
        /// <param name="data">The <see cref="Saml2NameIdentifier"/> to serialize.</param>
        protected virtual void WriteNameIdType(XmlWriter writer, Saml2NameIdentifier data)
        {
            // @Format - optional
            if (null != data.Format)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.Format, data.Format.AbsoluteUri);
            }

            // @NameQualifier - optional
            if (!string.IsNullOrEmpty(data.NameQualifier))
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.NameQualifier, data.NameQualifier);
            }

            // @SPNameQualifier - optional
            if (!string.IsNullOrEmpty(data.SPNameQualifier))
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.SPNameQualifier, data.SPNameQualifier);
            }

            // @SPProvidedId - optional
            if (!string.IsNullOrEmpty(data.SPProvidedId))
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.SPProvidedID, data.SPProvidedId);
            }

            // Content is string
            writer.WriteString(data.Value);
        }

        /// <summary>
        /// Reads the &lt;saml:ProxyRestriction> element, or a &lt;saml:Condition>
        /// element that specifies an xsi:type of saml:ProxyRestrictionType.
        /// </summary>
        /// <remarks>
        /// In the default implementation, the maximum value of the Count attribute 
        /// is limited to Int32.MaxValue.
        /// </remarks>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2ProxyRestriction"/> element.</param>
        /// <returns>An instance of <see cref="Saml2ProxyRestriction"/></returns>
        protected virtual Saml2ProxyRestriction ReadProxyRestriction(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            bool isConditionElement = false;
            if (reader.IsStartElement(Saml2Constants.Elements.Condition, Saml2Constants.Namespace))
            {
                isConditionElement = true;
            }
            else if (!reader.IsStartElement(Saml2Constants.Elements.ProxyRestricton, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.ProxyRestricton, Saml2Constants.Namespace);
            }

            try
            {
                Saml2ProxyRestriction proxyRestriction = new Saml2ProxyRestriction();

                bool isEmpty = reader.IsEmptyElement;

                // @attributes
                string value;

                // @xsi:type -- if we're a <Condition> element, this declaration must be present
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.ProxyRestrictionType, Saml2Constants.Namespace, isConditionElement);

                // @Count - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.Count);
                if (!string.IsNullOrEmpty(value))
                {
                    proxyRestriction.Count = XmlConvert.ToInt32(value);
                }

                // content
                reader.Read();
                if (!isEmpty)
                {
                    // <Audience> - 0-OO
                    while (reader.IsStartElement(Saml2Constants.Elements.Audience, Saml2Constants.Namespace))
                    {
                        proxyRestriction.Audiences.Add(ReadSimpleUriElement(reader));
                    }

                    reader.ReadEndElement();
                }

                return proxyRestriction;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:ProxyRestriction> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2ProxyRestriction"/>.</param>
        /// <param name="data">The <see cref="Saml2ProxyRestriction"/> to serialize.</param>
        protected virtual void WriteProxyRestriction(XmlWriter writer, Saml2ProxyRestriction data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            writer.WriteStartElement(Saml2Constants.Elements.ProxyRestricton, Saml2Constants.Namespace);

            // @Count - optional
            if (null != data.Count)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.Count, XmlConvert.ToString(data.Count.Value));
            }

            // <Audience> - 0-OO
            foreach (Uri uri in data.Audiences)
            {
                writer.WriteElementString(Saml2Constants.Elements.Audience, uri.AbsoluteUri);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:Statement> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Statement"/> element.</param>
        /// <returns>An instance of <see cref="Saml2Statement"/> derived type.</returns>
        /// <remarks>
        /// The default implementation only handles Statement elements which 
        /// specify an xsi:type of saml:AttributeStatementType, 
        /// saml:AuthnStatementType, and saml:AuthzDecisionStatementType. To 
        /// handle custom statements, override this method.
        /// </remarks>
        protected virtual Saml2Statement ReadStatement(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Statement, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Statement, Saml2Constants.Namespace);
            }

            // Since Statement is an abstract type, we have to switch off the xsi:type declaration
            XmlQualifiedName declaredType = XmlUtil.GetXsiType(reader);

            // No declaration, or declaring that this is just a "Statement", is invalid since 
            // statement is abstract
            if (null == declaredType
                || XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.StatementAbstractType, Saml2Constants.Namespace))
            {
                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4104, reader.LocalName, reader.NamespaceURI));
            }

            // Reroute to the known statement types if applicable
            if (XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.AttributeStatementType, Saml2Constants.Namespace))
            {
                return this.ReadAttributeStatement(reader);
            }
            else if (XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.AuthnStatementType, Saml2Constants.Namespace))
            {
                return this.ReadAuthenticationStatement(reader);
            }
            else if (XmlUtil.EqualsQName(declaredType, Saml2Constants.Types.AuthzDecisionStatementType, Saml2Constants.Namespace))
            {
                return this.ReadAuthorizationDecisionStatement(reader);
            }
            else
            {
                // Throw if we encounter an unknown concrete type
                throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4105, declaredType.Name, declaredType.Namespace));
            }
        }

        /// <summary>
        /// Writes a Saml2Statement.
        /// </summary>
        /// <remarks>
        /// This method may write a &lt;saml:AttributeStatement>, &lt;saml:AuthnStatement> 
        /// or &lt;saml:AuthzDecisionStatement> element. To handle custom Saml2Statement
        /// classes for writing a &lt;saml:Statement> element, override this method.
        /// </remarks>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Statement"/>.</param>
        /// <param name="data">The <see cref="Saml2Statement"/> to serialize.</param>
        protected virtual void WriteStatement(XmlWriter writer, Saml2Statement data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            Saml2AttributeStatement attributeStatement = data as Saml2AttributeStatement;
            if (null != attributeStatement)
            {
                this.WriteAttributeStatement(writer, attributeStatement);
                return;
            }

            Saml2AuthenticationStatement authnStatement = data as Saml2AuthenticationStatement;
            if (null != authnStatement)
            {
                this.WriteAuthenticationStatement(writer, authnStatement);
                return;
            }

            Saml2AuthorizationDecisionStatement authzStatement = data as Saml2AuthorizationDecisionStatement;
            if (null != authzStatement)
            {
                this.WriteAuthorizationDecisionStatement(writer, authzStatement);
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                new InvalidOperationException(SR.GetString(SR.ID4107, data.GetType().AssemblyQualifiedName)));
        }

        /// <summary>
        /// Reads the &lt;saml:Subject> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2Subject"/> element.</param>
        /// <returns>An instance of <see cref="Saml2Subject"/> .</returns>
        /// <remarks>
        /// The default implementation does not handle the optional
        /// &lt;EncryptedID> element. To handle encryped IDs in the Subject,
        /// override this method.
        /// </remarks>
        protected virtual Saml2Subject ReadSubject(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.Subject, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.Subject, Saml2Constants.Namespace);
            }

            try
            {
                // disallow empty
                if (reader.IsEmptyElement)
                {
#pragma warning suppress 56504 // bogus - thinks reader.LocalName, reader.NamespaceURI need validation
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID3061, reader.LocalName, reader.NamespaceURI));
                }

                // @attributes

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.SubjectType, Saml2Constants.Namespace);

                // <elements>
                Saml2Subject subject = new Saml2Subject();
                reader.Read();

                // <NameID> | <EncryptedID> | <BaseID> 0-1
                subject.NameId = this.ReadSubjectId(reader, Saml2Constants.Elements.Subject);

                // <SubjectConfirmation> 0-OO
                while (reader.IsStartElement(Saml2Constants.Elements.SubjectConfirmation, Saml2Constants.Namespace))
                {
                    subject.SubjectConfirmations.Add(this.ReadSubjectConfirmation(reader));
                }

                reader.ReadEndElement();

                // Must have a NameID or a SubjectConfirmation
                if (null == subject.NameId && 0 == subject.SubjectConfirmations.Count)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4108));
                }

                return subject;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:Subject> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2Subject"/>.</param>
        /// <param name="data">The <see cref="Saml2Subject"/> to serialize.</param>
        protected virtual void WriteSubject(XmlWriter writer, Saml2Subject data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // If there's no ID, there has to be a SubjectConfirmation
#pragma warning suppress 56506 // SubjectConfirmations is never null
            if (null == data.NameId && 0 == data.SubjectConfirmations.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4108)));
            }

            // <Subject>
            writer.WriteStartElement(Saml2Constants.Elements.Subject, Saml2Constants.Namespace);

            // no attributes

            // <NameID> 0-1
            if (null != data.NameId)
            {
                this.WriteNameId(writer, data.NameId);
            }

            // <SubjectConfirmation> 0-OO
            foreach (Saml2SubjectConfirmation subjectConfirmation in data.SubjectConfirmations)
            {
                this.WriteSubjectConfirmation(writer, subjectConfirmation);
            }

            // </Subject>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;SubjectConfirmation> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2SubjectConfirmation"/> element.</param>
        /// <returns>An instance of <see cref="Saml2SubjectConfirmation"/> .</returns>
        protected virtual Saml2SubjectConfirmation ReadSubjectConfirmation(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.SubjectConfirmation, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.SubjectConfirmation, Saml2Constants.Namespace);
            }

            try
            {
                bool isEmpty = reader.IsEmptyElement;

                // @attributes

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.SubjectConfirmationType, Saml2Constants.Namespace);

                // @Method - required
                string method = reader.GetAttribute(Saml2Constants.Attributes.Method);
                if (string.IsNullOrEmpty(method))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0001, Saml2Constants.Attributes.Method, Saml2Constants.Elements.SubjectConfirmation));
                }

                if (!UriUtil.CanCreateValidUri(method, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0011, Saml2Constants.Attributes.Method, Saml2Constants.Elements.SubjectConfirmation));
                }

                // Construct the appropriate SubjectConfirmation based on the method
                Saml2SubjectConfirmation subjectConfirmation = new Saml2SubjectConfirmation(new Uri(method));

                // <elements>
                reader.Read();
                if (!isEmpty)
                {
                    // <NameID> | <EncryptedID> | <BaseID> 0-1
                    subjectConfirmation.NameIdentifier = this.ReadSubjectId(reader, Saml2Constants.Elements.SubjectConfirmation);

                    // <SubjectConfirmationData> 0-1
                    if (reader.IsStartElement(Saml2Constants.Elements.SubjectConfirmationData, Saml2Constants.Namespace))
                    {
                        subjectConfirmation.SubjectConfirmationData = this.ReadSubjectConfirmationData(reader);
                    }

                    reader.ReadEndElement();
                }

                return subjectConfirmation;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:SubjectConfirmation> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2SubjectConfirmation"/>.</param>
        /// <param name="data">The <see cref="Saml2SubjectConfirmation"/> to serialize.</param>
        protected virtual void WriteSubjectConfirmation(XmlWriter writer, Saml2SubjectConfirmation data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            if (null == data.Method)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data.Method");
            }

            if (string.IsNullOrEmpty(data.Method.ToString()))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("data.Method");
            }

            // <SubjectConfirmation>
            writer.WriteStartElement(Saml2Constants.Elements.SubjectConfirmation, Saml2Constants.Namespace);

            // @Method - required
            writer.WriteAttributeString(Saml2Constants.Attributes.Method, data.Method.AbsoluteUri);

            // <NameID> 0-1
            if (null != data.NameIdentifier)
            {
                this.WriteNameId(writer, data.NameIdentifier);
            }

            // <SubjectConfirmationData> 0-1
            if (null != data.SubjectConfirmationData)
            {
                this.WriteSubjectConfirmationData(writer, data.SubjectConfirmationData);
            }

            // </SubjectConfirmation>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:SubjectConfirmationData> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2SubjectConfirmationData"/> element.</param>
        /// <returns>An instance of <see cref="Saml2SubjectConfirmationData"/> .</returns>
        /// <remarks>
        /// The default implementation handles the unextended element 
        /// as well as the extended type saml:KeyInfoConfirmationDataType.
        /// </remarks>
        protected virtual Saml2SubjectConfirmationData ReadSubjectConfirmationData(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement(Saml2Constants.Elements.SubjectConfirmationData, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.SubjectConfirmationData, Saml2Constants.Namespace);
            }

            try
            {
                Saml2SubjectConfirmationData confirmationData = new Saml2SubjectConfirmationData();
                bool isEmpty = reader.IsEmptyElement;

                // @attributes
                string value;

                // @xsi:type
                bool requireKeyInfo = false;
                XmlQualifiedName type = XmlUtil.GetXsiType(reader);

                if (null != type)
                {
                    if (XmlUtil.EqualsQName(type, Saml2Constants.Types.KeyInfoConfirmationDataType, Saml2Constants.Namespace))
                    {
                        requireKeyInfo = true;
                    }
                    else if (!XmlUtil.EqualsQName(type, Saml2Constants.Types.SubjectConfirmationDataType, Saml2Constants.Namespace))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4112, type.Name, type.Namespace));
                    }
                }

                // KeyInfoConfirmationData cannot be empty
                if (requireKeyInfo && isEmpty)
                {
                    throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.GetString(SR.ID4111)));
                }

                // @Address - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.Address);
                if (!string.IsNullOrEmpty(value))
                {
                    confirmationData.Address = value;
                }

                // @InResponseTo - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.InResponseTo);
                if (!string.IsNullOrEmpty(value))
                {
                    confirmationData.InResponseTo = new Saml2Id(value);
                }

                // @NotBefore - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.NotBefore);
                if (!string.IsNullOrEmpty(value))
                {
                    confirmationData.NotBefore = XmlConvert.ToDateTime(value, DateTimeFormats.Accepted);
                }

                // @NotOnOrAfter - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.NotOnOrAfter);
                if (!string.IsNullOrEmpty(value))
                {
                    confirmationData.NotOnOrAfter = XmlConvert.ToDateTime(value, DateTimeFormats.Accepted);
                }

                // @Recipient - optional
                value = reader.GetAttribute(Saml2Constants.Attributes.Recipient);
                if (!string.IsNullOrEmpty(value))
                {
                    if (!UriUtil.CanCreateValidUri(value, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID0011, Saml2Constants.Attributes.Recipient, Saml2Constants.Elements.SubjectConfirmationData));
                    }

                    confirmationData.Recipient = new Uri(value);
                }

                // Contents
                reader.Read();

                if (!isEmpty)
                {
                    // <ds:KeyInfo> 0-OO OR 1-OO
                    if (requireKeyInfo)
                    {
                        confirmationData.KeyIdentifiers.Add(this.ReadSubjectKeyInfo(reader));
                    }

                    while (reader.IsStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace))
                    {
                        confirmationData.KeyIdentifiers.Add(this.ReadSubjectKeyInfo(reader));
                    }

                    // If this isn't KeyInfo restricted, there might be open content here ...
                    if (!requireKeyInfo && XmlNodeType.EndElement != reader.NodeType)
                    {
                        // So throw and tell the user how to handle the open content
                        throw DiagnosticUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4114, Saml2Constants.Elements.SubjectConfirmationData));
                    }

                    reader.ReadEndElement();
                }

                return confirmationData;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:SubjectConfirmationData> element.
        /// </summary>
        /// <remarks>
        /// When the data.KeyIdentifiers collection is not empty, an xsi:type
        /// attribute will be written specifying saml:KeyInfoConfirmationDataType.
        /// </remarks>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2SubjectConfirmationData"/>.</param>
        /// <param name="data">The <see cref="Saml2SubjectConfirmationData"/> to serialize.</param>
        protected virtual void WriteSubjectConfirmationData(XmlWriter writer, Saml2SubjectConfirmationData data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // <SubjectConfirmationData>
            writer.WriteStartElement(Saml2Constants.Elements.SubjectConfirmationData, Saml2Constants.Namespace);

            // @attributes

            // @xsi:type
            if (data.KeyIdentifiers != null && data.KeyIdentifiers.Count > 0)
            {
                writer.WriteAttributeString("type", XmlSchema.InstanceNamespace, Saml2Constants.Types.KeyInfoConfirmationDataType);
            }

            // @Address - optional
            if (!string.IsNullOrEmpty(data.Address))
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.Address, data.Address);
            }

            // @InResponseTo - optional
            if (null != data.InResponseTo)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.InResponseTo, data.InResponseTo.Value);
            }

            // @NotBefore - optional
            if (null != data.NotBefore)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.NotBefore, XmlConvert.ToString(data.NotBefore.Value.ToUniversalTime(), DateTimeFormats.Generated));
            }

            // @NotOnOrAfter - optional
            if (null != data.NotOnOrAfter)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.NotOnOrAfter, XmlConvert.ToString(data.NotOnOrAfter.Value.ToUniversalTime(), DateTimeFormats.Generated));
            }

            // @Recipient - optional
            if (null != data.Recipient)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.Recipient, data.Recipient.OriginalString);
            }

            // Content

            // <ds:KeyInfo> 0-OO
            foreach (SecurityKeyIdentifier keyIdentifier in data.KeyIdentifiers)
            {
                this.WriteSubjectKeyInfo(writer, keyIdentifier);
            }

            // </SubjectConfirmationData>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the &lt;saml:SubjectLocality> element.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/> positioned at a <see cref="Saml2SubjectLocality"/> element.</param>
        /// <returns>An instance of <see cref="Saml2SubjectLocality"/> .</returns>
        protected virtual Saml2SubjectLocality ReadSubjectLocality(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // throw if wrong element
            if (!reader.IsStartElement(Saml2Constants.Elements.SubjectLocality, Saml2Constants.Namespace))
            {
                reader.ReadStartElement(Saml2Constants.Elements.SubjectLocality, Saml2Constants.Namespace);
            }

            try
            {
                Saml2SubjectLocality subjectLocality = new Saml2SubjectLocality();
                bool isEmpty = reader.IsEmptyElement;

                // @attributes

                // @xsi:type
                XmlUtil.ValidateXsiType(reader, Saml2Constants.Types.SubjectLocalityType, Saml2Constants.Namespace);

                // @Address - optional
                subjectLocality.Address = reader.GetAttribute(Saml2Constants.Attributes.Address);

                // @DNSName - optional
                subjectLocality.DnsName = reader.GetAttribute(Saml2Constants.Attributes.DNSName);

                // Empty content
                reader.Read();
                if (!isEmpty)
                {
                    reader.ReadEndElement();
                }

                return subjectLocality;
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                Exception wrapped = TryWrapReadException(reader, e);
                if (null == wrapped)
                {
                    throw;
                }
                else
                {
                    throw wrapped;
                }
            }
        }

        /// <summary>
        /// Writes the &lt;saml:SubjectLocality> element.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="Saml2SubjectLocality"/>.</param>
        /// <param name="data">The <see cref="Saml2SubjectLocality"/> to serialize.</param>
        protected virtual void WriteSubjectLocality(XmlWriter writer, Saml2SubjectLocality data)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == data)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("data");
            }

            // <SubjectLocality>
            writer.WriteStartElement(Saml2Constants.Elements.SubjectLocality, Saml2Constants.Namespace);

            // @Address - optional
            if (null != data.Address)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.Address, data.Address);
            }

            // @DNSName - optional
            if (null != data.DnsName)
            {
                writer.WriteAttributeString(Saml2Constants.Attributes.DNSName, data.DnsName);
            }

            // </SubjectLocality>
            writer.WriteEndElement();
        }

        // This thin wrapper is used to pass a serializer down into the 
        // EnvelpoedSignatureReader that will use the Saml2AssertionSerializer's
        // ReadKeyInfo method to read the KeyInfo.
        internal class WrappedSerializer : SecurityTokenSerializer
        {
            private Saml2SecurityTokenHandler parent;
            private Saml2Assertion assertion;

            public WrappedSerializer(Saml2SecurityTokenHandler parent, Saml2Assertion assertion)
            {
                this.assertion = assertion;
                this.parent = parent;
            }

            protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
            {
                return false;
            }

            protected override bool CanReadKeyIdentifierCore(XmlReader reader)
            {
                return true;
            }

            protected override bool CanReadTokenCore(XmlReader reader)
            {
                return false;
            }

            protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return false;
            }

            protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
            {
                return false;
            }

            protected override bool CanWriteTokenCore(SecurityToken token)
            {
                return false;
            }

            protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
            {
                return this.parent.ReadSigningKeyInfo(reader, this.assertion);
            }

            protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            /// <summary>
            /// Extensibility point for providing custom serialization.
            /// </summary>
            /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="SecurityKeyIdentifierClause"/>.</param>
            /// <param name="keyIdentifierClause">The <see cref="SecurityKeyIdentifierClause"/> to serialize.</param>
            /// <remarks>This is not supported.</remarks>
            protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            /// <summary>
            /// Extensibility point for providing custom serialization.
            /// </summary>
            /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="SecurityKeyIdentifier"/>.</param>
            /// <param name="keyIdentifier">The <see cref="SecurityKeyIdentifier"/> to serialize.</param>
            protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
            {
                this.parent.WriteSigningKeyInfo(writer, keyIdentifier);
            }

            /// <summary>
            /// Extensibility point for providing custom serialization.
            /// </summary>
            /// <param name="writer">A <see cref="XmlWriter"/> to serialize the <see cref="SecurityToken"/>.</param>
            /// <param name="token">The <see cref="SecurityToken"/> to serialize.</param>
            protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        /// <summary>
        /// When encrypted SAML 2.0 token is received, the credentials that are used
        /// to encrypt the token will be hydrated as a ReceivedEncryptingCredentials.
        /// This is to distinguish the case between a user explicitly setting an 
        /// encrypting credentials and a re-serialize case where a received token
        /// is re-serialized by a proxy to a backend service, in which case the token 
        /// should not be encrypted.
        /// </summary>
        internal class ReceivedEncryptingCredentials : EncryptingCredentials
        {
            /// <summary>
            /// Constructs an ReceivedEncryptingCredentials with a security key, a security key identifier and
            /// the encryption algorithm.
            /// </summary>
            /// <param name="key">A security key for encryption.</param>
            /// <param name="keyIdentifier">A security key identifier for the encryption key.</param>
            /// <param name="algorithm">The encryption algorithm.</param>
            public ReceivedEncryptingCredentials(SecurityKey key, SecurityKeyIdentifier keyIdentifier, string algorithm)
                : base(key, keyIdentifier, algorithm)
            {
            }
        }
    }
}
