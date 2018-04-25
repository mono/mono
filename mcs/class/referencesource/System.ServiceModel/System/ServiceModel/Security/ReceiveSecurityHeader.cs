//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;
    using System.Xml;
    using ISignatureValueSecurityElement = System.IdentityModel.ISignatureValueSecurityElement;
    using SignatureResourcePool = System.IdentityModel.SignatureResourcePool;
    using SignedXml = System.IdentityModel.SignedXml;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    abstract class ReceiveSecurityHeader : SecurityHeader
    {
        // client->server symmetric binding case: only primaryTokenAuthenticator is set
        // server->client symmetric binding case: only primary token is set
        // asymmetric binding case: primaryTokenAuthenticator and wrapping token is set

        SecurityTokenAuthenticator primaryTokenAuthenticator;
        bool allowFirstTokenMismatch;
        SecurityToken outOfBandPrimaryToken;
        IList<SecurityToken> outOfBandPrimaryTokenCollection;
        SecurityTokenParameters primaryTokenParameters;
        TokenTracker primaryTokenTracker;
        SecurityToken wrappingToken;
        SecurityTokenParameters wrappingTokenParameters;
        SecurityToken expectedEncryptionToken;
        SecurityTokenParameters expectedEncryptionTokenParameters;
        SecurityTokenAuthenticator derivedTokenAuthenticator;
        // assumes that the caller has done the check for uniqueness of types
        IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators;
        ChannelBinding channelBinding;
        ExtendedProtectionPolicy extendedProtectionPolicy;

        bool expectEncryption = true;
        // caller should precompute and set expectations
        bool expectBasicTokens;
        bool expectSignedTokens;
        bool expectEndorsingTokens;
        bool expectSignature = true;
        bool requireSignedPrimaryToken;
        bool expectSignatureConfirmation;
        // maps from token to wire form (for basic and signed), and also tracks operations done
        // maps from supporting token parameter to the operations done for that token type
        List<TokenTracker> supportingTokenTrackers;

        SignatureConfirmations receivedSignatureValues;
        SignatureConfirmations receivedSignatureConfirmations;
        List<SecurityTokenAuthenticator> allowedAuthenticators;

        SecurityTokenAuthenticator pendingSupportingTokenAuthenticator;

        WrappedKeySecurityToken wrappedKeyToken;
        Collection<SecurityToken> basicTokens;
        Collection<SecurityToken> signedTokens;
        Collection<SecurityToken> endorsingTokens;
        Collection<SecurityToken> signedEndorsingTokens;
        Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping;
        List<SecurityTokenAuthenticator> wrappedKeyAuthenticator;
        SecurityTimestamp timestamp;
        SecurityHeaderTokenResolver universalTokenResolver;
        SecurityHeaderTokenResolver primaryTokenResolver;
        ReadOnlyCollection<SecurityTokenResolver> outOfBandTokenResolver;
        SecurityTokenResolver combinedUniversalTokenResolver;
        SecurityTokenResolver combinedPrimaryTokenResolver;

        readonly int headerIndex;
        XmlAttributeHolder[] securityElementAttributes;
        OrderTracker orderTracker = new OrderTracker();
        OperationTracker signatureTracker = new OperationTracker();
        OperationTracker encryptionTracker = new OperationTracker();

        ReceiveSecurityHeaderElementManager elementManager;

        int maxDerivedKeys;
        int numDerivedKeys;
        int maxDerivedKeyLength;
        bool enforceDerivedKeyRequirement = true;

        NonceCache nonceCache;
        TimeSpan replayWindow;
        TimeSpan clockSkew;
        byte[] primarySignatureValue;
        TimeoutHelper timeoutHelper;
        SecurityVerifiedMessage securityVerifiedMessage;
        long maxReceivedMessageSize = TransportDefaults.MaxReceivedMessageSize;
        XmlDictionaryReaderQuotas readerQuotas;
        MessageProtectionOrder protectionOrder;
        bool hasAtLeastOneSupportingTokenExpectedToBeSigned;
        bool hasEndorsingOrSignedEndorsingSupportingTokens;
        SignatureResourcePool resourcePool;
        bool replayDetectionEnabled = false;

        bool hasAtLeastOneItemInsideSecurityHeaderEncrypted = false;

        const int AppendPosition = -1;

        EventTraceActivity eventTraceActivity;

        protected ReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay,
            SecurityStandardsManager standardsManager,
            SecurityAlgorithmSuite algorithmSuite,
            int headerIndex,
            MessageDirection direction)
            : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction)
        {
            this.headerIndex = headerIndex;
            this.elementManager = new ReceiveSecurityHeaderElementManager(this);
        }

        public Collection<SecurityToken> BasicSupportingTokens
        {
            get
            {
                return this.basicTokens;
            }
        }

        public Collection<SecurityToken> SignedSupportingTokens
        {
            get
            {
                return this.signedTokens;
            }
        }

        public Collection<SecurityToken> EndorsingSupportingTokens
        {
            get
            {
                return this.endorsingTokens;
            }
        }

        public ReceiveSecurityHeaderElementManager ElementManager
        {
            get
            {
                return this.elementManager;
            }
        }

        public Collection<SecurityToken> SignedEndorsingSupportingTokens
        {
            get
            {
                return this.signedEndorsingTokens;
            }
        }

        public SecurityTokenAuthenticator DerivedTokenAuthenticator
        {
            get
            {
                return this.derivedTokenAuthenticator;
            }
            set
            {
                ThrowIfProcessingStarted();
                this.derivedTokenAuthenticator = value;
            }
        }

        public List<SecurityTokenAuthenticator> WrappedKeySecurityTokenAuthenticator
        {
            get
            {
                return this.wrappedKeyAuthenticator;
            }
            set
            {
                ThrowIfProcessingStarted();
                this.wrappedKeyAuthenticator = value;
            }
        }

        public bool EnforceDerivedKeyRequirement
        {
            get
            {
                return this.enforceDerivedKeyRequirement;
            }
            set
            {
                ThrowIfProcessingStarted();
                this.enforceDerivedKeyRequirement = value;
            }
        }

        public byte[] PrimarySignatureValue
        {
            get { return this.primarySignatureValue; }
        }

        public bool EncryptBeforeSignMode
        {
            get { return this.orderTracker.EncryptBeforeSignMode; }
        }

        public SecurityToken EncryptionToken
        {
            get { return this.encryptionTracker.Token; }
        }

        public bool ExpectBasicTokens
        {
            get { return this.expectBasicTokens; }
            set
            {
                ThrowIfProcessingStarted();
                this.expectBasicTokens = value;
            }
        }

        public bool ReplayDetectionEnabled
        {
            get { return this.replayDetectionEnabled; }
            set
            {
                ThrowIfProcessingStarted();
                this.replayDetectionEnabled = value;
            }
        }

        public bool ExpectEncryption
        {
            get { return this.expectEncryption; }
            set
            {
                ThrowIfProcessingStarted();
                this.expectEncryption = value;
            }
        }

        public bool ExpectSignature
        {
            get { return this.expectSignature; }
            set
            {
                ThrowIfProcessingStarted();
                this.expectSignature = value;
            }
        }

        public bool ExpectSignatureConfirmation
        {
            get { return this.expectSignatureConfirmation; }
            set
            {
                ThrowIfProcessingStarted();
                this.expectSignatureConfirmation = value;
            }
        }

        public bool ExpectSignedTokens
        {
            get { return this.expectSignedTokens; }
            set
            {
                ThrowIfProcessingStarted();
                this.expectSignedTokens = value;
            }
        }

        public bool RequireSignedPrimaryToken
        {
            get { return this.requireSignedPrimaryToken; }
            set
            {
                ThrowIfProcessingStarted();
                this.requireSignedPrimaryToken = value;
            }
        }

        public bool ExpectEndorsingTokens
        {
            get { return this.expectEndorsingTokens; }
            set
            {
                ThrowIfProcessingStarted();
                this.expectEndorsingTokens = value;
            }
        }

        public bool HasAtLeastOneItemInsideSecurityHeaderEncrypted
        {
            get { return this.hasAtLeastOneItemInsideSecurityHeaderEncrypted; }
            set { this.hasAtLeastOneItemInsideSecurityHeaderEncrypted = value; }
        }

        public SecurityHeaderTokenResolver PrimaryTokenResolver
        {
            get
            {
                return this.primaryTokenResolver;
            }
        }

        public SecurityTokenResolver CombinedUniversalTokenResolver
        {
            get { return this.combinedUniversalTokenResolver; }
        }

        public SecurityTokenResolver CombinedPrimaryTokenResolver
        {
            get { return this.combinedPrimaryTokenResolver; }
        }

        protected EventTraceActivity EventTraceActivity
        {
            get
            {
                if (this.eventTraceActivity == null && FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
                {
                    this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity((OperationContext.Current != null) ? OperationContext.Current.IncomingMessage : null);
                }

                return this.eventTraceActivity;
            }
        }

        protected void VerifySignatureEncryption()
        {
            if ((this.protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature) &&
                (!this.orderTracker.AllSignaturesEncrypted))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                                SR.GetString(SR.PrimarySignatureIsRequiredToBeEncrypted)));
            }
        }

        internal int HeaderIndex
        {
            get { return this.headerIndex; }
        }

        internal long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
            set
            {
                ThrowIfProcessingStarted();
                this.maxReceivedMessageSize = value;
            }
        }

        internal XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return this.readerQuotas; }
            set
            {
                ThrowIfProcessingStarted();

                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                this.readerQuotas = value;
            }
        }

        public override string Name
        {
            get { return this.StandardsManager.SecurityVersion.HeaderName.Value; }
        }

        public override string Namespace
        {
            get { return this.StandardsManager.SecurityVersion.HeaderNamespace.Value; }
        }

        public Message ProcessedMessage
        {
            get { return this.Message; }
        }

        public MessagePartSpecification RequiredEncryptionParts
        {
            get { return this.encryptionTracker.Parts; }
            set
            {
                ThrowIfProcessingStarted();
                if (value == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("value"), this.Message);
                }
                if (!value.IsReadOnly)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.MessagePartSpecificationMustBeImmutable)), this.Message);
                }
                this.encryptionTracker.Parts = value;
            }
        }

        public MessagePartSpecification RequiredSignatureParts
        {
            get { return this.signatureTracker.Parts; }
            set
            {
                ThrowIfProcessingStarted();
                if (value == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("value"), this.Message);
                }
                if (!value.IsReadOnly)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.MessagePartSpecificationMustBeImmutable)), this.Message);
                }
                this.signatureTracker.Parts = value;
            }
        }

        protected SignatureResourcePool ResourcePool
        {
            get
            {
                if (this.resourcePool == null)
                {
                    this.resourcePool = new SignatureResourcePool();
                }
                return this.resourcePool;
            }
        }

        internal SecurityVerifiedMessage SecurityVerifiedMessage
        {
            get
            {
                return this.securityVerifiedMessage;
            }
        }

        public SecurityToken SignatureToken
        {
            get { return this.signatureTracker.Token; }
        }

        public Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> SecurityTokenAuthorizationPoliciesMapping
        {
            get
            {
                if (this.tokenPoliciesMapping == null)
                {
                    this.tokenPoliciesMapping = new Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>>();
                }
                return this.tokenPoliciesMapping;
            }
        }

        public SecurityTimestamp Timestamp
        {
            get { return this.timestamp; }
        }

        public int MaxDerivedKeyLength
        {
            get
            {
                return this.maxDerivedKeyLength;
            }
        }

        internal XmlDictionaryReader CreateSecurityHeaderReader()
        {
            return this.securityVerifiedMessage.GetReaderAtSecurityHeader();
        }

        public SignatureConfirmations GetSentSignatureConfirmations()
        {
            return this.receivedSignatureConfirmations;
        }

        public void ConfigureSymmetricBindingServerReceiveHeader(SecurityTokenAuthenticator primaryTokenAuthenticator, SecurityTokenParameters primaryTokenParameters, IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.primaryTokenAuthenticator = primaryTokenAuthenticator;
            this.primaryTokenParameters = primaryTokenParameters;
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        // encrypted key case
        public void ConfigureSymmetricBindingServerReceiveHeader(SecurityToken wrappingToken, SecurityTokenParameters wrappingTokenParameters, IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.wrappingToken = wrappingToken;
            this.wrappingTokenParameters = wrappingTokenParameters;
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        public void ConfigureAsymmetricBindingServerReceiveHeader(SecurityTokenAuthenticator primaryTokenAuthenticator, SecurityTokenParameters primaryTokenParameters, SecurityToken wrappingToken, SecurityTokenParameters wrappingTokenParameters, IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.primaryTokenAuthenticator = primaryTokenAuthenticator;
            this.primaryTokenParameters = primaryTokenParameters;
            this.wrappingToken = wrappingToken;
            this.wrappingTokenParameters = wrappingTokenParameters;
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        public void ConfigureTransportBindingServerReceiveHeader(IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        public void ConfigureAsymmetricBindingClientReceiveHeader(SecurityToken primaryToken, SecurityTokenParameters primaryTokenParameters, SecurityToken encryptionToken, SecurityTokenParameters encryptionTokenParameters, SecurityTokenAuthenticator primaryTokenAuthenticator)
        {
            this.outOfBandPrimaryToken = primaryToken;
            this.primaryTokenParameters = primaryTokenParameters;
            this.primaryTokenAuthenticator = primaryTokenAuthenticator;
            this.allowFirstTokenMismatch = primaryTokenAuthenticator != null;
            if (encryptionToken != null && !SecurityUtils.HasSymmetricSecurityKey(encryptionToken))
            {
                this.wrappingToken = encryptionToken;
                this.wrappingTokenParameters = encryptionTokenParameters;
            }
            else
            {
                this.expectedEncryptionToken = encryptionToken;
                this.expectedEncryptionTokenParameters = encryptionTokenParameters;
            }
        }

        public void ConfigureSymmetricBindingClientReceiveHeader(SecurityToken primaryToken, SecurityTokenParameters primaryTokenParameters)
        {
            this.outOfBandPrimaryToken = primaryToken;
            this.primaryTokenParameters = primaryTokenParameters;
        }

        public void ConfigureSymmetricBindingClientReceiveHeader(IList<SecurityToken> primaryTokens, SecurityTokenParameters primaryTokenParameters)
        {
            this.outOfBandPrimaryTokenCollection = primaryTokens;
            this.primaryTokenParameters = primaryTokenParameters;
        }

        public void ConfigureOutOfBandTokenResolver(ReadOnlyCollection<SecurityTokenResolver> outOfBandResolvers)
        {
            if (outOfBandResolvers == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outOfBandResolvers");
            if (outOfBandResolvers.Count == 0)
            {
                return;
            }
            this.outOfBandTokenResolver = outOfBandResolvers;
        }

        protected abstract EncryptedData ReadSecurityHeaderEncryptedItem(XmlDictionaryReader reader, bool readXmlreferenceKeyInfoClause);

        protected abstract byte[] DecryptSecurityHeaderElement(EncryptedData encryptedData, WrappedKeySecurityToken wrappedKeyToken, out SecurityToken encryptionToken);

        protected abstract WrappedKeySecurityToken DecryptWrappedKey(XmlDictionaryReader reader);

        public SignatureConfirmations GetSentSignatureValues()
        {
            return this.receivedSignatureValues;
        }

        protected abstract bool IsReaderAtEncryptedKey(XmlDictionaryReader reader);

        protected abstract bool IsReaderAtEncryptedData(XmlDictionaryReader reader);

        protected abstract bool IsReaderAtReferenceList(XmlDictionaryReader reader);

        protected abstract bool IsReaderAtSignature(XmlDictionaryReader reader);

        protected abstract bool IsReaderAtSecurityTokenReference(XmlDictionaryReader reader);

        protected abstract void OnDecryptionOfSecurityHeaderItemRequiringReferenceListEntry(string id);

        void MarkHeaderAsUnderstood()
        {
            // header decryption does not reorder or delete headers
            MessageHeaderInfo header = this.Message.Headers[this.headerIndex];
            Fx.Assert(header.Name == this.Name && header.Namespace == this.Namespace && header.Actor == this.Actor, "security header index mismatch");
            Message.Headers.UnderstoodHeaders.Add(header);
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.StandardsManager.SecurityVersion.WriteStartHeader(writer);
            XmlAttributeHolder[] attributes = this.securityElementAttributes;
            for (int i = 0; i < attributes.Length; ++i)
            {
                writer.WriteAttributeString(attributes[i].Prefix, attributes[i].LocalName, attributes[i].NamespaceUri, attributes[i].Value);
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            XmlDictionaryReader securityHeaderReader = GetReaderAtSecurityHeader();
            securityHeaderReader.ReadStartElement();
            for (int i = 0; i < this.ElementManager.Count; ++i)
            {
                ReceiveSecurityHeaderEntry entry;
                this.ElementManager.GetElementEntry(i, out entry);
                XmlDictionaryReader reader = null;
                if (entry.encrypted)
                {
                    reader = this.ElementManager.GetReader(i, false);
                    writer.WriteNode(reader, false);
                    reader.Close();
                    securityHeaderReader.Skip();
                }
                else
                {
                    writer.WriteNode(securityHeaderReader, false);
                }
            }
            securityHeaderReader.Close();
        }

        XmlDictionaryReader GetReaderAtSecurityHeader()
        {
            XmlDictionaryReader reader = this.SecurityVerifiedMessage.GetReaderAtFirstHeader();
            for (int i = 0; i < this.HeaderIndex; ++i)
            {
                reader.Skip();
            }

            return reader;
        }

        Collection<SecurityToken> EnsureSupportingTokens(ref Collection<SecurityToken> list)
        {
            if (list == null)
                list = new Collection<SecurityToken>();
            return list;
        }

        void VerifySupportingToken(TokenTracker tracker)
        {
            if (tracker == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tracker");

            Fx.Assert(tracker.spec != null, "Supporting token trackers cannot have null specification.");

            SupportingTokenAuthenticatorSpecification spec = tracker.spec;

            if (tracker.token == null)
            {
                if (spec.IsTokenOptional)
                    return;
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingTokenNotProvided, spec.TokenParameters, spec.SecurityTokenAttachmentMode)));
            }
            switch (spec.SecurityTokenAttachmentMode)
            {
                case SecurityTokenAttachmentMode.Endorsing:
                    if (!tracker.IsEndorsing)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingTokenIsNotEndorsing, spec.TokenParameters)));
                    }
                    if (this.EnforceDerivedKeyRequirement && spec.TokenParameters.RequireDerivedKeys && !spec.TokenParameters.HasAsymmetricKey && !tracker.IsDerivedFrom)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingSignatureIsNotDerivedFrom, spec.TokenParameters)));
                    }
                    EnsureSupportingTokens(ref endorsingTokens).Add(tracker.token);
                    break;
                case SecurityTokenAttachmentMode.Signed:
                    if (!tracker.IsSigned && this.RequireMessageProtection)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingTokenIsNotSigned, spec.TokenParameters)));
                    }
                    EnsureSupportingTokens(ref signedTokens).Add(tracker.token);
                    break;
                case SecurityTokenAttachmentMode.SignedEncrypted:
                    if (!tracker.IsSigned && this.RequireMessageProtection)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingTokenIsNotSigned, spec.TokenParameters)));
                    }
                    if (!tracker.IsEncrypted && this.RequireMessageProtection)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingTokenIsNotEncrypted, spec.TokenParameters)));
                    }
                    EnsureSupportingTokens(ref basicTokens).Add(tracker.token);
                    break;
                case SecurityTokenAttachmentMode.SignedEndorsing:
                    if (!tracker.IsSigned && this.RequireMessageProtection)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingTokenIsNotSigned, spec.TokenParameters)));
                    }
                    if (!tracker.IsEndorsing)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingTokenIsNotEndorsing, spec.TokenParameters)));
                    }
                    if (this.EnforceDerivedKeyRequirement && spec.TokenParameters.RequireDerivedKeys && !spec.TokenParameters.HasAsymmetricKey && !tracker.IsDerivedFrom)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SupportingSignatureIsNotDerivedFrom, spec.TokenParameters)));
                    }
                    EnsureSupportingTokens(ref signedEndorsingTokens).Add(tracker.token);
                    break;

                default:
                    Fx.Assert("Unknown token attachment mode " + spec.SecurityTokenAttachmentMode);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnknownTokenAttachmentMode, spec.SecurityTokenAttachmentMode)));
            }
        }

        // replay detection done if enableReplayDetection is set to true.
        public void SetTimeParameters(NonceCache nonceCache, TimeSpan replayWindow, TimeSpan clockSkew)
        {
            this.nonceCache = nonceCache;
            this.replayWindow = replayWindow;
            this.clockSkew = clockSkew;
        }

        public void Process(TimeSpan timeout, ChannelBinding channelBinding, ExtendedProtectionPolicy extendedProtectionPolicy)
        {
            Fx.Assert(this.ReaderQuotas != null, "Reader quotas must be set before processing");
            MessageProtectionOrder actualProtectionOrder = this.protectionOrder;
            bool wasProtectionOrderDowngraded = false;
            if (this.protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature)
            {
                if (this.RequiredEncryptionParts == null || !this.RequiredEncryptionParts.IsBodyIncluded)
                {
                    // Let's downgrade for now. If after signature verification we find a header that 
                    // is signed and encrypted, we will check for signature encryption too.
                    actualProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
                    wasProtectionOrderDowngraded = true;
                }
            }

            this.channelBinding = channelBinding;
            this.extendedProtectionPolicy = extendedProtectionPolicy;
            this.orderTracker.SetRequiredProtectionOrder(actualProtectionOrder);

            SetProcessingStarted();
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.Message = this.securityVerifiedMessage = new SecurityVerifiedMessage(this.Message, this);
            XmlDictionaryReader reader = CreateSecurityHeaderReader();
            reader.MoveToStartElement();
            if (reader.IsEmptyElement)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SecurityHeaderIsEmpty)), this.Message);
            }
            if (this.RequireMessageProtection)
            {
                this.securityElementAttributes = XmlAttributeHolder.ReadAttributes(reader);
            }
            else
            {
                this.securityElementAttributes = XmlAttributeHolder.emptyArray;
            }
            reader.ReadStartElement();

            if (this.primaryTokenParameters != null)
            {
                this.primaryTokenTracker = new TokenTracker(null, this.outOfBandPrimaryToken, this.allowFirstTokenMismatch);
            }
            // universalTokenResolver is used for resolving tokens
            universalTokenResolver = new SecurityHeaderTokenResolver(this);
            // primary token resolver is used for resolving primary signature and decryption
            primaryTokenResolver = new SecurityHeaderTokenResolver(this);
            if (this.outOfBandPrimaryToken != null)
            {
                universalTokenResolver.Add(this.outOfBandPrimaryToken, SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
                primaryTokenResolver.Add(this.outOfBandPrimaryToken, SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
            }
            else if (this.outOfBandPrimaryTokenCollection != null)
            {
                for (int i = 0; i < this.outOfBandPrimaryTokenCollection.Count; ++i)
                {
                    universalTokenResolver.Add(this.outOfBandPrimaryTokenCollection[i], SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
                    primaryTokenResolver.Add(this.outOfBandPrimaryTokenCollection[i], SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
                }
            }
            if (this.wrappingToken != null)
            {
                universalTokenResolver.ExpectedWrapper = this.wrappingToken;
                universalTokenResolver.ExpectedWrapperTokenParameters = this.wrappingTokenParameters;
                primaryTokenResolver.ExpectedWrapper = this.wrappingToken;
                primaryTokenResolver.ExpectedWrapperTokenParameters = this.wrappingTokenParameters;
            }
            else if (expectedEncryptionToken != null)
            {
                universalTokenResolver.Add(expectedEncryptionToken, SecurityTokenReferenceStyle.External, expectedEncryptionTokenParameters);
                primaryTokenResolver.Add(expectedEncryptionToken, SecurityTokenReferenceStyle.External, expectedEncryptionTokenParameters);
            }

            if (this.outOfBandTokenResolver == null)
            {
                this.combinedUniversalTokenResolver = this.universalTokenResolver;
                this.combinedPrimaryTokenResolver = this.primaryTokenResolver;
            }
            else
            {
                this.combinedUniversalTokenResolver = new AggregateSecurityHeaderTokenResolver(this.universalTokenResolver, this.outOfBandTokenResolver);
                this.combinedPrimaryTokenResolver = new AggregateSecurityHeaderTokenResolver(this.primaryTokenResolver, this.outOfBandTokenResolver);
            }

            allowedAuthenticators = new List<SecurityTokenAuthenticator>();
            if (this.primaryTokenAuthenticator != null)
            {
                allowedAuthenticators.Add(this.primaryTokenAuthenticator);
            }
            if (this.DerivedTokenAuthenticator != null)
            {
                allowedAuthenticators.Add(this.DerivedTokenAuthenticator);
            }
            pendingSupportingTokenAuthenticator = null;
            int numSupportingTokensRequiringDerivation = 0;
            if (this.supportingTokenAuthenticators != null && this.supportingTokenAuthenticators.Count > 0)
            {
                this.supportingTokenTrackers = new List<TokenTracker>(this.supportingTokenAuthenticators.Count);
                for (int i = 0; i < this.supportingTokenAuthenticators.Count; ++i)
                {
                    SupportingTokenAuthenticatorSpecification spec = this.supportingTokenAuthenticators[i];
                    switch (spec.SecurityTokenAttachmentMode)
                    {
                        case SecurityTokenAttachmentMode.Endorsing:
                            this.hasEndorsingOrSignedEndorsingSupportingTokens = true;
                            break;
                        case SecurityTokenAttachmentMode.Signed:
                            this.hasAtLeastOneSupportingTokenExpectedToBeSigned = true;
                            break;
                        case SecurityTokenAttachmentMode.SignedEndorsing:
                            this.hasEndorsingOrSignedEndorsingSupportingTokens = true;
                            this.hasAtLeastOneSupportingTokenExpectedToBeSigned = true;
                            break;
                        case SecurityTokenAttachmentMode.SignedEncrypted:
                            this.hasAtLeastOneSupportingTokenExpectedToBeSigned = true;
                            break;
                    }

                    if ((this.primaryTokenAuthenticator != null) && (this.primaryTokenAuthenticator.GetType().Equals(spec.TokenAuthenticator.GetType())))
                    {
                        pendingSupportingTokenAuthenticator = spec.TokenAuthenticator;
                    }
                    else
                    {
                        allowedAuthenticators.Add(spec.TokenAuthenticator);
                    }
                    if (spec.TokenParameters.RequireDerivedKeys && !spec.TokenParameters.HasAsymmetricKey &&
                        (spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing || spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing))
                    {
                        ++numSupportingTokensRequiringDerivation;
                    }
                    this.supportingTokenTrackers.Add(new TokenTracker(spec));
                }
            }

            if (this.DerivedTokenAuthenticator != null)
            {
                // we expect key derivation. Compute quotas for derived keys
                int maxKeyDerivationLengthInBits = this.AlgorithmSuite.DefaultEncryptionKeyDerivationLength >= this.AlgorithmSuite.DefaultSignatureKeyDerivationLength ?
                    this.AlgorithmSuite.DefaultEncryptionKeyDerivationLength : this.AlgorithmSuite.DefaultSignatureKeyDerivationLength;
                this.maxDerivedKeyLength = maxKeyDerivationLengthInBits / 8;
                // the upper bound of derived keys is (1 for primary signature + 1 for encryption + supporting token signatures requiring derivation)*2
                // the multiplication by 2 is to take care of interop scenarios that may arise that require more derived keys than the lower bound.
                this.maxDerivedKeys = (1 + 1 + numSupportingTokensRequiringDerivation) * 2;
            }

            SecurityHeaderElementInferenceEngine engine = SecurityHeaderElementInferenceEngine.GetInferenceEngine(this.Layout);
            engine.ExecuteProcessingPasses(this, reader);
            if (this.RequireMessageProtection)
            {
                this.ElementManager.EnsureAllRequiredSecurityHeaderTargetsWereProtected();
                ExecuteMessageProtectionPass(this.hasAtLeastOneSupportingTokenExpectedToBeSigned);
                if (this.RequiredSignatureParts != null && this.SignatureToken == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.RequiredSignatureMissing)), this.Message);
                }
            }

            EnsureDecryptionComplete();

            this.signatureTracker.SetDerivationSourceIfRequired();
            this.encryptionTracker.SetDerivationSourceIfRequired();
            if (this.EncryptionToken != null)
            {
                if (wrappingToken != null)
                {
                    if (!(this.EncryptionToken is WrappedKeySecurityToken) || ((WrappedKeySecurityToken)this.EncryptionToken).WrappingToken != this.wrappingToken)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.EncryptedKeyWasNotEncryptedWithTheRequiredEncryptingToken, this.wrappingToken)));
                    }
                }
                else if (expectedEncryptionToken != null)
                {
                    if (this.EncryptionToken != expectedEncryptionToken)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.MessageWasNotEncryptedWithTheRequiredEncryptingToken)));
                    }
                }
                else if (this.SignatureToken != null && this.EncryptionToken != this.SignatureToken)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SignatureAndEncryptionTokenMismatch, this.SignatureToken, this.EncryptionToken)));
                }
            }

            // ensure that the primary signature was signed with derived keys if required
            if (this.EnforceDerivedKeyRequirement)
            {
                if (this.SignatureToken != null)
                {
                    if (this.primaryTokenParameters != null)
                    {
                        if (this.primaryTokenParameters.RequireDerivedKeys && !this.primaryTokenParameters.HasAsymmetricKey && !this.primaryTokenTracker.IsDerivedFrom)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.PrimarySignatureWasNotSignedByDerivedKey, this.primaryTokenParameters)));
                        }
                    }
                    else if (this.wrappingTokenParameters != null && this.wrappingTokenParameters.RequireDerivedKeys)
                    {
                        if (!this.signatureTracker.IsDerivedToken)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.PrimarySignatureWasNotSignedByDerivedWrappedKey, this.wrappingTokenParameters)));
                        }
                    }
                }

                // verify that the encryption is using key derivation
                if (this.EncryptionToken != null)
                {
                    if (wrappingTokenParameters != null)
                    {
                        if (wrappingTokenParameters.RequireDerivedKeys && !this.encryptionTracker.IsDerivedToken)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.MessageWasNotEncryptedByDerivedWrappedKey, this.wrappingTokenParameters)));
                        }
                    }
                    else if (expectedEncryptionTokenParameters != null)
                    {
                        if (expectedEncryptionTokenParameters.RequireDerivedKeys && !this.encryptionTracker.IsDerivedToken)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.MessageWasNotEncryptedByDerivedEncryptionToken, this.expectedEncryptionTokenParameters)));
                        }
                    }
                    else if (primaryTokenParameters != null && !primaryTokenParameters.HasAsymmetricKey && primaryTokenParameters.RequireDerivedKeys && !this.encryptionTracker.IsDerivedToken)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.MessageWasNotEncryptedByDerivedEncryptionToken, this.primaryTokenParameters)));
                    }
                }
            }

            if (wasProtectionOrderDowngraded && (this.BasicSupportingTokens != null) && (this.BasicSupportingTokens.Count > 0))
            {
                // Basic tokens are always signed and encrypted. So check if Signatures 
                // are encrypted as well.
                this.VerifySignatureEncryption();
            }

            // verify all supporting token parameters have their requirements met
            if (this.supportingTokenTrackers != null)
            {
                for (int i = 0; i < this.supportingTokenTrackers.Count; ++i)
                {
                    VerifySupportingToken(this.supportingTokenTrackers[i]);
                }
            }

            if (this.replayDetectionEnabled)
            {
                if (this.timestamp == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.NoTimestampAvailableInSecurityHeaderToDoReplayDetection)), this.Message);
                }
                if (this.primarySignatureValue == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.NoSignatureAvailableInSecurityHeaderToDoReplayDetection)), this.Message);
                }

                AddNonce(this.nonceCache, this.primarySignatureValue);

                // if replay detection is on, redo creation range checks to ensure full coverage
                this.timestamp.ValidateFreshness(this.replayWindow, this.clockSkew);
            }

            if (this.ExpectSignatureConfirmation)
            {
                this.ElementManager.VerifySignatureConfirmationWasFound();
            }

            MarkHeaderAsUnderstood();
        }

        static void AddNonce(NonceCache cache, byte[] nonce)
        {
            if (!cache.TryAddNonce(nonce))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.InvalidOrReplayedNonce), true));
            }
        }

        static void CheckNonce(NonceCache cache, byte[] nonce)
        {
            if (cache.CheckNonce(nonce))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.InvalidOrReplayedNonce), true));
            }
        }

        protected abstract void EnsureDecryptionComplete();

        protected abstract void ExecuteMessageProtectionPass(bool hasAtLeastOneSupportingTokenExpectedToBeSigned);

        internal void ExecuteSignatureEncryptionProcessingPass()
        {
            for (int position = 0; position < this.elementManager.Count; position++)
            {
                ReceiveSecurityHeaderEntry entry;
                this.elementManager.GetElementEntry(position, out entry);
                switch (entry.elementCategory)
                {
                    case ReceiveSecurityHeaderElementCategory.Signature:
                        if (entry.bindingMode == ReceiveSecurityHeaderBindingModes.Primary)
                        {
                            ProcessPrimarySignature((SignedXml)entry.element, entry.encrypted);
                        }
                        else
                        {
                            ProcessSupportingSignature((SignedXml)entry.element, entry.encrypted);
                        }
                        break;
                    case ReceiveSecurityHeaderElementCategory.ReferenceList:
                        ProcessReferenceList((ReferenceList)entry.element);
                        break;
                    case ReceiveSecurityHeaderElementCategory.Token:
                        WrappedKeySecurityToken wrappedKeyToken = entry.element as WrappedKeySecurityToken;
                        if ((wrappedKeyToken != null) && (wrappedKeyToken.ReferenceList != null))
                        {
                            Fx.Assert(this.Layout != SecurityHeaderLayout.Strict, "Invalid Calling sequence. This method assumes it will be called only during Lax mode.");
                            // ExecuteSignatureEncryptionProcessingPass is called only durng Lax mode. In this
                            // case when we have a EncryptedKey with a ReferencList inside it, we would not 
                            // have processed the ReferenceList during reading pass. Process this here.
                            ProcessReferenceList(wrappedKeyToken.ReferenceList, wrappedKeyToken);
                        }
                        break;
                    case ReceiveSecurityHeaderElementCategory.Timestamp:
                    case ReceiveSecurityHeaderElementCategory.EncryptedKey:
                    case ReceiveSecurityHeaderElementCategory.EncryptedData:
                    case ReceiveSecurityHeaderElementCategory.SignatureConfirmation:
                    case ReceiveSecurityHeaderElementCategory.SecurityTokenReference:
                        // no op
                        break;
                    default:
                        Fx.Assert("invalid element category");
                        break;
                }
            }
        }

        internal void ExecuteSubheaderDecryptionPass()
        {
            for (int position = 0; position < this.elementManager.Count; position++)
            {
                if (this.elementManager.GetElementCategory(position) == ReceiveSecurityHeaderElementCategory.EncryptedData)
                {
                    EncryptedData encryptedData = this.elementManager.GetElement<EncryptedData>(position);
                    bool dummy = false;
                    ProcessEncryptedData(encryptedData, this.timeoutHelper.RemainingTime(), position, false, ref dummy);
                }
            }
        }

        internal void ExecuteReadingPass(XmlDictionaryReader reader)
        {
            int position = 0;
            while (reader.IsStartElement())
            {
                if (IsReaderAtSignature(reader))
                {
                    ReadSignature(reader, AppendPosition, null);
                }
                else if (IsReaderAtReferenceList(reader))
                {
                    ReadReferenceList(reader);
                }
                else if (this.StandardsManager.WSUtilitySpecificationVersion.IsReaderAtTimestamp(reader))
                {
                    ReadTimestamp(reader);
                }
                else if (IsReaderAtEncryptedKey(reader))
                {
                    ReadEncryptedKey(reader, false);
                }
                else if (IsReaderAtEncryptedData(reader))
                {
                    ReadEncryptedData(reader);
                }
                else if (this.StandardsManager.SecurityVersion.IsReaderAtSignatureConfirmation(reader))
                {
                    ReadSignatureConfirmation(reader, AppendPosition, null);
                }
                else if (IsReaderAtSecurityTokenReference(reader))
                {
                    ReadSecurityTokenReference(reader);
                }
                else
                {
                    ReadToken(reader, AppendPosition, null, null, null, this.timeoutHelper.RemainingTime());
                }
                position++;
            }

            reader.ReadEndElement(); // wsse:Security
            reader.Close();
        }

        internal void ExecuteFullPass(XmlDictionaryReader reader)
        {
            bool primarySignatureFound = !this.RequireMessageProtection;
            int position = 0;
            while (reader.IsStartElement())
            {
                if (IsReaderAtSignature(reader))
                {
                    SignedXml signedXml = ReadSignature(reader, AppendPosition, null);
                    if (primarySignatureFound)
                    {
                        this.elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Endorsing);
                        ProcessSupportingSignature(signedXml, false);
                    }
                    else
                    {
                        primarySignatureFound = true;
                        this.elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Primary);
                        ProcessPrimarySignature(signedXml, false);
                    }
                }
                else if (IsReaderAtReferenceList(reader))
                {
                    ReferenceList referenceList = ReadReferenceList(reader);
                    ProcessReferenceList(referenceList);
                }
                else if (this.StandardsManager.WSUtilitySpecificationVersion.IsReaderAtTimestamp(reader))
                {
                    ReadTimestamp(reader);
                }
                else if (IsReaderAtEncryptedKey(reader))
                {
                    ReadEncryptedKey(reader, true);
                }
                else if (IsReaderAtEncryptedData(reader))
                {
                    EncryptedData encryptedData = ReadEncryptedData(reader);
                    ProcessEncryptedData(encryptedData, this.timeoutHelper.RemainingTime(), position, true, ref primarySignatureFound);
                }
                else if (this.StandardsManager.SecurityVersion.IsReaderAtSignatureConfirmation(reader))
                {
                    ReadSignatureConfirmation(reader, AppendPosition, null);
                }
                else if (IsReaderAtSecurityTokenReference(reader))
                {
                    ReadSecurityTokenReference(reader);
                }
                else
                {
                    ReadToken(reader, AppendPosition, null, null, null, this.timeoutHelper.RemainingTime());
                }
                position++;
            }

            reader.ReadEndElement(); // wsse:Security
            reader.Close();
        }

        internal void EnsureDerivedKeyLimitNotReached()
        {
            ++this.numDerivedKeys;
            if (this.numDerivedKeys > this.maxDerivedKeys)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.DerivedKeyLimitExceeded, maxDerivedKeys)));
            }
        }

        internal void ExecuteDerivedKeyTokenStubPass(bool isFinalPass)
        {
            for (int position = 0; position < this.elementManager.Count; position++)
            {
                if (this.elementManager.GetElementCategory(position) == ReceiveSecurityHeaderElementCategory.Token)
                {
                    DerivedKeySecurityTokenStub stub = this.elementManager.GetElement(position) as DerivedKeySecurityTokenStub;
                    if (stub != null)
                    {
                        SecurityToken sourceToken = null;
                        this.universalTokenResolver.TryResolveToken(stub.TokenToDeriveIdentifier, out sourceToken);
                        if (sourceToken != null)
                        {
                            EnsureDerivedKeyLimitNotReached();
                            DerivedKeySecurityToken derivedKeyToken = stub.CreateToken(sourceToken, this.maxDerivedKeyLength);
                            this.elementManager.SetElement(position, derivedKeyToken);
                            AddDerivedKeyTokenToResolvers(derivedKeyToken);
                        }
                        else if (isFinalPass)
                        {
                            throw TraceUtility.ThrowHelperError(new MessageSecurityException(
                                SR.GetString(SR.UnableToResolveKeyInfoClauseInDerivedKeyToken, stub.TokenToDeriveIdentifier)), this.Message);
                        }
                    }
                }
            }
        }

        SecurityToken GetRootToken(SecurityToken token)
        {
            if (token is DerivedKeySecurityToken)
            {
                return ((DerivedKeySecurityToken)token).TokenToDerive;
            }
            else
            {
                return token;
            }
        }

        void RecordEncryptionTokenAndRemoveReferenceListEntry(string id, SecurityToken encryptionToken)
        {
            if (id == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MissingIdInEncryptedElement)), this.Message);
            }

            OnDecryptionOfSecurityHeaderItemRequiringReferenceListEntry(id);
            RecordEncryptionToken(encryptionToken);
        }

        EncryptedData ReadEncryptedData(XmlDictionaryReader reader)
        {
            EncryptedData encryptedData = ReadSecurityHeaderEncryptedItem(reader, this.MessageDirection == MessageDirection.Output);

            this.elementManager.AppendEncryptedData(encryptedData);
            return encryptedData;
        }

        internal XmlDictionaryReader CreateDecryptedReader(byte[] decryptedBuffer)
        {
            return ContextImportHelper.CreateSplicedReader(
                decryptedBuffer,
                this.SecurityVerifiedMessage.GetEnvelopeAttributes(),
                this.SecurityVerifiedMessage.GetHeaderAttributes(),
                this.securityElementAttributes,
                this.ReaderQuotas
                );
        }

        void ProcessEncryptedData(EncryptedData encryptedData, TimeSpan timeout, int position, bool eagerMode, ref bool primarySignatureFound)
        {
            if (TD.EncryptedDataProcessingStartIsEnabled())
            {
                TD.EncryptedDataProcessingStart(this.EventTraceActivity);
            }

            string id = encryptedData.Id;

            SecurityToken encryptionToken;
            byte[] decryptedBuffer = DecryptSecurityHeaderElement(encryptedData, this.wrappedKeyToken, out encryptionToken);

            XmlDictionaryReader decryptedReader = CreateDecryptedReader(decryptedBuffer);

            if (IsReaderAtSignature(decryptedReader))
            {
                RecordEncryptionTokenAndRemoveReferenceListEntry(id, encryptionToken);
                SignedXml signedXml = ReadSignature(decryptedReader, position, decryptedBuffer);
                if (eagerMode)
                {
                    if (primarySignatureFound)
                    {
                        this.elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Endorsing);
                        ProcessSupportingSignature(signedXml, true);
                    }
                    else
                    {
                        primarySignatureFound = true;
                        this.elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Primary);
                        ProcessPrimarySignature(signedXml, true);
                    }
                }
            }
            else if (this.StandardsManager.SecurityVersion.IsReaderAtSignatureConfirmation(decryptedReader))
            {
                RecordEncryptionTokenAndRemoveReferenceListEntry(id, encryptionToken);
                ReadSignatureConfirmation(decryptedReader, position, decryptedBuffer);
            }
            else
            {
                if (IsReaderAtEncryptedData(decryptedReader))
                {

                    // The purpose of this code is to process a token that arrived at a client as encryptedData.

                    // This is a common scenario for supporting tokens.

                    // We pass readXmlReferenceKeyIdentifierClause as false here because we do not expect the client 
                    // to receive an encrypted token for itself from the service. The encrypted token is encrypted for some other service. 
                    // Hence we assume that the KeyInfoClause entry in it is not an XMLReference entry that the client is supposed to understand.

                    // What if the service sends its authentication token as an EncryptedData to the client?

                    EncryptedData ed = ReadSecurityHeaderEncryptedItem(decryptedReader, false);
                    SecurityToken securityToken;
                    byte[] db = DecryptSecurityHeaderElement(ed, this.wrappedKeyToken, out securityToken);
                    XmlDictionaryReader dr = CreateDecryptedReader(db);


                    // read the actual token and put it into the system
                    ReadToken(dr, position, db, encryptionToken, id, timeout);

                    ReceiveSecurityHeaderEntry rshe;
                    this.ElementManager.GetElementEntry(position, out rshe);

                    // In EncryptBeforeSignMode, we have encrypted the outer token, remember the right id.
                    // The reason why I have both id's is in that case that one or the other is passed
                    // we won't have a problem with which one.  SHP accounting should ensure each item has 
                    // the correct hash.
                    if (this.EncryptBeforeSignMode)
                    {
                        rshe.encryptedFormId = encryptedData.Id;
                        rshe.encryptedFormWsuId = encryptedData.WsuId;
                    }
                    else
                    {
                        rshe.encryptedFormId = ed.Id;
                        rshe.encryptedFormWsuId = ed.WsuId;
                    }

                    rshe.decryptedBuffer = decryptedBuffer;

                    // setting this to true, will allow a different id match in ReceiveSecurityHeaderEntry.Match
                    // to one of the ids set above as the token id will not match what the signature reference is looking for.

                    rshe.doubleEncrypted = true;

                    this.ElementManager.ReplaceHeaderEntry(position, rshe);
                }
                else
                    ReadToken(decryptedReader, position, decryptedBuffer, encryptionToken, id, timeout);
            }

            if (TD.EncryptedDataProcessingSuccessIsEnabled())
            {
                TD.EncryptedDataProcessingSuccess(this.EventTraceActivity);
            }
        }

        void ReadEncryptedKey(XmlDictionaryReader reader, bool processReferenceListIfPresent)
        {
            this.orderTracker.OnEncryptedKey();

            WrappedKeySecurityToken wrappedKeyToken = DecryptWrappedKey(reader);
            if (wrappedKeyToken.WrappingToken != this.wrappingToken)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.EncryptedKeyWasNotEncryptedWithTheRequiredEncryptingToken, this.wrappingToken)));
            }
            this.universalTokenResolver.Add(wrappedKeyToken);
            this.primaryTokenResolver.Add(wrappedKeyToken);
            if (wrappedKeyToken.ReferenceList != null)
            {
                if (!this.EncryptedKeyContainsReferenceList)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptedKeyWithReferenceListNotAllowed)));
                }
                if (!this.ExpectEncryption)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptionNotExpected)), this.Message);
                }
                if (processReferenceListIfPresent)
                {
                    ProcessReferenceList(wrappedKeyToken.ReferenceList, wrappedKeyToken);
                }
                this.wrappedKeyToken = wrappedKeyToken;
            }
            this.elementManager.AppendToken(wrappedKeyToken, ReceiveSecurityHeaderBindingModes.Primary, null);
        }

        ReferenceList ReadReferenceList(XmlDictionaryReader reader)
        {
            if (!this.ExpectEncryption)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptionNotExpected)), this.Message);
            }
            ReferenceList referenceList = ReadReferenceListCore(reader);
            this.elementManager.AppendReferenceList(referenceList);
            return referenceList;
        }

        protected abstract ReferenceList ReadReferenceListCore(XmlDictionaryReader reader);

        void ProcessReferenceList(ReferenceList referenceList)
        {
            ProcessReferenceList(referenceList, null);
        }

        void ProcessReferenceList(ReferenceList referenceList, WrappedKeySecurityToken wrappedKeyToken)
        {
            this.orderTracker.OnProcessReferenceList();
            ProcessReferenceListCore(referenceList, wrappedKeyToken);
        }

        protected abstract void ProcessReferenceListCore(ReferenceList referenceList, WrappedKeySecurityToken wrappedKeyToken);

        SignedXml ReadSignature(XmlDictionaryReader reader, int position, byte[] decryptedBuffer)
        {
            Fx.Assert((position == AppendPosition) == (decryptedBuffer == null), "inconsistent position, decryptedBuffer parameters");
            if (!this.ExpectSignature)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SignatureNotExpected)), this.Message);
            }
            SignedXml signedXml = ReadSignatureCore(reader);
            signedXml.Signature.SignedInfo.ReaderProvider = this.ElementManager;
            int readerIndex;
            if (decryptedBuffer == null)
            {
                this.elementManager.AppendSignature(signedXml);
                readerIndex = this.elementManager.Count - 1;
            }
            else
            {
                this.elementManager.SetSignatureAfterDecryption(position, signedXml, decryptedBuffer);
                readerIndex = position;
            }
            signedXml.Signature.SignedInfo.SignatureReaderProviderCallbackContext = (object)(readerIndex);
            return signedXml;
        }

        protected abstract void ReadSecurityTokenReference(XmlDictionaryReader reader);

        void ProcessPrimarySignature(SignedXml signedXml, bool isFromDecryptedSource)
        {
            this.orderTracker.OnProcessSignature(isFromDecryptedSource);

            this.primarySignatureValue = signedXml.GetSignatureValue();
            if (this.replayDetectionEnabled)
            {
                CheckNonce(this.nonceCache, this.primarySignatureValue);
            }

            SecurityToken signingToken = VerifySignature(signedXml, true, this.primaryTokenResolver, null, null);
            // verify that the signing token is the same as the primary token
            SecurityToken rootSigningToken = GetRootToken(signingToken);
            bool isDerivedKeySignature = signingToken is DerivedKeySecurityToken;
            if (this.primaryTokenTracker != null)
            {
                this.primaryTokenTracker.RecordToken(rootSigningToken);
                this.primaryTokenTracker.IsDerivedFrom = isDerivedKeySignature;
            }
            this.AddIncomingSignatureValue(signedXml.GetSignatureValue(), isFromDecryptedSource);
        }

        void ReadSignatureConfirmation(XmlDictionaryReader reader, int position, byte[] decryptedBuffer)
        {
            Fx.Assert((position == AppendPosition) == (decryptedBuffer == null), "inconsistent position, decryptedBuffer parameters");
            if (!this.ExpectSignatureConfirmation)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SignatureConfirmationsNotExpected)), this.Message);
            }
            if (this.orderTracker.PrimarySignatureDone)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SignatureConfirmationsOccursAfterPrimarySignature)), this.Message);
            }
            ISignatureValueSecurityElement sigConfElement = this.StandardsManager.SecurityVersion.ReadSignatureConfirmation(reader);
            if (decryptedBuffer == null)
            {
                this.AddIncomingSignatureConfirmation(sigConfElement.GetSignatureValue(), false);
                this.elementManager.AppendSignatureConfirmation(sigConfElement);
            }
            else
            {
                this.AddIncomingSignatureConfirmation(sigConfElement.GetSignatureValue(), true);
                this.elementManager.SetSignatureConfirmationAfterDecryption(position, sigConfElement, decryptedBuffer);
            }
        }

        TokenTracker GetSupportingTokenTracker(SecurityToken token)
        {
            if (this.supportingTokenTrackers == null)
                return null;
            for (int i = 0; i < this.supportingTokenTrackers.Count; ++i)
            {
                if (supportingTokenTrackers[i].token == token)
                    return supportingTokenTrackers[i];
            }
            return null;
        }

        protected TokenTracker GetSupportingTokenTracker(SecurityTokenAuthenticator tokenAuthenticator, out SupportingTokenAuthenticatorSpecification spec)
        {
            spec = null;
            if (this.supportingTokenAuthenticators == null)
                return null;
            for (int i = 0; i < this.supportingTokenAuthenticators.Count; ++i)
            {
                if (supportingTokenAuthenticators[i].TokenAuthenticator == tokenAuthenticator)
                {
                    spec = supportingTokenAuthenticators[i];
                    return supportingTokenTrackers[i];
                }
            }
            return null;
        }

        protected TAuthenticator FindAllowedAuthenticator<TAuthenticator>(bool removeIfPresent)
            where TAuthenticator : SecurityTokenAuthenticator
        {
            if (this.allowedAuthenticators == null)
            {
                return null;
            }
            for (int i = 0; i < this.allowedAuthenticators.Count; ++i)
            {
                if (allowedAuthenticators[i] is TAuthenticator)
                {
                    TAuthenticator result = (TAuthenticator)allowedAuthenticators[i];
                    if (removeIfPresent)
                    {
                        this.allowedAuthenticators.RemoveAt(i);
                    }
                    return result;
                }
            }
            return null;
        }

        void ProcessSupportingSignature(SignedXml signedXml, bool isFromDecryptedSource)
        {
            if (!this.ExpectEndorsingTokens)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SupportingTokenSignaturesNotExpected)), this.Message);
            }
            string id;
            XmlDictionaryReader reader;
            object signatureTarget;
            if (!this.RequireMessageProtection)
            {
                if (this.timestamp == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.SigningWithoutPrimarySignatureRequiresTimestamp)), this.Message);
                }
                reader = null;
                id = this.timestamp.Id;
                // We would have pre-computed the timestamp digest, if the transport reader
                // was capable of canonicalization. If we were not able to compute the digest
                // before hand then the signature verification step will get a new reader
                // and will recompute the digest.
                signatureTarget = null;
            }
            else
            {
                this.elementManager.GetPrimarySignature(out reader, out id);
                if (reader == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.NoPrimarySignatureAvailableForSupportingTokenSignatureVerification)), this.Message);
                }
                signatureTarget = reader;
            }
            SecurityToken signingToken = VerifySignature(signedXml, false, this.universalTokenResolver, signatureTarget, id);
            if (reader != null)
            {
                reader.Close();
            }
            if (signingToken == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SignatureVerificationFailed)), this.Message);
            }
            SecurityToken rootSigningToken = GetRootToken(signingToken);
            TokenTracker tracker = GetSupportingTokenTracker(rootSigningToken);
            if (tracker == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.UnknownSupportingToken, signingToken)));
            }

            if (tracker.AlreadyReadEndorsingSignature)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MoreThanOneSupportingSignature, signingToken)));

            tracker.IsEndorsing = true;
            tracker.AlreadyReadEndorsingSignature = true;
            tracker.IsDerivedFrom = (signingToken is DerivedKeySecurityToken);
            AddIncomingSignatureValue(signedXml.GetSignatureValue(), isFromDecryptedSource);
        }

        void ReadTimestamp(XmlDictionaryReader reader)
        {
            if (this.timestamp != null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.DuplicateTimestampInSecurityHeader)), this.Message);
            }
            bool expectTimestampToBeSigned = this.RequireMessageProtection || this.hasEndorsingOrSignedEndorsingSupportingTokens;
            string expectedDigestAlgorithm = expectTimestampToBeSigned ? this.AlgorithmSuite.DefaultDigestAlgorithm : null;
            SignatureResourcePool resourcePool = expectTimestampToBeSigned ? this.ResourcePool : null;
            this.timestamp = this.StandardsManager.WSUtilitySpecificationVersion.ReadTimestamp(reader, expectedDigestAlgorithm, resourcePool);
            this.timestamp.ValidateRangeAndFreshness(this.replayWindow, this.clockSkew);
            this.elementManager.AppendTimestamp(this.timestamp);
        }

        bool IsPrimaryToken(SecurityToken token)
        {
            bool result = (token == outOfBandPrimaryToken
                || (primaryTokenTracker != null && token == primaryTokenTracker.token)
                || (token == expectedEncryptionToken)
                || ((token is WrappedKeySecurityToken) && ((WrappedKeySecurityToken)token).WrappingToken == this.wrappingToken));
            if (!result && this.outOfBandPrimaryTokenCollection != null)
            {
                for (int i = 0; i < this.outOfBandPrimaryTokenCollection.Count; ++i)
                {
                    if (this.outOfBandPrimaryTokenCollection[i] == token)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        void ReadToken(XmlDictionaryReader reader, int position, byte[] decryptedBuffer,
            SecurityToken encryptionToken, string idInEncryptedForm, TimeSpan timeout)
        {
            Fx.Assert((position == AppendPosition) == (decryptedBuffer == null), "inconsistent position, decryptedBuffer parameters");
            Fx.Assert((position == AppendPosition) == (encryptionToken == null), "inconsistent position, encryptionToken parameters");
            string localName = reader.LocalName;
            string namespaceUri = reader.NamespaceURI;
            string valueType = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);

            SecurityTokenAuthenticator usedTokenAuthenticator;
            SecurityToken token = ReadToken(reader, this.CombinedUniversalTokenResolver, allowedAuthenticators, out usedTokenAuthenticator);
            if (token == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCouldNotReadToken, localName, namespaceUri, valueType)), this.Message);
            }
            DerivedKeySecurityToken derivedKeyToken = token as DerivedKeySecurityToken;
            if (derivedKeyToken != null)
            {
                EnsureDerivedKeyLimitNotReached();
                derivedKeyToken.InitializeDerivedKey(this.maxDerivedKeyLength);
            }

            if ((usedTokenAuthenticator is SspiNegotiationTokenAuthenticator) ||
                (usedTokenAuthenticator == this.primaryTokenAuthenticator))
            {
                this.allowedAuthenticators.Remove(usedTokenAuthenticator);
            }

            ReceiveSecurityHeaderBindingModes mode;
            TokenTracker supportingTokenTracker = null;
            if (usedTokenAuthenticator == this.primaryTokenAuthenticator)
            {
                // this is the primary token. Add to resolver as such
                this.universalTokenResolver.Add(token, SecurityTokenReferenceStyle.Internal, this.primaryTokenParameters);
                this.primaryTokenResolver.Add(token, SecurityTokenReferenceStyle.Internal, this.primaryTokenParameters);
                if (this.pendingSupportingTokenAuthenticator != null)
                {
                    this.allowedAuthenticators.Add(this.pendingSupportingTokenAuthenticator);
                    this.pendingSupportingTokenAuthenticator = null;
                }
                this.primaryTokenTracker.RecordToken(token);
                mode = ReceiveSecurityHeaderBindingModes.Primary;
            }
            else if (usedTokenAuthenticator == this.DerivedTokenAuthenticator)
            {
                if (token is DerivedKeySecurityTokenStub)
                {
                    if (this.Layout == SecurityHeaderLayout.Strict)
                    {
                        DerivedKeySecurityTokenStub tmpToken = (DerivedKeySecurityTokenStub)token;
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(
                            SR.GetString(SR.UnableToResolveKeyInfoClauseInDerivedKeyToken, tmpToken.TokenToDeriveIdentifier)), this.Message);
                    }
                }
                else
                {
                    AddDerivedKeyTokenToResolvers(token);
                }
                mode = ReceiveSecurityHeaderBindingModes.Unknown;
            }
            else
            {
                SupportingTokenAuthenticatorSpecification supportingTokenSpec;
                supportingTokenTracker = GetSupportingTokenTracker(usedTokenAuthenticator, out supportingTokenSpec);
                if (supportingTokenTracker == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.UnknownTokenAuthenticatorUsedInTokenProcessing, usedTokenAuthenticator)));
                }
                if (supportingTokenTracker.token != null)
                {
                    supportingTokenTracker = new TokenTracker(supportingTokenSpec);
                    this.supportingTokenTrackers.Add(supportingTokenTracker);
                }

                supportingTokenTracker.RecordToken(token);
                if (encryptionToken != null)
                {
                    supportingTokenTracker.IsEncrypted = true;
                }

                bool isBasic;
                bool isSignedButNotBasic;
                SecurityTokenAttachmentModeHelper.Categorize(supportingTokenSpec.SecurityTokenAttachmentMode,
                   out isBasic, out isSignedButNotBasic, out mode);
                if (isBasic)
                {
                    if (!this.ExpectBasicTokens)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.BasicTokenNotExpected)));
                    }

                    // only basic tokens have to be part of the reference list. Encrypted Saml tokens dont for example
                    if (this.RequireMessageProtection && encryptionToken != null)
                    {
                        RecordEncryptionTokenAndRemoveReferenceListEntry(idInEncryptedForm, encryptionToken);
                    }
                }
                if (isSignedButNotBasic && !this.ExpectSignedTokens)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SignedSupportingTokenNotExpected)));
                }
                this.universalTokenResolver.Add(token, SecurityTokenReferenceStyle.Internal, supportingTokenSpec.TokenParameters);
            }
            if (position == AppendPosition)
            {
                this.elementManager.AppendToken(token, mode, supportingTokenTracker);
            }
            else
            {
                this.elementManager.SetTokenAfterDecryption(position, token, mode, decryptedBuffer, supportingTokenTracker);
            }
        }

        SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver tokenResolver, IList<SecurityTokenAuthenticator> allowedTokenAuthenticators, out SecurityTokenAuthenticator usedTokenAuthenticator)
        {
            SecurityToken token = this.StandardsManager.SecurityTokenSerializer.ReadToken(reader, tokenResolver);
            if (token is DerivedKeySecurityTokenStub)
            {
                if (this.DerivedTokenAuthenticator == null)
                {
                    // No Authenticator registered for DerivedKeySecurityToken
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.UnableToFindTokenAuthenticator, typeof(DerivedKeySecurityToken))));
                }

                // This is just the stub. Nothing to Validate. Set the usedTokenAuthenticator to 
                // DerivedKeySecurityTokenAuthenticator.
                usedTokenAuthenticator = this.DerivedTokenAuthenticator;
                return token;
            }

            for (int i = 0; i < allowedTokenAuthenticators.Count; ++i)
            {
                SecurityTokenAuthenticator tokenAuthenticator = allowedTokenAuthenticators[i];
                if (tokenAuthenticator.CanValidateToken(token))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
                    ServiceCredentialsSecurityTokenManager.KerberosSecurityTokenAuthenticatorWrapper kerbTokenAuthenticator =
                            tokenAuthenticator as ServiceCredentialsSecurityTokenManager.KerberosSecurityTokenAuthenticatorWrapper;
                    if (kerbTokenAuthenticator != null)
                    {
                        authorizationPolicies = kerbTokenAuthenticator.ValidateToken(token, this.channelBinding, this.extendedProtectionPolicy);
                    }
                    else
                    {
                        authorizationPolicies = tokenAuthenticator.ValidateToken(token);
                    }
                    SecurityTokenAuthorizationPoliciesMapping.Add(token, authorizationPolicies);
                    usedTokenAuthenticator = tokenAuthenticator;
                    return token;
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                SR.GetString(SR.UnableToFindTokenAuthenticator, token.GetType())));
        }


        void AddDerivedKeyTokenToResolvers(SecurityToken token)
        {
            this.universalTokenResolver.Add(token);
            // add it to the primary token resolver only if its root is primary
            SecurityToken rootToken = GetRootToken(token);
            if (IsPrimaryToken(rootToken))
            {
                primaryTokenResolver.Add(token);
            }
        }

        void AddIncomingSignatureConfirmation(byte[] signatureValue, bool isFromDecryptedSource)
        {
            if (this.MaintainSignatureConfirmationState)
            {
                if (this.receivedSignatureConfirmations == null)
                {
                    this.receivedSignatureConfirmations = new SignatureConfirmations();
                }
                this.receivedSignatureConfirmations.AddConfirmation(signatureValue, isFromDecryptedSource);
            }
        }

        void AddIncomingSignatureValue(byte[] signatureValue, bool isFromDecryptedSource)
        {
            // cache incoming signatures only on the server side
            if (this.MaintainSignatureConfirmationState && !this.ExpectSignatureConfirmation)
            {
                if (this.receivedSignatureValues == null)
                {
                    this.receivedSignatureValues = new SignatureConfirmations();
                }
                this.receivedSignatureValues.AddConfirmation(signatureValue, isFromDecryptedSource);
            }
        }

        protected void RecordEncryptionToken(SecurityToken token)
        {
            this.encryptionTracker.RecordToken(token);
        }

        protected void RecordSignatureToken(SecurityToken token)
        {
            this.signatureTracker.RecordToken(token);
        }

        public void SetRequiredProtectionOrder(MessageProtectionOrder protectionOrder)
        {
            ThrowIfProcessingStarted();
            this.protectionOrder = protectionOrder;
        }

        protected abstract SignedXml ReadSignatureCore(XmlDictionaryReader signatureReader);

        protected abstract SecurityToken VerifySignature(SignedXml signedXml, bool isPrimarySignature,
            SecurityHeaderTokenResolver resolver, object signatureTarget, string id);

        protected abstract bool TryDeleteReferenceListEntry(string id);

        struct OrderTracker
        {
            static readonly ReceiverProcessingOrder[] stateTransitionTableOnDecrypt = new ReceiverProcessingOrder[]
                {
                    ReceiverProcessingOrder.Decrypt, ReceiverProcessingOrder.VerifyDecrypt, ReceiverProcessingOrder.Decrypt,
                    ReceiverProcessingOrder.Mixed, ReceiverProcessingOrder.VerifyDecrypt, ReceiverProcessingOrder.Mixed
                };
            static readonly ReceiverProcessingOrder[] stateTransitionTableOnVerify = new ReceiverProcessingOrder[]
                {
                    ReceiverProcessingOrder.Verify, ReceiverProcessingOrder.Verify, ReceiverProcessingOrder.DecryptVerify,
                    ReceiverProcessingOrder.DecryptVerify, ReceiverProcessingOrder.Mixed, ReceiverProcessingOrder.Mixed
                };

            const int MaxAllowedWrappedKeys = 1;

            int referenceListCount;
            ReceiverProcessingOrder state;
            int signatureCount;
            int unencryptedSignatureCount;
            int numWrappedKeys;
            MessageProtectionOrder protectionOrder;
            bool enforce;

            public bool AllSignaturesEncrypted
            {
                get { return this.unencryptedSignatureCount == 0; }
            }

            public bool EncryptBeforeSignMode
            {
                get { return this.enforce && this.protectionOrder == MessageProtectionOrder.EncryptBeforeSign; }
            }

            public bool EncryptBeforeSignOrderRequirementMet
            {
                get { return this.state != ReceiverProcessingOrder.DecryptVerify && this.state != ReceiverProcessingOrder.Mixed; }
            }

            public bool PrimarySignatureDone
            {
                get { return this.signatureCount > 0; }
            }

            public bool SignBeforeEncryptOrderRequirementMet
            {
                get { return this.state != ReceiverProcessingOrder.VerifyDecrypt && this.state != ReceiverProcessingOrder.Mixed; }
            }

            void EnforceProtectionOrder()
            {
                switch (this.protectionOrder)
                {
                    case MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature:
                        if (!this.AllSignaturesEncrypted)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                                SR.GetString(SR.PrimarySignatureIsRequiredToBeEncrypted)));
                        }
                        goto case MessageProtectionOrder.SignBeforeEncrypt;
                    case MessageProtectionOrder.SignBeforeEncrypt:
                        if (!this.SignBeforeEncryptOrderRequirementMet)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                                SR.GetString(SR.MessageProtectionOrderMismatch, this.protectionOrder)));
                        }
                        break;
                    case MessageProtectionOrder.EncryptBeforeSign:
                        if (!this.EncryptBeforeSignOrderRequirementMet)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                                SR.GetString(SR.MessageProtectionOrderMismatch, this.protectionOrder)));
                        }
                        break;
                    default:
                        Fx.Assert("");
                        break;
                }
            }

            public void OnProcessReferenceList()
            {
                Fx.Assert(this.enforce, "OrderTracker should have 'enforce' set to true.");
                if (this.referenceListCount > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.AtMostOneReferenceListIsSupportedWithDefaultPolicyCheck)));
                }
                this.referenceListCount++;
                this.state = stateTransitionTableOnDecrypt[(int)this.state];
                if (this.enforce)
                {
                    EnforceProtectionOrder();
                }
            }

            public void OnProcessSignature(bool isEncrypted)
            {
                Fx.Assert(this.enforce, "OrderTracker should have 'enforce' set to true.");
                if (this.signatureCount > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.AtMostOneSignatureIsSupportedWithDefaultPolicyCheck)));
                }
                this.signatureCount++;
                if (!isEncrypted)
                {
                    this.unencryptedSignatureCount++;
                }
                this.state = stateTransitionTableOnVerify[(int)this.state];
                if (this.enforce)
                {
                    EnforceProtectionOrder();
                }
            }

            public void OnEncryptedKey()
            {
                Fx.Assert(this.enforce, "OrderTracker should have 'enforce' set to true.");

                if (this.numWrappedKeys == MaxAllowedWrappedKeys)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.WrappedKeyLimitExceeded, this.numWrappedKeys)));

                this.numWrappedKeys++;
            }

            public void SetRequiredProtectionOrder(MessageProtectionOrder protectionOrder)
            {
                this.protectionOrder = protectionOrder;
                this.enforce = true;
            }

            enum ReceiverProcessingOrder : int
            {
                None = 0,
                Verify = 1,
                Decrypt = 2,
                DecryptVerify = 3,
                VerifyDecrypt = 4,
                Mixed = 5
            }
        }

        struct OperationTracker
        {
            MessagePartSpecification parts;
            SecurityToken token;
            bool isDerivedToken;

            public MessagePartSpecification Parts
            {
                get { return this.parts; }
                set { this.parts = value; }
            }

            public SecurityToken Token
            {
                get { return this.token; }
            }

            public bool IsDerivedToken
            {
                get { return this.isDerivedToken; }
            }

            public void RecordToken(SecurityToken token)
            {
                if (this.token == null)
                {
                    this.token = token;
                }
                else if (!ReferenceEquals(this.token, token))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MismatchInSecurityOperationToken)));
                }
            }

            public void SetDerivationSourceIfRequired()
            {
                DerivedKeySecurityToken derivedKeyToken = this.token as DerivedKeySecurityToken;
                if (derivedKeyToken != null)
                {
                    this.token = derivedKeyToken.TokenToDerive;
                    this.isDerivedToken = true;
                }
            }
        }
    }

    class TokenTracker
    {
        public SecurityToken token;
        public bool IsDerivedFrom;
        public bool IsSigned;
        public bool IsEncrypted;
        public bool IsEndorsing;
        public bool AlreadyReadEndorsingSignature;
        bool allowFirstTokenMismatch;
        public SupportingTokenAuthenticatorSpecification spec;

        public TokenTracker(SupportingTokenAuthenticatorSpecification spec)
            : this(spec, null, false)
        {
        }

        public TokenTracker(SupportingTokenAuthenticatorSpecification spec, SecurityToken token, bool allowFirstTokenMismatch)
        {
            this.spec = spec;
            this.token = token;
            this.allowFirstTokenMismatch = allowFirstTokenMismatch;
        }

        public void RecordToken(SecurityToken token)
        {
            if (this.token == null)
            {
                this.token = token;
            }
            else if (this.allowFirstTokenMismatch)
            {
                if (!AreTokensEqual(this.token, token))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MismatchInSecurityOperationToken)));
                }
                this.token = token;
                this.allowFirstTokenMismatch = false;
            }
            else if (!object.ReferenceEquals(this.token, token))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MismatchInSecurityOperationToken)));
            }
        }

        static bool AreTokensEqual(SecurityToken outOfBandToken, SecurityToken replyToken)
        {
            // we support the serialized reply token legacy feature only for X509 certificates.
            // in this case the thumbprint of the reply certificate must match the outofband certificate's thumbprint
            if ((outOfBandToken is X509SecurityToken) && (replyToken is X509SecurityToken))
            {
                byte[] outOfBandCertificateThumbprint = ((X509SecurityToken)outOfBandToken).Certificate.GetCertHash();
                byte[] replyCertificateThumbprint = ((X509SecurityToken)replyToken).Certificate.GetCertHash();
                return (CryptoHelper.IsEqual(outOfBandCertificateThumbprint, replyCertificateThumbprint));
            }
            else
            {
                return false;
            }
        }
    }

    class AggregateSecurityHeaderTokenResolver : System.IdentityModel.Tokens.AggregateTokenResolver
    {
        SecurityHeaderTokenResolver tokenResolver;

        public AggregateSecurityHeaderTokenResolver(SecurityHeaderTokenResolver tokenResolver, ReadOnlyCollection<SecurityTokenResolver> outOfBandTokenResolvers) :
            base(outOfBandTokenResolvers)
        {
            if (tokenResolver == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenResolver");

            this.tokenResolver = tokenResolver;            
        }

        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            bool resolved = false;
            key = null;

            resolved = this.tokenResolver.TryResolveSecurityKey(keyIdentifierClause, false, out key);

            if (!resolved)
            {
                resolved = base.TryResolveSecurityKeyCore(keyIdentifierClause, out key);
            }

            if (!resolved)
            {
                resolved = SecurityUtils.TryCreateKeyFromIntrinsicKeyClause(keyIdentifierClause, this, out key);
            }

            return resolved;
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            bool resolved = false;
            token = null;

            resolved = this.tokenResolver.TryResolveToken(keyIdentifier, false, false, out token);

            if (!resolved)
            {
                resolved = base.TryResolveTokenCore(keyIdentifier, out token);
            }

            if (!resolved)
            {
                for (int i = 0; i < keyIdentifier.Count; ++i)
                {
                    if (this.TryResolveTokenFromIntrinsicKeyClause(keyIdentifier[i], out token))
                    {
                        resolved = true;
                        break;
                    }
                }
            }

            return resolved;
        }

        bool TryResolveTokenFromIntrinsicKeyClause(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            token = null;
            if (keyIdentifierClause is RsaKeyIdentifierClause)
            {
                token = new RsaSecurityToken(((RsaKeyIdentifierClause)keyIdentifierClause).Rsa);
                return true;
            }
            else if (keyIdentifierClause is X509RawDataKeyIdentifierClause)
            {
                token = new X509SecurityToken(new X509Certificate2(((X509RawDataKeyIdentifierClause)keyIdentifierClause).GetX509RawData()), false);
                return true;
            }
            else if (keyIdentifierClause is EncryptedKeyIdentifierClause)
            {
                EncryptedKeyIdentifierClause keyClause = (EncryptedKeyIdentifierClause)keyIdentifierClause;
                SecurityKeyIdentifier wrappingTokenReference = keyClause.EncryptingKeyIdentifier;
                SecurityToken unwrappingToken;
                if (this.TryResolveToken(wrappingTokenReference, out unwrappingToken))
                {
                    token = SecurityUtils.CreateTokenFromEncryptedKeyClause(keyClause, unwrappingToken);
                    return true;
                }
            }
            return false;
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            bool resolved = false;
            token = null;

            resolved = this.tokenResolver.TryResolveToken(keyIdentifierClause, false, false, out token);

            if (!resolved)
            {
                resolved = base.TryResolveTokenCore(keyIdentifierClause, out token);
            }

            if (!resolved)
            {
                resolved = TryResolveTokenFromIntrinsicKeyClause(keyIdentifierClause, out token);
            }

            return resolved;
        }
    }
}
