//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    public class IssuedSecurityTokenParameters : SecurityTokenParameters
    {
        const string wsidPrefix = "wsid";
        const string wsidNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";
        static readonly string wsidPPIClaim = String.Format(CultureInfo.InvariantCulture, "{0}/claims/privatepersonalidentifier", wsidNamespace);
        internal const SecurityKeyType defaultKeyType = SecurityKeyType.SymmetricKey;
        internal const bool defaultUseStrTransform = false;

        internal struct AlternativeIssuerEndpoint
        {
            public EndpointAddress IssuerAddress;
            public EndpointAddress IssuerMetadataAddress;
            public Binding IssuerBinding;
        }

        Collection<XmlElement> additionalRequestParameters = new Collection<XmlElement>();
        Collection<AlternativeIssuerEndpoint> alternativeIssuerEndpoints = new Collection<AlternativeIssuerEndpoint>();
        MessageSecurityVersion defaultMessageSecurityVersion;
        EndpointAddress issuerAddress;
        EndpointAddress issuerMetadataAddress;
        Binding issuerBinding;
        int keySize;
        SecurityKeyType keyType = defaultKeyType;
        Collection<ClaimTypeRequirement> claimTypeRequirements = new Collection<ClaimTypeRequirement>();
        bool useStrTransform = defaultUseStrTransform;
        string tokenType;

        protected IssuedSecurityTokenParameters(IssuedSecurityTokenParameters other)
            : base(other)
        {
            this.defaultMessageSecurityVersion = other.defaultMessageSecurityVersion;
            this.issuerAddress = other.issuerAddress;
            this.keyType = other.keyType;
            this.tokenType = other.tokenType;
            this.keySize = other.keySize;
            this.useStrTransform = other.useStrTransform;

            foreach (XmlElement parameter in other.additionalRequestParameters)
            {
                this.additionalRequestParameters.Add((XmlElement)parameter.Clone());
            }
            foreach (ClaimTypeRequirement c in other.claimTypeRequirements)
            {
                this.claimTypeRequirements.Add(c);
            }
            if (other.issuerBinding != null)
            {
                this.issuerBinding = new CustomBinding(other.issuerBinding);
            }
            this.issuerMetadataAddress = other.issuerMetadataAddress;
        }

        public IssuedSecurityTokenParameters()
            : this(null, null, null)
        {
            // empty
        }

        public IssuedSecurityTokenParameters(string tokenType)
            : this(tokenType, null, null)
        {
            // empty
        }

        public IssuedSecurityTokenParameters(string tokenType, EndpointAddress issuerAddress)
            : this(tokenType, issuerAddress, null)
        {
            // empty
        }

        public IssuedSecurityTokenParameters(string tokenType, EndpointAddress issuerAddress, Binding issuerBinding)
            : base()
        {
            this.tokenType = tokenType;
            this.issuerAddress = issuerAddress;
            this.issuerBinding = issuerBinding;
        }

        internal protected override bool HasAsymmetricKey { get { return this.KeyType == SecurityKeyType.AsymmetricKey; } }

        public Collection<XmlElement> AdditionalRequestParameters
        {
            get
            {
                return this.additionalRequestParameters;
            }
        }

        public MessageSecurityVersion DefaultMessageSecurityVersion
        {
            get
            {
                return this.defaultMessageSecurityVersion;
            }

            set
            {
                defaultMessageSecurityVersion = value;
            }
        }

        internal Collection<AlternativeIssuerEndpoint> AlternativeIssuerEndpoints
        {
            get
            {
                return this.alternativeIssuerEndpoints;
            }
        }

        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.issuerAddress;
            }
            set
            {
                this.issuerAddress = value;
            }
        }

        public EndpointAddress IssuerMetadataAddress
        {
            get
            {
                return this.issuerMetadataAddress;
            }
            set
            {
                this.issuerMetadataAddress = value;
            }
        }

        public Binding IssuerBinding
        {
            get
            {
                return this.issuerBinding;
            }
            set
            {
                this.issuerBinding = value;
            }
        }

        public SecurityKeyType KeyType
        {
            get
            {
                return this.keyType;
            }
            set
            {
                SecurityKeyTypeHelper.Validate(value);
                this.keyType = value;
            }
        }

        public int KeySize
        {
            get
            {
                return this.keySize;
            }
            set
            {
                if (value < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeNonNegative)));
                this.keySize = value;
            }
        }

        public bool UseStrTransform
        {
            get
            {
                return this.useStrTransform;
            }
            set
            {
                this.useStrTransform = value;
            }
        }

        public Collection<ClaimTypeRequirement> ClaimTypeRequirements
        {
            get
            {
                return this.claimTypeRequirements;
            }
        }

        public string TokenType
        {
            get
            {
                return this.tokenType;
            }
            set
            {
                this.tokenType = value;
            }
        }

        internal protected override bool SupportsClientAuthentication { get { return true; } }
        internal protected override bool SupportsServerAuthentication { get { return true; } }
        internal protected override bool SupportsClientWindowsIdentity { get { return false; } }

        protected override SecurityTokenParameters CloneCore()
        {
            return new IssuedSecurityTokenParameters(this);
        }

        internal protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (token is GenericXmlSecurityToken)
                return base.CreateGenericXmlTokenKeyIdentifierClause(token, referenceStyle);
            else
                return this.CreateKeyIdentifierClause<SamlAssertionKeyIdentifierClause, SamlAssertionKeyIdentifierClause>(token, referenceStyle);
        }

        internal void SetRequestParameters(Collection<XmlElement> requestParameters, TrustDriver trustDriver)
        {
            if (requestParameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestParameters");

            if (trustDriver == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustDriver");

            Collection<XmlElement> unknownRequestParameters = new Collection<XmlElement>();

            foreach (XmlElement element in requestParameters)
            {
                int keySize;
                string tokenType;
                SecurityKeyType keyType;
                Collection<XmlElement> requiredClaims;
                if (trustDriver.TryParseKeySizeElement(element, out keySize))
                    this.keySize = keySize;
                else if (trustDriver.TryParseKeyTypeElement(element, out keyType))
                    this.KeyType = keyType;
                else if (trustDriver.TryParseTokenTypeElement(element, out tokenType))
                    this.TokenType = tokenType;
                // Only copy RP policy to client policy for TrustFeb2005
                else if (trustDriver.StandardsManager.TrustVersion == TrustVersion.WSTrustFeb2005)
                {
                    if (trustDriver.TryParseRequiredClaimsElement(element, out requiredClaims))
                    {
                        Collection<XmlElement> unrecognizedRequiredClaims = new Collection<XmlElement>();
                        foreach (XmlElement claimRequirement in requiredClaims)
                        {
                            if (claimRequirement.LocalName == "ClaimType" && claimRequirement.NamespaceURI == wsidNamespace)
                            {
                                string claimValue = claimRequirement.GetAttribute("Uri", string.Empty);
                                if (!string.IsNullOrEmpty(claimValue))
                                {
                                    ClaimTypeRequirement claimTypeRequirement;
                                    string optional = claimRequirement.GetAttribute("Optional", string.Empty);
                                    if (String.IsNullOrEmpty(optional))
                                    {
                                        claimTypeRequirement = new ClaimTypeRequirement(claimValue);
                                    }
                                    else
                                    {
                                        claimTypeRequirement = new ClaimTypeRequirement(claimValue, XmlConvert.ToBoolean(optional));
                                    }

                                    this.claimTypeRequirements.Add(claimTypeRequirement);
                                }
                            }
                            else
                            {
                                unrecognizedRequiredClaims.Add(claimRequirement);
                            }
                        }
                        if (unrecognizedRequiredClaims.Count > 0)
                            unknownRequestParameters.Add(trustDriver.CreateRequiredClaimsElement(unrecognizedRequiredClaims));
                    }
                    else
                    {
                        unknownRequestParameters.Add(element);
                    }
                }
            }

            unknownRequestParameters = trustDriver.ProcessUnknownRequestParameters(unknownRequestParameters, requestParameters);
            if (unknownRequestParameters.Count > 0)
            {
                for (int i = 0; i < unknownRequestParameters.Count; ++i)
                    this.AdditionalRequestParameters.Add(unknownRequestParameters[i]);
            }
        }

        public Collection<XmlElement> CreateRequestParameters(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
        {
            return CreateRequestParameters(SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer).TrustDriver);
        }

        internal Collection<XmlElement> CreateRequestParameters(TrustDriver driver)
        {
            if (driver == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("driver");

            Collection<XmlElement> result = new Collection<XmlElement>();

            if (this.tokenType != null)
            {
                result.Add(driver.CreateTokenTypeElement(tokenType));
            }

            result.Add(driver.CreateKeyTypeElement(this.keyType));

            if (this.keySize != 0)
            {
                result.Add(driver.CreateKeySizeElement(keySize));
            }
            if (this.claimTypeRequirements.Count > 0)
            {
                Collection<XmlElement> claimsElements = new Collection<XmlElement>();
                XmlDocument doc = new XmlDocument();
                foreach (ClaimTypeRequirement claimType in this.claimTypeRequirements)
                {
                    XmlElement element = doc.CreateElement(wsidPrefix, "ClaimType", wsidNamespace);
                    XmlAttribute attr = doc.CreateAttribute("Uri");
                    attr.Value = claimType.ClaimType;
                    element.Attributes.Append(attr);
                    if (claimType.IsOptional != ClaimTypeRequirement.DefaultIsOptional)
                    {
                        attr = doc.CreateAttribute("Optional");
                        attr.Value = XmlConvert.ToString(claimType.IsOptional);
                        element.Attributes.Append(attr);
                    }
                    claimsElements.Add(element);
                }
                result.Add(driver.CreateRequiredClaimsElement(claimsElements));
            }

            if (this.additionalRequestParameters.Count > 0)
            {
                Collection<XmlElement> trustNormalizedParameters = NormalizeAdditionalParameters(this.additionalRequestParameters,
                                                                                                 driver,
                                                                                                 (this.claimTypeRequirements.Count > 0));

                foreach (XmlElement parameter in trustNormalizedParameters)
                {
                    result.Add(parameter);
                }
            }

            return result;
        }

        private Collection<XmlElement> NormalizeAdditionalParameters(Collection<XmlElement> additionalParameters,
                                                                     TrustDriver driver,
                                                                     bool clientSideClaimTypeRequirementsSpecified)
        {
            // Ensure STS trust version is one of the currently supported versions: Feb 05 / Trust 1.3
            Fx.Assert(((driver.StandardsManager.TrustVersion == TrustVersion.WSTrustFeb2005) ||
                                           (driver.StandardsManager.TrustVersion == TrustVersion.WSTrust13)),
                                           "Unsupported trust version specified for the STS.");

            // We have a mismatch. Make a local copy of additionalParameters for making any potential modifications 
            // as part of normalization
            Collection<XmlElement> tmpCollection = new Collection<XmlElement>();
            foreach (XmlElement e in additionalParameters)
            {
                tmpCollection.Add(e);
            }


            // 1. For Trust 1.3 EncryptionAlgorithm, CanonicalizationAlgorithm and KeyWrapAlgorithm should not be
            //    specified as top-level element if "SecondaryParameters" element already specifies this.
            if (driver.StandardsManager.TrustVersion == TrustVersion.WSTrust13)
            {
                Fx.Assert(driver.GetType() == typeof(WSTrustDec2005.DriverDec2005), "Invalid Trust Driver specified for Trust 1.3.");

                XmlElement encryptionAlgorithmElement = null;
                XmlElement canonicalizationAlgorithmElement = null;
                XmlElement keyWrapAlgorithmElement = null;
                XmlElement secondaryParameter = null;

                for (int i = 0; i < tmpCollection.Count; ++i)
                {
                    string algorithm;

                    if (driver.IsEncryptionAlgorithmElement(tmpCollection[i], out algorithm))
                    {
                        encryptionAlgorithmElement = tmpCollection[i];
                    }
                    else if (driver.IsCanonicalizationAlgorithmElement(tmpCollection[i], out algorithm))
                    {
                        canonicalizationAlgorithmElement = tmpCollection[i];
                    }
                    else if (driver.IsKeyWrapAlgorithmElement(tmpCollection[i], out algorithm))
                    {
                        keyWrapAlgorithmElement = tmpCollection[i];
                    }
                    else if (((WSTrustDec2005.DriverDec2005)driver).IsSecondaryParametersElement(tmpCollection[i]))
                    {
                        secondaryParameter = tmpCollection[i];
                    }
                }

                if (secondaryParameter != null)
                {
                    foreach (XmlNode node in secondaryParameter.ChildNodes)
                    {
                        XmlElement child = node as XmlElement;
                        if (child != null)
                        {
                            string algorithm = null;

                            if (driver.IsEncryptionAlgorithmElement(child, out algorithm) && (encryptionAlgorithmElement != null))
                            {
                                tmpCollection.Remove(encryptionAlgorithmElement);
                            }
                            else if (driver.IsCanonicalizationAlgorithmElement(child, out algorithm) && (canonicalizationAlgorithmElement != null))
                            {
                                tmpCollection.Remove(canonicalizationAlgorithmElement);
                            }
                            else if (driver.IsKeyWrapAlgorithmElement(child, out algorithm) && (keyWrapAlgorithmElement != null))
                            {
                                tmpCollection.Remove(keyWrapAlgorithmElement);
                            }
                        }
                    }
                }
            }

            // 2. Check for Mismatch.
            //      a. Trust Feb 2005 -> Trust 1.3. do the following,
            //          (i) Copy EncryptionAlgorithm and CanonicalizationAlgorithm as the top-level elements.
            //              Note, this is in contradiction to step 1. But we don't have a choice here as we cannot say from the 
            //              Additional Parameters section in the config what came from the service and what came from the client.
            //          (ii) Convert SignWith and EncryptWith elements to Trust 1.3 namespace.
            //      b. For Trust 1.3 -> Trust Feb 2005, do the following,
            //          (i) Find EncryptionAlgorithm, CanonicalizationAlgorithm from inside the "SecondaryParameters" element. 
            //              If found, then promote these as the top-level elements replacing the existing values.
            //          (ii) Convert the SignWith and EncryptWith elements to the Trust Feb 2005 namespace and drop the KeyWrapAlgorithm
            //               element.

            // make an optimistic check to detect mismatched trust-versions between STS and RP
            bool mismatch = (((driver.StandardsManager.TrustVersion == TrustVersion.WSTrustFeb2005) &&
                              !CollectionContainsElementsWithTrustNamespace(additionalParameters, TrustFeb2005Strings.Namespace)) ||
                             ((driver.StandardsManager.TrustVersion == TrustVersion.WSTrust13) &&
                              !CollectionContainsElementsWithTrustNamespace(additionalParameters, TrustDec2005Strings.Namespace)));
            // if no mismatch, return unmodified collection
            if (!mismatch)
            {
                return tmpCollection;
            }

            // 2.a
            // If we are talking to a Trust 1.3 STS, replace any Feb '05 algorithm parameters with their Trust 1.3 counterparts
            if (driver.StandardsManager.TrustVersion == TrustVersion.WSTrust13)
            {
                SecurityStandardsManager trustFeb2005StandardsManager = SecurityStandardsManager.DefaultInstance;
                // the following cast is guaranteed to succeed
                WSTrustFeb2005.DriverFeb2005 trustFeb2005Driver = (WSTrustFeb2005.DriverFeb2005)trustFeb2005StandardsManager.TrustDriver;

                for (int i = 0; i < tmpCollection.Count; i++)
                {
                    string algorithmParameter = string.Empty;

                    if (trustFeb2005Driver.IsSignWithElement(tmpCollection[i], out algorithmParameter))
                    {
                        tmpCollection[i] = driver.CreateSignWithElement(algorithmParameter);
                    }
                    else if (trustFeb2005Driver.IsEncryptWithElement(tmpCollection[i], out algorithmParameter))
                    {
                        tmpCollection[i] = driver.CreateEncryptWithElement(algorithmParameter);
                    }
                    else if (trustFeb2005Driver.IsEncryptionAlgorithmElement(tmpCollection[i], out algorithmParameter))
                    {
                        tmpCollection[i] = driver.CreateEncryptionAlgorithmElement(algorithmParameter);
                    }
                    else if (trustFeb2005Driver.IsCanonicalizationAlgorithmElement(tmpCollection[i], out algorithmParameter))
                    {
                        tmpCollection[i] = driver.CreateCanonicalizationAlgorithmElement(algorithmParameter);
                    }
                }
            }
            else
            {
                // 2.b
                // We are talking to a Feb 05 STS. Filter out any SecondaryParameters element.
                Collection<XmlElement> childrenToPromote = null;
                WSSecurityTokenSerializer trust13Serializer = new WSSecurityTokenSerializer(SecurityVersion.WSSecurity11,
                                                                                            TrustVersion.WSTrust13,
                                                                                            SecureConversationVersion.WSSecureConversation13,
                                                                                            true, null, null, null);
                SecurityStandardsManager trust13StandardsManager = new SecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12, trust13Serializer);
                // the following cast is guaranteed to succeed
                WSTrustDec2005.DriverDec2005 trust13Driver = (WSTrustDec2005.DriverDec2005)trust13StandardsManager.TrustDriver;

                foreach (XmlElement parameter in tmpCollection)
                {
                    // check if SecondaryParameters is present
                    if (trust13Driver.IsSecondaryParametersElement(parameter))
                    {
                        childrenToPromote = new Collection<XmlElement>();
                        // walk SecondaryParameters and collect any 'non-standard' children
                        foreach (XmlNode innerNode in parameter.ChildNodes)
                        {
                            XmlElement innerElement = innerNode as XmlElement;
                            if ((innerElement != null) && CanPromoteToRoot(innerElement, trust13Driver, clientSideClaimTypeRequirementsSpecified))
                            {
                                childrenToPromote.Add(innerElement);
                            }
                        }

                        // remove SecondaryParameters element
                        tmpCollection.Remove(parameter);

                        // we are done - break out of the loop
                        break;
                    }
                }

                // Probe of standard Trust elements and remember them.
                if ((childrenToPromote != null) && (childrenToPromote.Count > 0))
                {
                    XmlElement encryptionElement = null;
                    string encryptionAlgorithm = String.Empty;

                    XmlElement canonicalizationElement = null;
                    string canonicalizationAlgoritm = String.Empty;

                    XmlElement requiredClaimsElement = null;
                    Collection<XmlElement> requiredClaims = null;

                    Collection<XmlElement> processedElements = new Collection<XmlElement>();

                    foreach (XmlElement e in childrenToPromote)
                    {
                        if ((encryptionElement == null) && trust13Driver.IsEncryptionAlgorithmElement(e, out encryptionAlgorithm))
                        {
                            encryptionElement = driver.CreateEncryptionAlgorithmElement(encryptionAlgorithm);
                            processedElements.Add(e);
                        }
                        else if ((canonicalizationElement == null) && trust13Driver.IsCanonicalizationAlgorithmElement(e, out canonicalizationAlgoritm))
                        {
                            canonicalizationElement = driver.CreateCanonicalizationAlgorithmElement(canonicalizationAlgoritm);
                            processedElements.Add(e);
                        }
                        else if ((requiredClaimsElement == null) && trust13Driver.TryParseRequiredClaimsElement(e, out requiredClaims))
                        {
                            requiredClaimsElement = driver.CreateRequiredClaimsElement(requiredClaims);
                            processedElements.Add(e);
                        }
                    }

                    for (int i = 0; i < processedElements.Count; ++i)
                    {
                        childrenToPromote.Remove(processedElements[i]);
                    }

                    XmlElement keyWrapAlgorithmElement = null;

                    // Replace the appropriate elements.
                    for (int i = 0; i < tmpCollection.Count; ++i)
                    {
                        string algorithmParameter;
                        Collection<XmlElement> reqClaims;

                        if (trust13Driver.IsSignWithElement(tmpCollection[i], out algorithmParameter))
                        {
                            tmpCollection[i] = driver.CreateSignWithElement(algorithmParameter);
                        }
                        else if (trust13Driver.IsEncryptWithElement(tmpCollection[i], out algorithmParameter))
                        {
                            tmpCollection[i] = driver.CreateEncryptWithElement(algorithmParameter);
                        }
                        else if (trust13Driver.IsEncryptionAlgorithmElement(tmpCollection[i], out algorithmParameter) && (encryptionElement != null))
                        {
                            tmpCollection[i] = encryptionElement;
                            encryptionElement = null;
                        }
                        else if (trust13Driver.IsCanonicalizationAlgorithmElement(tmpCollection[i], out algorithmParameter) && (canonicalizationElement != null))
                        {
                            tmpCollection[i] = canonicalizationElement;
                            canonicalizationElement = null;
                        }
                        else if (trust13Driver.IsKeyWrapAlgorithmElement(tmpCollection[i], out algorithmParameter) && (keyWrapAlgorithmElement == null))
                        {
                            keyWrapAlgorithmElement = tmpCollection[i];
                        }
                        else if (trust13Driver.TryParseRequiredClaimsElement(tmpCollection[i], out reqClaims) && (requiredClaimsElement != null))
                        {
                            tmpCollection[i] = requiredClaimsElement;
                            requiredClaimsElement = null;
                        }
                    }

                    if (keyWrapAlgorithmElement != null)
                    {
                        // Remove KeyWrapAlgorithmElement as this is not define in Trust Feb 2005.
                        tmpCollection.Remove(keyWrapAlgorithmElement);
                    }

                    // Add the remaining elements to the additionaParameters list to the end.
                    if (encryptionElement != null) tmpCollection.Add(encryptionElement);
                    if (canonicalizationElement != null) tmpCollection.Add(canonicalizationElement);
                    if (requiredClaimsElement != null) tmpCollection.Add(requiredClaimsElement);

                    if (childrenToPromote.Count > 0)
                    {
                        // There are some non-standard elements. Just bump them to the top-level element.
                        for (int i = 0; i < childrenToPromote.Count; ++i)
                        {
                            tmpCollection.Add(childrenToPromote[i]);
                        }
                    }
                }

            }

            return tmpCollection;
        }

        private bool CollectionContainsElementsWithTrustNamespace(Collection<XmlElement> collection, string trustNamespace)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if ((collection[i] != null) && (collection[i].NamespaceURI == trustNamespace))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanPromoteToRoot(XmlElement innerElement, WSTrustDec2005.DriverDec2005 trust13Driver, bool clientSideClaimTypeRequirementsSpecified)
        {
            SecurityKeyType dummyOutParamForKeyType;
            int dummyOutParamForKeySize;
            string dummyStringOutParam;
            Collection<XmlElement> dummyOutParamForRequiredClaims = null;

            // check if SecondaryParameters has claim requirements specified
            if (trust13Driver.TryParseRequiredClaimsElement(innerElement, out dummyOutParamForRequiredClaims))
            {
                // if client has not specified any claim requirements, promote claim requirements 
                // in SecondaryParameters to root level (and subsequently fix up the trust namespace)
                return !clientSideClaimTypeRequirementsSpecified;
            }

            // KeySize, KeyType and TokenType were converted to top-level property values when the WSDL was
            // imported, so drop it here. We check for EncryptWith and SignWith as these are Client specific algorithm values and we
            // don't have to promote the service specified values. KeyWrapAlgorithm was never sent in the RST
            // in V1 and hence we are dropping it here as well.
            return (!trust13Driver.TryParseKeyTypeElement(innerElement, out dummyOutParamForKeyType) &&
                    !trust13Driver.TryParseKeySizeElement(innerElement, out dummyOutParamForKeySize) &&
                    !trust13Driver.TryParseTokenTypeElement(innerElement, out dummyStringOutParam) &&
                    !trust13Driver.IsSignWithElement(innerElement, out dummyStringOutParam) &&
                    !trust13Driver.IsEncryptWithElement(innerElement, out dummyStringOutParam) &&
                    !trust13Driver.IsKeyWrapAlgorithmElement(innerElement, out dummyStringOutParam));
        }

        internal void AddAlgorithmParameters(SecurityAlgorithmSuite algorithmSuite, SecurityStandardsManager standardsManager, SecurityKeyType issuedKeyType)
        {
            this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateEncryptionAlgorithmElement(algorithmSuite.DefaultEncryptionAlgorithm));
            this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateCanonicalizationAlgorithmElement(algorithmSuite.DefaultCanonicalizationAlgorithm));

            if (this.keyType == SecurityKeyType.BearerKey)
            {
                // As the client does not have a proof token in the Bearer case
                // we don't have any specific algorithms to request for.
                return;
            }

            string signWithAlgorithm = (this.keyType == SecurityKeyType.SymmetricKey) ? algorithmSuite.DefaultSymmetricSignatureAlgorithm : algorithmSuite.DefaultAsymmetricSignatureAlgorithm;
            this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateSignWithElement(signWithAlgorithm));
            string encryptWithAlgorithm;
            if (issuedKeyType == SecurityKeyType.SymmetricKey)
            {
                encryptWithAlgorithm = algorithmSuite.DefaultEncryptionAlgorithm;
            }
            else
            {
                encryptWithAlgorithm = algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm;
            }
            this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateEncryptWithElement(encryptWithAlgorithm));

            if (standardsManager.TrustVersion != TrustVersion.WSTrustFeb2005)
            {
                this.additionalRequestParameters.Insert(0, ((WSTrustDec2005.DriverDec2005)standardsManager.TrustDriver).CreateKeyWrapAlgorithmElement(algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm));
            }

            return;
        }

        internal bool DoAlgorithmsMatch(SecurityAlgorithmSuite algorithmSuite, SecurityStandardsManager standardsManager, out Collection<XmlElement> otherRequestParameters)
        {
            bool doesSignWithAlgorithmMatch = false;
            bool doesEncryptWithAlgorithmMatch = false;
            bool doesEncryptionAlgorithmMatch = false;
            bool doesCanonicalizationAlgorithmMatch = false;
            bool doesKeyWrapAlgorithmMatch = false;
            otherRequestParameters = new Collection<XmlElement>();
            bool trustNormalizationPerformed = false;

            Collection<XmlElement> trustVersionNormalizedParameterCollection;

            // For Trust 1.3 we move all the additional parameters into the secondaryParameters
            // element. So the list contains just one element called SecondaryParameters that 
            // contains all the other elements as child elements.
            if ((standardsManager.TrustVersion == TrustVersion.WSTrust13) &&
                (this.AdditionalRequestParameters.Count == 1) &&
                (((WSTrustDec2005.DriverDec2005)standardsManager.TrustDriver).IsSecondaryParametersElement(this.AdditionalRequestParameters[0])))
            {
                trustNormalizationPerformed = true;
                trustVersionNormalizedParameterCollection = new Collection<XmlElement>();
                foreach (XmlElement innerElement in this.AdditionalRequestParameters[0])
                {
                    trustVersionNormalizedParameterCollection.Add(innerElement);
                }
            }
            else
            {
                trustVersionNormalizedParameterCollection = this.AdditionalRequestParameters;
            }

            for (int i = 0; i < trustVersionNormalizedParameterCollection.Count; i++)
            {
                string algorithm;
                XmlElement element = trustVersionNormalizedParameterCollection[i];
                if (standardsManager.TrustDriver.IsCanonicalizationAlgorithmElement(element, out algorithm))
                {
                    if (algorithmSuite.DefaultCanonicalizationAlgorithm != algorithm)
                    {
                        return false;
                    }
                    doesCanonicalizationAlgorithmMatch = true;
                }
                else if (standardsManager.TrustDriver.IsSignWithElement(element, out algorithm))
                {
                    if ((this.keyType == SecurityKeyType.SymmetricKey && algorithm != algorithmSuite.DefaultSymmetricSignatureAlgorithm)
                        || (this.keyType == SecurityKeyType.AsymmetricKey && algorithm != algorithmSuite.DefaultAsymmetricSignatureAlgorithm))
                    {
                        return false;
                    }
                    doesSignWithAlgorithmMatch = true;
                }
                else if (standardsManager.TrustDriver.IsEncryptWithElement(element, out algorithm))
                {
                    if ((this.keyType == SecurityKeyType.SymmetricKey && algorithm != algorithmSuite.DefaultEncryptionAlgorithm)
                        || (this.keyType == SecurityKeyType.AsymmetricKey && algorithm != algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm))
                    {
                        return false;
                    }
                    doesEncryptWithAlgorithmMatch = true;
                }
                else if (standardsManager.TrustDriver.IsEncryptionAlgorithmElement(element, out algorithm))
                {
                    if (algorithm != algorithmSuite.DefaultEncryptionAlgorithm)
                    {
                        return false;
                    }
                    doesEncryptionAlgorithmMatch = true;
                }
                else if (standardsManager.TrustDriver.IsKeyWrapAlgorithmElement(element, out algorithm))
                {
                    if (algorithm != algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm)
                    {
                        return false;
                    }
                    doesKeyWrapAlgorithmMatch = true;
                }
                else
                {
                    otherRequestParameters.Add(element);
                }
            }

            // Undo normalization if performed
            // move all back into secondaryParameters
            if (trustNormalizationPerformed)
            {
                otherRequestParameters = this.AdditionalRequestParameters;
            }

            if (this.keyType == SecurityKeyType.BearerKey)
            {
                // As the client does not have a proof token in the Bearer case
                // we don't have any specific algorithms to request for.
                return true;
            }
            if (standardsManager.TrustVersion == TrustVersion.WSTrustFeb2005)
            {
                // For V1 compatibility check all algorithms
                return (doesSignWithAlgorithmMatch && doesCanonicalizationAlgorithmMatch && doesEncryptionAlgorithmMatch && doesEncryptWithAlgorithmMatch);
            }
            else
            {
                return (doesSignWithAlgorithmMatch && doesCanonicalizationAlgorithmMatch && doesEncryptionAlgorithmMatch && doesEncryptWithAlgorithmMatch && doesKeyWrapAlgorithmMatch);
            }
        }

        internal static IssuedSecurityTokenParameters CreateInfoCardParameters(SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithm)
        {
            IssuedSecurityTokenParameters result = new IssuedSecurityTokenParameters(SecurityXXX2005Strings.SamlTokenType);
            result.KeyType = SecurityKeyType.AsymmetricKey;
            result.ClaimTypeRequirements.Add(new ClaimTypeRequirement(wsidPPIClaim));
            result.IssuerAddress = null;
            result.AddAlgorithmParameters(algorithm, standardsManager, result.KeyType);
            return result;
        }

        internal static bool IsInfoCardParameters(IssuedSecurityTokenParameters parameters, SecurityStandardsManager standardsManager)
        {
            if (parameters == null)
                return false;
            if (parameters.TokenType != SecurityXXX2005Strings.SamlTokenType)
                return false;
            if (parameters.KeyType != SecurityKeyType.AsymmetricKey)
                return false;

            if (parameters.ClaimTypeRequirements.Count == 1)
            {
                ClaimTypeRequirement claimTypeRequirement = parameters.ClaimTypeRequirements[0] as ClaimTypeRequirement;
                if (claimTypeRequirement == null)
                    return false;
                if (claimTypeRequirement.ClaimType != wsidPPIClaim)
                    return false;
            }
            else if ((parameters.AdditionalRequestParameters != null) && (parameters.AdditionalRequestParameters.Count > 0))
            {
                // Check the AdditionalRequest Parameters to see if ClaimTypeRequirements got imported there.
                bool claimTypeRequirementMatched = false;
                XmlElement claimTypeRequirement = GetClaimTypeRequirement(parameters.AdditionalRequestParameters, standardsManager);
                if (claimTypeRequirement != null && claimTypeRequirement.ChildNodes.Count == 1)
                {
                    XmlElement claimTypeElement = claimTypeRequirement.ChildNodes[0] as XmlElement;
                    if (claimTypeElement != null)
                    {
                        XmlNode claimType = claimTypeElement.Attributes.GetNamedItem("Uri");
                        if (claimType != null && claimType.Value == wsidPPIClaim)
                        {
                            claimTypeRequirementMatched = true;
                        }
                    }
                }

                if (!claimTypeRequirementMatched)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            if (parameters.IssuerAddress != null)
                return false;
            if (parameters.AlternativeIssuerEndpoints != null && parameters.AlternativeIssuerEndpoints.Count > 0)
            {
                return false;
            }
            return true;
        }

        // The method walks through the entire set of AdditionalRequestParameters and return the Claims Type requirement alone.
        internal static XmlElement GetClaimTypeRequirement(Collection<XmlElement> additionalRequestParameters, SecurityStandardsManager standardsManager)
        {
            foreach (XmlElement requestParameter in additionalRequestParameters)
            {
                if ((requestParameter.LocalName == ((System.ServiceModel.Security.WSTrust.Driver)standardsManager.TrustDriver).DriverDictionary.Claims.Value) &&
                    (requestParameter.NamespaceURI == ((System.ServiceModel.Security.WSTrust.Driver)standardsManager.TrustDriver).DriverDictionary.Namespace.Value))
                {
                    return requestParameter;
                }

                if ((requestParameter.LocalName == DXD.TrustDec2005Dictionary.SecondaryParameters.Value) &&
                    (requestParameter.NamespaceURI == DXD.TrustDec2005Dictionary.Namespace.Value))
                {
                    Collection<XmlElement> secondaryParameters = new Collection<XmlElement>();
                    foreach (XmlNode node in requestParameter.ChildNodes)
                    {
                        XmlElement nodeAsElement = node as XmlElement;
                        if (nodeAsElement != null)
                        {
                            secondaryParameters.Add(nodeAsElement);
                        }
                    }
                    XmlElement claimTypeRequirement = GetClaimTypeRequirement(secondaryParameters, standardsManager);
                    if (claimTypeRequirement != null)
                    {
                        return claimTypeRequirement;
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "TokenType: {0}", this.tokenType == null ? "null" : this.tokenType));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "KeyType: {0}", this.keyType.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "KeySize: {0}", this.keySize.ToString(CultureInfo.InvariantCulture)));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "IssuerAddress: {0}", this.issuerAddress == null ? "null" : this.issuerAddress.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "IssuerMetadataAddress: {0}", this.issuerMetadataAddress == null ? "null" : this.issuerMetadataAddress.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "DefaultMessgeSecurityVersion: {0}", this.defaultMessageSecurityVersion == null ? "null" : this.defaultMessageSecurityVersion.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "UseStrTransform: {0}", this.useStrTransform.ToString()));

            if (this.issuerBinding == null)
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "IssuerBinding: null"));
            }
            else
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "IssuerBinding:"));
                BindingElementCollection bindingElements = this.issuerBinding.CreateBindingElements();
                for (int i = 0; i < bindingElements.Count; i++)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "  BindingElement[{0}]:", i.ToString(CultureInfo.InvariantCulture)));
                    sb.AppendLine("    " + bindingElements[i].ToString().Trim().Replace("\n", "\n    "));
                }
            }

            if (this.claimTypeRequirements.Count == 0)
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "ClaimTypeRequirements: none"));
            }
            else
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "ClaimTypeRequirements:"));
                for (int i = 0; i < this.claimTypeRequirements.Count; i++)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "  {0}, optional={1}", this.claimTypeRequirements[i].ClaimType, this.claimTypeRequirements[i].IsOptional));
                }
            }

            return sb.ToString().Trim();
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = this.TokenType;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = this.KeyType;

            ServiceModelSecurityTokenRequirement serviceModelSecurityTokenRequirement = requirement as ServiceModelSecurityTokenRequirement;
            if (serviceModelSecurityTokenRequirement != null)
            {
                serviceModelSecurityTokenRequirement.DefaultMessageSecurityVersion = this.DefaultMessageSecurityVersion;
            }
            else
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.DefaultMessageSecurityVersionProperty] = this.DefaultMessageSecurityVersion;
            }

            if (this.KeySize > 0)
            {
                requirement.KeySize = this.KeySize;
            }
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerAddressProperty] = this.IssuerAddress;
            if (this.IssuerBinding != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerBindingProperty] = this.IssuerBinding;
            }
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty] = this.Clone();
        }
    }
}
