//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Globalization;

    /*
     * See
     * http://xws/gxa/main/specs/security/security_profiles/SecurityProfiles.doc
     * for details on security protocols

     * Concrete implementations are required to me thread safe after
     * Open() is called;

     * instances of concrete protocol factories are scoped to a
     * channel/listener factory;

     * Each channel/listener factory must have a
     * SecurityProtocolFactory set on it before open/first use; the
     * factory instance cannot be changed once the factory is opened
     * or listening;

     * security protocol instances are scoped to a channel and will be
     * created by the Create calls on protocol factories;

     * security protocol instances are required to be thread-safe.

     * for typical subclasses, factory wide state and immutable
     * settings are expected to be on the ProtocolFactory itself while
     * channel-wide state is maintained internally in each security
     * protocol instance;

     * the security protocol instance set on a channel cannot be
     * changed; however, the protocol instance may change internal
     * state; this covers RM's SCT renego case; by keeping state
     * change internal to protocol instances, we get better
     * coordination with concurrent message security on channels;

     * the primary pivot in creating a security protocol instance is
     * initiator (client) vs. responder (server), NOT sender vs
     * receiver

     * Create calls for input and reply channels will contain the
     * listener-wide state (if any) created by the corresponding call
     * on the factory;

     */

    // Whether we need to add support for targetting different SOAP roles is tracked by 19144
 
    abstract class SecurityProtocolFactory : ISecurityCommunicationObject
    {
        internal const bool defaultAddTimestamp = true;
        internal const bool defaultDeriveKeys = true;
        internal const bool defaultDetectReplays = true;
        internal const string defaultMaxClockSkewString = "00:05:00";
        internal const string defaultReplayWindowString = "00:05:00";
        internal static readonly TimeSpan defaultMaxClockSkew = TimeSpan.Parse(defaultMaxClockSkewString, CultureInfo.InvariantCulture);
        internal static readonly TimeSpan defaultReplayWindow = TimeSpan.Parse(defaultReplayWindowString, CultureInfo.InvariantCulture);
        internal const int defaultMaxCachedNonces = 900000;
        internal const string defaultTimestampValidityDurationString = "00:05:00";
        internal static readonly TimeSpan defaultTimestampValidityDuration = TimeSpan.Parse(defaultTimestampValidityDurationString, CultureInfo.InvariantCulture);
        internal const SecurityHeaderLayout defaultSecurityHeaderLayout = SecurityHeaderLayout.Strict;

        static ReadOnlyCollection<SupportingTokenAuthenticatorSpecification> emptyTokenAuthenticators;

        bool actAsInitiator;
        bool isDuplexReply;
        bool addTimestamp = defaultAddTimestamp;
        bool detectReplays = defaultDetectReplays;
        bool expectIncomingMessages;
        bool expectOutgoingMessages;
        SecurityAlgorithmSuite incomingAlgorithmSuite = SecurityAlgorithmSuite.Default;


        // per receiver protocol factory lists
        ICollection<SupportingTokenAuthenticatorSpecification> channelSupportingTokenAuthenticatorSpecification;
        Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>> scopedSupportingTokenAuthenticatorSpecification;        
        Dictionary<string, MergedSupportingTokenAuthenticatorSpecification> mergedSupportingTokenAuthenticatorsMap;

        int maxCachedNonces = defaultMaxCachedNonces;
        TimeSpan maxClockSkew = defaultMaxClockSkew;
        NonceCache nonceCache = null;
        SecurityAlgorithmSuite outgoingAlgorithmSuite = SecurityAlgorithmSuite.Default;
        TimeSpan replayWindow = defaultReplayWindow;
        SecurityStandardsManager standardsManager = SecurityStandardsManager.DefaultInstance;
        SecurityTokenManager securityTokenManager;
        SecurityBindingElement securityBindingElement;
        string requestReplyErrorPropertyName;
        NonValidatingSecurityTokenAuthenticator<DerivedKeySecurityToken> derivedKeyTokenAuthenticator;
        TimeSpan timestampValidityDuration = defaultTimestampValidityDuration;
        AuditLogLocation auditLogLocation;
        bool suppressAuditFailure;
        SecurityHeaderLayout securityHeaderLayout;
        AuditLevel serviceAuthorizationAuditLevel;
        AuditLevel messageAuthenticationAuditLevel;
        bool expectKeyDerivation;
        bool expectChannelBasicTokens;
        bool expectChannelSignedTokens;
        bool expectChannelEndorsingTokens;
        bool expectSupportingTokens;
        Uri listenUri;
        MessageSecurityVersion messageSecurityVersion;
        WrapperSecurityCommunicationObject communicationObject;
        Uri privacyNoticeUri;
        int privacyNoticeVersion;
        IMessageFilterTable<EndpointAddress> endpointFilterTable;
        ExtendedProtectionPolicy extendedProtectionPolicy;
        BufferManager streamBufferManager = null;

        protected SecurityProtocolFactory()
        {
            this.channelSupportingTokenAuthenticatorSpecification = new Collection<SupportingTokenAuthenticatorSpecification>();
            this.scopedSupportingTokenAuthenticatorSpecification = new Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>>();
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        internal SecurityProtocolFactory(SecurityProtocolFactory factory)
            : this()
        {
            if (factory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("factory");
            }

            this.actAsInitiator = factory.actAsInitiator;
            this.addTimestamp = factory.addTimestamp;
            this.detectReplays = factory.detectReplays;
            this.incomingAlgorithmSuite = factory.incomingAlgorithmSuite;
            this.maxCachedNonces = factory.maxCachedNonces;
            this.maxClockSkew = factory.maxClockSkew;
            this.outgoingAlgorithmSuite = factory.outgoingAlgorithmSuite;
            this.replayWindow = factory.replayWindow;
            this.channelSupportingTokenAuthenticatorSpecification = new Collection<SupportingTokenAuthenticatorSpecification>(new List<SupportingTokenAuthenticatorSpecification>(factory.channelSupportingTokenAuthenticatorSpecification));
            this.scopedSupportingTokenAuthenticatorSpecification = new Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>>(factory.scopedSupportingTokenAuthenticatorSpecification);
            this.standardsManager = factory.standardsManager;
            this.timestampValidityDuration = factory.timestampValidityDuration;
            this.auditLogLocation = factory.auditLogLocation;
            this.suppressAuditFailure = factory.suppressAuditFailure;
            this.serviceAuthorizationAuditLevel = factory.serviceAuthorizationAuditLevel;
            this.messageAuthenticationAuditLevel = factory.messageAuthenticationAuditLevel;
            if (factory.securityBindingElement != null)
            {
                this.securityBindingElement = (SecurityBindingElement) factory.securityBindingElement.Clone();
            }
            this.securityTokenManager = factory.securityTokenManager;
            this.privacyNoticeUri = factory.privacyNoticeUri;
            this.privacyNoticeVersion = factory.privacyNoticeVersion;
            this.endpointFilterTable = factory.endpointFilterTable;
            this.extendedProtectionPolicy = factory.extendedProtectionPolicy;
            this.nonceCache = factory.nonceCache;
        }

        protected WrapperSecurityCommunicationObject CommunicationObject
        {
            get { return this.communicationObject; } 
        }

        // The ActAsInitiator value is set automatically on Open and
        // remains unchanged thereafter.  ActAsInitiator is true for
        // the initiator of the message exchange, such as the sender
        // of a datagram, sender of a request and sender of either leg
        // of a duplex exchange.
        public bool ActAsInitiator
        {
            get
            {
                return this.actAsInitiator;
            }
        }

        public BufferManager StreamBufferManager
        {
            get
            {
                if (this.streamBufferManager == null)
                {
                    this.streamBufferManager = BufferManager.CreateBufferManager(0, int.MaxValue);
                }

                return this.streamBufferManager;
            }
            set
            {
                this.streamBufferManager = value;
            }
        }

        public ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get { return this.extendedProtectionPolicy; }
            set { this.extendedProtectionPolicy = value; }
        }

        internal bool IsDuplexReply
        {
            get
            {
                return this.isDuplexReply;
            }
            set
            {
                this.isDuplexReply = value;
            }
        }

        public bool AddTimestamp
        {
            get
            {
                return this.addTimestamp;
            }
            set
            {
                ThrowIfImmutable();
                this.addTimestamp = value;
            }
        }

        public AuditLogLocation AuditLogLocation
        {
            get
            {
                return this.auditLogLocation;
            }
            set
            {
                ThrowIfImmutable();
                AuditLogLocationHelper.Validate(value);
                this.auditLogLocation = value;
            }
        }

        public bool SuppressAuditFailure
        {
            get
            {
                return this.suppressAuditFailure;
            }
            set
            {
                ThrowIfImmutable();
                this.suppressAuditFailure = value;
            }
        }

        public AuditLevel ServiceAuthorizationAuditLevel
        {
            get
            {
                return this.serviceAuthorizationAuditLevel;
            }
            set
            {
                ThrowIfImmutable();
                AuditLevelHelper.Validate(value);
                this.serviceAuthorizationAuditLevel = value;
            }
        }

        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return this.messageAuthenticationAuditLevel;
            }
            set
            {
                ThrowIfImmutable();
                AuditLevelHelper.Validate(value);
                this.messageAuthenticationAuditLevel = value;
            }
        }


        public bool DetectReplays
        {
            get
            {
                return this.detectReplays;
            }
            set
            {
                ThrowIfImmutable();
                this.detectReplays = value;
            }
        }

        public Uri PrivacyNoticeUri
        {
            get 
            {
                return this.privacyNoticeUri; 
            }
            set
            {
                ThrowIfImmutable();
                this.privacyNoticeUri = value;
            }
        }

        public int PrivacyNoticeVersion
        {
            get
            {
                return this.privacyNoticeVersion;
            }
            set
            {
                ThrowIfImmutable();
                this.privacyNoticeVersion = value;
            }
        }

        public IMessageFilterTable<EndpointAddress> EndpointFilterTable
        {
            get
            {
                return this.endpointFilterTable;
            }
            set
            {
                ThrowIfImmutable();
                this.endpointFilterTable = value;
            }
        }

        static ReadOnlyCollection<SupportingTokenAuthenticatorSpecification> EmptyTokenAuthenticators
        {
            get
            {
                if (emptyTokenAuthenticators == null)
                {
                    emptyTokenAuthenticators = Array.AsReadOnly(new SupportingTokenAuthenticatorSpecification[0]);
                }
                return emptyTokenAuthenticators;
            }
        }

        internal NonValidatingSecurityTokenAuthenticator<DerivedKeySecurityToken> DerivedKeyTokenAuthenticator
        {
            get
            {
                return this.derivedKeyTokenAuthenticator;
            }
        }

        internal bool ExpectIncomingMessages
        {
            get
            {
                return this.expectIncomingMessages;
            }
        }

        internal bool ExpectOutgoingMessages
        {
            get
            {
                return this.expectOutgoingMessages;
            }
        }

        internal bool ExpectKeyDerivation
        {
            get { return this.expectKeyDerivation; }
            set { this.expectKeyDerivation = value; }
        }

        internal bool ExpectSupportingTokens
        {
            get { return this.expectSupportingTokens; }
            set { this.expectSupportingTokens = value; }
        }

        public SecurityAlgorithmSuite IncomingAlgorithmSuite
        {
            get
            {
                return this.incomingAlgorithmSuite;
            }
            set
            {
                ThrowIfImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.incomingAlgorithmSuite = value;
            }
        }

        protected bool IsReadOnly
        {
            get
            {
                return this.CommunicationObject.State != CommunicationState.Created;
            }
        }

        public int MaxCachedNonces
        {
            get
            {
                return this.maxCachedNonces;
            }
            set
            {
                ThrowIfImmutable();
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.maxCachedNonces = value;
            }
        }

        public TimeSpan MaxClockSkew
        {
            get
            {
                return this.maxClockSkew;
            }
            set
            {
                ThrowIfImmutable();
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.maxClockSkew = value;
            }
        }

        public NonceCache NonceCache
        {
            get
            {
                return this.nonceCache;
            }
            set
            {
                ThrowIfImmutable();
                this.nonceCache = value;
            }
        }
        
        public SecurityAlgorithmSuite OutgoingAlgorithmSuite
        {
            get
            {
                return this.outgoingAlgorithmSuite;
            }
            set
            {
                ThrowIfImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.outgoingAlgorithmSuite = value;
            }
        }

        public TimeSpan ReplayWindow
        {
            get
            {
                return this.replayWindow;
            }
            set
            {
                ThrowIfImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }
                this.replayWindow = value;
            }
        }

        public ICollection<SupportingTokenAuthenticatorSpecification> ChannelSupportingTokenAuthenticatorSpecification
        {
            get 
            {
                return this.channelSupportingTokenAuthenticatorSpecification;
            }
        }

        public Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>> ScopedSupportingTokenAuthenticatorSpecification
        {
            get
            {
                return this.scopedSupportingTokenAuthenticatorSpecification;
            }
        }

        public SecurityBindingElement SecurityBindingElement
        {
            get { return this.securityBindingElement; }
            set
            {
                ThrowIfImmutable();
                if (value != null)
                {
                    value = (SecurityBindingElement) value.Clone();
                }
                this.securityBindingElement = value;
            }
        }

        public SecurityTokenManager SecurityTokenManager
        {
            get { return this.securityTokenManager; }
            set
            {
                ThrowIfImmutable();
                this.securityTokenManager = value;
            }
        }

        public virtual bool SupportsDuplex
        {
            get
            {
                return false;
            }
        }

        public SecurityHeaderLayout SecurityHeaderLayout
        {
            get
            {
                return this.securityHeaderLayout;
            }
            set
            {
                ThrowIfImmutable();
                this.securityHeaderLayout = value;
            }
        }

        public virtual bool SupportsReplayDetection
        {
            get
            {
                return true;
            }
        }

        public virtual bool SupportsRequestReply
        {
            get
            {
                return true;
            }
        }

        public SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                ThrowIfImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.standardsManager = value;
            }
        }

        public TimeSpan TimestampValidityDuration
        {
            get
            {
                return this.timestampValidityDuration;
            }
            set
            {
                ThrowIfImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }
                this.timestampValidityDuration = value;
            }
        }

        public Uri ListenUri
        {
            get { return this.listenUri; }
            set 
            {
                ThrowIfImmutable();
                this.listenUri = value;
            }
        }

        internal MessageSecurityVersion MessageSecurityVersion
        {
            get { return this.messageSecurityVersion; }
        }

        // ISecurityCommunicationObject members
        public TimeSpan DefaultOpenTimeout
        {
            get { return ServiceDefaults.OpenTimeout; }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get { return ServiceDefaults.CloseTimeout; }
        }

        public IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        public IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        public void OnClosed()
        {
        }

        public void OnClosing()
        {
        }

        public void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnFaulted()
        {
        }

        public void OnOpened()
        {
        }

        public void OnOpening()
        {
        }

        public virtual void OnAbort()
        {
            if (!this.actAsInitiator)
            {
                foreach (SupportingTokenAuthenticatorSpecification spec in this.channelSupportingTokenAuthenticatorSpecification)
                {
                    SecurityUtils.AbortTokenAuthenticatorIfRequired(spec.TokenAuthenticator);
                }
                foreach (string action in this.scopedSupportingTokenAuthenticatorSpecification.Keys)
                {
                    ICollection<SupportingTokenAuthenticatorSpecification> supportingAuthenticators = this.scopedSupportingTokenAuthenticatorSpecification[action];
                    foreach (SupportingTokenAuthenticatorSpecification spec in supportingAuthenticators)
                    {
                        SecurityUtils.AbortTokenAuthenticatorIfRequired(spec.TokenAuthenticator);
                    }
                }
            }
        }

        public virtual void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.actAsInitiator)
            {
                foreach (SupportingTokenAuthenticatorSpecification spec in this.channelSupportingTokenAuthenticatorSpecification)
                {
                    SecurityUtils.CloseTokenAuthenticatorIfRequired(spec.TokenAuthenticator, timeoutHelper.RemainingTime());
                }
                foreach (string action in this.scopedSupportingTokenAuthenticatorSpecification.Keys)
                {
                    ICollection<SupportingTokenAuthenticatorSpecification> supportingAuthenticators = this.scopedSupportingTokenAuthenticatorSpecification[action];
                    foreach (SupportingTokenAuthenticatorSpecification spec in supportingAuthenticators)
                    {
                        SecurityUtils.CloseTokenAuthenticatorIfRequired(spec.TokenAuthenticator, timeoutHelper.RemainingTime());
                    }
                }
            }
        }

        public virtual object CreateListenerSecurityState()
        {
            return null;
        }

        public SecurityProtocol CreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, bool isReturnLegSecurityRequired, TimeSpan timeout)
        {
            ThrowIfNotOpen();
            SecurityProtocol securityProtocol = OnCreateSecurityProtocol(target, via, listenerSecurityState, timeout);
            if (securityProtocol == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.ProtocolFactoryCouldNotCreateProtocol)));
            }
            return securityProtocol;
        }

        public virtual EndpointIdentity GetIdentityOfSelf()
        {
            return null;
        }

        public virtual T GetProperty<T>()
        {
            if (typeof(T) == typeof(Collection<ISecurityContextSecurityTokenCache>))
            {
                ThrowIfNotOpen();
                Collection<ISecurityContextSecurityTokenCache> result = new Collection<ISecurityContextSecurityTokenCache>();
                if (channelSupportingTokenAuthenticatorSpecification != null)
                {
                    foreach (SupportingTokenAuthenticatorSpecification spec in this.channelSupportingTokenAuthenticatorSpecification)
                    {
                        if (spec.TokenAuthenticator is ISecurityContextSecurityTokenCacheProvider)
                        {
                            result.Add(((ISecurityContextSecurityTokenCacheProvider)spec.TokenAuthenticator).TokenCache);
                        }
                    }
                }
                return (T)(object)(result);
            }
            else
            {
                return default(T);
            }
        }

        protected abstract SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout);

        void VerifyTypeUniqueness(ICollection<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            // its ok to go brute force here since we are dealing with a small number of authenticators
            foreach (SupportingTokenAuthenticatorSpecification spec in supportingTokenAuthenticators)
            {
                Type authenticatorType = spec.TokenAuthenticator.GetType();
                int numSkipped = 0;
                foreach (SupportingTokenAuthenticatorSpecification spec2 in supportingTokenAuthenticators)
                {
                    Type spec2AuthenticatorType = spec2.TokenAuthenticator.GetType();
                    if (object.ReferenceEquals(spec, spec2))
                    {
                        if (numSkipped > 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MultipleSupportingAuthenticatorsOfSameType, spec.TokenParameters.GetType())));
                        }
                        ++numSkipped;
                        continue;
                    }
                    else if (authenticatorType.IsAssignableFrom(spec2AuthenticatorType) || spec2AuthenticatorType.IsAssignableFrom(authenticatorType))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MultipleSupportingAuthenticatorsOfSameType, spec.TokenParameters.GetType())));
                    }
                }
            }
        }

        internal IList<SupportingTokenAuthenticatorSpecification> GetSupportingTokenAuthenticators(string action, out bool expectSignedTokens, out bool expectBasicTokens, out bool expectEndorsingTokens)
        {
            if (this.mergedSupportingTokenAuthenticatorsMap != null && this.mergedSupportingTokenAuthenticatorsMap.Count > 0)
            {
                if (action != null && this.mergedSupportingTokenAuthenticatorsMap.ContainsKey(action))
                {
                    MergedSupportingTokenAuthenticatorSpecification mergedSpec = this.mergedSupportingTokenAuthenticatorsMap[action];
                    expectSignedTokens = mergedSpec.ExpectSignedTokens;
                    expectBasicTokens = mergedSpec.ExpectBasicTokens;
                    expectEndorsingTokens = mergedSpec.ExpectEndorsingTokens;
                    return mergedSpec.SupportingTokenAuthenticators;
                }
                else if (this.mergedSupportingTokenAuthenticatorsMap.ContainsKey(MessageHeaders.WildcardAction))
                {
                    MergedSupportingTokenAuthenticatorSpecification mergedSpec = this.mergedSupportingTokenAuthenticatorsMap[MessageHeaders.WildcardAction];
                    expectSignedTokens = mergedSpec.ExpectSignedTokens;
                    expectBasicTokens = mergedSpec.ExpectBasicTokens;
                    expectEndorsingTokens = mergedSpec.ExpectEndorsingTokens;
                    return mergedSpec.SupportingTokenAuthenticators;
                }
            }
            expectSignedTokens = this.expectChannelSignedTokens;
            expectBasicTokens = this.expectChannelBasicTokens;
            expectEndorsingTokens = this.expectChannelEndorsingTokens;
            // in case the channelSupportingTokenAuthenticators is empty return null so that its Count does not get accessed.
            return (Object.ReferenceEquals(this.channelSupportingTokenAuthenticatorSpecification, EmptyTokenAuthenticators)) ? null : (IList<SupportingTokenAuthenticatorSpecification>) this.channelSupportingTokenAuthenticatorSpecification;
        }

        void MergeSupportingTokenAuthenticators(TimeSpan timeout)
        {
            if (this.scopedSupportingTokenAuthenticatorSpecification.Count == 0)
            {
                this.mergedSupportingTokenAuthenticatorsMap = null;
            }
            else
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.expectSupportingTokens = true;
                this.mergedSupportingTokenAuthenticatorsMap = new Dictionary<string, MergedSupportingTokenAuthenticatorSpecification>();
                foreach (string action in this.scopedSupportingTokenAuthenticatorSpecification.Keys)
                {
                    ICollection<SupportingTokenAuthenticatorSpecification> scopedAuthenticators = this.scopedSupportingTokenAuthenticatorSpecification[action];
                    if (scopedAuthenticators == null || scopedAuthenticators.Count == 0)
                    {
                        continue;
                    }
                    Collection<SupportingTokenAuthenticatorSpecification> mergedAuthenticators = new Collection<SupportingTokenAuthenticatorSpecification>();
                    bool expectSignedTokens = this.expectChannelSignedTokens;
                    bool expectBasicTokens = this.expectChannelBasicTokens;
                    bool expectEndorsingTokens = this.expectChannelEndorsingTokens;
                    foreach (SupportingTokenAuthenticatorSpecification spec in this.channelSupportingTokenAuthenticatorSpecification)
                    {
                        mergedAuthenticators.Add(spec);
                    }
                    foreach (SupportingTokenAuthenticatorSpecification spec in scopedAuthenticators)
                    {
                        SecurityUtils.OpenTokenAuthenticatorIfRequired(spec.TokenAuthenticator, timeoutHelper.RemainingTime());
                        mergedAuthenticators.Add(spec);
                        if (spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing ||
                            spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)
                        {
                            if (spec.TokenParameters.RequireDerivedKeys && !spec.TokenParameters.HasAsymmetricKey)
                            {
                                this.expectKeyDerivation = true;
                            }
                        }
                        SecurityTokenAttachmentMode mode = spec.SecurityTokenAttachmentMode;
                        if (mode == SecurityTokenAttachmentMode.SignedEncrypted
                            || mode == SecurityTokenAttachmentMode.Signed
                            || mode == SecurityTokenAttachmentMode.SignedEndorsing)
                        {
                            expectSignedTokens = true;
                            if (mode == SecurityTokenAttachmentMode.SignedEncrypted)
                            {
                                expectBasicTokens = true;
                            }
                        }
                        if (mode == SecurityTokenAttachmentMode.Endorsing || mode == SecurityTokenAttachmentMode.SignedEndorsing)
                        {
                            expectEndorsingTokens = true;
                        }
                    }
                    VerifyTypeUniqueness(mergedAuthenticators);
                    MergedSupportingTokenAuthenticatorSpecification mergedSpec = new MergedSupportingTokenAuthenticatorSpecification();
                    mergedSpec.SupportingTokenAuthenticators = mergedAuthenticators;
                    mergedSpec.ExpectBasicTokens = expectBasicTokens;
                    mergedSpec.ExpectEndorsingTokens = expectEndorsingTokens;
                    mergedSpec.ExpectSignedTokens = expectSignedTokens;
                    mergedSupportingTokenAuthenticatorsMap.Add(action, mergedSpec);
                }
            }
        }

        protected RecipientServiceModelSecurityTokenRequirement CreateRecipientSecurityTokenRequirement()
        {
            RecipientServiceModelSecurityTokenRequirement requirement = new RecipientServiceModelSecurityTokenRequirement();
            requirement.SecurityBindingElement = this.securityBindingElement;
            requirement.SecurityAlgorithmSuite = this.IncomingAlgorithmSuite;
            requirement.ListenUri = this.listenUri;
            requirement.MessageSecurityVersion = this.MessageSecurityVersion.SecurityTokenVersion;
            requirement.AuditLogLocation = this.auditLogLocation;
            requirement.SuppressAuditFailure = this.suppressAuditFailure;
            requirement.MessageAuthenticationAuditLevel = this.messageAuthenticationAuditLevel;
            requirement.Properties[ServiceModelSecurityTokenRequirement.ExtendedProtectionPolicy] = this.extendedProtectionPolicy;
            if (this.endpointFilterTable != null)
            {
                requirement.Properties.Add(ServiceModelSecurityTokenRequirement.EndpointFilterTableProperty, this.endpointFilterTable);
            }
            return requirement;
        }

        RecipientServiceModelSecurityTokenRequirement CreateRecipientSecurityTokenRequirement(SecurityTokenParameters parameters, SecurityTokenAttachmentMode attachmentMode)
        {
            RecipientServiceModelSecurityTokenRequirement requirement = CreateRecipientSecurityTokenRequirement();
            parameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = SecurityKeyUsage.Signature;
            requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportingTokenAttachmentModeProperty] = attachmentMode;
            requirement.Properties[ServiceModelSecurityTokenRequirement.ExtendedProtectionPolicy] = this.extendedProtectionPolicy;
            return requirement;
        }

        void AddSupportingTokenAuthenticators(SupportingTokenParameters supportingTokenParameters, bool isOptional, IList<SupportingTokenAuthenticatorSpecification> authenticatorSpecList)
        {
            for (int i = 0; i < supportingTokenParameters.Endorsing.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.Endorsing[i], SecurityTokenAttachmentMode.Endorsing);
                try
                {
                    System.IdentityModel.Selectors.SecurityTokenResolver resolver;
                    System.IdentityModel.Selectors.SecurityTokenAuthenticator authenticator = this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out resolver);
                    SupportingTokenAuthenticatorSpecification authenticatorSpec = new SupportingTokenAuthenticatorSpecification(authenticator, resolver, SecurityTokenAttachmentMode.Endorsing, supportingTokenParameters.Endorsing[i], isOptional);
                    authenticatorSpecList.Add(authenticatorSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
            for (int i = 0; i < supportingTokenParameters.SignedEndorsing.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.SignedEndorsing[i], SecurityTokenAttachmentMode.SignedEndorsing);
                try
                {
                    System.IdentityModel.Selectors.SecurityTokenResolver resolver;
                    System.IdentityModel.Selectors.SecurityTokenAuthenticator authenticator = this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out resolver);
                    SupportingTokenAuthenticatorSpecification authenticatorSpec = new SupportingTokenAuthenticatorSpecification(authenticator, resolver, SecurityTokenAttachmentMode.SignedEndorsing, supportingTokenParameters.SignedEndorsing[i], isOptional);
                    authenticatorSpecList.Add(authenticatorSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
            for (int i = 0; i < supportingTokenParameters.SignedEncrypted.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.SignedEncrypted[i], SecurityTokenAttachmentMode.SignedEncrypted);
                try
                {
                    System.IdentityModel.Selectors.SecurityTokenResolver resolver;
                    System.IdentityModel.Selectors.SecurityTokenAuthenticator authenticator = this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out resolver);
                    SupportingTokenAuthenticatorSpecification authenticatorSpec = new SupportingTokenAuthenticatorSpecification(authenticator, resolver, SecurityTokenAttachmentMode.SignedEncrypted, supportingTokenParameters.SignedEncrypted[i], isOptional);
                    authenticatorSpecList.Add(authenticatorSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
            for (int i = 0; i < supportingTokenParameters.Signed.Count; ++i)
            {
                SecurityTokenRequirement requirement = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.Signed[i], SecurityTokenAttachmentMode.Signed);
                try
                {
                    System.IdentityModel.Selectors.SecurityTokenResolver resolver;
                    System.IdentityModel.Selectors.SecurityTokenAuthenticator authenticator = this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out resolver);
                    SupportingTokenAuthenticatorSpecification authenticatorSpec = new SupportingTokenAuthenticatorSpecification(authenticator, resolver, SecurityTokenAttachmentMode.Signed, supportingTokenParameters.Signed[i], isOptional);
                    authenticatorSpecList.Add(authenticatorSpec);
                }
                catch (Exception e)
                {
                    if (!isOptional || Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
        }

        public virtual void OnOpen(TimeSpan timeout)
        {
            if (this.SecurityBindingElement == null)
            {
                this.OnPropertySettingsError("SecurityBindingElement", true);
            }
            if (this.SecurityTokenManager == null)
            {
                this.OnPropertySettingsError("SecurityTokenManager", true);
            }
            this.messageSecurityVersion = this.standardsManager.MessageSecurityVersion;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.expectOutgoingMessages = this.ActAsInitiator || this.SupportsRequestReply;
            this.expectIncomingMessages = !this.ActAsInitiator || this.SupportsRequestReply;
            if (!this.actAsInitiator)
            {
                AddSupportingTokenAuthenticators(this.securityBindingElement.EndpointSupportingTokenParameters, false, (IList<SupportingTokenAuthenticatorSpecification>)this.channelSupportingTokenAuthenticatorSpecification);
                AddSupportingTokenAuthenticators(this.securityBindingElement.OptionalEndpointSupportingTokenParameters, true, (IList<SupportingTokenAuthenticatorSpecification>)this.channelSupportingTokenAuthenticatorSpecification);
                foreach (string action in this.securityBindingElement.OperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenAuthenticatorSpecification> authenticatorSpecList = new Collection<SupportingTokenAuthenticatorSpecification>();
                    AddSupportingTokenAuthenticators(this.securityBindingElement.OperationSupportingTokenParameters[action], false, authenticatorSpecList);
                    this.scopedSupportingTokenAuthenticatorSpecification.Add(action, authenticatorSpecList);
                }
                foreach (string action in this.securityBindingElement.OptionalOperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenAuthenticatorSpecification> authenticatorSpecList;
                    ICollection<SupportingTokenAuthenticatorSpecification> existingList;
                    if (this.scopedSupportingTokenAuthenticatorSpecification.TryGetValue(action, out existingList))
                    {
                        authenticatorSpecList = ((Collection<SupportingTokenAuthenticatorSpecification>)existingList);
                    }
                    else
                    {
                        authenticatorSpecList = new Collection<SupportingTokenAuthenticatorSpecification>();
                        this.scopedSupportingTokenAuthenticatorSpecification.Add(action, authenticatorSpecList);
                    }
                    this.AddSupportingTokenAuthenticators(this.securityBindingElement.OptionalOperationSupportingTokenParameters[action], true, authenticatorSpecList);
                }
                // validate the token authenticator types and create a merged map if needed.
                if (!this.channelSupportingTokenAuthenticatorSpecification.IsReadOnly)
                {
                    if (this.channelSupportingTokenAuthenticatorSpecification.Count == 0)
                    {
                        this.channelSupportingTokenAuthenticatorSpecification = EmptyTokenAuthenticators;
                    }
                    else
                    {
                        this.expectSupportingTokens = true;
                        foreach (SupportingTokenAuthenticatorSpecification tokenAuthenticatorSpec in this.channelSupportingTokenAuthenticatorSpecification)
                        {
                            SecurityUtils.OpenTokenAuthenticatorIfRequired(tokenAuthenticatorSpec.TokenAuthenticator, timeoutHelper.RemainingTime());
                            if (tokenAuthenticatorSpec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing
                                || tokenAuthenticatorSpec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)
                            {
                                if (tokenAuthenticatorSpec.TokenParameters.RequireDerivedKeys && !tokenAuthenticatorSpec.TokenParameters.HasAsymmetricKey)
                                {
                                    expectKeyDerivation = true;
                                }
                            }
                            SecurityTokenAttachmentMode mode = tokenAuthenticatorSpec.SecurityTokenAttachmentMode;
                            if (mode == SecurityTokenAttachmentMode.SignedEncrypted
                                || mode == SecurityTokenAttachmentMode.Signed
                                || mode == SecurityTokenAttachmentMode.SignedEndorsing)
                            {
                                this.expectChannelSignedTokens = true;
                                if (mode == SecurityTokenAttachmentMode.SignedEncrypted)
                                {
                                    this.expectChannelBasicTokens = true;
                                }
                            }
                            if (mode == SecurityTokenAttachmentMode.Endorsing || mode == SecurityTokenAttachmentMode.SignedEndorsing)
                            {
                                this.expectChannelEndorsingTokens = true;
                            }
                        }
                        this.channelSupportingTokenAuthenticatorSpecification =
                            new ReadOnlyCollection<SupportingTokenAuthenticatorSpecification>((Collection<SupportingTokenAuthenticatorSpecification>)this.channelSupportingTokenAuthenticatorSpecification);
                    }
                }
                VerifyTypeUniqueness(this.channelSupportingTokenAuthenticatorSpecification);
                MergeSupportingTokenAuthenticators(timeoutHelper.RemainingTime());
            }

            if (this.DetectReplays)
            {
                if (!this.SupportsReplayDetection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("DetectReplays", SR.GetString(SR.SecurityProtocolCannotDoReplayDetection, this));
                }
                if (this.MaxClockSkew == TimeSpan.MaxValue || this.ReplayWindow == TimeSpan.MaxValue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoncesCachedInfinitely)));
                }

                // If DetectReplays is true and nonceCache is null then use the default InMemoryNonceCache. 
                if (this.nonceCache == null)
                {
                    // The nonce needs to be cached for replayWindow + 2*clockSkew to eliminate replays
                    this.nonceCache = new InMemoryNonceCache(this.ReplayWindow + this.MaxClockSkew + this.MaxClockSkew, this.MaxCachedNonces);
                }
            }

            this.derivedKeyTokenAuthenticator = new NonValidatingSecurityTokenAuthenticator<DerivedKeySecurityToken>();
        }

        public void Open(bool actAsInitiator, TimeSpan timeout)
        {
            this.actAsInitiator = actAsInitiator;
            this.communicationObject.Open(timeout);
        }

        public IAsyncResult BeginOpen(bool actAsInitiator, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.actAsInitiator = actAsInitiator;
            return this.CommunicationObject.BeginOpen(timeout, callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.CommunicationObject.EndOpen(result);
        }

        public void Close(bool aborted, TimeSpan timeout)
        {
            if (aborted)
            {
                this.CommunicationObject.Abort();
            }
            else
            {
                this.CommunicationObject.Close(timeout);
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.CommunicationObject.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            this.CommunicationObject.EndClose(result);
        }

        internal void Open(string propertyName, bool requiredForForwardDirection, SecurityTokenAuthenticator authenticator, TimeSpan timeout)
        {
            if (authenticator != null)
            {
                SecurityUtils.OpenTokenAuthenticatorIfRequired(authenticator, timeout);
            }
            else
            {
                OnPropertySettingsError(propertyName, requiredForForwardDirection);
            }
        }

        internal void Open(string propertyName, bool requiredForForwardDirection, SecurityTokenProvider provider, TimeSpan timeout)
        {
            if (provider != null)
            {
                SecurityUtils.OpenTokenProviderIfRequired(provider, timeout);
            }
            else
            {
                OnPropertySettingsError(propertyName, requiredForForwardDirection);
            }
        }

        internal void OnPropertySettingsError(string propertyName, bool requiredForForwardDirection)
        {
            if (requiredForForwardDirection)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(
                    SR.GetString(SR.PropertySettingErrorOnProtocolFactory, propertyName, this),
                    propertyName));
            }
            else if (this.requestReplyErrorPropertyName == null)
            {
                this.requestReplyErrorPropertyName = propertyName;
            }
        }

        void ThrowIfReturnDirectionSecurityNotSupported()
        {
            if (this.requestReplyErrorPropertyName != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(
                    SR.GetString(SR.PropertySettingErrorOnProtocolFactory, this.requestReplyErrorPropertyName, this),
                    this.requestReplyErrorPropertyName));
            }
        }

        internal void ThrowIfImmutable()
        {
            this.communicationObject.ThrowIfDisposedOrImmutable();
        }

        void ThrowIfNotOpen()
        {
            this.communicationObject.ThrowIfNotOpened();
        }
    }

    struct MergedSupportingTokenAuthenticatorSpecification
    {
        public Collection<SupportingTokenAuthenticatorSpecification> SupportingTokenAuthenticators;
        public bool ExpectSignedTokens;
        public bool ExpectEndorsingTokens;
        public bool ExpectBasicTokens;
    }
}
