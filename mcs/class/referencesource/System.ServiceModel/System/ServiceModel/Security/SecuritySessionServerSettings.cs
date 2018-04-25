//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Threading;
    using System.Xml;
    using System.Globalization;
    using System.ServiceModel.Diagnostics.Application;

    // Please use 'sdv //depot/devdiv/private/indigo_xws/ndp/indigo/src/ServiceModel/System/ServiceModel/Security/SecuritySessionListenerFactory.cs' 
    // to see version history before the file was renamed
    // This class is named Settings since the only public APIs are for
    // settings; however, this class also manages all functionality
    // for session channels through internal APIs
    sealed class SecuritySessionServerSettings : IListenerSecureConversationSessionSettings, ISecurityCommunicationObject
    {
        internal const string defaultKeyRenewalIntervalString = "15:00:00";
        internal const string defaultKeyRolloverIntervalString = "00:05:00";
        internal const string defaultInactivityTimeoutString = "00:02:00";

        internal static readonly TimeSpan defaultKeyRenewalInterval = TimeSpan.Parse(defaultKeyRenewalIntervalString, CultureInfo.InvariantCulture);
        internal static readonly TimeSpan defaultKeyRolloverInterval = TimeSpan.Parse(defaultKeyRolloverIntervalString, CultureInfo.InvariantCulture);
        internal const bool defaultTolerateTransportFailures = true;
        internal const int defaultMaximumPendingSessions = 128;
        internal static readonly TimeSpan defaultInactivityTimeout = TimeSpan.Parse(defaultInactivityTimeoutString, CultureInfo.InvariantCulture);

        int maximumPendingSessions;
        Dictionary<UniqueId, IServerReliableChannelBinder> pendingSessions1;
        Dictionary<UniqueId, IServerReliableChannelBinder> pendingSessions2;
        IOThreadTimer inactivityTimer;
        TimeSpan inactivityTimeout;
        bool tolerateTransportFailures;
        TimeSpan maximumKeyRenewalInterval;
        TimeSpan keyRolloverInterval;
        int maximumPendingKeysPerSession;
        SecurityProtocolFactory sessionProtocolFactory;
        ICommunicationObject channelAcceptor;
        Dictionary<UniqueId, IServerSecuritySessionChannel> activeSessions;
        ChannelListenerBase securityChannelListener;
        ChannelBuilder channelBuilder;
        SecurityStandardsManager standardsManager;
        SecurityTokenParameters issuedTokenParameters;
        SecurityTokenAuthenticator sessionTokenAuthenticator;
        ISecurityContextSecurityTokenCache sessionTokenCache;
        SecurityTokenResolver sessionTokenResolver;
        WrapperSecurityCommunicationObject communicationObject;
        volatile bool acceptNewWork;
        MessageVersion messageVersion;
        TimeSpan closeTimeout;
        TimeSpan openTimeout;
        TimeSpan sendTimeout;
        Uri listenUri;
        SecurityListenerSettingsLifetimeManager settingsLifetimeManager;
        bool canRenewSession = true;
        object thisLock = new object();

        public SecuritySessionServerSettings()
        {
            activeSessions = new Dictionary<UniqueId, IServerSecuritySessionChannel>();

            this.maximumKeyRenewalInterval = defaultKeyRenewalInterval;
            this.maximumPendingKeysPerSession = 5;
            this.keyRolloverInterval = defaultKeyRolloverInterval;
            this.inactivityTimeout = defaultInactivityTimeout;
            this.tolerateTransportFailures = defaultTolerateTransportFailures;
            this.maximumPendingSessions = defaultMaximumPendingSessions;
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        internal ChannelBuilder ChannelBuilder
        {
            get
            {
                return this.channelBuilder;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.channelBuilder = value;
            }
        }

        internal SecurityListenerSettingsLifetimeManager SettingsLifetimeManager
        {
            get
            {
                return this.settingsLifetimeManager;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.settingsLifetimeManager = value;
            }
        }

        internal ChannelListenerBase SecurityChannelListener
        {
            get
            {
                return this.securityChannelListener;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.securityChannelListener = value;
            }
        }

        Uri Uri
        {
            get
            {
                this.communicationObject.ThrowIfNotOpened();
                return this.listenUri;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public SecurityTokenAuthenticator SessionTokenAuthenticator
        {
            get
            {
                return this.sessionTokenAuthenticator;
            }
        }

        public ISecurityContextSecurityTokenCache SessionTokenCache
        {
            get
            {
                return this.sessionTokenCache;
            }
        }

        public SecurityTokenResolver SessionTokenResolver
        {
            get
            {
                return this.sessionTokenResolver;
            }
        }

        public SecurityTokenParameters IssuedSecurityTokenParameters
        {
            get
            {
                return this.issuedTokenParameters;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.issuedTokenParameters = value;
            }
        }

        public SecurityStandardsManager SecurityStandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.standardsManager = value;
            }
        }

        public bool TolerateTransportFailures
        {
            get
            {
                return this.tolerateTransportFailures;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.tolerateTransportFailures = value;
            }
        }

        public bool CanRenewSession
        {
            get
            {
                return this.canRenewSession;
            }
            set
            {
                this.canRenewSession = value;
            }
        }

        public int MaximumPendingSessions
        {
            get
            {
                return this.maximumPendingSessions;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.maximumPendingSessions = value;
            }
        }

        public TimeSpan InactivityTimeout
        {
            get
            {
                return this.inactivityTimeout;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.inactivityTimeout = value;
            }
        }

        public TimeSpan MaximumKeyRenewalInterval
        {
            get
            {
                return this.maximumKeyRenewalInterval;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.maximumKeyRenewalInterval = value;
            }
        }

        public TimeSpan KeyRolloverInterval
        {
            get
            {
                return this.keyRolloverInterval;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.keyRolloverInterval = value;
            }
        }

        public int MaximumPendingKeysPerSession
        {
            get
            {
                return this.maximumPendingKeysPerSession;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeGreaterThanZero)));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.maximumPendingKeysPerSession = value;
            }
        }

        public SecurityProtocolFactory SessionProtocolFactory
        {
            get
            {
                return this.sessionProtocolFactory;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.sessionProtocolFactory = value;
            }
        }

        public MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public TimeSpan OpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
        }

        public TimeSpan CloseTimeout
        {
            get
            {
                return this.closeTimeout;
            }
        }

        public TimeSpan SendTimeout
        {
            get
            {
                return this.sendTimeout;
            }
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

        public void OnAbort()
        {
            this.AbortPendingChannels();
            this.OnAbortCore();
        }

        public void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.ClosePendingChannels(timeoutHelper.RemainingTime());
            this.OnCloseCore(timeoutHelper.RemainingTime());
        }

        internal void Close(TimeSpan timeout)
        {
            this.communicationObject.Close(timeout);
        }

        internal IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        internal void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        internal void Abort()
        {
            this.communicationObject.Abort();
        }

        internal void Open(TimeSpan timeout)
        {
            this.communicationObject.Open(timeout);
        }

        internal IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(timeout, callback, state);
        }

        internal void EndOpen(IAsyncResult result)
        {
            this.communicationObject.EndOpen(result);
        }

        void OnCloseCore(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.inactivityTimer != null)
            {
                this.inactivityTimer.Cancel();
            }
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(false, timeoutHelper.RemainingTime());
            }
            if (this.sessionTokenAuthenticator != null)
            {
                SecurityUtils.CloseTokenAuthenticatorIfRequired(this.sessionTokenAuthenticator, timeoutHelper.RemainingTime());
            }
        }

        void OnAbortCore()
        {
            if (this.inactivityTimer != null)
            {
                this.inactivityTimer.Cancel();
            }
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(true, TimeSpan.Zero);
            }
            if (this.sessionTokenAuthenticator != null)
            {
                SecurityUtils.AbortTokenAuthenticatorIfRequired(this.sessionTokenAuthenticator);
            }
        }

        void SetupSessionTokenAuthenticator()
        {
            RecipientServiceModelSecurityTokenRequirement requirement = new RecipientServiceModelSecurityTokenRequirement();
            this.issuedTokenParameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = SecurityKeyUsage.Signature;
            requirement.ListenUri = this.listenUri;
            requirement.SecurityBindingElement = this.sessionProtocolFactory.SecurityBindingElement;
            requirement.SecurityAlgorithmSuite = this.sessionProtocolFactory.IncomingAlgorithmSuite;
            requirement.SupportSecurityContextCancellation = true;
            requirement.MessageSecurityVersion = sessionProtocolFactory.MessageSecurityVersion.SecurityTokenVersion;
            requirement.AuditLogLocation = sessionProtocolFactory.AuditLogLocation;
            requirement.SuppressAuditFailure = sessionProtocolFactory.SuppressAuditFailure;
            requirement.MessageAuthenticationAuditLevel = sessionProtocolFactory.MessageAuthenticationAuditLevel;
            requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
            if (sessionProtocolFactory.EndpointFilterTable != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.EndpointFilterTableProperty] = sessionProtocolFactory.EndpointFilterTable;
            }
            this.sessionTokenAuthenticator = this.sessionProtocolFactory.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out this.sessionTokenResolver);
            if (!(this.sessionTokenAuthenticator is IIssuanceSecurityTokenAuthenticator))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecuritySessionRequiresIssuanceAuthenticator, typeof(IIssuanceSecurityTokenAuthenticator), this.sessionTokenAuthenticator.GetType())));
            }
            if (sessionTokenResolver == null || (!(sessionTokenResolver is ISecurityContextSecurityTokenCache)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecuritySessionRequiresSecurityContextTokenCache, this.sessionTokenResolver.GetType(), typeof(ISecurityContextSecurityTokenCache))));
            }
            this.sessionTokenCache = (ISecurityContextSecurityTokenCache)this.sessionTokenResolver;
        }


        public void OnOpen(TimeSpan timeout)
        {
            if (this.sessionProtocolFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecuritySessionProtocolFactoryShouldBeSetBeforeThisOperation)));
            }
            if (this.standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityStandardsManagerNotSet, this.GetType())));
            }
            if (this.issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuedSecurityTokenParametersNotSet, this.GetType())));
            }
            if (this.maximumKeyRenewalInterval < this.keyRolloverInterval)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.KeyRolloverGreaterThanKeyRenewal)));
            }
            if (this.securityChannelListener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityChannelListenerNotSet, this.GetType())));
            }
            if (this.settingsLifetimeManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecuritySettingsLifetimeManagerNotSet, this.GetType())));
            }

            this.messageVersion = this.channelBuilder.Binding.MessageVersion;
            this.listenUri = this.securityChannelListener.Uri;
            this.openTimeout = this.securityChannelListener.InternalOpenTimeout;
            this.closeTimeout = this.securityChannelListener.InternalCloseTimeout;
            this.sendTimeout = this.securityChannelListener.InternalSendTimeout;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.pendingSessions1 = new Dictionary<UniqueId, IServerReliableChannelBinder>();
            this.pendingSessions2 = new Dictionary<UniqueId, IServerReliableChannelBinder>();
            if (this.inactivityTimeout < TimeSpan.MaxValue)
            {
                this.inactivityTimer = new IOThreadTimer(new Action<object>(this.OnTimer), this, false);
                this.inactivityTimer.Set(this.inactivityTimeout);
            }
            this.ConfigureSessionSecurityProtocolFactory();
            this.sessionProtocolFactory.Open(false, timeoutHelper.RemainingTime());
            SetupSessionTokenAuthenticator();
            ((IIssuanceSecurityTokenAuthenticator)this.sessionTokenAuthenticator).IssuedSecurityTokenHandler = this.OnTokenIssued;
            ((IIssuanceSecurityTokenAuthenticator)this.sessionTokenAuthenticator).RenewedSecurityTokenHandler = this.OnTokenRenewed;
            this.acceptNewWork = true;
            SecurityUtils.OpenTokenAuthenticatorIfRequired(this.sessionTokenAuthenticator, timeoutHelper.RemainingTime());
        }

        public void StopAcceptingNewWork()
        {
            this.acceptNewWork = false;
        }

        int GetPendingSessionCount()
        {
            return this.pendingSessions1.Count + this.pendingSessions2.Count + ((IInputQueueChannelAcceptor)this.channelAcceptor).PendingCount;
        }

        void AbortPendingChannels()
        {
            lock (ThisLock)
            {
                if (this.pendingSessions1 != null)
                {
                    foreach (IServerReliableChannelBinder pendingChannelBinder in pendingSessions1.Values)
                    {
                        pendingChannelBinder.Abort();
                    }
                }
                if (this.pendingSessions2 != null)
                {
                    foreach (IServerReliableChannelBinder pendingChannelBinder in pendingSessions2.Values)
                    {
                        pendingChannelBinder.Abort();
                    }
                }
            }
        }

        void ClosePendingChannels(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            lock (ThisLock)
            {
                foreach (IServerReliableChannelBinder pendingChannelBinder in pendingSessions1.Values)
                {
                    pendingChannelBinder.Close(timeoutHelper.RemainingTime());
                }
                foreach (IServerReliableChannelBinder pendingChannelBinder in pendingSessions2.Values)
                {
                    pendingChannelBinder.Close(timeoutHelper.RemainingTime());
                }
            }
        }

        void ConfigureSessionSecurityProtocolFactory()
        {
            if (this.sessionProtocolFactory is SessionSymmetricMessageSecurityProtocolFactory)
            {
                AddressingVersion addressing = MessageVersion.Default.Addressing;
                if (this.channelBuilder != null)
                {
                    MessageEncodingBindingElement encoding = this.channelBuilder.Binding.Elements.Find<MessageEncodingBindingElement>();
                    if (encoding != null)
                    {
                        addressing = encoding.MessageVersion.Addressing;
                    }
                }

                if (addressing != AddressingVersion.WSAddressing10 && addressing != AddressingVersion.WSAddressingAugust2004)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, addressing)));
                }

                SessionSymmetricMessageSecurityProtocolFactory messagePf = (SessionSymmetricMessageSecurityProtocolFactory)this.sessionProtocolFactory;
                if (!messagePf.ApplyIntegrity || !messagePf.RequireIntegrity)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecuritySessionRequiresMessageIntegrity)));
                }
                MessagePartSpecification bodyPart = new MessagePartSpecification(true);
                messagePf.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                messagePf.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                messagePf.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                messagePf.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                messagePf.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, addressing.FaultAction);
                messagePf.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, addressing.DefaultFaultAction);
                messagePf.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, DotNetSecurityStrings.SecuritySessionFaultAction);
                if (messagePf.ApplyConfidentiality)
                {
                    messagePf.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                    messagePf.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    messagePf.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, addressing.FaultAction);
                    messagePf.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, addressing.DefaultFaultAction);
                    messagePf.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, DotNetSecurityStrings.SecuritySessionFaultAction);
                }
                if (messagePf.RequireConfidentiality)
                {
                    messagePf.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    messagePf.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                }
                messagePf.SecurityTokenParameters = this.IssuedSecurityTokenParameters;
            }
            else if (this.sessionProtocolFactory is SessionSymmetricTransportSecurityProtocolFactory)
            {
                SessionSymmetricTransportSecurityProtocolFactory transportPf = (SessionSymmetricTransportSecurityProtocolFactory)this.sessionProtocolFactory;
                transportPf.AddTimestamp = true;
                transportPf.SecurityTokenParameters = this.IssuedSecurityTokenParameters;
                transportPf.SecurityTokenParameters.RequireDerivedKeys = false;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        internal IChannelAcceptor<TChannel> CreateAcceptor<TChannel>()
            where TChannel : class, IChannel
        {
            if (this.channelAcceptor != null)
            {
                Fx.Assert("SecuritySessionServerSettings.CreateAcceptor (this.channelAcceptor != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SSSSCreateAcceptor)));
            }
            object listenerSecurityState = this.sessionProtocolFactory.CreateListenerSecurityState();
            if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                this.channelAcceptor = new SecuritySessionChannelAcceptor<IReplySessionChannel>(this.SecurityChannelListener, listenerSecurityState);
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                this.channelAcceptor = new SecuritySessionChannelAcceptor<IDuplexSessionChannel>(this.SecurityChannelListener, listenerSecurityState);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return (IChannelAcceptor<TChannel>)this.channelAcceptor;
        }

        internal IChannelListener CreateInnerChannelListener()
        {
            if (this.ChannelBuilder.CanBuildChannelListener<IDuplexSessionChannel>())
            {
                return this.ChannelBuilder.BuildChannelListener<IDuplexSessionChannel>(new MatchNoneMessageFilter(), int.MinValue);
            }
            else if (this.ChannelBuilder.CanBuildChannelListener<IDuplexChannel>())
            {
                return this.ChannelBuilder.BuildChannelListener<IDuplexChannel>(new MatchNoneMessageFilter(), int.MinValue);
            }
            else if (this.ChannelBuilder.CanBuildChannelListener<IReplyChannel>())
            {
                return this.ChannelBuilder.BuildChannelListener<IReplyChannel>(new MatchNoneMessageFilter(), int.MinValue);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        void OnTokenRenewed(SecurityToken newToken, SecurityToken oldToken)
        {
            this.communicationObject.ThrowIfClosed();
            if (!this.acceptNewWork)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(SR.SecurityListenerClosing)));
            }
            SecurityContextSecurityToken newSecurityContextToken = newToken as SecurityContextSecurityToken;
            if (newSecurityContextToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SessionTokenIsNotSecurityContextToken, newToken.GetType(), typeof(SecurityContextSecurityToken))));
            }
            SecurityContextSecurityToken oldSecurityContextToken = oldToken as SecurityContextSecurityToken;
            if (oldSecurityContextToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SessionTokenIsNotSecurityContextToken, oldToken.GetType(), typeof(SecurityContextSecurityToken))));
            }
            IServerSecuritySessionChannel sessionChannel = this.FindSessionChannel(newSecurityContextToken.ContextId);
            if (sessionChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.CannotFindSecuritySession, newSecurityContextToken.ContextId)));
            }
            sessionChannel.RenewSessionToken(newSecurityContextToken, oldSecurityContextToken);
        }

        IServerReliableChannelBinder CreateChannelBinder(SecurityContextSecurityToken sessionToken, EndpointAddress remoteAddress)
        {
            IServerReliableChannelBinder result = null;
            MessageFilter sctFilter = new SecuritySessionFilter(sessionToken.ContextId, this.sessionProtocolFactory.StandardsManager, (this.sessionProtocolFactory.SecurityHeaderLayout == SecurityHeaderLayout.Strict), this.SecurityStandardsManager.SecureConversationDriver.RenewAction.Value, this.SecurityStandardsManager.SecureConversationDriver.RenewResponseAction.Value);
            int sctPriority = Int32.MaxValue;
            TolerateFaultsMode faultMode = this.TolerateTransportFailures ? TolerateFaultsMode.Always : TolerateFaultsMode.Never;
            lock (ThisLock)
            {
                if (this.ChannelBuilder.CanBuildChannelListener<IDuplexSessionChannel>())
                {
                    result = ServerReliableChannelBinder<IDuplexSessionChannel>.CreateBinder(this.ChannelBuilder, remoteAddress, sctFilter, sctPriority, faultMode,
                        this.CloseTimeout, this.SendTimeout);
                }
                else if (this.ChannelBuilder.CanBuildChannelListener<IDuplexChannel>())
                {
                    result = ServerReliableChannelBinder<IDuplexChannel>.CreateBinder(this.ChannelBuilder, remoteAddress, sctFilter, sctPriority, faultMode,
                        this.CloseTimeout, this.SendTimeout);
                }
                else if (this.ChannelBuilder.CanBuildChannelListener<IReplyChannel>())
                {
                    result = ServerReliableChannelBinder<IReplyChannel>.CreateBinder(this.ChannelBuilder, remoteAddress, sctFilter, sctPriority, faultMode,
                        this.CloseTimeout, this.SendTimeout);
                }
            }
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            result.Open(this.OpenTimeout);
            SessionInitiationMessageHandler handler = new SessionInitiationMessageHandler(result, this, sessionToken);
            handler.BeginReceive(TimeSpan.MaxValue);
            return result;
        }

        void OnTokenIssued(SecurityToken issuedToken, EndpointAddress tokenRequestor)
        {
            this.communicationObject.ThrowIfClosed();
            if (!this.acceptNewWork)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(SR.SecurityListenerClosing)));
            }
            SecurityContextSecurityToken issuedSecurityContextToken = issuedToken as SecurityContextSecurityToken;
            if (issuedSecurityContextToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SessionTokenIsNotSecurityContextToken, issuedToken.GetType(), typeof(SecurityContextSecurityToken))));
            }
            IServerReliableChannelBinder channelBinder = CreateChannelBinder(issuedSecurityContextToken, tokenRequestor ?? EndpointAddress.AnonymousAddress);
            bool wasSessionAdded = false;
            try
            {
                this.AddPendingSession(issuedSecurityContextToken.ContextId, channelBinder);
                wasSessionAdded = true;
            }
            finally
            {
                if (!wasSessionAdded)
                {
                    channelBinder.Abort();
                }
            }
        }

        void OnTimer(object state)
        {
            if (this.communicationObject.State == CommunicationState.Closed
                || this.communicationObject.State == CommunicationState.Faulted)
            {
                return;
            }
            try
            {
                this.ClearPendingSessions();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
            }
            finally
            {
                if (this.communicationObject.State != CommunicationState.Closed
                    && this.communicationObject.State != CommunicationState.Closing
                    && this.communicationObject.State != CommunicationState.Faulted)
                {
                    this.inactivityTimer.Set(this.inactivityTimeout);
                }
            }
        }

        void AddPendingSession(UniqueId sessionId, IServerReliableChannelBinder channelBinder)
        {
            lock (ThisLock)
            {
                if ((GetPendingSessionCount() + 1) > this.MaximumPendingSessions)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(SR.GetString(SR.SecuritySessionLimitReached)));
                }
                if (this.pendingSessions1.ContainsKey(sessionId) || this.pendingSessions2.ContainsKey(sessionId))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SecuritySessionAlreadyPending, sessionId)));
                }
                this.pendingSessions1.Add(sessionId, channelBinder);
            }
            SecurityTraceRecordHelper.TracePendingSessionAdded(sessionId, this.Uri);
            if (TD.SecuritySessionRatioIsEnabled())
            {
                TD.SecuritySessionRatio(GetPendingSessionCount(), this.MaximumPendingSessions);
            }
        }

        void TryCloseBinder(IServerReliableChannelBinder binder, TimeSpan timeout)
        {
            bool abortBinder = false;
            try
            {
                binder.Close(timeout);
            }
            catch (CommunicationException e)
            {
                abortBinder = true;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (TimeoutException e)
            {
                abortBinder = true;

                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            finally
            {
                if (abortBinder)
                {
                    binder.Abort();
                }
            }
        }

        // this method should be called by the timer under ThisLock
        void ClearPendingSessions()
        {
            lock (ThisLock)
            {
                if (this.pendingSessions1.Count == 0 && this.pendingSessions2.Count == 0)
                {
                    return;
                }
                foreach (UniqueId sessionId in this.pendingSessions2.Keys)
                {
                    IServerReliableChannelBinder channelBinder = this.pendingSessions2[sessionId];
                    try
                    {
                        TryCloseBinder(channelBinder, this.CloseTimeout);
                        this.SessionTokenCache.RemoveAllContexts(sessionId);
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.CloseTimeoutIsEnabled())
                        {
                            TD.CloseTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (ObjectDisposedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    SecurityTraceRecordHelper.TracePendingSessionClosed(sessionId, this.Uri);
                }
                this.pendingSessions2.Clear();
                Dictionary<UniqueId, IServerReliableChannelBinder> temp = this.pendingSessions2;
                this.pendingSessions2 = this.pendingSessions1;
                this.pendingSessions1 = temp;
            }
        }

        bool RemovePendingSession(UniqueId sessionId)
        {
            bool result;
            lock (ThisLock)
            {
                if (this.pendingSessions1.ContainsKey(sessionId))
                {
                    this.pendingSessions1.Remove(sessionId);
                    result = true;
                }
                else if (pendingSessions2.ContainsKey(sessionId))
                {
                    this.pendingSessions2.Remove(sessionId);
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            if (result)
            {
                SecurityTraceRecordHelper.TracePendingSessionActivated(sessionId, this.Uri);
                if (TD.SecuritySessionRatioIsEnabled())
                {
                    TD.SecuritySessionRatio(GetPendingSessionCount(), this.MaximumPendingSessions);
                }
            }
            return result;
        }

        IServerSecuritySessionChannel FindSessionChannel(UniqueId sessionId)
        {
            IServerSecuritySessionChannel result;
            lock (ThisLock)
            {
                this.activeSessions.TryGetValue(sessionId, out result);
            }
            return result;
        }

        void AddSessionChannel(UniqueId sessionId, IServerSecuritySessionChannel channel)
        {
            lock (ThisLock)
            {
                this.activeSessions.Add(sessionId, channel);
            }
        }

        void RemoveSessionChannel(string sessionId)
        {
            RemoveSessionChannel(new UniqueId(sessionId));
        }

        void RemoveSessionChannel(UniqueId sessionId)
        {
            lock (ThisLock)
            {
                this.activeSessions.Remove(sessionId);
            }
            SecurityTraceRecordHelper.TraceActiveSessionRemoved(sessionId, this.Uri);
        }

        class SessionInitiationMessageHandler
        {
            static AsyncCallback receiveCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveCallback));
            IServerReliableChannelBinder channelBinder;
            SecuritySessionServerSettings settings;
            SecurityContextSecurityToken sessionToken;
            bool processedInitiation = false;

            public SessionInitiationMessageHandler(IServerReliableChannelBinder channelBinder, SecuritySessionServerSettings settings, SecurityContextSecurityToken sessionToken)
            {
                this.channelBinder = channelBinder;
                this.settings = settings;
                this.sessionToken = sessionToken;
            }

            public IAsyncResult BeginReceive(TimeSpan timeout)
            {
                return this.channelBinder.BeginTryReceive(timeout, receiveCallback, this);
            }

            public void ProcessMessage(IAsyncResult result)
            {
                bool threwException = false;
                try
                {
                    RequestContext requestContext;
                    if (!this.channelBinder.EndTryReceive(result, out requestContext))
                    {
                        // we should never have timed out since the receive was called with an Infinite timeout
                        // if we did then do a BeginReceive and return
                        this.BeginReceive(TimeSpan.MaxValue);
                        return;
                    }

                    if (requestContext == null)
                    {
                        return;
                    }

                    Message message = requestContext.RequestMessage;
                    lock (this.settings.ThisLock)
                    {
                        if (this.settings.communicationObject.State != CommunicationState.Opened)
                        {
                            ((IDisposable)requestContext).Dispose();
                            return;
                        }
                        if (this.processedInitiation)
                        {
                            return;
                        }
                        this.processedInitiation = true;
                    }
                    if (!this.settings.RemovePendingSession(this.sessionToken.ContextId))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(SR.GetString(SR.SecuritySessionNotPending, this.sessionToken.ContextId)));
                    }
                    if (this.settings.channelAcceptor is SecuritySessionChannelAcceptor<IReplySessionChannel>)
                    {
                        SecuritySessionChannelAcceptor<IReplySessionChannel> replyAcceptor = ((SecuritySessionChannelAcceptor<IReplySessionChannel>)this.settings.channelAcceptor);
                        SecurityReplySessionChannel replySessionChannel = new SecurityReplySessionChannel(this.settings,
                            this.channelBinder,
                            sessionToken,
                            replyAcceptor.ListenerSecurityState,
                            this.settings.SettingsLifetimeManager);
                        settings.AddSessionChannel(this.sessionToken.ContextId, replySessionChannel);
                        replySessionChannel.StartReceiving(requestContext);
                        replyAcceptor.EnqueueAndDispatch(replySessionChannel);
                    }
                    else if (this.settings.channelAcceptor is SecuritySessionChannelAcceptor<IDuplexSessionChannel>)
                    {
                        SecuritySessionChannelAcceptor<IDuplexSessionChannel> duplexAcceptor = ((SecuritySessionChannelAcceptor<IDuplexSessionChannel>)this.settings.channelAcceptor);
                        ServerSecurityDuplexSessionChannel duplexSessionChannel = new ServerSecurityDuplexSessionChannel(this.settings,
                            this.channelBinder,
                            sessionToken,
                            duplexAcceptor.ListenerSecurityState,
                            this.settings.SettingsLifetimeManager);
                        settings.AddSessionChannel(this.sessionToken.ContextId, duplexSessionChannel);
                        duplexSessionChannel.StartReceiving(requestContext);
                        duplexAcceptor.EnqueueAndDispatch(duplexSessionChannel);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new EndpointNotFoundException(SR.GetString(SR.SecuritySessionListenerNotFound, message.Headers.Action)));
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    threwException = true;
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                finally
                {
                    if (threwException)
                    {
                        this.channelBinder.Abort();
                    }
                }
            }

            static void ReceiveCallback(IAsyncResult result)
            {
                ((SessionInitiationMessageHandler)result.AsyncState).ProcessMessage(result);
            }
        }

        interface IInputQueueChannelAcceptor
        {
            int PendingCount { get; }
        }

        class SecuritySessionChannelAcceptor<T> : InputQueueChannelAcceptor<T>, IInputQueueChannelAcceptor
            where T : class, IChannel
        {
            object listenerState;

            public SecuritySessionChannelAcceptor(ChannelListenerBase manager, object listenerState)
                : base(manager)
            {
                this.listenerState = listenerState;
            }

            public object ListenerSecurityState
            {
                get
                {
                    return this.listenerState;
                }
            }

            int IInputQueueChannelAcceptor.PendingCount
            {
                get { return this.PendingCount; }
            }
        }

        interface IServerSecuritySessionChannel
        {
            void RenewSessionToken(SecurityContextSecurityToken newToken, SecurityContextSecurityToken supportingToken);
        }

        abstract class ServerSecuritySessionChannel : ChannelBase, IServerSecuritySessionChannel
        {
            FaultCode renewFaultCode;
            FaultReason renewFaultReason;
            FaultCode sessionAbortedFaultCode;
            FaultReason sessionAbortedFaultReason;
            
            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile bool areFaultCodesInitialized;
            IServerReliableChannelBinder channelBinder;
            SecurityProtocol securityProtocol;
            // This is used to sign outgoing messages
            SecurityContextSecurityToken currentSessionToken;
            UniqueId sessionId;
            // These are renewed tokens that have not been used as yet
            List<SecurityContextSecurityToken> futureSessionTokens;
            SecuritySessionServerSettings settings;
            RequestContext initialRequestContext;
            volatile bool isInputClosed;
            ThreadNeutralSemaphore receiveLock;
            MessageVersion messageVersion;
            SecurityListenerSettingsLifetimeManager settingsLifetimeManager;
            volatile bool hasSecurityStateReference;

            protected ServerSecuritySessionChannel(SecuritySessionServerSettings settings,
                IServerReliableChannelBinder channelBinder,
                SecurityContextSecurityToken sessionToken,
                object listenerSecurityProtocolState,
                SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(settings.SecurityChannelListener)
            {
                this.settings = settings;
                this.channelBinder = channelBinder;
                this.messageVersion = settings.MessageVersion;
                this.channelBinder.Faulted += this.OnInnerFaulted;
                this.securityProtocol = this.Settings.SessionProtocolFactory.CreateSecurityProtocol(null, null, listenerSecurityProtocolState, true, TimeSpan.Zero);
                if (!(this.securityProtocol is IAcceptorSecuritySessionProtocol))
                {
                    Fx.Assert("Security protocol must be IAcceptorSecuritySessionProtocol.");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ProtocolMisMatch, "IAcceptorSecuritySessionProtocol", this.GetType().ToString())));
                }
                this.currentSessionToken = sessionToken;
                this.sessionId = sessionToken.ContextId;
                this.futureSessionTokens = new List<SecurityContextSecurityToken>(1);
                ((IAcceptorSecuritySessionProtocol)this.securityProtocol).SetOutgoingSessionToken(sessionToken);
                ((IAcceptorSecuritySessionProtocol)this.securityProtocol).SetSessionTokenAuthenticator(this.sessionId, this.settings.SessionTokenAuthenticator, this.settings.SessionTokenResolver);
                this.settingsLifetimeManager = settingsLifetimeManager;
                this.receiveLock = new ThreadNeutralSemaphore(1);
            }

            protected SecuritySessionServerSettings Settings
            {
                get
                {
                    return this.settings;
                }
            }

            protected virtual bool CanDoSecurityCorrelation
            {
                get
                {
                    return false;
                }
            }

            internal IServerReliableChannelBinder ChannelBinder
            {
                get
                {
                    return this.channelBinder;
                }
            }

            internal TimeSpan InternalSendTimeout
            {
                get
                {
                    return this.DefaultSendTimeout;
                }
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return this.channelBinder.LocalAddress;
                }
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.securityProtocol.Open(timeout);
                if (this.CanDoSecurityCorrelation)
                {
                    ((IAcceptorSecuritySessionProtocol)this.securityProtocol).ReturnCorrelationState = true;
                }
                lock (ThisLock)
                {
                    // if an abort happened concurrently with the open, then return
                    if (this.State == CommunicationState.Closed || this.State == CommunicationState.Closing)
                    {
                        return;
                    }
                    this.settingsLifetimeManager.AddReference();
                    this.hasSecurityStateReference = true;
                }
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                OnOpen(timeout);
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected virtual void AbortCore()
            {
                if (this.channelBinder != null)
                {
                    this.channelBinder.Abort();
                }
                if (this.securityProtocol != null)
                {
                    this.securityProtocol.Close(true, TimeSpan.Zero);
                }
                this.Settings.SessionTokenCache.RemoveAllContexts(this.currentSessionToken.ContextId);
                bool abortLifetimeManager = false;
                lock (ThisLock)
                {
                    if (hasSecurityStateReference)
                    {
                        abortLifetimeManager = true;
                        hasSecurityStateReference = false;
                    }
                }
                if (abortLifetimeManager)
                {
                    this.settingsLifetimeManager.Abort();
                }
            }

            protected virtual void CloseCore(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                try
                {
                    if (this.channelBinder != null)
                    {
                        this.channelBinder.Close(timeoutHelper.RemainingTime());
                    }
                    if (this.securityProtocol != null)
                    {
                        this.securityProtocol.Close(false, timeoutHelper.RemainingTime());
                    }
                    bool closeLifetimeManager = false;
                    lock (ThisLock)
                    {
                        if (hasSecurityStateReference)
                        {
                            closeLifetimeManager = true;
                            hasSecurityStateReference = false;
                        }
                    }
                    if (closeLifetimeManager)
                    {
                        this.settingsLifetimeManager.Close(timeoutHelper.RemainingTime());
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (this.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                    // a parallel thread aborted the channel. Ignore the exception
                }

                this.Settings.SessionTokenCache.RemoveAllContexts(this.currentSessionToken.ContextId);
            }

            protected virtual IAsyncResult BeginCloseCore(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseCoreAsyncResult(this, timeout, callback, state);
            }

            protected virtual void EndCloseCore(IAsyncResult result)
            {
                CloseCoreAsyncResult.End(result);
            }

            protected abstract void OnCloseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout);

            protected abstract void OnCloseResponseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout);

            public void RenewSessionToken(SecurityContextSecurityToken newToken, SecurityContextSecurityToken supportingToken)
            {
                ThrowIfClosedOrNotOpen();
                // enforce that the token being renewed is the current session token
                lock (ThisLock)
                {
                    if (supportingToken.ContextId != this.currentSessionToken.ContextId || supportingToken.KeyGeneration != this.currentSessionToken.KeyGeneration)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.CurrentSessionTokenNotRenewed, supportingToken.KeyGeneration, this.currentSessionToken.KeyGeneration)));
                    }
                    if (this.futureSessionTokens.Count == this.Settings.MaximumPendingKeysPerSession)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.TooManyPendingSessionKeys)));
                    }
                    this.futureSessionTokens.Add(newToken);
                }
                SecurityTraceRecordHelper.TraceNewServerSessionKeyIssued(newToken, supportingToken, GetLocalUri());
            }

            protected Uri GetLocalUri()
            {
                if (this.channelBinder.LocalAddress == null)
                    return null;
                else
                    return this.channelBinder.LocalAddress.Uri;
            }

            void OnInnerFaulted(IReliableChannelBinder sender, Exception exception)
            {
                this.Fault(exception);
            }

            SecurityContextSecurityToken GetSessionToken(SecurityMessageProperty securityProperty)
            {
                SecurityContextSecurityToken sct = (securityProperty.ProtectionToken != null) ? securityProperty.ProtectionToken.SecurityToken as SecurityContextSecurityToken : null;
                if (sct != null && sct.ContextId == this.sessionId)
                {
                    return sct;
                }
                if (securityProperty.HasIncomingSupportingTokens)
                {
                    for (int i = 0; i < securityProperty.IncomingSupportingTokens.Count; ++i)
                    {
                        if (securityProperty.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing)
                        {
                            sct = (securityProperty.IncomingSupportingTokens[i].SecurityToken as SecurityContextSecurityToken);
                            if (sct != null && sct.ContextId == this.sessionId)
                            {
                                return sct;
                            }
                        }
                    }
                }
                return null;
            }

            bool CheckIncomingToken(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                SecurityMessageProperty securityProperty = message.Properties.Security;
                // this is guaranteed to be non-null and matches the session ID since the binding checked it
                SecurityContextSecurityToken incomingToken = GetSessionToken(securityProperty);
                if (incomingToken == null)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.NoSessionTokenPresentInMessage)), message);
                }
                // the incoming token's key should have been issued within keyRenewalPeriod time in the past
                // if not, send back a renewal fault. However if this is a session close message then its ok to not require the client 
                // to renew the key in order to send the close.
                if (incomingToken.KeyExpirationTime < DateTime.UtcNow &&
                    message.Headers.Action != this.settings.SecurityStandardsManager.SecureConversationDriver.CloseAction.Value)
                {
                    if (this.settings.CanRenewSession)
                    {
                        SendRenewFault(requestContext, correlationState, timeout);
                        return false;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SessionKeyExpiredException(SR.GetString(SR.SecurityContextKeyExpired, incomingToken.ContextId, incomingToken.KeyGeneration)));
                    }
                }
                // this is a valid token. If it corresponds to a newly issued session token, make it the current
                // session token.
                lock (ThisLock)
                {
                    if (this.futureSessionTokens.Count > 0 && incomingToken.KeyGeneration != this.currentSessionToken.KeyGeneration)
                    {
                        bool changedCurrentSessionToken = false;
                        for (int i = 0; i < this.futureSessionTokens.Count; ++i)
                        {
                            if (futureSessionTokens[i].KeyGeneration == incomingToken.KeyGeneration)
                            {
                                // let the current token expire after KeyRollover time interval
                                DateTime keyRolloverTime = TimeoutHelper.Add(DateTime.UtcNow, this.settings.KeyRolloverInterval);
                                this.settings.SessionTokenCache.UpdateContextCachingTime(this.currentSessionToken, keyRolloverTime);
                                this.currentSessionToken = futureSessionTokens[i];
                                futureSessionTokens.RemoveAt(i);
                                ((IAcceptorSecuritySessionProtocol)this.securityProtocol).SetOutgoingSessionToken(this.currentSessionToken);
                                changedCurrentSessionToken = true;
                                break;
                            }
                        }
                        if (changedCurrentSessionToken)
                        {
                            SecurityTraceRecordHelper.TraceServerSessionKeyUpdated(this.currentSessionToken, GetLocalUri());
                            // remove all renewed tokens that will never be used.
                            for (int i = 0; i < futureSessionTokens.Count; ++i)
                            {
                                this.Settings.SessionTokenCache.RemoveContext(futureSessionTokens[i].ContextId, futureSessionTokens[i].KeyGeneration);
                            }
                            this.futureSessionTokens.Clear();
                        }
                    }
                }

                return true;
            }

            public void StartReceiving(RequestContext initialRequestContext)
            {
                if (this.initialRequestContext != null)
                {
                    Fx.Assert("The initial request context was already specified.");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.AttemptToCreateMultipleRequestContext)));
                }
                this.initialRequestContext = initialRequestContext;
            }

            public RequestContext ReceiveRequest()
            {
                return this.ReceiveRequest(this.DefaultReceiveTimeout);
            }

            public RequestContext ReceiveRequest(TimeSpan timeout)
            {
                RequestContext requestContext;
                if (this.TryReceiveRequest(timeout, out requestContext))
                {
                    return requestContext;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
            }

            public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
            {
                return this.BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.BeginTryReceiveRequest(timeout, callback, state);
            }

            public RequestContext EndReceiveRequest(IAsyncResult result)
            {
                RequestContext requestContext;
                if (this.EndTryReceiveRequest(result, out requestContext))
                {
                    return requestContext;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
            }

            public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReceiveRequestAsyncResult(this, timeout, callback, state);
            }

            public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext requestContext)
            {
                return ReceiveRequestAsyncResult.EndAsRequestContext(result, out requestContext);
            }

            public bool TryReceiveRequest(TimeSpan timeout, out RequestContext requestContext)
            {
                ThrowIfFaulted();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                if (!this.receiveLock.TryEnter(timeoutHelper.RemainingTime()))
                {
                    requestContext = null;
                    return false;
                }
                try
                {
                    while (true)
                    {
                        if (isInputClosed || this.State == CommunicationState.Faulted)
                        {
                            break;
                        }

                        // schedule another Receive if the timeout has not been reached
                        if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            requestContext = null;
                            return false;
                        }

                        RequestContext innerRequestContext;
                        if (initialRequestContext != null)
                        {
                            innerRequestContext = initialRequestContext;
                            initialRequestContext = null;
                        }
                        else
                        {
                            if (!channelBinder.TryReceive(timeoutHelper.RemainingTime(), out innerRequestContext))
                            {
                                requestContext = null;
                                return false;
                            }
                        }
                        if (innerRequestContext == null)
                        {
                            // the channel could have been aborted or closed
                            break;
                        }
                        if (this.isInputClosed && innerRequestContext.RequestMessage != null)
                        {
                            Message message = innerRequestContext.RequestMessage;
                            try
                            {
                                ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                                throw TraceUtility.ThrowHelperWarning(error, message);
                            }
                            finally
                            {
                                message.Close();
                                innerRequestContext.Abort();
                            }
                        }
                        SecurityProtocolCorrelationState correlationState = null;
                        bool isSecurityProcessingFailure;
                        Message requestMessage = ProcessRequestContext(innerRequestContext, timeoutHelper.RemainingTime(), out correlationState, out isSecurityProcessingFailure);
                        if (requestMessage != null)
                        {
                            requestContext = new SecuritySessionRequestContext(innerRequestContext, requestMessage, correlationState, this);
                            return true;
                        }
                    }
                }
                finally
                {
                    this.receiveLock.Exit();
                }
                ThrowIfFaulted();
                requestContext = null;
                return true;
            }

            public Message Receive()
            {
                return this.Receive(this.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                Message message;
                if (this.TryReceive(timeout, out message))
                {
                    return message;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.BeginTryReceive(timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                Message message;
                if (this.EndTryReceive(result, out message))
                {
                    return message;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReceiveRequestAsyncResult(this, timeout, callback, state);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return ReceiveRequestAsyncResult.EndAsMessage(result, out message);
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                RequestContext requestContext;

                if (this.TryReceiveRequest(timeoutHelper.RemainingTime(), out requestContext))
                {
                    if (requestContext != null)
                    {
                        message = requestContext.RequestMessage;
                        try
                        {
                            requestContext.Close(timeoutHelper.RemainingTime());
                        }
                        catch (TimeoutException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, System.Diagnostics.TraceEventType.Information);
                        }
                    }
                    else
                    {
                        message = null;
                    }
                    return true;
                }
                else
                {
                    message = null;
                    return false;
                }
            }

            public override T GetProperty<T>()
            {
                if (typeof(T) == typeof(FaultConverter) && (this.channelBinder != null))
                {
                    return new SecurityChannelFaultConverter(this.channelBinder.Channel) as T;
                }

                T result = base.GetProperty<T>();
                if ((result == null) && (channelBinder != null) && (channelBinder.Channel != null))
                {
                    result = channelBinder.Channel.GetProperty<T>();
                }

                return result;
            }

            void SendFaultIfRequired(Exception e, Message unverifiedMessage, RequestContext requestContext, TimeSpan timeout)
            {
                try
                {
                    // return if the underlying channel does not implement IDuplexSession or IReply
                    if (!(this.channelBinder.Channel is IReplyChannel) && !(this.channelBinder.Channel is IDuplexSessionChannel))
                    {
                        return;
                    }

                    MessageFault fault = SecurityUtils.CreateSecurityMessageFault(e, this.securityProtocol.SecurityProtocolFactory.StandardsManager);
                    if (fault == null)
                    {
                        return;
                    }
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    try
                    {
                        using (Message faultMessage = Message.CreateMessage(unverifiedMessage.Version, fault, unverifiedMessage.Version.Addressing.DefaultFaultAction))
                        {
                            if (unverifiedMessage.Headers.MessageId != null)
                                faultMessage.InitializeReply(unverifiedMessage);
                            requestContext.Reply(faultMessage, timeoutHelper.RemainingTime());
                            requestContext.Close(timeoutHelper.RemainingTime());
                        }
                    }
                    catch (CommunicationException ex)
                    {
                        DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
                    }
                    catch (TimeoutException ex)
                    {
                        if (TD.CloseTimeoutIsEnabled())
                        {
                            TD.CloseTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
                    }
                }
                finally
                {
                    unverifiedMessage.Close();
                    requestContext.Abort();
                }
            }

            bool ShouldWrapException(Exception e)
            {
                return ((e is FormatException) || (e is XmlException));
            }

            Message ProcessRequestContext(RequestContext requestContext, TimeSpan timeout, out SecurityProtocolCorrelationState correlationState, out bool isSecurityProcessingFailure)
            {
                correlationState = null;
                isSecurityProcessingFailure = false;
                if (requestContext == null)
                {
                    return null;
                }

                Message result = null;
                Message message = requestContext.RequestMessage;
                bool cleanupContextState = true;
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    Message unverifiedMessage = message;
                    Exception securityException = null;
                    try
                    {
                        correlationState = VerifyIncomingMessage(ref message, timeoutHelper.RemainingTime());
                    }
                    catch (MessageSecurityException e)
                    {
                        isSecurityProcessingFailure = true;
                        securityException = e;
                    }
                    if (securityException != null)
                    {
                        // SendFaultIfRequired closes the unverified message and context
                        SendFaultIfRequired(securityException, unverifiedMessage, requestContext, timeoutHelper.RemainingTime());
                        cleanupContextState = false;
                        return null;
                    }
                    else if (CheckIncomingToken(requestContext, message, correlationState, timeoutHelper.RemainingTime()))
                    {
                        if (message.Headers.Action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction.Value)
                        {
                            SecurityTraceRecordHelper.TraceServerSessionCloseReceived(this.currentSessionToken, GetLocalUri());
                            this.isInputClosed = true;
                            // OnCloseMessageReceived is responsible for closing the message and requestContext if required.
                            this.OnCloseMessageReceived(requestContext, message, correlationState, timeoutHelper.RemainingTime());
                            correlationState = null;
                        }
                        else if (message.Headers.Action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction.Value)
                        {
                            SecurityTraceRecordHelper.TraceServerSessionCloseResponseReceived(this.currentSessionToken, GetLocalUri());
                            this.isInputClosed = true;
                            // OnCloseResponseMessageReceived is responsible for closing the message and requestContext if required.
                            this.OnCloseResponseMessageReceived(requestContext, message, correlationState, timeoutHelper.RemainingTime());
                            correlationState = null;
                        }
                        else
                        {
                            result = message;
                        }
                        cleanupContextState = false;
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if ((e is CommunicationException) || (e is TimeoutException) || (Fx.IsFatal(e)) || !ShouldWrapException(e))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.MessageSecurityVerificationFailed), e));
                }
                finally
                {
                    if (cleanupContextState)
                    {
                        if (requestContext.RequestMessage != null)
                        {
                            requestContext.RequestMessage.Close();
                        }
                        requestContext.Abort();
                    }
                }

                return result;
            }

            internal void CheckOutgoingToken()
            {
                lock (ThisLock)
                {
                    if (this.currentSessionToken.KeyExpirationTime < DateTime.UtcNow)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SessionKeyExpiredException(SR.GetString(SR.SecuritySessionKeyIsStale)));
                    }
                }
            }

            internal void SecureApplicationMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                ThrowIfFaulted();
                ThrowIfClosedOrNotOpen();
                CheckOutgoingToken();
                this.securityProtocol.SecureOutgoingMessage(ref message, timeout, correlationState);
            }

            internal SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout)
            {
                return this.securityProtocol.VerifyIncomingMessage(ref message, timeout, null);
            }

            void PrepareReply(Message request, Message reply)
            {
                if (request.Headers.ReplyTo != null)
                {
                    request.Headers.ReplyTo.ApplyTo(reply);
                }
                else if (request.Headers.From != null)
                {
                    request.Headers.From.ApplyTo(reply);
                }
                if (request.Headers.MessageId != null)
                {
                    reply.Headers.RelatesTo = request.Headers.MessageId;
                }
                TraceUtility.CopyActivity(request, reply);
                if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
                {
                    TraceUtility.AddActivityHeader(reply);
                }
            }

            protected void InitializeFaultCodesIfRequired()
            {
                if (!areFaultCodesInitialized)
                {
                    lock (ThisLock)
                    {
                        if (!areFaultCodesInitialized)
                        {
                            SecurityStandardsManager standardsManager = this.securityProtocol.SecurityProtocolFactory.StandardsManager;
                            SecureConversationDriver scDriver = standardsManager.SecureConversationDriver;
                            renewFaultCode = FaultCode.CreateSenderFaultCode(scDriver.RenewNeededFaultCode.Value, scDriver.Namespace.Value);
                            renewFaultReason = new FaultReason(SR.GetString(SR.SecurityRenewFaultReason), System.Globalization.CultureInfo.InvariantCulture);
                            sessionAbortedFaultCode = FaultCode.CreateSenderFaultCode(DotNetSecurityStrings.SecuritySessionAbortedFault, DotNetSecurityStrings.Namespace);
                            sessionAbortedFaultReason = new FaultReason(SR.GetString(SR.SecuritySessionAbortedFaultReason), System.Globalization.CultureInfo.InvariantCulture);
                            areFaultCodesInitialized = true;
                        }
                    }
                }
            }

            void SendRenewFault(RequestContext requestContext, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                Message message = requestContext.RequestMessage;
                try
                {
                    InitializeFaultCodesIfRequired();
                    MessageFault renewFault = MessageFault.CreateFault(renewFaultCode, renewFaultReason);
                    Message response;
                    if (message.Headers.MessageId != null)
                    {
                        response = Message.CreateMessage(message.Version, renewFault, DotNetSecurityStrings.SecuritySessionFaultAction);
                        response.InitializeReply(message);
                    }
                    else
                    {
                        response = Message.CreateMessage(message.Version, renewFault, DotNetSecurityStrings.SecuritySessionFaultAction);
                    }
                    try
                    {
                        PrepareReply(message, response);
                        TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                        this.securityProtocol.SecureOutgoingMessage(ref response, timeoutHelper.RemainingTime(), correlationState);
                        response.Properties.AllowOutputBatching = false;
                        SendMessage(requestContext, response, timeoutHelper.RemainingTime());
                    }
                    finally
                    {
                        response.Close();
                    }
                    SecurityTraceRecordHelper.TraceSessionRenewalFaultSent(this.currentSessionToken, GetLocalUri(), message);
                }
                catch (CommunicationException e)
                {
                    SecurityTraceRecordHelper.TraceRenewFaultSendFailure(this.currentSessionToken, GetLocalUri(), e);
                }
                catch (TimeoutException e)
                {
                    SecurityTraceRecordHelper.TraceRenewFaultSendFailure(this.currentSessionToken, GetLocalUri(), e);
                }
            }

            Message ProcessCloseRequest(Message request)
            {
                RequestSecurityToken rst;
                XmlDictionaryReader bodyReader = request.GetReaderAtBodyContents();
                using (bodyReader)
                {
                    rst = this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityToken(bodyReader);
                    request.ReadFromBodyContentsToEnd(bodyReader);
                }
                if (rst.RequestType != null && rst.RequestType != this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.InvalidRstRequestType, rst.RequestType)), request);
                }
                if (rst.CloseTarget == null)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.NoCloseTargetSpecified)), request);
                }
                SecurityContextKeyIdentifierClause sctSkiClause = rst.CloseTarget as SecurityContextKeyIdentifierClause;
                if (sctSkiClause == null || !SecuritySessionSecurityTokenAuthenticator.DoesSkiClauseMatchSigningToken(sctSkiClause, request))
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.BadCloseTarget, rst.CloseTarget)), request);
                }
                RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse(this.Settings.SecurityStandardsManager);
                rstr.Context = rst.Context;
                rstr.IsRequestedTokenClosed = true;
                rstr.MakeReadOnly();
                BodyWriter bodyWriter = rstr;
                if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    List<RequestSecurityTokenResponse> rstrList = new List<RequestSecurityTokenResponse>(1);
                    rstrList.Add(rstr);
                    RequestSecurityTokenResponseCollection rstrc = new RequestSecurityTokenResponseCollection(rstrList, this.Settings.SecurityStandardsManager);
                    bodyWriter = rstrc;
                }
                Message response = Message.CreateMessage(request.Version, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction, request.Version.Addressing), bodyWriter);
                PrepareReply(request, response);
                return response;
            }

            internal Message CreateCloseResponse(Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                using (message)
                {
                    Message response = this.ProcessCloseRequest(message);
                    this.securityProtocol.SecureOutgoingMessage(ref response, timeout, correlationState);
                    response.Properties.AllowOutputBatching = false;
                    return response;
                }
            }

            internal void TraceSessionClosedResponseSuccess()
            {
                SecurityTraceRecordHelper.TraceSessionClosedResponseSent(this.currentSessionToken, GetLocalUri());
            }

            internal void TraceSessionClosedResponseFailure(Exception e)
            {
                SecurityTraceRecordHelper.TraceSessionClosedResponseSendFailure(this.currentSessionToken, GetLocalUri(), e);
            }

            internal void TraceSessionClosedSuccess()
            {
                SecurityTraceRecordHelper.TraceSessionClosedSent(this.currentSessionToken, GetLocalUri());
            }

            internal void TraceSessionClosedFailure(Exception e)
            {
                SecurityTraceRecordHelper.TraceSessionCloseSendFailure(this.currentSessionToken, GetLocalUri(), e);
            }

            // SendCloseResponse closes the message and underlying context if the operation completes successfully
            protected void SendCloseResponse(RequestContext requestContext, Message closeResponse, TimeSpan timeout)
            {
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    using (closeResponse)
                    {
                        SendMessage(requestContext, closeResponse, timeoutHelper.RemainingTime());
                    }
                    TraceSessionClosedResponseSuccess();
                }
                catch (CommunicationException e)
                {
                    TraceSessionClosedResponseFailure(e);
                }
                catch (TimeoutException e)
                {
                    TraceSessionClosedResponseFailure(e);
                }
            }

            // BeginSendCloseResponse closes the message and underlying context if the operation completes successfully
            internal IAsyncResult BeginSendCloseResponse(RequestContext requestContext, Message closeResponse, TimeSpan timeout, AsyncCallback callback, object state)
            {
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    return this.BeginSendMessage(requestContext, closeResponse, timeoutHelper.RemainingTime(), callback, state);
                }
                catch (CommunicationException e)
                {
                    TraceSessionClosedResponseFailure(e);
                }
                catch (TimeoutException e)
                {
                    TraceSessionClosedResponseFailure(e);
                }
                return new CompletedAsyncResult(callback, state);
            }

            // EndSendCloseResponse closes the message and underlying context if the operation completes successfully
            internal void EndSendCloseResponse(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                    return;
                }
                try
                {
                    this.EndSendMessage(result);
                }
                catch (CommunicationException e)
                {
                    TraceSessionClosedResponseFailure(e);
                }
                catch (TimeoutException e)
                {
                    TraceSessionClosedResponseFailure(e);
                }
            }

            internal Message CreateCloseMessage(TimeSpan timeout)
            {
                RequestSecurityToken rst = new RequestSecurityToken(this.Settings.SecurityStandardsManager);
                rst.RequestType = this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose;
                rst.CloseTarget = this.Settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(this.currentSessionToken, SecurityTokenReferenceStyle.External);
                rst.MakeReadOnly();
                Message closeMessage = Message.CreateMessage(this.messageVersion, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction, this.messageVersion.Addressing), rst);
                RequestReplyCorrelator.PrepareRequest(closeMessage);
                if (this.LocalAddress != null)
                {
                    closeMessage.Headers.ReplyTo = this.LocalAddress;
                }
                else
                {
                    if (closeMessage.Version.Addressing == AddressingVersion.WSAddressing10)
                    {
                        closeMessage.Headers.ReplyTo = null;
                    }
                    else if (closeMessage.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        closeMessage.Headers.ReplyTo = EndpointAddress.AnonymousAddress;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, closeMessage.Version.Addressing)));
                    }
                }
                this.securityProtocol.SecureOutgoingMessage(ref closeMessage, timeout, null);
                closeMessage.Properties.AllowOutputBatching = false;
                return closeMessage;
            }

            protected void SendClose(TimeSpan timeout)
            {
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    using (Message closeMessage = CreateCloseMessage(timeoutHelper.RemainingTime()))
                    {
                        SendMessage(null, closeMessage, timeoutHelper.RemainingTime());
                    }
                    TraceSessionClosedSuccess();
                }
                catch (CommunicationException e)
                {
                    TraceSessionClosedFailure(e);
                }
                catch (TimeoutException e)
                {
                    TraceSessionClosedFailure(e);
                }
            }

            internal IAsyncResult BeginSendClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    Message closeMessage = CreateCloseMessage(timeoutHelper.RemainingTime());
                    return this.BeginSendMessage(null, closeMessage, timeoutHelper.RemainingTime(), callback, state);
                }
                catch (CommunicationException e)
                {
                    TraceSessionClosedFailure(e);
                }
                catch (TimeoutException e)
                {
                    TraceSessionClosedFailure(e);
                }
                return new CompletedAsyncResult(callback, state);
            }

            internal void EndSendClose(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                    return;
                }
                try
                {
                    this.EndSendMessage(result);
                }
                catch (CommunicationException e)
                {
                    TraceSessionClosedFailure(e);
                }
                catch (TimeoutException e)
                {
                    TraceSessionClosedFailure(e);
                }
            }

            protected void SendMessage(RequestContext requestContext, Message message, TimeSpan timeout)
            {
                if (this.channelBinder.CanSendAsynchronously)
                {
                    this.channelBinder.Send(message, timeout);
                }
                else if (requestContext != null)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    requestContext.Reply(message, timeoutHelper.RemainingTime());
                    requestContext.Close(timeoutHelper.RemainingTime());
                }
            }

            internal IAsyncResult BeginSendMessage(RequestContext requestContext, Message response, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendMessageAsyncResult(this, requestContext, response, timeout, callback, state);
            }

            internal void EndSendMessage(IAsyncResult result)
            {
                SendMessageAsyncResult.End(result);
            }

            class SendMessageAsyncResult : AsyncResult
            {
                static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SendCallback));
                ServerSecuritySessionChannel sessionChannel;
                TimeoutHelper timeoutHelper;
                RequestContext requestContext;
                Message message;

                public SendMessageAsyncResult(ServerSecuritySessionChannel sessionChannel, RequestContext requestContext, Message message, TimeSpan timeout, AsyncCallback callback,
                    object state)
                    : base(callback, state)
                {
                    this.sessionChannel = sessionChannel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.requestContext = requestContext;
                    this.message = message;

                    bool closeMessage = true;
                    try
                    {
                        IAsyncResult result = this.BeginSend(message);
                        if (!result.CompletedSynchronously)
                        {
                            closeMessage = false;
                            return;
                        }
                        this.EndSend(result);
                        closeMessage = false;
                    }
                    finally
                    {
                        if (closeMessage)
                        {
                            this.message.Close();
                        }
                    }
                    Complete(true);
                }

                IAsyncResult BeginSend(Message response)
                {
                    if (this.sessionChannel.channelBinder.CanSendAsynchronously)
                    {
                        return this.sessionChannel.channelBinder.BeginSend(response, timeoutHelper.RemainingTime(), sendCallback, this);
                    }
                    else if (requestContext != null)
                    {
                        return requestContext.BeginReply(response, sendCallback, this);
                    }
                    else
                    {
                        return new SendCompletedAsyncResult(sendCallback, this);
                    }
                }

                void EndSend(IAsyncResult result)
                {
                    try
                    {
                        if (result is SendCompletedAsyncResult)
                        {
                            SendCompletedAsyncResult.End(result);
                        }
                        else if (this.sessionChannel.channelBinder.CanSendAsynchronously)
                        {
                            this.sessionChannel.channelBinder.EndSend(result);
                        }
                        else
                        {
                            this.requestContext.EndReply(result);
                            this.requestContext.Close(timeoutHelper.RemainingTime());
                        }
                    }
                    finally
                    {
                        if (this.message != null)
                        {
                            this.message.Close();
                        }
                    }
                }

                static void SendCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    SendMessageAsyncResult self = (SendMessageAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    try
                    {
                        self.EndSend(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                    self.Complete(false, completionException);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SendMessageAsyncResult>(result);
                }

                class SendCompletedAsyncResult : CompletedAsyncResult
                {
                    public SendCompletedAsyncResult(AsyncCallback callback, object state)
                        : base(callback, state)
                    {
                    }

                    new public static void End(IAsyncResult result)
                    {
                        AsyncResult.End<SendCompletedAsyncResult>(result);
                    }
                }
            }

            class CloseCoreAsyncResult : AsyncResult
            {
                static AsyncCallback channelBinderCloseCallback = Fx.ThunkCallback(new AsyncCallback(ChannelBinderCloseCallback));
                static AsyncCallback settingsLifetimeManagerCloseCallback = Fx.ThunkCallback(new AsyncCallback(SettingsLifetimeManagerCloseCallback));

                ServerSecuritySessionChannel channel;
                TimeoutHelper timeoutHelper;

                public CloseCoreAsyncResult(ServerSecuritySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    bool completeSelf = false;
                    if (this.channel.channelBinder != null)
                    {
                        try
                        {
                            IAsyncResult result = this.channel.channelBinder.BeginClose(timeoutHelper.RemainingTime(), channelBinderCloseCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return;
                            }
                            this.channel.channelBinder.EndClose(result);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (this.channel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            else
                            {
                                completeSelf = true;
                            }
                        }
                    }
                    if (!completeSelf)
                    {
                        completeSelf = this.OnChannelBinderClosed();
                    }
                    if (completeSelf)
                    {
                        RemoveSessionTokenFromCache();
                        Complete(true);
                    }
                }

                void RemoveSessionTokenFromCache()
                {
                    this.channel.Settings.SessionTokenCache.RemoveAllContexts(this.channel.currentSessionToken.ContextId);
                }

                static void ChannelBinderCloseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseCoreAsyncResult self = (CloseCoreAsyncResult)(result.AsyncState);
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        try
                        {
                            self.channel.channelBinder.EndClose(result);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (self.channel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            completeSelf = true;
                        }
                        if (!completeSelf)
                        {
                            completeSelf = self.OnChannelBinderClosed();
                        }
                        if (completeSelf)
                        {
                            self.RemoveSessionTokenFromCache();
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        self.Complete(false, completionException);
                    }
                }

                bool OnChannelBinderClosed()
                {
                    try
                    {
                        if (this.channel.securityProtocol != null)
                        {
                            this.channel.securityProtocol.Close(false, timeoutHelper.RemainingTime());
                        }
                        bool closeLifetimeManager = false;
                        lock (this.channel.ThisLock)
                        {
                            if (this.channel.hasSecurityStateReference)
                            {
                                closeLifetimeManager = true;
                                this.channel.hasSecurityStateReference = false;
                            }
                        }
                        if (!closeLifetimeManager)
                        {
                            return true;
                        }
                        IAsyncResult result = this.channel.settingsLifetimeManager.BeginClose(timeoutHelper.RemainingTime(), settingsLifetimeManagerCloseCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.channel.settingsLifetimeManager.EndClose(result);
                        return true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        return true;
                    }
                }

                static void SettingsLifetimeManagerCloseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseCoreAsyncResult self = (CloseCoreAsyncResult)(result.AsyncState);
                    bool removeSessionToken = false;
                    Exception completionException = null;
                    try
                    {
                        self.channel.settingsLifetimeManager.EndClose(result);
                        removeSessionToken = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (self.channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        removeSessionToken = true;
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                    finally
                    {
                        if (removeSessionToken)
                        {
                            self.RemoveSessionTokenFromCache();
                        }
                    }
                    self.Complete(false, completionException);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseCoreAsyncResult>(result);
                }
            }

            protected class SoapSecurityInputSession : ISecureConversationSession, IInputSession
            {
                ServerSecuritySessionChannel channel;
                UniqueId securityContextTokenId;
                EndpointIdentity remoteIdentity;
                SecurityKeyIdentifierClause sessionTokenIdentifier;
                SecurityStandardsManager standardsManager;

                public SoapSecurityInputSession(SecurityContextSecurityToken sessionToken,
                    SecuritySessionServerSettings settings, ServerSecuritySessionChannel channel)
                {
                    this.channel = channel;
                    this.securityContextTokenId = sessionToken.ContextId;
                    Claim identityClaim = SecurityUtils.GetPrimaryIdentityClaim(sessionToken.AuthorizationPolicies);
                    if (identityClaim != null)
                    {
                        this.remoteIdentity = EndpointIdentity.CreateIdentity(identityClaim);
                    }
                    this.sessionTokenIdentifier = settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(sessionToken, SecurityTokenReferenceStyle.External);
                    this.standardsManager = settings.SessionProtocolFactory.StandardsManager;
                }

                public string Id
                {
                    get
                    {
                        return this.securityContextTokenId.ToString();
                    }
                }

                public EndpointIdentity RemoteIdentity
                {
                    get
                    {
                        return this.remoteIdentity;
                    }
                }

                public void WriteSessionTokenIdentifier(XmlDictionaryWriter writer)
                {
                    this.channel.ThrowIfDisposedOrNotOpen();
                    this.standardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, this.sessionTokenIdentifier);
                }

                public bool TryReadSessionTokenIdentifier(XmlReader reader)
                {
                    this.channel.ThrowIfDisposedOrNotOpen();
                    if (!this.standardsManager.SecurityTokenSerializer.CanReadKeyIdentifierClause(reader))
                    {
                        return false;
                    }
                    SecurityContextKeyIdentifierClause incomingTokenIdentifier = this.standardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(reader) as SecurityContextKeyIdentifierClause;
                    return incomingTokenIdentifier != null && incomingTokenIdentifier.Matches(this.securityContextTokenId, null);
                }
            }

            class ReceiveRequestAsyncResult : AsyncResult
            {
                static FastAsyncCallback onWait = new FastAsyncCallback(OnWait);
                static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
                ServerSecuritySessionChannel channel;
                RequestContext innerRequestContext;
                Message requestMessage;
                SecurityProtocolCorrelationState correlationState;
                bool expired;
                TimeoutHelper timeoutHelper;

                public ReceiveRequestAsyncResult(ServerSecuritySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    channel.ThrowIfFaulted();
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.channel = channel;
                    if (!channel.receiveLock.EnterAsync(this.timeoutHelper.RemainingTime(), onWait, this))
                    {
                        return;
                    }

                    bool completeSelf = false;
                    bool throwing = true;
                    try
                    {
                        completeSelf = WaitComplete();
                        throwing = false;
                    }
                    finally
                    {
                        if (throwing)
                        {
                            this.channel.receiveLock.Exit();
                        }
                    }

                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                static void OnWait(object state, Exception asyncException)
                {
                    ReceiveRequestAsyncResult self = (ReceiveRequestAsyncResult)state;
                    bool completeSelf = false;
                    Exception completionException = asyncException;
                    if (completionException != null)
                    {
                        completeSelf = true;
                    }
                    else
                    {
                        try
                        {
                            completeSelf = self.WaitComplete();
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            completeSelf = true;
                            completionException = e;
                        }
                    }

                    if (completeSelf)
                    {
                        self.Complete(false, completionException);
                    }
                }

                bool WaitComplete()
                {
                    if (channel.isInputClosed)
                    {
                        return true;
                    }
                    channel.ThrowIfFaulted();

                    ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity &&
                        channel.initialRequestContext != null ?
                        TraceUtility.ExtractActivity(channel.initialRequestContext.RequestMessage) : null;

                    using (ServiceModelActivity.BoundOperation(activity))
                    {
                        if (channel.initialRequestContext != null)
                        {
                            innerRequestContext = channel.initialRequestContext;
                            channel.initialRequestContext = null;
                            bool isSecurityProcessingFailure;
                            requestMessage = channel.ProcessRequestContext(innerRequestContext, timeoutHelper.RemainingTime(), out this.correlationState, out isSecurityProcessingFailure);
                            if (requestMessage != null || channel.isInputClosed)
                            {
                                this.expired = false;
                                return true;
                            }
                        }
                        if (this.timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            this.expired = true;
                            return true;
                        }
                        IAsyncResult result = channel.ChannelBinder.BeginTryReceive(this.timeoutHelper.RemainingTime(), onReceive, this);
                        if (!result.CompletedSynchronously)
                            return false;

                        return CompleteReceive(result);
                    }
                }

                bool CompleteReceive(IAsyncResult result)
                {
                    while (true)
                    {
                        this.expired = !channel.ChannelBinder.EndTryReceive(result, out this.innerRequestContext);
                        if (this.expired || innerRequestContext == null)
                            break;

                        bool isSecurityProcessingFailure;
                        requestMessage = channel.ProcessRequestContext(innerRequestContext, timeoutHelper.RemainingTime(), out this.correlationState, out isSecurityProcessingFailure);
                        if (requestMessage != null)
                        {
                            if (channel.isInputClosed)
                            {
                                ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(requestMessage);
                                try
                                {
                                    throw TraceUtility.ThrowHelperWarning(error, requestMessage);
                                }
                                finally
                                {
                                    requestMessage.Close();
                                    innerRequestContext.Abort();
                                }
                            }
                            break;
                        }

                        if (channel.isInputClosed || channel.State == CommunicationState.Faulted)
                            break;

                        // retry the receive unless the timeout is reached
                        if (this.timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            this.expired = true;
                            break;
                        }

                        result = channel.ChannelBinder.BeginTryReceive(this.timeoutHelper.RemainingTime(), onReceive, this);

                        if (!result.CompletedSynchronously)
                            return false;
                    }
                    this.channel.ThrowIfFaulted();
                    return true;
                }

                new void Complete(bool synchronous)
                {
                    try
                    {
                        this.channel.receiveLock.Exit();
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.AsyncCallbackThrewException, SR.GetString(SR.TraceCodeAsyncCallbackThrewException), e.ToString());
                        }

                    }

                    base.Complete(synchronous);
                }

                new void Complete(bool synchronous, Exception exception)
                {
                    try
                    {
                        this.channel.receiveLock.Exit();
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.AsyncCallbackThrewException, SR.GetString(SR.TraceCodeAsyncCallbackThrewException), e.ToString());
                        }

                    }

                    base.Complete(synchronous, exception);
                }

                static ReceiveRequestAsyncResult End(IAsyncResult result)
                {
                    return AsyncResult.End<ReceiveRequestAsyncResult>(result);
                }

                public static bool EndAsMessage(IAsyncResult result, out Message message)
                {
                    ReceiveRequestAsyncResult receiveResult = End(result);
                    message = receiveResult.requestMessage;
                    // if the message is not null, then dispose the inner request context
                    // if the message is null its either a close or protocol fault in which case the request context
                    // will be closed by the channel
                    if (message != null)
                    {
                        if (receiveResult.innerRequestContext != null)
                        {
                            try
                            {
                                receiveResult.innerRequestContext.Close(receiveResult.timeoutHelper.RemainingTime());
                            }
                            catch (TimeoutException e)
                            {
                                DiagnosticUtility.TraceHandledException(e, System.Diagnostics.TraceEventType.Information);
                            }
                        }
                    }
                    return !receiveResult.expired;
                }

                public static bool EndAsRequestContext(IAsyncResult result, out RequestContext requestContext)
                {
                    ReceiveRequestAsyncResult receiveResult = End(result);

                    if (receiveResult.requestMessage == null)
                    {
                        requestContext = null;
                    }
                    else
                    {
                        requestContext = new SecuritySessionRequestContext(receiveResult.innerRequestContext, receiveResult.requestMessage, receiveResult.correlationState, receiveResult.channel);
                    }

                    return !receiveResult.expired;
                }

                static void OnReceive(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;

                    ReceiveRequestAsyncResult self = (ReceiveRequestAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        completeSelf = self.CompleteReceive(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        self.Complete(false, completionException);
                    }
                }
            }
        }

        abstract class ServerSecuritySimplexSessionChannel : ServerSecuritySessionChannel
        {
            SoapSecurityInputSession session;
            bool receivedClose;
            bool canSendCloseResponse;
            bool sentCloseResponse;
            RequestContext closeRequestContext;
            Message closeResponse;
            InterruptibleWaitObject inputSessionClosedHandle = new InterruptibleWaitObject(false);

            public ServerSecuritySimplexSessionChannel(
                SecuritySessionServerSettings settings,
                IServerReliableChannelBinder channelBinder,
                SecurityContextSecurityToken sessionToken,
                object listenerSecurityState, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(settings, channelBinder, sessionToken, listenerSecurityState, settingsLifetimeManager)
            {
                this.session = new SoapSecurityInputSession(sessionToken, settings, this);
            }

            public IInputSession Session
            {
                get
                {
                    return this.session;
                }
            }

            void CleanupPendingCloseState()
            {
                lock (ThisLock)
                {
                    if (this.closeResponse != null)
                    {
                        this.closeResponse.Close();
                        this.closeResponse = null;
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                        this.closeRequestContext = null;
                    }
                }
            }

            protected override void AbortCore()
            {
                base.AbortCore();
                this.Settings.RemoveSessionChannel(this.session.Id);
                CleanupPendingCloseState();
            }

            protected override void CloseCore(TimeSpan timeout)
            {
                base.CloseCore(timeout);
                this.inputSessionClosedHandle.Abort(this);
                this.Settings.RemoveSessionChannel(this.session.Id);
            }

            protected override void EndCloseCore(IAsyncResult result)
            {
                base.EndCloseCore(result);
                this.inputSessionClosedHandle.Abort(this);
                this.Settings.RemoveSessionChannel(this.session.Id);
            }

            protected override void OnAbort()
            {
                AbortCore();
                this.inputSessionClosedHandle.Abort(this);
            }

            protected override void OnFaulted()
            {
                this.AbortCore();
                this.inputSessionClosedHandle.Fault(this);
                base.OnFaulted();
            }

            bool ShouldSendCloseResponseOnClose(out RequestContext pendingCloseRequestContext, out Message pendingCloseResponse)
            {
                bool sendCloseResponse = false;
                lock (ThisLock)
                {
                    this.canSendCloseResponse = true;
                    if (!this.sentCloseResponse && this.receivedClose && this.closeResponse != null)
                    {
                        this.sentCloseResponse = true;
                        sendCloseResponse = true;
                        pendingCloseRequestContext = this.closeRequestContext;
                        pendingCloseResponse = this.closeResponse;
                        this.closeResponse = null;
                        this.closeRequestContext = null;
                    }
                    else
                    {
                        canSendCloseResponse = false;
                        pendingCloseRequestContext = null;
                        pendingCloseResponse = null;
                    }
                }
                return sendCloseResponse;
            }

            bool SendCloseResponseOnCloseIfRequired(TimeSpan timeout)
            {
                bool aborted = false;
                RequestContext pendingCloseRequestContext;
                Message pendingCloseResponse;
                bool sendCloseResponse = ShouldSendCloseResponseOnClose(out pendingCloseRequestContext, out pendingCloseResponse);
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                bool cleanupCloseState = true;
                if (sendCloseResponse)
                {
                    try
                    {
                        this.SendCloseResponse(pendingCloseRequestContext, pendingCloseResponse, timeoutHelper.RemainingTime());
                        this.inputSessionClosedHandle.Set();
                        cleanupCloseState = false;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        aborted = true;
                    }
                    finally
                    {
                        if (cleanupCloseState)
                        {
                            if (pendingCloseResponse != null)
                            {
                                pendingCloseResponse.Close();
                            }
                            if (pendingCloseRequestContext != null)
                            {
                                pendingCloseRequestContext.Abort();
                            }
                        }
                    }
                }

                return aborted;
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                // send a close response if one was not sent yet
                bool wasAborted = SendCloseResponseOnCloseIfRequired(timeoutHelper.RemainingTime());
                if (wasAborted)
                {
                    return;
                }

                bool wasInputSessionClosed = this.WaitForInputSessionClose(timeoutHelper.RemainingTime(), out wasAborted);
                if (wasAborted)
                {
                    return;
                }
                if (!wasInputSessionClosed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, timeoutHelper.OriginalTimeout)));
                }
                else
                {
                    this.CloseCore(timeoutHelper.RemainingTime());
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                RequestContext pendingCloseRequestContext;
                Message pendingCloseResponse;
                bool sendCloseResponse = ShouldSendCloseResponseOnClose(out pendingCloseRequestContext, out pendingCloseResponse);
                return new CloseAsyncResult(this, sendCloseResponse, pendingCloseRequestContext, pendingCloseResponse, timeout, callback, state);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CloseAsyncResult.End(result);
            }

            bool WaitForInputSessionClose(TimeSpan timeout, out bool wasAborted)
            {
                Message message;
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                wasAborted = false;
                try
                {
                    if (this.TryReceive(timeoutHelper.RemainingTime(), out message))
                    {
                        if (message != null)
                        {
                            using (message)
                            {
                                ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                                throw TraceUtility.ThrowHelperWarning(error, message);
                            }
                        }
                        return this.inputSessionClosedHandle.Wait(timeoutHelper.RemainingTime(), false);
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (this.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                    wasAborted = true;
                }
                return false;
            }

            protected override void OnCloseResponseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                // we dont expect a close-response for non-duplex security session
                message.Close();
                requestContext.Abort();
                this.Fault(new ProtocolException(SR.GetString(SR.UnexpectedSecuritySessionCloseResponse)));
            }

            protected override void OnCloseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                if (this.State == CommunicationState.Created)
                {
                    Fx.Assert("ServerSecuritySimplexSessionChannel.OnCloseMessageReceived (this.State == Created)");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ServerReceivedCloseMessageStateIsCreated, this.GetType().ToString())));
                }

                if (SendCloseResponseOnCloseReceivedIfRequired(requestContext, message, correlationState, timeout))
                {
                    this.inputSessionClosedHandle.Set();
                }
            }

            bool SendCloseResponseOnCloseReceivedIfRequired(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                bool sendCloseResponse = false;
                ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(message) : null;
                bool cleanupContext = true;
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    Message localCloseResponse = null;
                    lock (ThisLock)
                    {
                        if (!this.receivedClose)
                        {
                            this.receivedClose = true;
                            localCloseResponse = CreateCloseResponse(message, correlationState, timeoutHelper.RemainingTime());
                            if (canSendCloseResponse)
                            {
                                this.sentCloseResponse = true;
                                sendCloseResponse = true;
                            }
                            else
                            {
                                // save the close requestContext to reply later
                                this.closeRequestContext = requestContext;
                                this.closeResponse = localCloseResponse;
                                cleanupContext = false;
                            }
                        }
                    }
                    if (sendCloseResponse)
                    {
                        this.SendCloseResponse(requestContext, localCloseResponse, timeoutHelper.RemainingTime());
                        cleanupContext = false;
                    }
                    else if (cleanupContext)
                    {
                        requestContext.Close(timeoutHelper.RemainingTime());
                        cleanupContext = false;
                    }
                    return sendCloseResponse;
                }
                finally
                {
                    message.Close();
                    if (cleanupContext)
                    {
                        requestContext.Abort();
                    }
                    if (DiagnosticUtility.ShouldUseActivity && (activity != null))
                    {
                        activity.Stop();
                    }
                }
            }

            class CloseAsyncResult : AsyncResult
            {
                static readonly AsyncCallback sendCloseResponseCallback = Fx.ThunkCallback(new AsyncCallback(SendCloseResponseCallback));
                static readonly AsyncCallback receiveCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveCallback));
                static readonly AsyncCallback waitCallback = Fx.ThunkCallback(new AsyncCallback(WaitForInputSessionCloseCallback));
                static readonly AsyncCallback closeCoreCallback = Fx.ThunkCallback(new AsyncCallback(CloseCoreCallback));

                ServerSecuritySimplexSessionChannel sessionChannel;
                TimeoutHelper timeoutHelper;
                RequestContext closeRequestContext;
                Message closeResponse;

                public CloseAsyncResult(ServerSecuritySimplexSessionChannel sessionChannel, bool sendCloseResponse, RequestContext closeRequestContext, Message closeResponse, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    this.closeRequestContext = closeRequestContext;
                    this.closeResponse = closeResponse;
                    bool wasChannelAborted = false;
                    bool completeSelf = this.OnSendCloseResponse(sendCloseResponse, out wasChannelAborted);
                    if (wasChannelAborted || completeSelf)
                    {
                        Complete(true);
                    }
                }

                bool OnSendCloseResponse(bool shouldSendCloseResponse, out bool wasChannelAborted)
                {
                    wasChannelAborted = false;
                    try
                    {
                        if (shouldSendCloseResponse)
                        {
                            bool cleanupCloseState = true;
                            try
                            {
                                IAsyncResult result = this.sessionChannel.BeginSendCloseResponse(closeRequestContext, closeResponse, timeoutHelper.RemainingTime(), sendCloseResponseCallback, this);
                                if (!result.CompletedSynchronously)
                                {
                                    cleanupCloseState = false;
                                    return false;
                                }
                                this.sessionChannel.EndSendCloseResponse(result);
                                this.sessionChannel.inputSessionClosedHandle.Set();
                            }
                            finally
                            {
                                if (cleanupCloseState)
                                {
                                    CleanupCloseState();
                                }
                            }
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        wasChannelAborted = true;
                    }
                    if (wasChannelAborted)
                    {
                        return true;
                    }
                    return this.OnReceiveNullMessage(out wasChannelAborted);
                }

                void CleanupCloseState()
                {
                    if (this.closeResponse != null)
                    {
                        this.closeResponse.Close();
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                    }
                }

                static void SendCloseResponseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult thisResult = (CloseAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        bool wasAborted = false;
                        try
                        {
                            thisResult.sessionChannel.EndSendCloseResponse(result);
                            thisResult.sessionChannel.inputSessionClosedHandle.Set();
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            wasAborted = true;
                            completeSelf = true;
                        }
                        finally
                        {
                            thisResult.CleanupCloseState();
                        }
                        if (!wasAborted)
                        {
                            completeSelf = thisResult.OnReceiveNullMessage(out wasAborted);
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisResult.Complete(false, completionException);
                    }
                }

                bool OnReceiveNullMessage(out bool wasChannelAborted)
                {
                    wasChannelAborted = false;
                    bool receivedMessage = false;
                    Message message = null;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.BeginTryReceive(this.timeoutHelper.RemainingTime(), receiveCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        receivedMessage = this.sessionChannel.EndTryReceive(result, out message);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        // another thread aborted the channel
                        wasChannelAborted = true;
                    }
                    if (wasChannelAborted)
                    {
                        return true;
                    }

                    if (receivedMessage)
                    {
                        return this.OnMessageReceived(message);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, timeoutHelper.OriginalTimeout)));
                    }
                }

                static void ReceiveCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult thisResult = (CloseAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        Message message = null;
                        bool wasAborted = false;
                        bool receivedMessage = false;
                        try
                        {
                            receivedMessage = thisResult.sessionChannel.EndTryReceive(result, out message);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            wasAborted = true;
                            completeSelf = true;
                        }
                        if (!wasAborted)
                        {
                            if (receivedMessage)
                            {
                                completeSelf = thisResult.OnMessageReceived(message);
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, thisResult.timeoutHelper.OriginalTimeout)));
                            }
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisResult.Complete(false, completionException);
                    }
                }

                bool OnMessageReceived(Message message)
                {
                    if (message != null)
                    {
                        using (message)
                        {
                            ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                            throw TraceUtility.ThrowHelperWarning(error, message);
                        }
                    }
                    bool inputSessionClosed = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.inputSessionClosedHandle.BeginWait(this.timeoutHelper.RemainingTime(), true, waitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.inputSessionClosedHandle.EndWait(result);
                        inputSessionClosed = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        // a parallel thread aborted the channel
                        return true;
                    }
                    catch (TimeoutException)
                    {
                        inputSessionClosed = false;
                    }

                    return this.OnWaitOver(inputSessionClosed);
                }

                static void WaitForInputSessionCloseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult thisResult = (CloseAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        bool inputSessionClosed = false;
                        bool wasChannelAborted = false;
                        try
                        {
                            thisResult.sessionChannel.inputSessionClosedHandle.EndWait(result);
                            inputSessionClosed = true;
                        }
                        catch (TimeoutException)
                        {
                            inputSessionClosed = false;
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            wasChannelAborted = true;
                            completeSelf = true;
                        }
                        if (!wasChannelAborted)
                        {
                            completeSelf = thisResult.OnWaitOver(inputSessionClosed);
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisResult.Complete(false, completionException);
                    }
                }

                bool OnWaitOver(bool closeCompleted)
                {
                    if (closeCompleted)
                    {
                        IAsyncResult result = this.sessionChannel.BeginCloseCore(this.timeoutHelper.RemainingTime(), closeCoreCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.EndCloseCore(result);
                        return true;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, this.timeoutHelper.OriginalTimeout)));
                    }
                }

                static void CloseCoreCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult self = (CloseAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    try
                    {
                        self.sessionChannel.EndCloseCore(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                    self.Complete(false, completionException);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseAsyncResult>(result);
                }
            }
        }

        class SecurityReplySessionChannel : ServerSecuritySimplexSessionChannel, IReplySessionChannel
        {
            public SecurityReplySessionChannel(
                SecuritySessionServerSettings settings,
                IServerReliableChannelBinder channelBinder,
                SecurityContextSecurityToken sessionToken,
                object listenerSecurityState, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(settings, channelBinder, sessionToken, listenerSecurityState, settingsLifetimeManager)
            {
            }

            protected override bool CanDoSecurityCorrelation
            {
                get
                {
                    return true;
                }
            }

            public bool WaitForRequest(TimeSpan timeout)
            {
                return this.ChannelBinder.WaitForRequest(timeout);
            }

            public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.ChannelBinder.BeginWaitForRequest(timeout, callback, state);
            }

            public bool EndWaitForRequest(IAsyncResult result)
            {
                return this.ChannelBinder.EndWaitForRequest(result);
            }
        }

        class SecuritySessionRequestContext : RequestContextBase
        {
            RequestContext requestContext;
            ServerSecuritySessionChannel channel;
            SecurityProtocolCorrelationState correlationState;

            public SecuritySessionRequestContext(RequestContext requestContext, Message requestMessage, SecurityProtocolCorrelationState correlationState, ServerSecuritySessionChannel channel)
                : base(requestMessage, channel.InternalCloseTimeout, channel.InternalSendTimeout)
            {
                this.requestContext = requestContext;
                this.correlationState = correlationState;
                this.channel = channel;
            }

            protected override void OnAbort()
            {
                this.requestContext.Abort();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.requestContext.Close(timeout);
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (message != null)
                {
                    this.channel.SecureApplicationMessage(ref message, timeoutHelper.RemainingTime(), correlationState);
                }
                this.requestContext.Reply(message, timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (message != null)
                {
                    this.channel.SecureApplicationMessage(ref message, timeoutHelper.RemainingTime(), correlationState);
                }
                return this.requestContext.BeginReply(message, timeoutHelper.RemainingTime(), callback, state);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                this.requestContext.EndReply(result);
            }
        }

        class ServerSecurityDuplexSessionChannel : ServerSecuritySessionChannel, IDuplexSessionChannel
        {
            SoapSecurityServerDuplexSession session;
            bool isInputClosed;
            bool isOutputClosed;
            bool sentClose;
            bool receivedClose;
            RequestContext closeRequestContext;
            Message closeResponseMessage;
            InterruptibleWaitObject outputSessionCloseHandle = new InterruptibleWaitObject(true);
            InterruptibleWaitObject inputSessionCloseHandle = new InterruptibleWaitObject(false);

            public ServerSecurityDuplexSessionChannel(
                SecuritySessionServerSettings settings,
                IServerReliableChannelBinder channelBinder,
                SecurityContextSecurityToken sessionToken,
                object listenerSecurityState, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(settings, channelBinder, sessionToken, listenerSecurityState, settingsLifetimeManager)
            {
                this.session = new SoapSecurityServerDuplexSession(sessionToken, settings, this);
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.ChannelBinder.RemoteAddress;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.RemoteAddress.Uri;
                }
            }

            public IDuplexSession Session
            {
                get
                {
                    return this.session;
                }
            }

            public void Send(Message message)
            {
                this.Send(message, this.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                CheckOutputOpen();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.SecureApplicationMessage(ref message, timeoutHelper.RemainingTime(), null);
                this.ChannelBinder.Send(message, timeoutHelper.RemainingTime());
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                CheckOutputOpen();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.SecureApplicationMessage(ref message, timeoutHelper.RemainingTime(), null);
                return this.ChannelBinder.BeginSend(message, timeoutHelper.RemainingTime(), callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                this.ChannelBinder.EndSend(result);
            }

            protected override void AbortCore()
            {
                base.AbortCore();
                this.Settings.RemoveSessionChannel(this.session.Id);
                CleanupPendingCloseState();
            }

            void CleanupPendingCloseState()
            {
                lock (ThisLock)
                {
                    if (this.closeResponseMessage != null)
                    {
                        this.closeResponseMessage.Close();
                        this.closeResponseMessage = null;
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                        this.closeRequestContext = null;
                    }
                }
            }

            protected override void OnAbort()
            {
                AbortCore();
                this.inputSessionCloseHandle.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
            }

            protected override void OnFaulted()
            {
                this.AbortCore();
                this.inputSessionCloseHandle.Fault(this);
                this.outputSessionCloseHandle.Fault(this);
                base.OnFaulted();
            }

            protected override void CloseCore(TimeSpan timeout)
            {
                base.CloseCore(timeout);
                this.inputSessionCloseHandle.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
                this.Settings.RemoveSessionChannel(this.session.Id);
            }

            protected override void EndCloseCore(IAsyncResult result)
            {
                base.EndCloseCore(result);
                this.inputSessionCloseHandle.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
                this.Settings.RemoveSessionChannel(this.session.Id);
            }

            protected void CheckOutputOpen()
            {
                ThrowIfClosedOrNotOpen();
                lock (ThisLock)
                {
                    if (this.isOutputClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(SR.GetString(SR.OutputNotExpected)));
                    }
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseAsyncResult(this, timeout, callback, state);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CloseAsyncResult.End(result);
            }

            internal bool WaitForOutputSessionClose(TimeSpan timeout, out bool wasAborted)
            {
                wasAborted = false;
                try
                {
                    return this.outputSessionCloseHandle.Wait(timeout, false);
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (this.State != CommunicationState.Closed) throw;
                    wasAborted = true;
                    return true;
                }
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                // step 1: close output session
                this.CloseOutputSession(timeoutHelper.RemainingTime());

                // if the channel was aborted while closing the output session, return
                if (this.State == CommunicationState.Closed)
                {
                    return;
                }

                // step 2: wait for input session to be closed
                bool wasAborted;
                bool didInputSessionClose = this.WaitForInputSessionClose(timeoutHelper.RemainingTime(), out wasAborted);
                if (wasAborted)
                {
                    return;
                }
                if (!didInputSessionClose)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, timeoutHelper.OriginalTimeout)));
                }

                // wait for any concurrent CloseOutputSessions to finish
                bool didOutputSessionClose = this.WaitForOutputSessionClose(timeoutHelper.RemainingTime(), out wasAborted);
                if (wasAborted)
                {
                    return;
                }

                if (!didOutputSessionClose)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseOutputSessionTimeout, timeoutHelper.OriginalTimeout)));
                }

                this.CloseCore(timeoutHelper.RemainingTime());
            }

            bool WaitForInputSessionClose(TimeSpan timeout, out bool wasAborted)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                Message message;
                wasAborted = false;
                try
                {
                    if (!this.TryReceive(timeoutHelper.RemainingTime(), out message))
                    {
                        return false;
                    }

                    if (message != null)
                    {
                        using (message)
                        {
                            ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                            throw TraceUtility.ThrowHelperWarning(error, message);
                        }
                    }

                    // wait for remote close
                    if (!this.inputSessionCloseHandle.Wait(timeoutHelper.RemainingTime(), false))
                    {
                        return false;
                    }
                    else
                    {
                        lock (ThisLock)
                        {
                            if (!(this.isInputClosed))
                            {
                                Fx.Assert("Shutdown request was not received.");
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ShutdownRequestWasNotReceived)));
                            }
                        }
                        return true;
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (this.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                    wasAborted = true;
                }
                return false;
            }

            protected override void OnCloseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                if (this.State == CommunicationState.Created)
                {
                    Fx.Assert("ServerSecurityDuplexSessionChannel.OnCloseMessageReceived (this.State == Created)");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ServerReceivedCloseMessageStateIsCreated, this.GetType().ToString())));
                }

                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                bool setInputSessionCloseHandle = false;
                bool cleanupContext = true;
                try
                {
                    lock (ThisLock)
                    {
                        this.receivedClose = true;
                        if (!this.isInputClosed)
                        {
                            this.isInputClosed = true;
                            setInputSessionCloseHandle = true;

                            if (!this.isOutputClosed)
                            {
                                this.closeRequestContext = requestContext;
                                // CreateCloseResponse closes the message passed in
                                this.closeResponseMessage = CreateCloseResponse(message, null, timeoutHelper.RemainingTime());
                                cleanupContext = false;
                            }
                        }
                    }

                    if (setInputSessionCloseHandle)
                    {
                        this.inputSessionCloseHandle.Set();
                    }
                    if (cleanupContext)
                    {
                        requestContext.Close(timeoutHelper.RemainingTime());
                        cleanupContext = false;
                    }
                }
                finally
                {
                    message.Close();
                    if (cleanupContext)
                    {
                        requestContext.Abort();
                    }
                }
            }

            protected override void OnCloseResponseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                bool cleanupContext = true;
                try
                {
                    bool isCloseResponseExpected = false;
                    bool setInputSessionCloseHandle = false;
                    lock (ThisLock)
                    {
                        isCloseResponseExpected = this.sentClose;
                        if (isCloseResponseExpected && !this.isInputClosed)
                        {
                            this.isInputClosed = true;
                            setInputSessionCloseHandle = true;
                        }
                    }
                    if (!isCloseResponseExpected)
                    {
                        this.Fault(new ProtocolException(SR.GetString(SR.UnexpectedSecuritySessionCloseResponse)));
                        return;
                    }
                    if (setInputSessionCloseHandle)
                    {
                        this.inputSessionCloseHandle.Set();
                    }

                    requestContext.Close(timeout);
                    cleanupContext = false;
                }
                finally
                {
                    message.Close();
                    if (cleanupContext)
                    {
                        requestContext.Abort();
                    }
                }
            }

            void DetermineCloseOutputSessionMessage(out bool sendClose, out bool sendCloseResponse, out Message pendingCloseResponseMessage, out RequestContext pendingCloseRequestContext)
            {
                sendClose = false;
                sendCloseResponse = false;
                pendingCloseResponseMessage = null;
                pendingCloseRequestContext = null;
                lock (ThisLock)
                {
                    if (!this.isOutputClosed)
                    {
                        this.isOutputClosed = true;
                        if (this.receivedClose)
                        {
                            if (this.closeResponseMessage != null)
                            {
                                pendingCloseResponseMessage = this.closeResponseMessage;
                                pendingCloseRequestContext = this.closeRequestContext;
                                this.closeResponseMessage = null;
                                this.closeRequestContext = null;
                                sendCloseResponse = true;
                            }
                        }
                        else
                        {
                            sendClose = true;
                            this.sentClose = true;
                        }
                        this.outputSessionCloseHandle.Reset();
                    }
                }
            }

            void CloseOutputSession(TimeSpan timeout)
            {
                bool sendClose = false;
                bool sendCloseResponse = false;
                Message pendingCloseResponseMessage;
                RequestContext pendingCloseRequestContext;
                try
                {
                    DetermineCloseOutputSessionMessage(out sendClose, out sendCloseResponse, out pendingCloseResponseMessage, out pendingCloseRequestContext);
                    if (sendCloseResponse)
                    {
                        bool cleanupCloseState = true;
                        try
                        {
                            this.SendCloseResponse(pendingCloseRequestContext, pendingCloseResponseMessage, timeout);
                            cleanupCloseState = false;
                        }
                        finally
                        {
                            if (cleanupCloseState)
                            {
                                pendingCloseResponseMessage.Close();
                                pendingCloseRequestContext.Abort();
                            }
                        }
                    }
                    else if (sendClose)
                    {
                        this.SendClose(timeout);
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (this.State != CommunicationState.Closed) throw;
                    // a parallel thread aborted the channel. ignore the exception
                }
                finally
                {
                    if (sendClose || sendCloseResponse)
                    {
                        this.outputSessionCloseHandle.Set();
                    }
                }
            }

            IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseOutputSessionAsyncResult(this, timeout, callback, state);
            }

            void EndCloseOutputSession(IAsyncResult result)
            {
                CloseOutputSessionAsyncResult.End(result);
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.ChannelBinder.WaitForRequest(timeout);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.ChannelBinder.BeginWaitForRequest(timeout, callback, state);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.ChannelBinder.EndWaitForRequest(result);
            }

            class CloseOutputSessionAsyncResult : AsyncResult
            {
                static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SendCallback));
                ServerSecurityDuplexSessionChannel sessionChannel;
                TimeoutHelper timeoutHelper;
                bool sendClose;
                bool sendCloseResponse;
                Message closeResponseMessage;
                RequestContext closeRequestContext;

                public CloseOutputSessionAsyncResult(ServerSecurityDuplexSessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.sessionChannel = sessionChannel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel.DetermineCloseOutputSessionMessage(out sendClose, out sendCloseResponse, out closeResponseMessage, out closeRequestContext);
                    if (!sendClose && !sendCloseResponse)
                    {
                        Complete(true);
                        return;
                    }
                    bool doCleanup = true;
                    try
                    {
                        IAsyncResult result = this.BeginSend(sendCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            doCleanup = false;
                            return;
                        }
                        this.EndSend(result);
                    }
                    finally
                    {
                        if (doCleanup)
                        {
                            Cleanup();
                        }
                    }
                    Complete(true);
                }

                static void SendCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseOutputSessionAsyncResult self = (CloseOutputSessionAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    try
                    {
                        self.EndSend(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                    self.Cleanup();
                    self.Complete(false, completionException);
                }

                IAsyncResult BeginSend(AsyncCallback callback, object state)
                {
                    if (this.sendClose)
                    {
                        return this.sessionChannel.BeginSendClose(timeoutHelper.RemainingTime(), callback, state);
                    }
                    else
                    {
                        return this.sessionChannel.BeginSendCloseResponse(this.closeRequestContext, this.closeResponseMessage, this.timeoutHelper.RemainingTime(), callback, state);
                    }
                }

                void EndSend(IAsyncResult result)
                {
                    if (this.sendClose)
                    {
                        this.sessionChannel.EndSendClose(result);
                    }
                    else
                    {
                        this.sessionChannel.EndSendCloseResponse(result);
                    }
                }

                void Cleanup()
                {
                    if (this.closeResponseMessage != null)
                    {
                        this.closeResponseMessage.Close();
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                    }
                    this.sessionChannel.outputSessionCloseHandle.Set();
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseOutputSessionAsyncResult>(result);
                }
            }

            class CloseAsyncResult : AsyncResult
            {
                static readonly AsyncCallback receiveCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveCallback));
                static readonly AsyncCallback inputSessionWaitCallback = Fx.ThunkCallback(new AsyncCallback(WaitForInputSessionCloseCallback));
                static readonly AsyncCallback closeOutputSessionCallback = Fx.ThunkCallback(new AsyncCallback(CloseOutputSessionCallback));
                static readonly AsyncCallback outputSessionWaitCallback = Fx.ThunkCallback(new AsyncCallback(WaitForOutputSessionCloseCallback));
                static readonly AsyncCallback closeCoreCallback = Fx.ThunkCallback(new AsyncCallback(CloseCoreCallback));
                ServerSecurityDuplexSessionChannel sessionChannel;
                TimeoutHelper timeoutHelper;

                public CloseAsyncResult(ServerSecurityDuplexSessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;

                    bool wasAborted = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.BeginCloseOutputSession(timeoutHelper.RemainingTime(), closeOutputSessionCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        this.sessionChannel.EndCloseOutputSession(result);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (sessionChannel.State != CommunicationState.Closed) throw;
                        // a parallel thread must have aborted the channel. No need to close 
                        wasAborted = true;
                    }
                    if (wasAborted || this.OnOutputSessionClosed())
                    {
                        Complete(true);
                    }
                }

                static void CloseOutputSessionCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult thisResult = (CloseAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        bool wasAborted = false;
                        try
                        {
                            thisResult.sessionChannel.Session.EndCloseOutputSession(result);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            completeSelf = true;
                            wasAborted = true;
                        }
                        if (!wasAborted)
                        {
                            completeSelf = thisResult.OnOutputSessionClosed();
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisResult.Complete(false, completionException);
                    }
                }

                bool OnOutputSessionClosed()
                {
                    bool wasAborted = false;
                    Message message = null;
                    bool receivedMessage = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.BeginTryReceive(this.timeoutHelper.RemainingTime(), receiveCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        receivedMessage = this.sessionChannel.EndTryReceive(result, out message);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        wasAborted = true;
                    }
                    if (wasAborted)
                    {
                        return true;
                    }

                    if (receivedMessage)
                    {
                        return this.OnMessageReceived(message);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, this.timeoutHelper.OriginalTimeout)));
                    }
                }

                static void ReceiveCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult thisResult = (CloseAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        Message message = null;
                        bool receivedRequest = false;
                        bool wasAborted = false;
                        try
                        {
                            receivedRequest = thisResult.sessionChannel.EndTryReceive(result, out message);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            completeSelf = true;
                            wasAborted = true;
                        }
                        if (!wasAborted)
                        {
                            if (receivedRequest)
                            {
                                completeSelf = thisResult.OnMessageReceived(message);
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, thisResult.timeoutHelper.OriginalTimeout)));
                            }
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisResult.Complete(false, completionException);
                    }
                }

                bool OnMessageReceived(Message message)
                {
                    if (message != null)
                    {
                        using (message)
                        {
                            ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                            throw TraceUtility.ThrowHelperWarning(error, message);
                        }
                    }
                    bool wasAborted = false;
                    bool inputSessionClosed = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.inputSessionCloseHandle.BeginWait(this.timeoutHelper.RemainingTime(), inputSessionWaitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        try
                        {
                            this.sessionChannel.inputSessionCloseHandle.EndWait(result);
                            inputSessionClosed = true;
                        }
                        catch (TimeoutException)
                        {
                            inputSessionClosed = false;
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        wasAborted = true;
                    }
                    if (wasAborted)
                    {
                        return true;
                    }
                    return this.OnInputSessionWaitOver(inputSessionClosed);
                }

                static void WaitForInputSessionCloseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult thisResult = (CloseAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    bool inputSessionClosed = false;
                    try
                    {
                        bool wasAborted = false;
                        try
                        {
                            thisResult.sessionChannel.inputSessionCloseHandle.EndWait(result);
                            inputSessionClosed = true;
                        }
                        catch (TimeoutException)
                        {
                            inputSessionClosed = false;
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            wasAborted = true;
                            completeSelf = true;
                        }
                        if (!wasAborted)
                        {
                            completeSelf = thisResult.OnInputSessionWaitOver(inputSessionClosed);
                        }

                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisResult.Complete(false, completionException);
                    }
                }

                bool OnInputSessionWaitOver(bool inputSessionClosed)
                {
                    if (inputSessionClosed)
                    {
                        lock (this.sessionChannel.ThisLock)
                        {
                            if (!(this.sessionChannel.isInputClosed))
                            {
                                Fx.Assert("Shutdown request was not received.");
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ShutdownRequestWasNotReceived)));
                            }
                        }
                        bool outputSessionClosed = false;
                        bool wasAborted = false;
                        try
                        {
                            IAsyncResult result = this.sessionChannel.outputSessionCloseHandle.BeginWait(timeoutHelper.RemainingTime(), true, outputSessionWaitCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return false;
                            }
                            this.sessionChannel.outputSessionCloseHandle.EndWait(result);
                            outputSessionClosed = true;
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (this.sessionChannel.State != CommunicationState.Closed) throw;
                            wasAborted = true;
                        }
                        catch (TimeoutException)
                        {
                            outputSessionClosed = false;
                        }
                        if (wasAborted)
                        {
                            return true;
                        }
                        else
                        {
                            return this.OnOutputSessionWaitOver(outputSessionClosed);
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseTimeout, timeoutHelper.OriginalTimeout)));
                    }
                }

                static void WaitForOutputSessionCloseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult thisResult = (CloseAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    bool outputSessionClosed = false;
                    try
                    {
                        bool wasAborted = false;
                        try
                        {
                            thisResult.sessionChannel.outputSessionCloseHandle.EndWait(result);
                            outputSessionClosed = true;
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            wasAborted = true;
                            completeSelf = true;
                        }
                        catch (TimeoutException)
                        {
                            outputSessionClosed = false;
                        }
                        if (!wasAborted)
                        {
                            completeSelf = thisResult.OnOutputSessionWaitOver(outputSessionClosed);
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisResult.Complete(false, completionException);
                    }
                }

                bool OnOutputSessionWaitOver(bool outputSessionClosed)
                {
                    if (outputSessionClosed)
                    {
                        IAsyncResult result = this.sessionChannel.BeginCloseCore(this.timeoutHelper.RemainingTime(), closeCoreCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.EndCloseCore(result);
                        return true;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ServiceSecurityCloseOutputSessionTimeout, timeoutHelper.OriginalTimeout)));
                    }
                }

                static void CloseCoreCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult self = (CloseAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    try
                    {
                        self.sessionChannel.EndCloseCore(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                    self.Complete(false, completionException);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseAsyncResult>(result);
                }
            }

            class SoapSecurityServerDuplexSession : SoapSecurityInputSession, IDuplexSession
            {
                ServerSecurityDuplexSessionChannel channel;

                public SoapSecurityServerDuplexSession(SecurityContextSecurityToken sessionToken, SecuritySessionServerSettings settings, ServerSecurityDuplexSessionChannel channel)
                    : base(sessionToken, settings, channel)
                {
                    this.channel = channel;
                }

                public void CloseOutputSession()
                {
                    this.CloseOutputSession(this.channel.DefaultCloseTimeout);
                }

                public void CloseOutputSession(TimeSpan timeout)
                {
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception pendingException = null;
                    try
                    {
                        this.channel.CloseOutputSession(timeout);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        pendingException = e;
                    }
                    if (pendingException != null)
                    {
                        this.channel.Fault(pendingException);
                        if (pendingException is CommunicationException)
                        {
                            throw pendingException;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(pendingException);
                        }
                    }
                }

                public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
                {
                    return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
                }

                public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception pendingException = null;
                    try
                    {
                        return this.channel.BeginCloseOutputSession(timeout, callback, state);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        pendingException = e;
                    }
                    if (pendingException != null)
                    {
                        this.channel.Fault(pendingException);
                        if (pendingException is CommunicationException)
                        {
                            throw pendingException;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(pendingException);
                        }
                    }
                    return null;
                }

                public void EndCloseOutputSession(IAsyncResult result)
                {
                    Exception pendingException = null;
                    try
                    {
                        this.channel.EndCloseOutputSession(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        pendingException = e;
                    }
                    if (pendingException != null)
                    {
                        this.channel.Fault(pendingException);
                        if (pendingException is CommunicationException)
                        {
                            throw pendingException;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(pendingException);
                        }
                    }
                }
            }
        }

        internal class SecuritySessionDemuxFailureHandler : IChannelDemuxFailureHandler
        {
            SecurityStandardsManager standardsManager;

            public SecuritySessionDemuxFailureHandler(SecurityStandardsManager standardsManager)
            {
                if (standardsManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("standardsManager");
                }
                this.standardsManager = standardsManager;
            }

            public void HandleDemuxFailure(Message message)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionDemuxFailure, SR.GetString(SR.TraceCodeSecuritySessionDemuxFailure), message);
                }
            }

            public Message CreateSessionDemuxFaultMessage(Message message)
            {
                MessageFault fault = SecurityUtils.CreateSecurityContextNotFoundFault(this.standardsManager, message.Headers.Action);
                Message faultMessage = Message.CreateMessage(message.Version, fault, message.Version.Addressing.DefaultFaultAction);
                if (message.Headers.MessageId != null)
                {
                    faultMessage.InitializeReply(message);
                }
                return faultMessage;
            }

            IAsyncResult BeginHandleDemuxFailure<TFaultContext>(Message message, TFaultContext faultContext, AsyncCallback callback, object state)
            {
                this.HandleDemuxFailure(message);
                Message fault = CreateSessionDemuxFaultMessage(message);
                return new SendFaultAsyncResult<TFaultContext>(fault, faultContext, callback, state);
            }

            public IAsyncResult BeginHandleDemuxFailure(Message message, RequestContext faultContext, AsyncCallback callback, object state)
            {
                return BeginHandleDemuxFailure<RequestContext>(message, faultContext, callback, state);
            }

            public IAsyncResult BeginHandleDemuxFailure(Message message, IOutputChannel faultContext, AsyncCallback callback, object state)
            {
                return BeginHandleDemuxFailure<IOutputChannel>(message, faultContext, callback, state);
            }

            public void EndHandleDemuxFailure(IAsyncResult result)
            {
                if (result is SendFaultAsyncResult<RequestContext>)
                {
                    SendFaultAsyncResult<RequestContext>.End(result);
                }
                else if (result is SendFaultAsyncResult<IOutputChannel>)
                {
                    SendFaultAsyncResult<IOutputChannel>.End(result);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidAsyncResult), "result"));
                }
            }

            class SendFaultAsyncResult<TFaultContext> : AsyncResult
            {
                Message message;
                static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SendCallback));
                TFaultContext faultContext;

                public SendFaultAsyncResult(Message fault, TFaultContext faultContext, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.faultContext = faultContext;
                    this.message = fault;
                    IAsyncResult result = BeginSend(fault);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    EndSend(result);
                    Complete(true);
                }

                IAsyncResult BeginSend(Message message)
                {
                    bool throwing = true;
                    try
                    {
                        IAsyncResult result = null;
                        if (faultContext is RequestContext)
                        {
                            result = ((RequestContext)(object)faultContext).BeginReply(message, sendCallback, this);
                        }
                        else
                        {
                            result = ((IOutputChannel)faultContext).BeginSend(message, sendCallback, this);
                        }
                        throwing = false;
                        return result;
                    }
                    finally
                    {
                        if (throwing && message != null)
                        {
                            message.Close();
                        }
                    }
                }

                void EndSend(IAsyncResult result)
                {
                    using (this.message)
                    {
                        if (faultContext is RequestContext)
                        {
                            ((RequestContext)(object)faultContext).EndReply(result);
                        }
                        else
                        {
                            ((IOutputChannel)faultContext).EndSend(result);
                        }
                    }
                }

                static void SendCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    SendFaultAsyncResult<TFaultContext> self = (SendFaultAsyncResult<TFaultContext>)result.AsyncState;
                    Exception completionException = null;
                    try
                    {
                        self.EndSend(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                    }
                    self.Complete(false, completionException);
                }

                internal static void End(IAsyncResult result)
                {
                    AsyncResult.End<SendFaultAsyncResult<TFaultContext>>(result);
                }
            }
        }
    }
}
