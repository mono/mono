//-----------------------------------------------------------------------
// <copyright file="X509SecurityTokenHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel.Security;
    using System.Xml;
    using Claim = System.Security.Claims.Claim;

    /// <summary>
    /// SecurityTokenHandler for X509SecurityToken. By default, the
    /// handler will do chain-trust validation of the Certificate.
    /// </summary>
    public class X509SecurityTokenHandler : SecurityTokenHandler
    {
        //
        // The below defaults will only be used if some verification properties are set in config and others are not
        //
        private static X509RevocationMode defaultRevocationMode = X509RevocationMode.Online;
        private static X509CertificateValidationMode defaultValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
        private static StoreLocation defaultStoreLocation = StoreLocation.LocalMachine;
        private X509NTAuthChainTrustValidator x509NTAuthChainTrustValidator;
        object lockObject = new object();

        private bool mapToWindows;
        private X509CertificateValidator certificateValidator;
        private bool writeXmlDSigDefinedClauseTypes;

        private X509DataSecurityKeyIdentifierClauseSerializer x509DataKeyIdentifierClauseSerializer = new X509DataSecurityKeyIdentifierClauseSerializer();

        /// <summary>
        /// Creates an instance of <see cref="X509SecurityTokenHandler"/>. MapToWindows is defaulted to false.
        /// Uses <see cref="X509CertificateValidator.PeerOrChainTrust"/> as the default certificate validator.
        /// </summary>
        public X509SecurityTokenHandler()
            : this(false, null)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="X509SecurityTokenHandler"/> with an X509 certificate validator.
        /// MapToWindows is to false by default.
        /// </summary>
        /// <param name="certificateValidator">The certificate validator.</param>
        public X509SecurityTokenHandler(X509CertificateValidator certificateValidator)
            : this(false, certificateValidator)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="X509SecurityTokenHandler"/>. Uses <see cref="X509CertificateValidator.PeerOrChainTrust"/> 
        /// as the default certificate validator.
        /// </summary>
        /// <param name="mapToWindows">Boolean to indicate if the certificate should be mapped to a 
        /// windows account. Default is false.</param>
        public X509SecurityTokenHandler(bool mapToWindows)
            : this(mapToWindows, null)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="X509SecurityTokenHandler"/>.
        /// </summary>
        /// <param name="mapToWindows">Boolean to indicate if the certificate should be mapped to a windows account.</param>
        /// <param name="certificateValidator">The certificate validator.</param>
        public X509SecurityTokenHandler(bool mapToWindows, X509CertificateValidator certificateValidator)
        {
            this.mapToWindows = mapToWindows;
            this.certificateValidator = certificateValidator;
        }

        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="customConfigElements">XmlElement to custom configuration.</param>
        /// <exception cref="ArgumentNullException">The param 'customConfigElements' is null.</exception>
        /// <exception cref="InvalidOperationException">Custom configuration specified was invalid.</exception>
        public override void LoadCustomConfiguration(XmlNodeList customConfigElements)
        {
            if (customConfigElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("customConfigElements");
            }

            List<XmlElement> configNodes = XmlUtil.GetXmlElements(customConfigElements);

            bool foundValidConfig = false;

            bool foundCustomX509Validator = false;
            X509RevocationMode revocationMode = defaultRevocationMode;
            X509CertificateValidationMode certificateValidationMode = defaultValidationMode;
            StoreLocation trustedStoreLocation = defaultStoreLocation;
            string customValidator = null;

            foreach (XmlElement customConfigElement in configNodes)
            {
                if (!StringComparer.Ordinal.Equals(customConfigElement.LocalName, ConfigurationStrings.X509SecurityTokenHandlerRequirement))
                {
                    continue;
                }

                if (foundValidConfig)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7026, ConfigurationStrings.X509SecurityTokenHandlerRequirement));
                }

                foreach (XmlAttribute attribute in customConfigElement.Attributes)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.MapToWindows))
                    {
                        mapToWindows = XmlConvert.ToBoolean(attribute.Value.ToLowerInvariant());
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.X509CertificateValidator))
                    {
                        customValidator = attribute.Value.ToString();
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.X509CertificateRevocationMode))
                    {
                        foundCustomX509Validator = true;

                        string revocationModeString = attribute.Value.ToString();

                        if (StringComparer.OrdinalIgnoreCase.Equals(revocationModeString, ConfigurationStrings.X509RevocationModeNoCheck))
                        {
                            revocationMode = X509RevocationMode.NoCheck;
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(revocationModeString, ConfigurationStrings.X509RevocationModeOffline))
                        {
                            revocationMode = X509RevocationMode.Offline;
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(revocationModeString, ConfigurationStrings.X509RevocationModeOnline))
                        {
                            revocationMode = X509RevocationMode.Online;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7011, attribute.LocalName, customConfigElement.LocalName)));
                        }
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.X509CertificateValidationMode))
                    {
                        foundCustomX509Validator = true;

                        string validationModeString = attribute.Value.ToString();

                        if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModeChainTrust))
                        {
                            certificateValidationMode = X509CertificateValidationMode.ChainTrust;
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModePeerOrChainTrust))
                        {
                            certificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModePeerTrust))
                        {
                            certificateValidationMode = X509CertificateValidationMode.PeerTrust;
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModeNone))
                        {
                            certificateValidationMode = X509CertificateValidationMode.None;
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(validationModeString, ConfigurationStrings.X509CertificateValidationModeCustom))
                        {
                            certificateValidationMode = X509CertificateValidationMode.Custom;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7011, attribute.LocalName, customConfigElement.LocalName)));
                        }
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.X509TrustedStoreLocation))
                    {
                        foundCustomX509Validator = true;

                        string trustedStoreLocationString = attribute.Value.ToString();

                        if (StringComparer.OrdinalIgnoreCase.Equals(trustedStoreLocationString, ConfigurationStrings.X509TrustedStoreLocationCurrentUser))
                        {
                            trustedStoreLocation = StoreLocation.CurrentUser;
                        }
                        else if (StringComparer.OrdinalIgnoreCase.Equals(trustedStoreLocationString, ConfigurationStrings.X509TrustedStoreLocationLocalMachine))
                        {
                            trustedStoreLocation = StoreLocation.LocalMachine;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7011, attribute.LocalName, customConfigElement.LocalName)));
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7004, attribute.LocalName, customConfigElement.LocalName)));
                    }
                }

                foundValidConfig = true;
            }

            if (certificateValidationMode == X509CertificateValidationMode.Custom)
            {
                if (String.IsNullOrEmpty(customValidator))
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7028));
                }

                Type customValidatorType = Type.GetType(customValidator, true);

                if (customValidatorType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID7007, customValidatorType));
                }

                certificateValidator = CustomTypeElement.Resolve<X509CertificateValidator>(new CustomTypeElement(customValidatorType));
            }
            else if (foundCustomX509Validator)
            {
                certificateValidator = X509Util.CreateCertificateValidator(certificateValidationMode, revocationMode, trustedStoreLocation);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether if the validating token should be mapped to a 
        /// Windows account.
        /// </summary>
        public bool MapToWindows
        {
            get { return mapToWindows; }
            set { mapToWindows = value; }
        }

        /// <summary>
        /// Gets or sets the X509CeritificateValidator that is used by the current instance.
        /// </summary>
        public X509CertificateValidator CertificateValidator
        {
            get
            {
                if (certificateValidator == null)
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
                    return certificateValidator;
                }
            }

            set
            {
                certificateValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets the X509NTAuthChainTrustValidator that is used by the current instance during certificate validation when the incoming certificate is mapped to windows.
        /// </summary>
        public X509NTAuthChainTrustValidator X509NTAuthChainTrustValidator
        {
            get
            {
                return this.x509NTAuthChainTrustValidator;
            }

            set
            {
                this.x509NTAuthChainTrustValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether XmlDsig defined clause types are 
        /// preferred. Supported XmlDSig defined SecurityKeyIdentifierClause types
        /// are,
        /// 1. X509IssuerSerial
        /// 2. X509SKI
        /// 3. X509Certificate
        /// </summary>
        public bool WriteXmlDSigDefinedClauseTypes
        {
            get { return writeXmlDSigDefinedClauseTypes; }
            set { writeXmlDSigDefinedClauseTypes = value; }
        }

        /// <summary>
        /// Gets a boolean indicating if the handler can validate tokens. 
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

            return x509DataKeyIdentifierClauseSerializer.CanReadKeyIdentifierClause(reader);
        }

        /// <summary>
        /// Checks if the reader points to a X.509 Security Token as defined in WS-Security.
        /// </summary>
        /// <param name="reader">Reader pointing to the token XML.</param>
        /// <returns>Returns true if the element is pointing to a X.509 Security Token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'reader' is null.</exception>
        public override bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (reader.IsStartElement(WSSecurity10Constants.Elements.BinarySecurityToken, WSSecurity10Constants.Namespace))
            {
                string valueTypeUri = reader.GetAttribute(WSSecurity10Constants.Attributes.ValueType, null);
                return StringComparer.Ordinal.Equals(valueTypeUri, WSSecurity10Constants.X509TokenType);
            }

            return false;
        }

        /// <summary>
        /// Checks if the given SecurityKeyIdentifierClause can be serialized by this handler. The
        /// supported SecurityKeyIdentifierClause are,
        /// 1. <see cref="System.IdentityModel.Tokens.X509IssuerSerialKeyIdentifierClause"/>
        /// 2. <see cref="System.IdentityModel.Tokens.X509RawDataKeyIdentifierClause"/>
        /// 3. <see cref="System.IdentityModel.Tokens.X509SubjectKeyIdentifierClause"/>
        /// </summary>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to be serialized.</param>
        /// <returns>True if the 'securityKeyIdentifierClause' is supported and if WriteXmlDSigDefinedClausTypes
        /// is set to true.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'securityKeyIdentifierClause' is null.</exception>
        public override bool CanWriteKeyIdentifierClause(SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            if (securityKeyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityKeyIdentifierClause");
            }

            return writeXmlDSigDefinedClauseTypes && x509DataKeyIdentifierClauseSerializer.CanWriteKeyIdentifierClause(securityKeyIdentifierClause);
        }

        /// <summary>
        /// Gets X509SecurityToken type.
        /// </summary>
        public override Type TokenType
        {
            get { return typeof(X509SecurityToken); }
        }

        /// <summary>
        /// Deserializes a SecurityKeyIdentifierClause referenced by the XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader referencing the SecurityKeyIdentifierClause.</param>
        /// <returns>Instance of SecurityKeyIdentifierClause.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        public override SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return x509DataKeyIdentifierClauseSerializer.ReadKeyIdentifierClause(reader);
        }

        /// <summary>
        /// Reads the X.509 Security token referenced by the XmlReader.
        /// </summary>
        /// <param name="reader">XmlReader pointing to a X.509 Security token.</param>
        /// <returns>An instance of <see cref="X509SecurityToken"/>.</returns> 
        /// <exception cref="ArgumentNullException">The parameter 'reader' is null.</exception>
        /// <exception cref="XmlException">XmlReader is not pointing to an valid X509SecurityToken as
        /// defined in WS-Security X.509 Token Profile. Or the encodingType specified is other than Base64 
        /// or HexBinary.</exception>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            XmlDictionaryReader dicReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            if (!dicReader.IsStartElement(WSSecurity10Constants.Elements.BinarySecurityToken, WSSecurity10Constants.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(
                        SR.GetString(
                        SR.ID4065,
                        WSSecurity10Constants.Elements.BinarySecurityToken,
                        WSSecurity10Constants.Namespace,
                        dicReader.LocalName,
                        dicReader.NamespaceURI)));
            }

            string valueTypeUri = dicReader.GetAttribute(WSSecurity10Constants.Attributes.ValueType, null);

            if (!StringComparer.Ordinal.Equals(valueTypeUri, WSSecurity10Constants.X509TokenType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(
                        SR.GetString(
                        SR.ID4066,
                        WSSecurity10Constants.Elements.BinarySecurityToken,
                        WSSecurity10Constants.Namespace,
                        WSSecurity10Constants.Attributes.ValueType,
                        WSSecurity10Constants.X509TokenType,
                        valueTypeUri)));
            }

            string wsuId = dicReader.GetAttribute(WSSecurityUtilityConstants.Attributes.Id, WSSecurityUtilityConstants.Namespace);
            string encoding = dicReader.GetAttribute(WSSecurity10Constants.Attributes.EncodingType, null);

            byte[] binaryData;
            if (encoding == null || StringComparer.Ordinal.Equals(encoding, WSSecurity10Constants.Base64EncodingType))
            {
                binaryData = dicReader.ReadElementContentAsBase64();
            }
            else if (StringComparer.Ordinal.Equals(encoding, WSSecurity10Constants.HexBinaryEncodingType))
            {
                binaryData = SoapHexBinary.Parse(dicReader.ReadElementContentAsString()).Value;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ID4068)));
            }

            return String.IsNullOrEmpty(wsuId) ?
                new X509SecurityToken(new X509Certificate2(binaryData)) :
                new X509SecurityToken(new X509Certificate2(binaryData), wsuId);
        }

        /// <summary>
        /// Gets the X.509 Security Token Type defined in WS-Security X.509 Token profile.
        /// </summary>
        /// <returns>The token type identifier.</returns>
        public override string[] GetTokenTypeIdentifiers()
        {
            return new string[] { SecurityTokenTypes.X509Certificate };
        }

        /// <summary>
        /// Validates an <see cref="X509SecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="X509SecurityToken"/> to validate.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not assignable from <see cref="X509SecurityToken"/>.</exception>
        /// <exception cref="InvalidOperationException">Configuration <see cref="SecurityTokenHandlerConfiguration"/>is null.</exception>
        /// <exception cref="SecurityTokenValidationException">The current <see cref="X509CertificateValidator"/> was unable to validate the certificate in the Token.</exception>
        /// <exception cref="InvalidOperationException">Configuration.IssuerNameRegistry is null.</exception>
        /// <exception cref="SecurityTokenException">Configuration.IssuerNameRegistry return null when resolving the issuer of the certificate in the Token.</exception>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            X509SecurityToken x509Token = token as X509SecurityToken;
            if (x509Token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID0018, typeof(X509SecurityToken)));
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            try
            {
                // Validate the token.
                try
                {
                    this.CertificateValidator.Validate(x509Token.Certificate);
                }
                catch (SecurityTokenValidationException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4257,
                        X509Util.GetCertificateId(x509Token.Certificate)), e));
                }

                if (this.Configuration.IssuerNameRegistry == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4277));
                }

                string issuer = X509Util.GetCertificateIssuerName(x509Token.Certificate, this.Configuration.IssuerNameRegistry);
                if (String.IsNullOrEmpty(issuer))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4175)));
                }

                ClaimsIdentity identity = null;

                if (!mapToWindows)
                {
                    identity = new ClaimsIdentity(AuthenticationTypes.X509);

                    // PARTIAL TRUST: will fail when adding claims, AddClaim is SecurityCritical.
                    identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.X509));
                }
                else
                {
                    WindowsIdentity windowsIdentity;
                    X509WindowsSecurityToken x509WindowsSecurityToken = token as X509WindowsSecurityToken;

                    // if this is the case, then the user has already been mapped to a windows account, just return the identity after adding a couple of claims.
                    if (x509WindowsSecurityToken != null && x509WindowsSecurityToken.WindowsIdentity != null)
                    {
                        // X509WindowsSecurityToken is disposable, make a copy.
                        windowsIdentity = new WindowsIdentity(x509WindowsSecurityToken.WindowsIdentity.Token, x509WindowsSecurityToken.AuthenticationType);
                    }
                    else
                    {
                        // Ensure NT_AUTH chain policy for certificate account mapping
                        if (this.x509NTAuthChainTrustValidator == null)
                        {
                            lock (this.lockObject)
                            {
                                if (this.x509NTAuthChainTrustValidator == null)
                                {
                                    this.x509NTAuthChainTrustValidator = new X509NTAuthChainTrustValidator();
                                }
                            }
                        }

                        this.x509NTAuthChainTrustValidator.Validate(x509Token.Certificate);
                        windowsIdentity = ClaimsHelper.CertificateLogon(x509Token.Certificate);
                    }

                    // PARTIAL TRUST: will fail when adding claims, AddClaim is SecurityCritical.
                    windowsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.X509));
                    identity = windowsIdentity;
                }

                if (this.Configuration.SaveBootstrapContext)
                {
                    identity.BootstrapContext = new BootstrapContext(token, this);
                }

                identity.AddClaim(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated), ClaimValueTypes.DateTime));
                identity.AddClaims(X509Util.GetClaimsFromCertificate(x509Token.Certificate, issuer));

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
        /// Serializes a given SecurityKeyIdentifierClause to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to which the 'securityKeyIdentifierClause' should be serialized.</param>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to serialize.</param>
        /// <exception cref="ArgumentNullException">Input parameter 'wrtier' or 'securityKeyIdentifierClause' is null.</exception>
        /// <exception cref="InvalidOperationException">The property WriteXmlDSigDefinedClauseTypes is false.</exception>
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

            if (!writeXmlDSigDefinedClauseTypes)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4261));
            }

            x509DataKeyIdentifierClauseSerializer.WriteKeyIdentifierClause(writer, securityKeyIdentifierClause);
        }

        /// <summary>
        /// Writes the X509SecurityToken to the given XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to write the token into.</param>
        /// <param name="token">The SecurityToken of type X509SecurityToken to be written.</param>
        /// <exception cref="ArgumentNullException">The parameter 'writer' or 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not of type X509SecurityToken.</exception>
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

            X509SecurityToken x509Token = token as X509SecurityToken;
            if (x509Token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID0018, typeof(X509SecurityToken)));
            }

            writer.WriteStartElement(WSSecurity10Constants.Elements.BinarySecurityToken, WSSecurity10Constants.Namespace);
            if (!String.IsNullOrEmpty(x509Token.Id))
            {
                writer.WriteAttributeString(WSSecurityUtilityConstants.Attributes.Id, WSSecurityUtilityConstants.Namespace, x509Token.Id);
            }

            writer.WriteAttributeString(WSSecurity10Constants.Attributes.ValueType, null, WSSecurity10Constants.X509TokenType);
            writer.WriteAttributeString(WSSecurity10Constants.Attributes.EncodingType, WSSecurity10Constants.Base64EncodingType);

            byte[] rawData = x509Token.Certificate.GetRawCertData();
            writer.WriteBase64(rawData, 0, rawData.Length);
            writer.WriteEndElement();
        }

        internal static WindowsIdentity KerberosCertificateLogon(X509Certificate2 certificate)
        {
            return X509SecurityTokenAuthenticator.KerberosCertificateLogon(certificate);
        }
    }
}
