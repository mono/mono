//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    abstract class NegotiationTokenAuthenticator<T> : CommunicationObjectSecurityTokenAuthenticator, IIssuanceSecurityTokenAuthenticator, ISecurityContextSecurityTokenCacheProvider
        where T : NegotiationTokenAuthenticatorState
    {
        internal const string defaultServerMaxNegotiationLifetimeString = "00:01:00";
        internal const string defaultServerIssuedTokenLifetimeString = "10:00:00";
        internal const string defaultServerIssuedTransitionTokenLifetimeString = "00:15:00";
        internal const int defaultServerMaxActiveNegotiations = 128;
        internal static readonly TimeSpan defaultServerMaxNegotiationLifetime = TimeSpan.Parse(defaultServerMaxNegotiationLifetimeString, CultureInfo.InvariantCulture);
        internal static readonly TimeSpan defaultServerIssuedTokenLifetime = TimeSpan.Parse(defaultServerIssuedTokenLifetimeString, CultureInfo.InvariantCulture);
        internal static readonly TimeSpan defaultServerIssuedTransitionTokenLifetime = TimeSpan.Parse(defaultServerIssuedTransitionTokenLifetimeString, CultureInfo.InvariantCulture);
        internal const int defaultServerMaxCachedTokens = 1000;
        internal const bool defaultServerMaintainState = true;
        internal static readonly SecurityStandardsManager defaultStandardsManager = SecurityStandardsManager.DefaultInstance;
        internal static readonly SecurityStateEncoder defaultSecurityStateEncoder = new DataProtectionSecurityStateEncoder();

        NegotiationTokenAuthenticatorStateCache<T> stateCache;
        RenewedSecurityTokenHandler renewedSecurityTokenHandler;
        NegotiationHost negotiationHost;
        bool encryptStateInServiceToken;
        TimeSpan serviceTokenLifetime;
        int maximumCachedNegotiationState;
        TimeSpan negotiationTimeout;
        bool isClientAnonymous;
        SecurityStandardsManager standardsManager;
        SecurityAlgorithmSuite securityAlgorithmSuite;
        SecurityTokenParameters issuedSecurityTokenParameters;
        ISecurityContextSecurityTokenCache issuedTokenCache;
        BindingContext issuerBindingContext;
        Uri listenUri;
        string sctUri;
        AuditLogLocation auditLogLocation;
        bool suppressAuditFailure;
        AuditLevel messageAuthenticationAuditLevel;
        SecurityStateEncoder securityStateEncoder;
        SecurityContextCookieSerializer cookieSerializer;
        IMessageFilterTable<EndpointAddress> endpointFilterTable;
        IssuedSecurityTokenHandler issuedSecurityTokenHandler;
        int maxMessageSize;
        IList<Type> knownTypes;
        int maximumConcurrentNegotiations;
        List<IChannel> activeNegotiationChannels1;
        List<IChannel> activeNegotiationChannels2;
        IOThreadTimer idlingNegotiationSessionTimer;
        bool isTimerCancelled;

        protected NegotiationTokenAuthenticator() : base()
        {
            InitializeDefaults();
        }

        public IssuedSecurityTokenHandler IssuedSecurityTokenHandler
        {
            get
            {
                return this.issuedSecurityTokenHandler;
            }
            set
            {
                this.issuedSecurityTokenHandler = value;
            }

        }

        public RenewedSecurityTokenHandler RenewedSecurityTokenHandler
        {
            get
            {
                return this.renewedSecurityTokenHandler;
            }
            set
            {
                this.renewedSecurityTokenHandler = value;
            }
        }

        // settings
        public bool EncryptStateInServiceToken
        {
            get
            {
                return this.encryptStateInServiceToken;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.encryptStateInServiceToken = value;
            }
        }

        public TimeSpan ServiceTokenLifetime
        {
            get
            {
                return this.serviceTokenLifetime;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }
                this.serviceTokenLifetime = value;
            }
        }

        public int MaximumCachedNegotiationState
        {
            get
            {
                return this.maximumCachedNegotiationState;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeNonNegative)));
                }
                this.maximumCachedNegotiationState = value;
            }
        }

        public int MaximumConcurrentNegotiations
        {
            get
            {
                return this.maximumConcurrentNegotiations;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeNonNegative)));
                }
                this.maximumConcurrentNegotiations = value;
            }
        }

        public TimeSpan NegotiationTimeout
        {
            get
            {
                return this.negotiationTimeout;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }
                this.negotiationTimeout = value;
            }
        }

        public bool IsClientAnonymous
        {
            get
            {
                return this.isClientAnonymous;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.isClientAnonymous = value;
            }
        }

        public SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.securityAlgorithmSuite;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.securityAlgorithmSuite = value;
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
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.endpointFilterTable = value;
            }
        }

        ISecurityContextSecurityTokenCache ISecurityContextSecurityTokenCacheProvider.TokenCache 
        {
            get 
            {
                return this.IssuedTokenCache;
            }
        }

        public virtual XmlDictionaryString RequestSecurityTokenAction
        {
            get
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenAction;
            }
        }

        public virtual XmlDictionaryString RequestSecurityTokenResponseAction
        {
            get
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction;
            }
        }

        public virtual XmlDictionaryString RequestSecurityTokenResponseFinalAction
        {
            get
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenResponseFinalAction;
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
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.standardsManager = (value != null ? value : SecurityStandardsManager.DefaultInstance);
            }
        }

        public SecurityTokenParameters IssuedSecurityTokenParameters
        {
            get
            {
                return this.issuedSecurityTokenParameters;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuedSecurityTokenParameters = value;
            }
        }

        public ISecurityContextSecurityTokenCache IssuedTokenCache
        {
            get { return this.issuedTokenCache; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuedTokenCache = value;
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
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
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
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.suppressAuditFailure = value;
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
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.messageAuthenticationAuditLevel = value;
            }
        }

        public BindingContext IssuerBindingContext
        {
            get { return this.issuerBindingContext; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.issuerBindingContext = value.Clone();
            }
        }

        public Uri ListenUri
        {
            get { return this.listenUri; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.listenUri = value;
            }
        }

        public SecurityStateEncoder SecurityStateEncoder
        {
            get { return this.securityStateEncoder; }
            set 
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.securityStateEncoder = value;
            }
        }

        public IList<Type> KnownTypes
        {
            get { return this.knownTypes; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value != null)
                {
                    this.knownTypes = new Collection<Type>(value);
                }
                else
                {
                    this.knownTypes = null;
                }
            }
        }

        public int MaxMessageSize
        {
            get { return this.maxMessageSize; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.maxMessageSize = value;
            }
        }

        protected string SecurityContextTokenUri
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.sctUri;
            }
        }

        Object ThisLock
        {
            get
            {
                return this.CommunicationObject;
            }
        }

        // helpers
        protected SecurityContextSecurityToken IssueSecurityContextToken(UniqueId contextId, string id, byte[] key,
            DateTime tokenEffectiveTime, DateTime tokenExpirationTime,
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode)
        {
            return IssueSecurityContextToken(contextId, id, key, tokenEffectiveTime, tokenExpirationTime, null,
                tokenEffectiveTime, tokenExpirationTime, authorizationPolicies, isCookieMode);
        }

        protected SecurityContextSecurityToken IssueSecurityContextToken(UniqueId contextId, string id, byte[] key,
            DateTime tokenEffectiveTime, DateTime tokenExpirationTime, UniqueId keyGeneration, DateTime keyEffectiveTime,
            DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            if (this.securityStateEncoder == null && isCookieMode)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SctCookieNotSupported)));
            }
            byte[] cookieBlob = (isCookieMode) ? this.cookieSerializer.CreateCookieFromSecurityContext(contextId, id, key, tokenEffectiveTime, tokenExpirationTime, keyGeneration,
                                keyEffectiveTime, keyExpirationTime, authorizationPolicies) : null;

            SecurityContextSecurityToken issuedToken = new SecurityContextSecurityToken(contextId, id, key, tokenEffectiveTime, tokenExpirationTime,
                authorizationPolicies, isCookieMode, cookieBlob, keyGeneration, keyEffectiveTime, keyExpirationTime);
            return issuedToken;
        }

        void InitializeDefaults()
        {
            this.encryptStateInServiceToken = !defaultServerMaintainState;
            this.serviceTokenLifetime = defaultServerIssuedTokenLifetime;
            this.maximumCachedNegotiationState = defaultServerMaxActiveNegotiations;
            this.negotiationTimeout = defaultServerMaxNegotiationLifetime;
            this.isClientAnonymous = false;
            this.standardsManager = defaultStandardsManager;
            this.securityStateEncoder = defaultSecurityStateEncoder;
            this.maximumConcurrentNegotiations = defaultServerMaxActiveNegotiations;
            // we rely on the transport encoders to enforce the message size except in the 
            // mixed mode nego case, where the client is unauthenticated and the maxMessageSize is too
            // large to be a mitigation
            this.maxMessageSize = Int32.MaxValue;
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.negotiationHost != null)
            {
                this.negotiationHost.Close(timeoutHelper.RemainingTime());
                this.negotiationHost = null;
            }
           
            lock (ThisLock)
            {
                if (this.idlingNegotiationSessionTimer != null && !this.isTimerCancelled)
                {
                    this.isTimerCancelled = true;
                    this.idlingNegotiationSessionTimer.Cancel();
                }
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        public override void OnAbort()
        {
            if (this.negotiationHost != null)
            {
                this.negotiationHost.Abort();
                this.negotiationHost = null;
            }
            
            lock (ThisLock)
            {
                if (this.idlingNegotiationSessionTimer != null && !this.isTimerCancelled)
                {
                    this.isTimerCancelled = true;
                    this.idlingNegotiationSessionTimer.Cancel();
                }
            }
            base.OnAbort();
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.IssuerBindingContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuerBuildContextNotSet, this.GetType())));
            }
            if (this.IssuedSecurityTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuedSecurityTokenParametersNotSet, this.GetType())));
            }
            if (this.SecurityAlgorithmSuite == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityAlgorithmSuiteNotSet, this.GetType())));
            }
            if (this.IssuedTokenCache == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuedTokenCacheNotSet, this.GetType())));
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.SetupServiceHost();
            negotiationHost.Open(timeoutHelper.RemainingTime());
            this.stateCache = new NegotiationTokenAuthenticatorStateCache<T>(this.NegotiationTimeout, this.MaximumCachedNegotiationState);
            this.sctUri = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
            if (this.SecurityStateEncoder != null)
            {
                this.cookieSerializer = new SecurityContextCookieSerializer(this.SecurityStateEncoder, this.KnownTypes);
            }
            if (this.negotiationTimeout < TimeSpan.MaxValue)
            {
                lock (ThisLock)
                {
                    this.activeNegotiationChannels1 = new List<IChannel>();
                    this.activeNegotiationChannels2 = new List<IChannel>();
                    this.idlingNegotiationSessionTimer = new IOThreadTimer(new Action<object>(this.OnIdlingNegotiationSessionTimer), this, false);
                    this.isTimerCancelled = false;
                    this.idlingNegotiationSessionTimer.Set(this.negotiationTimeout);
                }
            }
            base.OnOpen(timeoutHelper.RemainingTime());
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is SecurityContextSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            SecurityContextSecurityToken sct = (SecurityContextSecurityToken)token;
            return sct.AuthorizationPolicies;
        }

        protected abstract Binding GetNegotiationBinding(Binding binding);
        protected abstract bool IsMultiLegNegotiation { get; }
        protected abstract MessageFilter GetListenerFilter();

        void SetupServiceHost()
        {
            ChannelBuilder channelBuilder = new ChannelBuilder(this.IssuerBindingContext.Clone(), true);
            channelBuilder.Binding.Elements.Insert(0, new ReplyAdapterBindingElement());
            channelBuilder.Binding = new CustomBinding(this.GetNegotiationBinding(channelBuilder.Binding));
            negotiationHost = new NegotiationHost(this, this.ListenUri, channelBuilder, this.GetListenerFilter());
        }


        // message processing abstract method
        protected abstract BodyWriter ProcessRequestSecurityToken(Message request, RequestSecurityToken requestSecurityToken, out T negotiationState);
        protected abstract BodyWriter ProcessRequestSecurityTokenResponse(T negotiationState, Message request, RequestSecurityTokenResponse requestSecurityTokenResponse);

        // message handlers
        protected virtual void ParseMessageBody(Message message, out string context, out RequestSecurityToken requestSecurityToken, out RequestSecurityTokenResponse requestSecurityTokenResponse)
        {
            requestSecurityToken = null;
            requestSecurityTokenResponse = null;
            if (message.Headers.Action == this.RequestSecurityTokenAction.Value)
            {
                XmlDictionaryReader reader = message.GetReaderAtBodyContents();
                using (reader)
                {
                    requestSecurityToken = RequestSecurityToken.CreateFrom(this.StandardsManager, reader);
                    message.ReadFromBodyContentsToEnd(reader);
                }
                context = requestSecurityToken.Context;
            }
            else if (message.Headers.Action == this.RequestSecurityTokenResponseAction.Value)
            {
                XmlDictionaryReader reader = message.GetReaderAtBodyContents();
                using (reader)
                {
                    requestSecurityTokenResponse = RequestSecurityTokenResponse.CreateFrom(this.StandardsManager, reader);
                    message.ReadFromBodyContentsToEnd(reader);
                }
                context = requestSecurityTokenResponse.Context;
            }
            else
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidActionForNegotiationMessage, message.Headers.Action)), message);
            }
        }

        static Message CreateReply(Message request, XmlDictionaryString action, BodyWriter body)
        {
            if (request.Headers.MessageId != null)
            {
                Message reply = Message.CreateMessage(request.Version, ActionHeader.Create(action, request.Version.Addressing), body);
                reply.InitializeReply(request);
                return reply;
            }
            else
            {
                // the message id may not be present if MapToHttp is true
                return Message.CreateMessage(request.Version, ActionHeader.Create(action, request.Version.Addressing), body);
            }
        }

        void OnTokenIssued(SecurityToken token)
        {
            if (this.issuedSecurityTokenHandler != null)
            {
                this.issuedSecurityTokenHandler(token, null);
            }
        }

        void AddNegotiationChannelForIdleTracking()
        {
            if (OperationContext.Current.SessionId == null)
            {
                return;
            }
            lock (ThisLock)
            {
                if (this.idlingNegotiationSessionTimer == null)
                {
                    return;
                }
                IChannel channel = OperationContext.Current.Channel;
                if (!this.activeNegotiationChannels1.Contains(channel) && !this.activeNegotiationChannels2.Contains(channel))
                {
                    this.activeNegotiationChannels1.Add(channel);
                }
                if (this.isTimerCancelled)
                {
                    this.isTimerCancelled = false;
                    this.idlingNegotiationSessionTimer.Set(this.negotiationTimeout);
                }
            }
        }

        void RemoveNegotiationChannelFromIdleTracking()
        {
            if (OperationContext.Current.SessionId == null)
            {
                return;
            }
            lock (ThisLock)
            {
                if (this.idlingNegotiationSessionTimer == null)
                {
                    return;
                }
                IChannel channel = OperationContext.Current.Channel;
                this.activeNegotiationChannels1.Remove(channel);
                this.activeNegotiationChannels2.Remove(channel);
                if (this.activeNegotiationChannels1.Count == 0 && this.activeNegotiationChannels2.Count == 0)
                {
                    this.isTimerCancelled = true;
                    this.idlingNegotiationSessionTimer.Cancel();
                }
            }
        }

        void OnIdlingNegotiationSessionTimer(object state)
        {
            lock (ThisLock)
            {
                if (this.isTimerCancelled || (this.CommunicationObject.State != CommunicationState.Opened && this.CommunicationObject.State != CommunicationState.Opening))
                {
                    return;
                }

                try
                {
                    for (int i = 0; i < this.activeNegotiationChannels2.Count; ++i)
                    {
                        this.activeNegotiationChannels2[i].Abort();
                    }
                    List<IChannel> temp = this.activeNegotiationChannels2;
                    temp.Clear();
                    this.activeNegotiationChannels2 = this.activeNegotiationChannels1;
                    this.activeNegotiationChannels1 = temp;
                }
#pragma warning suppress 56500
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
                finally
                {
                    if (this.CommunicationObject.State == CommunicationState.Opened || this.CommunicationObject.State == CommunicationState.Opening)
                    {
                        if (this.activeNegotiationChannels1.Count == 0 && this.activeNegotiationChannels2.Count == 0)
                        {
                            this.isTimerCancelled = true;
                            this.idlingNegotiationSessionTimer.Cancel();
                        }
                        else
                        {
                            this.idlingNegotiationSessionTimer.Set(this.negotiationTimeout);
                        }
                    }
                }
            }
        }

        Message ProcessRequestCore(Message request)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            Uri to = null;
            RequestSecurityToken rst = null;
            RequestSecurityTokenResponse rstr = null;
            string context = null;
            bool disposeRequest = false;
            bool isNegotiationFailure = true;
            T negotiationState = null;

            try
            {
                // validate the message size if needed
                if (this.maxMessageSize < int.MaxValue)
                {
                    string action = request.Headers.Action;
                    try
                    {
                        using (MessageBuffer buffer = request.CreateBufferedCopy(this.maxMessageSize))
                        {
                            request = buffer.CreateMessage();
                            disposeRequest = true;
                        }
                    }
                    catch (QuotaExceededException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.SecurityNegotiationMessageTooLarge, action, this.maxMessageSize), e));
                    }
                }
                try
                {
                    to = request.Headers.To;
                    ParseMessageBody(request, out context, out rst, out rstr);
                    // check if there is existing state
                    if (context != null)
                    {
                        negotiationState = this.stateCache.GetState(context);
                    }
                    else
                    {
                        negotiationState = null;
                    }
                    bool disposeState = false;
                    BodyWriter replyBody;
                    try
                    {
                        if (rst != null)
                        {
                            if (negotiationState != null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.NegotiationStateAlreadyPresent, context)));
                            }
                            replyBody = this.ProcessRequestSecurityToken(request, rst, out negotiationState);
                            lock (negotiationState.ThisLock)
                            {
                                if (negotiationState.IsNegotiationCompleted)
                                {
                                    // if session-sct add it to cache and add a redirect header
                                    if (!negotiationState.ServiceToken.IsCookieMode)
                                    {
                                        this.IssuedTokenCache.AddContext(negotiationState.ServiceToken);
                                    }
                                    this.OnTokenIssued(negotiationState.ServiceToken);
                                    SecurityTraceRecordHelper.TraceServiceSecurityNegotiationCompleted(request, this, negotiationState.ServiceToken);
                                    disposeState = true;
                                }
                                else
                                {
                                    this.stateCache.AddState(context, negotiationState);
                                    disposeState = false;
                                }
                                AddNegotiationChannelForIdleTracking();
                            }
                        }
                        else
                        {
                            if (negotiationState == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.CannotFindNegotiationState, context)));
                            }
                            lock (negotiationState.ThisLock)
                            {
                                replyBody = this.ProcessRequestSecurityTokenResponse(negotiationState, request, rstr);
                                if (negotiationState.IsNegotiationCompleted)
                                {
                                    // if session-sct add it to cache and add a redirect header
                                    if (!negotiationState.ServiceToken.IsCookieMode)
                                    {
                                        this.IssuedTokenCache.AddContext(negotiationState.ServiceToken);
                                    }
                                    this.OnTokenIssued(negotiationState.ServiceToken);
                                    SecurityTraceRecordHelper.TraceServiceSecurityNegotiationCompleted(request, this, negotiationState.ServiceToken);
                                    disposeState = true;
                                }
                                else
                                {
                                    disposeState = false;
                                }
                            }
                        }

                        if (negotiationState.IsNegotiationCompleted && null != this.ListenUri)
                        {
                            if (AuditLevel.Success == (this.messageAuthenticationAuditLevel & AuditLevel.Success))
                            {
                                string primaryIdentity = negotiationState.GetRemoteIdentityName();
                                SecurityAuditHelper.WriteSecurityNegotiationSuccessEvent(this.auditLogLocation,
                                    this.suppressAuditFailure, request, request.Headers.To, request.Headers.Action,
                                    primaryIdentity, this.GetType().Name);
                            }
                        }
                        isNegotiationFailure = false;
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                            throw;

                        if (PerformanceCounters.PerformanceCountersEnabled && null != this.ListenUri)
                        {
                            PerformanceCounters.AuthenticationFailed(request, this.ListenUri);
                        }
                        if (AuditLevel.Failure == (this.messageAuthenticationAuditLevel & AuditLevel.Failure))
                        {
                            try
                            {
                                string primaryIdentity = (negotiationState != null) ? negotiationState.GetRemoteIdentityName() : String.Empty;
                                SecurityAuditHelper.WriteSecurityNegotiationFailureEvent(this.auditLogLocation,
                                    this.suppressAuditFailure, request, request.Headers.To, request.Headers.Action,
                                    primaryIdentity, this.GetType().Name, exception);
                            }
#pragma warning suppress 56500
                            catch (Exception auditException)
                            {
                                if (Fx.IsFatal(auditException))
                                    throw;

                                DiagnosticUtility.TraceHandledException(auditException, TraceEventType.Error);
                            }
                        }

                        disposeState = true;
                        throw;
                    }
                    finally
                    {
                        if (disposeState)
                        {
                            if (negotiationState != null)
                            {
                                if (context != null)
                                {
                                    stateCache.RemoveState(context);
                                }
                                negotiationState.Dispose();
                            }
                        }
                    }

                    return CreateReply(request, (replyBody is RequestSecurityTokenResponseCollection) ? RequestSecurityTokenResponseFinalAction : RequestSecurityTokenResponseAction, replyBody);
                }
                finally
                {
                    if (disposeRequest)
                    {
                        request.Close();
                    }
                }
            }
            finally
            {
                if (isNegotiationFailure)
                {
                    AddNegotiationChannelForIdleTracking();
                }
                else if (negotiationState != null && negotiationState.IsNegotiationCompleted)
                {
                    RemoveNegotiationChannelFromIdleTracking();
                }
            }
        }

        // negotiation failure methods
        Message HandleNegotiationException(Message request, Exception e)
        {
            
            SecurityTraceRecordHelper.TraceServiceSecurityNegotiationFailure<T>(
                                    EventTraceActivityHelper.TryExtractActivity(request),
                                    this, 
                                    e);
            return CreateFault(request, e);
        }

        Message CreateFault(Message request, Exception e)
        {
            MessageVersion version = request.Version;
            FaultCode subCode;
            FaultReason reason;
            bool isSenderFault;
            if (e is SecurityTokenValidationException || e is System.ComponentModel.Win32Exception)
            {
                subCode = new FaultCode(TrustApr2004Strings.FailedAuthenticationFaultCode, TrustFeb2005Strings.Namespace);
                reason = new FaultReason(SR.GetString(SR.FailedAuthenticationTrustFaultCode), CultureInfo.CurrentCulture);
                isSenderFault = true;
            }
            else if (e is QuotaExceededException)
            {
                // send a receiver fault so that the sender can retry
                subCode = new FaultCode(DotNetSecurityStrings.SecurityServerTooBusyFault, DotNetSecurityStrings.Namespace);
                reason = new FaultReason(SR.GetString(SR.NegotiationQuotasExceededFaultReason), CultureInfo.CurrentCulture);
                isSenderFault = false;
            }
            else
            {
                subCode = new FaultCode(TrustApr2004Strings.InvalidRequestFaultCode, TrustFeb2005Strings.Namespace);
                reason = new FaultReason(SR.GetString(SR.InvalidRequestTrustFaultCode), CultureInfo.CurrentCulture);
                isSenderFault = true;
            }
            FaultCode faultCode;
            if (isSenderFault)
            {
                faultCode = FaultCode.CreateSenderFaultCode(subCode);
            }
            else
            {
                faultCode = FaultCode.CreateReceiverFaultCode(subCode);
            }
            MessageFault fault = MessageFault.CreateFault(faultCode, reason);
            Message faultReply = Message.CreateMessage(version, fault, version.Addressing.DefaultFaultAction);
            faultReply.Headers.RelatesTo = request.Headers.MessageId;

            return faultReply;
        }

        class NegotiationHost : ServiceHostBase
        {
            NegotiationTokenAuthenticator<T> authenticator;
            Uri listenUri;
            ChannelBuilder channelBuilder;
            MessageFilter listenerFilter;

            public NegotiationHost(NegotiationTokenAuthenticator<T> authenticator, Uri listenUri, ChannelBuilder channelBuilder, MessageFilter listenerFilter)
            {
                this.authenticator = authenticator;
                this.listenUri = listenUri;
                this.channelBuilder = channelBuilder;
                this.listenerFilter = listenerFilter;
            }

            protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
            {
                implementedContracts = null;
                return null;
            }

            protected override void InitializeRuntime()
            {
                MessageFilter contractFilter = this.listenerFilter;
                int filterPriority = 10;
                Type[] endpointChannelTypes = new Type[] { typeof(IReplyChannel),
                                                           typeof(IDuplexChannel),
                                                           typeof(IReplySessionChannel),
                                                           typeof(IDuplexSessionChannel) };
                IChannelListener listener = null;
                BindingParameterCollection parameters = new BindingParameterCollection(this.channelBuilder.BindingParameters);
                Binding binding = this.channelBuilder.Binding;
                binding.ReceiveTimeout = this.authenticator.NegotiationTimeout;
                parameters.Add(new ChannelDemuxerFilter(contractFilter, filterPriority));
                DispatcherBuilder.MaybeCreateListener(true, endpointChannelTypes, binding, parameters,
                                                      this.listenUri, "", ListenUriMode.Explicit, this.ServiceThrottle, out listener);
                if (listener == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotCreateTwoWayListenerForNegotiation)));
                }
                ChannelDispatcher channelDispatcher = new ChannelDispatcher(listener, null, binding);
                channelDispatcher.MessageVersion = binding.MessageVersion;
                channelDispatcher.ManualAddressing = true;
                channelDispatcher.ServiceThrottle = new ServiceThrottle(this);
                channelDispatcher.ServiceThrottle.MaxConcurrentCalls = this.authenticator.MaximumConcurrentNegotiations;
                channelDispatcher.ServiceThrottle.MaxConcurrentSessions = this.authenticator.MaximumConcurrentNegotiations;

                EndpointDispatcher endpointDispatcher = new EndpointDispatcher(new EndpointAddress(this.listenUri), "SecurityNegotiationContract", NamingHelper.DefaultNamespace, true);
                endpointDispatcher.DispatchRuntime.SingletonInstanceContext = new InstanceContext(null, this.authenticator, false);
                endpointDispatcher.DispatchRuntime.ConcurrencyMode = ConcurrencyMode.Multiple;
                endpointDispatcher.AddressFilter = new MatchAllMessageFilter();
                endpointDispatcher.ContractFilter = contractFilter;
                endpointDispatcher.FilterPriority = filterPriority;
                endpointDispatcher.DispatchRuntime.PrincipalPermissionMode = PrincipalPermissionMode.None;
                endpointDispatcher.DispatchRuntime.InstanceContextProvider = new SingletonInstanceContextProvider(endpointDispatcher.DispatchRuntime);
                endpointDispatcher.DispatchRuntime.SynchronizationContext = null;
                
                DispatchOperation operation = new DispatchOperation(endpointDispatcher.DispatchRuntime, "*", MessageHeaders.WildcardAction, MessageHeaders.WildcardAction);
                operation.Formatter = new MessageOperationFormatter();
                operation.Invoker = new NegotiationSyncInvoker(this.authenticator);

                endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation = operation;
                channelDispatcher.Endpoints.Add(endpointDispatcher);
                this.ChannelDispatchers.Add(channelDispatcher);
            }
           
            class NegotiationSyncInvoker : IOperationInvoker
            {
                NegotiationTokenAuthenticator<T> parent;

                internal NegotiationSyncInvoker(NegotiationTokenAuthenticator<T> parent)
                {
                    this.parent = parent;
                }

                public bool IsSynchronous { get { return true; } }

                public object[] AllocateInputs()
                {
                    return EmptyArray<object>.Allocate(1);
                }

                public object Invoke(object instance, object[] inputs, out object[] outputs)
                {
                    Message request = (Message)inputs[0];
                    outputs = EmptyArray<object>.Allocate(0);
                    try
                    {
                        return parent.ProcessRequestCore(request);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        return parent.HandleNegotiationException(request, e);
                    }
                }

                public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }

                public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
            }
        }
    }
}
