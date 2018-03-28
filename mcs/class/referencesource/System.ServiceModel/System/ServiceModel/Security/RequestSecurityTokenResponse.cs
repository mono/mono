//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Xml;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Security.Principal;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security.Tokens;
    using System.IO;
    using System.ServiceModel.Security;

    using Psha1DerivedKeyGenerator = System.IdentityModel.Psha1DerivedKeyGenerator;
    using System.ServiceModel.Dispatcher;

    class RequestSecurityTokenResponse : BodyWriter
    {
        static int minSaneKeySizeInBits = 8 * 8; // 8 Bytes.
        static int maxSaneKeySizeInBits = (16 * 1024) * 8; // 16 K

        SecurityStandardsManager standardsManager;
        string context;
        int keySize;
        bool computeKey;
        string tokenType;
        SecurityKeyIdentifierClause requestedAttachedReference;
        SecurityKeyIdentifierClause requestedUnattachedReference;
        SecurityToken issuedToken;
        SecurityToken proofToken;
        SecurityToken entropyToken;
        BinaryNegotiation negotiationData;
        XmlElement rstrXml;
        DateTime effectiveTime;
        DateTime expirationTime;
        bool isLifetimeSet;
        byte[] authenticator;
        bool isReceiver;
        bool isReadOnly;
        byte[] cachedWriteBuffer;
        int cachedWriteBufferLength;
        bool isRequestedTokenClosed;
        object appliesTo;
        XmlObjectSerializer appliesToSerializer;
        Type appliesToType;
        Object thisLock = new Object();
        System.IdentityModel.XmlBuffer issuedTokenBuffer;

        public RequestSecurityTokenResponse()
            : this(SecurityStandardsManager.DefaultInstance)
        {
        }

        public RequestSecurityTokenResponse(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
            : this(SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer))
        {
        }

        public RequestSecurityTokenResponse(XmlElement requestSecurityTokenResponseXml,
                                            string context,
                                            string tokenType,
                                            int keySize,
                                            SecurityKeyIdentifierClause requestedAttachedReference,
                                            SecurityKeyIdentifierClause requestedUnattachedReference,
                                            bool computeKey,
                                            DateTime validFrom,
                                            DateTime validTo,
                                            bool isRequestedTokenClosed)
            : this(SecurityStandardsManager.DefaultInstance,
                   requestSecurityTokenResponseXml,
                   context,
                   tokenType,
                   keySize,
                   requestedAttachedReference,
                   requestedUnattachedReference,
                   computeKey,
                   validFrom,
                   validTo,
                   isRequestedTokenClosed,
                   null)
        {
        }

        public RequestSecurityTokenResponse(MessageSecurityVersion messageSecurityVersion, 
                                            SecurityTokenSerializer securityTokenSerializer, 
                                            XmlElement requestSecurityTokenResponseXml,
                                            string context,
                                            string tokenType,
                                            int keySize,
                                            SecurityKeyIdentifierClause requestedAttachedReference,
                                            SecurityKeyIdentifierClause requestedUnattachedReference,
                                            bool computeKey,
                                            DateTime validFrom,
                                            DateTime validTo,
                                            bool isRequestedTokenClosed)
            : this(SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer),
                   requestSecurityTokenResponseXml,
                   context,
                   tokenType,
                   keySize,
                   requestedAttachedReference,
                   requestedUnattachedReference,
                   computeKey,
                   validFrom,
                   validTo,
                   isRequestedTokenClosed, 
                   null)
        {
        }

        internal RequestSecurityTokenResponse(SecurityStandardsManager standardsManager)
            : base(true)
        {
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            effectiveTime = SecurityUtils.MinUtcDateTime;
            expirationTime = SecurityUtils.MaxUtcDateTime;
            isRequestedTokenClosed = false;
            this.isLifetimeSet = false;
            this.isReceiver = false;
            this.isReadOnly = false;
        }
        
        internal RequestSecurityTokenResponse(SecurityStandardsManager standardsManager,
                                              XmlElement rstrXml,
                                              string context,
                                              string tokenType,
                                              int keySize,
                                              SecurityKeyIdentifierClause requestedAttachedReference,
                                              SecurityKeyIdentifierClause requestedUnattachedReference,
                                              bool computeKey,
                                              DateTime validFrom,
                                              DateTime validTo,
                                              bool isRequestedTokenClosed,
                                              System.IdentityModel.XmlBuffer issuedTokenBuffer)
            : base(true)
        {
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            if (rstrXml == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstrXml");
            this.rstrXml = rstrXml;
            this.context = context;
            this.tokenType = tokenType;
            this.keySize = keySize;
            this.requestedAttachedReference = requestedAttachedReference;
            this.requestedUnattachedReference = requestedUnattachedReference;
            this.computeKey = computeKey;
            this.effectiveTime = validFrom.ToUniversalTime();
            this.expirationTime = validTo.ToUniversalTime();
            this.isLifetimeSet = true;
            this.isRequestedTokenClosed = isRequestedTokenClosed;
            this.issuedTokenBuffer = issuedTokenBuffer;
            this.isReceiver = true;
            this.isReadOnly = true;
        }

        public string Context
        {
            get
            {
                return this.context;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.context = value;
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
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.tokenType = value;
            }
        }

        public SecurityKeyIdentifierClause RequestedAttachedReference 
        {
            get 
            { 
                return this.requestedAttachedReference;
            } 
            set 
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.requestedAttachedReference = value;
            } 
        }

        public SecurityKeyIdentifierClause RequestedUnattachedReference
        {
            get
            {
                return this.requestedUnattachedReference;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.requestedUnattachedReference = value;
            }
        }

        public DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public DateTime ValidTo
        {
            get 
            {
                return this.expirationTime;
            }
        }

        public bool ComputeKey
        {
            get 
            {
                return this.computeKey;
            }
            set 
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.computeKey = value;
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
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                if (value < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeNonNegative)));
                this.keySize = value;
            } 
        }

        public bool IsRequestedTokenClosed
        {
            get 
            {
                return this.isRequestedTokenClosed;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.isRequestedTokenClosed = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {   
                return this.isReadOnly;
            }
        }

        protected Object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        internal bool IsReceiver
        {
            get
            {
                return this.isReceiver;
            }
        }

        internal SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.standardsManager = (value != null ? value : SecurityStandardsManager.DefaultInstance);
            }
        }

        public SecurityToken EntropyToken
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRSTR, "EntropyToken")));
                }
                return this.entropyToken;
            }
        }

        public SecurityToken RequestedSecurityToken
        {
            get 
            {
                if (this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRSTR, "IssuedToken")));
                }
                return this.issuedToken;
            }
            set
            {
                if (this.isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.issuedToken = value;
            }
        }

        public SecurityToken RequestedProofToken
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRSTR, "ProofToken")));
                }
                return this.proofToken;
            }
            set
            {
                if (this.isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.proofToken = value;
            }
        }

        public XmlElement RequestSecurityTokenResponseXml
        {
            get
            {
                if (!this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemAvailableInDeserializedRSTROnly, "RequestSecurityTokenXml")));
                }
                return this.rstrXml;
            }
        }

        internal object AppliesTo
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRST, "AppliesTo")));
                }
                return this.appliesTo;
            }
        }

        internal XmlObjectSerializer AppliesToSerializer
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRST, "AppliesToSerializer")));
                }
                return this.appliesToSerializer;
            }
        }

        internal Type AppliesToType
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRST, "AppliesToType")));
                }
                return this.appliesToType;
            }
        }

        internal bool IsLifetimeSet
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRSTR, "IsLifetimeSet")));
                }
                return this.isLifetimeSet;
            }
        }

        internal System.IdentityModel.XmlBuffer IssuedTokenBuffer
        {
            get
            {
                return this.issuedTokenBuffer;
            }
        }

        public void SetIssuerEntropy(byte[] issuerEntropy)
        {
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            this.entropyToken = (issuerEntropy != null) ? new NonceToken(issuerEntropy) : null;
        }

        internal void SetIssuerEntropy(WrappedKeySecurityToken issuerEntropy)
        {
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            this.entropyToken = issuerEntropy;
        }

        public SecurityToken GetIssuerEntropy()
        {
            return this.GetIssuerEntropy(null);
        }

        internal SecurityToken GetIssuerEntropy(SecurityTokenResolver resolver)
        {
            if (this.isReceiver)
            {
                return this.standardsManager.TrustDriver.GetEntropy(this, resolver);
            }
            else
                return this.entropyToken;
        }

        public void SetLifetime(DateTime validFrom, DateTime validTo)
        {
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            if (validFrom.ToUniversalTime() > validTo.ToUniversalTime())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.EffectiveGreaterThanExpiration));
            }
            this.effectiveTime = validFrom.ToUniversalTime();
            this.expirationTime = validTo.ToUniversalTime();
            this.isLifetimeSet = true;
        }

        public void SetAppliesTo<T>(T appliesTo, XmlObjectSerializer serializer)
        {
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            if (appliesTo != null && serializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            this.appliesTo = appliesTo;
            this.appliesToSerializer = serializer;
            this.appliesToType = typeof(T);
        }

        public void GetAppliesToQName(out string localName, out string namespaceUri)
        {
            if (!this.isReceiver)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemAvailableInDeserializedRSTOnly, "MatchesAppliesTo")));
            this.standardsManager.TrustDriver.GetAppliesToQName(this, out localName, out namespaceUri);
        }

        public T GetAppliesTo<T>()
        {
            return this.GetAppliesTo<T>(DataContractSerializerDefaults.CreateSerializer(typeof(T), DataContractSerializerDefaults.MaxItemsInObjectGraph));
        }

        public T GetAppliesTo<T>(XmlObjectSerializer serializer)
        {
            if (this.isReceiver)
            {
                if (serializer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
                }
                return this.standardsManager.TrustDriver.GetAppliesTo<T>(this, serializer);
            }
            else
            {
                return (T)this.appliesTo;
            }
        }

        internal void SetBinaryNegotiation(BinaryNegotiation negotiation)
        {
            if (negotiation == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("negotiation");
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            this.negotiationData = negotiation;
        }

        internal BinaryNegotiation GetBinaryNegotiation()
        {
            if (this.isReceiver)
                return this.standardsManager.TrustDriver.GetBinaryNegotiation(this);
            else
                return this.negotiationData;
        }

        internal void SetAuthenticator(byte[] authenticator)
        {
            if (authenticator == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authenticator");
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            this.authenticator = DiagnosticUtility.Utility.AllocateByteArray(authenticator.Length);
            Buffer.BlockCopy(authenticator, 0, this.authenticator, 0, authenticator.Length);
        }

        internal byte[] GetAuthenticator()
        {
            if (this.isReceiver)
                return this.standardsManager.TrustDriver.GetAuthenticator(this);
            else
            {
                if (this.authenticator == null)
                    return null;
                else 
                {
                    byte[] result = DiagnosticUtility.Utility.AllocateByteArray(this.authenticator.Length);
                    Buffer.BlockCopy(this.authenticator, 0, result, 0, this.authenticator.Length);
                    return result;
                }
            }
        }

        void OnWriteTo(XmlWriter w)
        {
            if (this.isReceiver)
            {
                this.rstrXml.WriteTo(w);
            }
            else
            {
                this.standardsManager.TrustDriver.WriteRequestSecurityTokenResponse(this, w);
            }
        }

        public void WriteTo(XmlWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            if (this.IsReadOnly)
            {
                // cache the serialized bytes to ensure repeatability
                if (this.cachedWriteBuffer == null)
                {
                    MemoryStream stream = new MemoryStream();
                    using (XmlDictionaryWriter binaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, XD.Dictionary))
                    {
                        this.OnWriteTo(binaryWriter);
                        binaryWriter.Flush();
                        stream.Flush();
                        stream.Seek(0, SeekOrigin.Begin);
                        this.cachedWriteBuffer = stream.GetBuffer();
                        this.cachedWriteBufferLength = (int)stream.Length;
                    }
                }
                writer.WriteNode(XmlDictionaryReader.CreateBinaryReader(this.cachedWriteBuffer, 0, this.cachedWriteBufferLength, XD.Dictionary, XmlDictionaryReaderQuotas.Max), false);
            }
            else
                this.OnWriteTo(writer);
        }

        public static RequestSecurityTokenResponse CreateFrom(XmlReader reader)
        {
            return CreateFrom(SecurityStandardsManager.DefaultInstance, reader);
        }

        public static RequestSecurityTokenResponse CreateFrom(XmlReader reader, MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
        {
            return CreateFrom(SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer), reader);
        }

        internal static RequestSecurityTokenResponse CreateFrom(SecurityStandardsManager standardsManager, XmlReader reader)
        {
            return standardsManager.TrustDriver.CreateRequestSecurityTokenResponse(reader);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WriteTo(writer);
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.isReadOnly = true;
                this.OnMakeReadOnly();
            }
        }

        public GenericXmlSecurityToken GetIssuedToken(SecurityTokenResolver resolver, IList<SecurityTokenAuthenticator> allowedAuthenticators, SecurityKeyEntropyMode keyEntropyMode, byte[] requestorEntropy, string expectedTokenType,
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            return this.GetIssuedToken(resolver, allowedAuthenticators, keyEntropyMode, requestorEntropy, expectedTokenType, authorizationPolicies, 0, false);
        }

        public virtual GenericXmlSecurityToken GetIssuedToken(SecurityTokenResolver resolver, IList<SecurityTokenAuthenticator> allowedAuthenticators, SecurityKeyEntropyMode keyEntropyMode, byte[] requestorEntropy, string expectedTokenType,
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, int defaultKeySize, bool isBearerKeyType)
        {
            if (!this.isReceiver)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemAvailableInDeserializedRSTROnly, "GetIssuedToken")));

            return this.standardsManager.TrustDriver.GetIssuedToken(this, resolver, allowedAuthenticators, keyEntropyMode, requestorEntropy, expectedTokenType, authorizationPolicies, defaultKeySize, isBearerKeyType);
        }

        public virtual GenericXmlSecurityToken GetIssuedToken(string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, RSA clientKey)
        {
            if (!this.isReceiver)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemAvailableInDeserializedRSTROnly, "GetIssuedToken")));

            return this.standardsManager.TrustDriver.GetIssuedToken(this, expectedTokenType, authorizationPolicies, clientKey);
        }

        protected internal virtual void OnWriteCustomAttributes(XmlWriter writer)
        { }

        protected internal virtual void OnWriteCustomElements(XmlWriter writer) 
        { }

        protected virtual void OnMakeReadOnly() { }

        public static byte[] ComputeCombinedKey(byte[] requestorEntropy, byte[] issuerEntropy, int keySizeInBits)
        {
            if (requestorEntropy == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestorEntropy");
            if (issuerEntropy == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerEntropy");
            // Do a sanity check here. We don't want to allow invalid keys or keys that are too
            // large.
            if ((keySizeInBits < minSaneKeySizeInBits) || (keySizeInBits > maxSaneKeySizeInBits))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidKeySizeSpecifiedInNegotiation, keySizeInBits, minSaneKeySizeInBits, maxSaneKeySizeInBits)));
            Psha1DerivedKeyGenerator generator = new Psha1DerivedKeyGenerator(requestorEntropy);
            return generator.GenerateDerivedKey(new byte[] { }, issuerEntropy, keySizeInBits, 0);
        }
    }
}
