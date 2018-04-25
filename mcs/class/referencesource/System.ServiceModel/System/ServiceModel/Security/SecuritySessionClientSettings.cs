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
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Net;
    using System.Threading;
    using System.Xml;
    using System.Globalization;
    using System.ServiceModel.Diagnostics.Application;

    // Please use 'sdv //depot/devdiv/private/indigo_xws/ndp/indigo/src/ServiceModel/System/ServiceModel/Security/SecuritySessionChannelFactory.cs' 
    // to see version history before the file was renamed
    // This class is named Settings since the only public APIs are for
    // settings; however, this class also manages all functionality
    // for session channels through internal APIs

    static class SecuritySessionClientSettings
    {
        internal const string defaultKeyRenewalIntervalString = "10:00:00";
        internal const string defaultKeyRolloverIntervalString = "00:05:00";

        internal static readonly TimeSpan defaultKeyRenewalInterval = TimeSpan.Parse(defaultKeyRenewalIntervalString, CultureInfo.InvariantCulture);
        internal static readonly TimeSpan defaultKeyRolloverInterval = TimeSpan.Parse(defaultKeyRolloverIntervalString, CultureInfo.InvariantCulture);
        internal const bool defaultTolerateTransportFailures = true;
    }

    sealed class SecuritySessionClientSettings<TChannel> : IChannelSecureConversationSessionSettings, ISecurityCommunicationObject
    {
        SecurityProtocolFactory sessionProtocolFactory;
        TimeSpan keyRenewalInterval;
        TimeSpan keyRolloverInterval;
        bool tolerateTransportFailures;
        SecurityChannelFactory<TChannel> securityChannelFactory;
        IChannelFactory innerChannelFactory;
        ChannelBuilder channelBuilder;
        WrapperSecurityCommunicationObject communicationObject;
        SecurityStandardsManager standardsManager;
        SecurityTokenParameters issuedTokenParameters;
        int issuedTokenRenewalThreshold;
        bool canRenewSession = true;
        object thisLock = new object();

        public SecuritySessionClientSettings()
        {
            this.keyRenewalInterval = SecuritySessionClientSettings.defaultKeyRenewalInterval;
            this.keyRolloverInterval = SecuritySessionClientSettings.defaultKeyRolloverInterval;
            this.tolerateTransportFailures = SecuritySessionClientSettings.defaultTolerateTransportFailures;
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        IChannelFactory InnerChannelFactory
        {
            get
            {
                return this.innerChannelFactory;
            }
        }

        internal ChannelBuilder ChannelBuilder
        {
            get
            {
                return this.channelBuilder;
            }
            set
            {
                this.channelBuilder = value;
            }
        }

        SecurityChannelFactory<TChannel> SecurityChannelFactory
        {
            get
            {
                return this.securityChannelFactory;
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

        public TimeSpan KeyRenewalInterval
        {
            get
            {
                return this.keyRenewalInterval;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.keyRenewalInterval = value;
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

        // ISecurityCommunicationObject members
        public TimeSpan DefaultOpenTimeout
        {
            get { return ServiceDefaults.OpenTimeout; }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get { return ServiceDefaults.CloseTimeout; }
        }

        internal IChannelFactory CreateInnerChannelFactory()
        {
            if (this.ChannelBuilder.CanBuildChannelFactory<IDuplexSessionChannel>())
            {
                return this.ChannelBuilder.BuildChannelFactory<IDuplexSessionChannel>();
            }
            else if (this.ChannelBuilder.CanBuildChannelFactory<IDuplexChannel>())
            {
                return this.ChannelBuilder.BuildChannelFactory<IDuplexChannel>();
            }
            else if (this.ChannelBuilder.CanBuildChannelFactory<IRequestChannel>())
            {
                return this.ChannelBuilder.BuildChannelFactory<IRequestChannel>();
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        IAsyncResult ISecurityCommunicationObject.OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        IAsyncResult ISecurityCommunicationObject.OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        public void OnClosed()
        {
        }

        public void OnClosing()
        {
        }

        void ISecurityCommunicationObject.OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        void ISecurityCommunicationObject.OnEndOpen(IAsyncResult result)
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

        public void OnClose(TimeSpan timeout)
        {
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(false, timeout);
            }
        }

        public void OnAbort()
        {
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(true, TimeSpan.Zero);
            }
        }

        public void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.sessionProtocolFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecuritySessionProtocolFactoryShouldBeSetBeforeThisOperation)));
            }
            if (this.standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityStandardsManagerNotSet, this.GetType().ToString())));
            }
            if (this.issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuedSecurityTokenParametersNotSet, this.GetType())));
            }
            if (this.keyRenewalInterval < this.keyRolloverInterval)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.KeyRolloverGreaterThanKeyRenewal)));
            }
            this.issuedTokenRenewalThreshold = this.sessionProtocolFactory.SecurityBindingElement.LocalClientSettings.CookieRenewalThresholdPercentage;
            this.ConfigureSessionProtocolFactory();
            this.sessionProtocolFactory.Open(true, timeoutHelper.RemainingTime());
        }

        internal void Close(TimeSpan timeout)
        {
            this.communicationObject.Close(timeout);
        }

        internal void Abort()
        {
            this.communicationObject.Abort();
        }

        internal void Open(SecurityChannelFactory<TChannel> securityChannelFactory,
            IChannelFactory innerChannelFactory, ChannelBuilder channelBuilder, TimeSpan timeout)
        {
            this.securityChannelFactory = securityChannelFactory;
            this.innerChannelFactory = innerChannelFactory;
            this.channelBuilder = channelBuilder;
            this.communicationObject.Open(timeout);
        }

        internal TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            return OnCreateChannel(remoteAddress, via, null);
        }

        internal TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via, MessageFilter filter)
        {
            this.communicationObject.ThrowIfClosed();
            if (filter != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (TChannel)((object)(new SecurityRequestSessionChannel(this, remoteAddress, via)));
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                // typeof(TChannel) == typeof(IDuplexSessionChannel)
                return (TChannel)((object)(new ClientSecurityDuplexSessionChannel(this, remoteAddress, via)));
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)), "TChannel"));
            }
        }

        void ConfigureSessionProtocolFactory()
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

                SessionSymmetricMessageSecurityProtocolFactory symmetric = (SessionSymmetricMessageSecurityProtocolFactory)this.sessionProtocolFactory;
                if (!symmetric.ApplyIntegrity || !symmetric.RequireIntegrity)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecuritySessionRequiresMessageIntegrity)));
                MessagePartSpecification bodyPart = new MessagePartSpecification(true);
                symmetric.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                symmetric.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                symmetric.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, addressing.FaultAction);
                symmetric.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, addressing.DefaultFaultAction);
                symmetric.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, DotNetSecurityStrings.SecuritySessionFaultAction);
                symmetric.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                symmetric.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                if (symmetric.ApplyConfidentiality)
                {
                    symmetric.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    symmetric.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                }
                if (symmetric.RequireConfidentiality)
                {
                    symmetric.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                    symmetric.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    symmetric.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, addressing.FaultAction);
                    symmetric.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, addressing.DefaultFaultAction);
                    symmetric.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, DotNetSecurityStrings.SecuritySessionFaultAction);
                }
            }
            else if (this.sessionProtocolFactory is SessionSymmetricTransportSecurityProtocolFactory)
            {
                SessionSymmetricTransportSecurityProtocolFactory transport = (SessionSymmetricTransportSecurityProtocolFactory)this.sessionProtocolFactory;
                transport.AddTimestamp = true;
                transport.SecurityTokenParameters.RequireDerivedKeys = false;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        abstract class ClientSecuritySessionChannel : ChannelBase
        {
            EndpointAddress to;
            Uri via;
            IClientReliableChannelBinder channelBinder;
            ChannelParameterCollection channelParameters;
            SecurityToken currentSessionToken;
            SecurityToken previousSessionToken;
            DateTime keyRenewalTime;
            DateTime keyRolloverTime;
            SecurityProtocol securityProtocol;
            SecuritySessionClientSettings<TChannel> settings;
            SecurityTokenProvider sessionTokenProvider;
            bool isKeyRenewalOngoing = false;
            InterruptibleWaitObject keyRenewalCompletedEvent;
            bool sentClose;
            bool receivedClose;
            volatile bool isOutputClosed;
            volatile bool isInputClosed;
            InterruptibleWaitObject inputSessionClosedHandle = new InterruptibleWaitObject(false);
            bool sendCloseHandshake = false;
            MessageVersion messageVersion;
            bool isCompositeDuplexConnection;
            Message closeResponse;
            InterruptibleWaitObject outputSessionCloseHandle = new InterruptibleWaitObject(true);
            WebHeaderCollection webHeaderCollection;

            protected ClientSecuritySessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via)
                : base(settings.SecurityChannelFactory)
            {
                this.settings = settings;
                this.to = to;
                this.via = via;
                this.keyRenewalCompletedEvent = new InterruptibleWaitObject(false);
                this.messageVersion = settings.SecurityChannelFactory.MessageVersion;
                this.channelParameters = new ChannelParameterCollection(this);
                this.InitializeChannelBinder();
                this.webHeaderCollection = new WebHeaderCollection();
            }

            protected SecuritySessionClientSettings<TChannel> Settings
            {
                get
                {
                    return this.settings;
                }
            }

            protected IClientReliableChannelBinder ChannelBinder
            {
                get
                {
                    return this.channelBinder;
                }
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.to;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.via;
                }
            }

            protected bool SendCloseHandshake
            {
                get
                {
                    return this.sendCloseHandshake;
                }
            }

            protected EndpointAddress InternalLocalAddress
            {
                get
                {
                    if (this.channelBinder != null)
                        return this.channelBinder.LocalAddress;
                    return null;
                }
            }

            protected virtual bool CanDoSecurityCorrelation
            {
                get
                {
                    return false;
                }
            }

            public MessageVersion MessageVersion
            {
                get { return this.messageVersion; }
            }

            protected bool IsInputClosed
            {
                get { return isInputClosed; }
            }

            protected bool IsOutputClosed
            {
                get { return isOutputClosed; }
            }

            protected abstract bool ExpectClose
            {
                get;
            }

            protected abstract string SessionId
            {
                get;
            }

            public override T GetProperty<T>()
            {
                if (typeof(T) == typeof(ChannelParameterCollection))
                {
                    return this.channelParameters as T;
                }

                if (typeof(T) == typeof(FaultConverter) && (this.channelBinder != null))
                {
                    return new SecurityChannelFaultConverter(this.channelBinder.Channel) as T;
                }
                else if (typeof(T) == typeof(WebHeaderCollection))
                {
                    return (T)(object)this.webHeaderCollection;
                }


                T result = base.GetProperty<T>();
                if ((result == null) && (channelBinder != null) && (channelBinder.Channel != null))
                {
                    result = channelBinder.Channel.GetProperty<T>();
                }

                return result;
            }

            protected abstract void InitializeSession(SecurityToken sessionToken);

            void InitializeSecurityState(SecurityToken sessionToken)
            {
                InitializeSession(sessionToken);
                this.currentSessionToken = sessionToken;
                this.previousSessionToken = null;
                List<SecurityToken> incomingSessionTokens = new List<SecurityToken>(1);
                incomingSessionTokens.Add(sessionToken);
                ((IInitiatorSecuritySessionProtocol)this.securityProtocol).SetIdentityCheckAuthenticator(new GenericXmlSecurityTokenAuthenticator());
                ((IInitiatorSecuritySessionProtocol)this.securityProtocol).SetIncomingSessionTokens(incomingSessionTokens);
                ((IInitiatorSecuritySessionProtocol)this.securityProtocol).SetOutgoingSessionToken(sessionToken);
                if (this.CanDoSecurityCorrelation)
                {
                    ((IInitiatorSecuritySessionProtocol)this.securityProtocol).ReturnCorrelationState = true;
                }
                this.keyRenewalTime = GetKeyRenewalTime(sessionToken);
            }

            void SetupSessionTokenProvider()
            {
                InitiatorServiceModelSecurityTokenRequirement requirement = new InitiatorServiceModelSecurityTokenRequirement();
                this.Settings.IssuedSecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
                requirement.KeyUsage = SecurityKeyUsage.Signature;
                requirement.SupportSecurityContextCancellation = true;
                requirement.SecurityAlgorithmSuite = this.Settings.SessionProtocolFactory.OutgoingAlgorithmSuite;
                requirement.SecurityBindingElement = this.Settings.SessionProtocolFactory.SecurityBindingElement;
                requirement.TargetAddress = this.to;
                requirement.Via = this.Via;
                requirement.MessageSecurityVersion = this.Settings.SessionProtocolFactory.MessageSecurityVersion.SecurityTokenVersion;
                requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty] = this.Settings.SessionProtocolFactory.PrivacyNoticeUri;
                requirement.WebHeaders = this.webHeaderCollection;

                if (this.channelParameters != null)
                {
                    requirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = this.channelParameters;
                }
                requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty] = this.Settings.SessionProtocolFactory.PrivacyNoticeVersion;
                if (this.channelBinder.LocalAddress != null)
                {
                    requirement.DuplexClientLocalAddress = this.channelBinder.LocalAddress;
                }
                this.sessionTokenProvider = this.Settings.SessionProtocolFactory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
            }

            void OpenCore(SecurityToken sessionToken, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.securityProtocol = this.Settings.SessionProtocolFactory.CreateSecurityProtocol(this.to, this.Via, null, true, timeoutHelper.RemainingTime());
                if (!(this.securityProtocol is IInitiatorSecuritySessionProtocol))
                {
                    Fx.Assert("Security protocol must be IInitiatorSecuritySessionProtocol.");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ProtocolMisMatch, "IInitiatorSecuritySessionProtocol", this.GetType().ToString())));
                }
                this.securityProtocol.Open(timeoutHelper.RemainingTime());
                this.channelBinder.Open(timeoutHelper.RemainingTime());
                this.InitializeSecurityState(sessionToken);
            }

            protected override void OnFaulted()
            {
                this.AbortCore();
                this.inputSessionClosedHandle.Fault(this);
                this.keyRenewalCompletedEvent.Fault(this);
                this.outputSessionCloseHandle.Fault(this);
                base.OnFaulted();
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                SetupSessionTokenProvider();
                SecurityUtils.OpenTokenProviderIfRequired(this.sessionTokenProvider, timeoutHelper.RemainingTime());
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ?
                    ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivitySecuritySetup), ActivityType.SecuritySetup);
                    }
                    SecurityToken sessionToken = this.sessionTokenProvider.GetToken(timeoutHelper.RemainingTime());
                    // Token was issued, do send cancel on close;
                    this.sendCloseHandshake = true;
                    this.OpenCore(sessionToken, timeoutHelper.RemainingTime());
                }
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ?
                    ServiceModelActivity.CreateAsyncActivity() : null;
                using (ServiceModelActivity.BoundOperation(activity, true))
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivitySecuritySetup), ActivityType.SecuritySetup);
                    }
                    return new OpenAsyncResult(this, timeout, callback, state);
                }
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                OpenAsyncResult.End(result);
            }

            void InitializeChannelBinder()
            {
                ChannelBuilder channelBuilder = this.Settings.ChannelBuilder;
                TolerateFaultsMode faultMode = this.Settings.TolerateTransportFailures ? TolerateFaultsMode.Always : TolerateFaultsMode.Never;
                if (channelBuilder.CanBuildChannelFactory<IDuplexSessionChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IDuplexSessionChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IDuplexSessionChannel>)(object)this.Settings.InnerChannelFactory,
                        MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, this.DefaultSendTimeout);
                }
                else if (channelBuilder.CanBuildChannelFactory<IDuplexChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IDuplexChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IDuplexChannel>)(object)this.Settings.InnerChannelFactory,
                        MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, this.DefaultSendTimeout);
                    this.isCompositeDuplexConnection = true;
                }
                else if (channelBuilder.CanBuildChannelFactory<IRequestChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IRequestChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IRequestChannel>)(object)this.Settings.InnerChannelFactory,
                        MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, this.DefaultSendTimeout);
                }
                else if (channelBuilder.CanBuildChannelFactory<IRequestSessionChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IRequestSessionChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IRequestSessionChannel>)(object)this.Settings.InnerChannelFactory,
                        MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, this.DefaultSendTimeout);
                }
                this.channelBinder.Faulted += this.OnInnerFaulted;
            }

            void OnInnerFaulted(IReliableChannelBinder sender, Exception exception)
            {
                this.Fault(exception);
            }

            protected virtual bool OnCloseResponseReceived()
            {
                bool setInputSessionClosedHandle = false;
                bool isCloseResponseExpected = false;
                lock (ThisLock)
                {
                    isCloseResponseExpected = this.sentClose;
                    if (isCloseResponseExpected && !this.isInputClosed)
                    {
                        this.isInputClosed = true;
                        setInputSessionClosedHandle = true;
                    }
                }
                if (!isCloseResponseExpected)
                {
                    this.Fault(new ProtocolException(SR.GetString(SR.UnexpectedSecuritySessionCloseResponse)));
                    return false;
                }
                if (setInputSessionClosedHandle)
                {
                    this.inputSessionClosedHandle.Set();
                }
                return true;
            }

            protected virtual bool OnCloseReceived()
            {
                if (!ExpectClose)
                {
                    this.Fault(new ProtocolException(SR.GetString(SR.UnexpectedSecuritySessionClose)));
                    return false;
                }
                bool setInputSessionClosedHandle = false;
                lock (ThisLock)
                {
                    if (!this.isInputClosed)
                    {
                        this.isInputClosed = true;
                        this.receivedClose = true;
                        setInputSessionClosedHandle = true;
                    }
                }
                if (setInputSessionClosedHandle)
                {
                    this.inputSessionClosedHandle.Set();
                }
                return true;
            }

            Message PrepareCloseMessage()
            {
                SecurityToken tokenToClose;
                lock (ThisLock)
                {
                    tokenToClose = this.currentSessionToken;
                }
                RequestSecurityToken rst = new RequestSecurityToken(this.Settings.SecurityStandardsManager);
                rst.RequestType = this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose;
                rst.CloseTarget = this.Settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(tokenToClose, SecurityTokenReferenceStyle.External);
                rst.MakeReadOnly();
                Message closeMessage = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction, this.MessageVersion.Addressing), rst);

                RequestReplyCorrelator.PrepareRequest(closeMessage);
                if (this.webHeaderCollection != null && this.webHeaderCollection.Count > 0)
                {
                    object prop = null;
                    HttpRequestMessageProperty rmp = null;
                    if (closeMessage.Properties.TryGetValue(HttpRequestMessageProperty.Name, out prop))
                    {
                        rmp = prop as HttpRequestMessageProperty;
                    }
                    else
                    {
                        rmp = new HttpRequestMessageProperty();
                        closeMessage.Properties.Add(HttpRequestMessageProperty.Name, rmp);
                    }

                    if (rmp != null && rmp.Headers != null)
                    {
                        rmp.Headers.Add(this.webHeaderCollection);
                    }
                }


                if (this.InternalLocalAddress != null)
                {
                    closeMessage.Headers.ReplyTo = this.InternalLocalAddress;
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
                if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
                {
                    TraceUtility.AddAmbientActivityToMessage(closeMessage);
                }
                return closeMessage;
            }

            protected SecurityProtocolCorrelationState SendCloseMessage(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                SecurityProtocolCorrelationState closeCorrelationState;
                Message closeMessage = PrepareCloseMessage();
                try
                {
                    closeCorrelationState = this.securityProtocol.SecureOutgoingMessage(ref closeMessage, timeoutHelper.RemainingTime(), null);
                    this.ChannelBinder.Send(closeMessage, timeoutHelper.RemainingTime());
                }
                finally
                {
                    closeMessage.Close();
                }

                SecurityTraceRecordHelper.TraceCloseMessageSent(this.currentSessionToken, this.RemoteAddress);
                return closeCorrelationState;
            }

            protected void SendCloseResponseMessage(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                Message message = null;
                try
                {
                    message = this.closeResponse;
                    this.securityProtocol.SecureOutgoingMessage(ref message, timeoutHelper.RemainingTime(), null);
                    this.ChannelBinder.Send(message, timeoutHelper.RemainingTime());
                    SecurityTraceRecordHelper.TraceCloseResponseMessageSent(this.currentSessionToken, this.RemoteAddress);
                }
                finally
                {
                    message.Close();
                }
            }

            IAsyncResult BeginSendCloseMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ?
                    ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivitySecurityClose), ActivityType.SecuritySetup);
                    }
                    Message closeMessage = PrepareCloseMessage();
                    return new SecureSendAsyncResult(closeMessage, this, timeout, callback, state, true);
                }
            }

            SecurityProtocolCorrelationState EndSendCloseMessage(IAsyncResult result)
            {
                SecurityProtocolCorrelationState correlationState = SecureSendAsyncResult.End(result);
                SecurityTraceRecordHelper.TraceCloseMessageSent(this.currentSessionToken, this.RemoteAddress);
                return correlationState;
            }

            IAsyncResult BeginSendCloseResponseMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SecureSendAsyncResult(this.closeResponse, this, timeout, callback, state, true);
            }

            void EndSendCloseResponseMessage(IAsyncResult result)
            {
                SecureSendAsyncResult.End(result);
                SecurityTraceRecordHelper.TraceCloseResponseMessageSent(this.currentSessionToken, this.RemoteAddress);
            }

            MessageFault GetProtocolFault(ref Message message, out bool isKeyRenewalFault, out bool isSessionAbortedFault)
            {
                isKeyRenewalFault = false;
                isSessionAbortedFault = false;
                MessageFault result = null;
                using (MessageBuffer buffer = message.CreateBufferedCopy(int.MaxValue))
                {
                    message = buffer.CreateMessage();
                    Message copy = buffer.CreateMessage();
                    MessageFault fault = MessageFault.CreateFault(copy, TransportDefaults.MaxSecurityFaultSize);
                    if (fault.Code.IsSenderFault)
                    {
                        FaultCode subCode = fault.Code.SubCode;
                        if (subCode != null)
                        {
                            SecurityStandardsManager standardsManager = this.securityProtocol.SecurityProtocolFactory.StandardsManager;
                            SecureConversationDriver scDriver = standardsManager.SecureConversationDriver;
                            if (subCode.Namespace == scDriver.Namespace.Value && subCode.Name == scDriver.RenewNeededFaultCode.Value)
                            {
                                result = fault;
                                isKeyRenewalFault = true;
                            }
                            else if (subCode.Namespace == DotNetSecurityStrings.Namespace && subCode.Name == DotNetSecurityStrings.SecuritySessionAbortedFault)
                            {
                                result = fault;
                                isSessionAbortedFault = true;
                            }
                        }
                    }
                }
                return result;
            }


            void ProcessKeyRenewalFault()
            {
                SecurityTraceRecordHelper.TraceSessionKeyRenewalFault(this.currentSessionToken, this.RemoteAddress);
                lock (ThisLock)
                {
                    this.keyRenewalTime = DateTime.UtcNow;
                }
            }

            void ProcessSessionAbortedFault(MessageFault sessionAbortedFault)
            {
                SecurityTraceRecordHelper.TraceRemoteSessionAbortedFault(this.currentSessionToken, this.RemoteAddress);
                this.Fault(new FaultException(sessionAbortedFault));
            }

            void ProcessCloseResponse(Message response)
            {
                // the close message may have been received by the channel after the channel factory has been closed
                if (response.Headers.Action != this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction.Value)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.InvalidCloseResponseAction, response.Headers.Action)), response);
                }
                RequestSecurityTokenResponse rstr = null;
                XmlDictionaryReader bodyReader = response.GetReaderAtBodyContents();
                using (bodyReader)
                {
                    if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                        rstr = this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityTokenResponse(bodyReader);
                    else if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                    {
                        RequestSecurityTokenResponseCollection rstrc = this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(bodyReader);
                        foreach (RequestSecurityTokenResponse rstrItem in rstrc.RstrCollection)
                        {
                            if (rstr != null)
                            {
                                // More than one RSTR is found. So throw an exception.
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MoreThanOneRSTRInRSTRC)));
                            }
                            rstr = rstrItem;
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }

                    response.ReadFromBodyContentsToEnd(bodyReader);
                }
                if (!rstr.IsRequestedTokenClosed)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SessionTokenWasNotClosed)), response);
                }
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

            bool DoesSkiClauseMatchSigningToken(SecurityContextKeyIdentifierClause skiClause, Message request)
            {
                if (this.SessionId == null)
                {
                    return false;
                }
                return (skiClause.ContextId.ToString() == this.SessionId);
            }

            void ProcessCloseMessage(Message message)
            {
                RequestSecurityToken rst;
                XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents();
                using (bodyReader)
                {
                    rst = this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityToken(bodyReader);
                    message.ReadFromBodyContentsToEnd(bodyReader);
                }
                if (rst.RequestType != null && rst.RequestType != this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.InvalidRstRequestType, rst.RequestType)), message);
                }
                if (rst.CloseTarget == null)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.NoCloseTargetSpecified)), message);
                }
                SecurityContextKeyIdentifierClause sctSkiClause = rst.CloseTarget as SecurityContextKeyIdentifierClause;
                if (sctSkiClause == null || !DoesSkiClauseMatchSigningToken(sctSkiClause, message))
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.BadCloseTarget, rst.CloseTarget)), message);
                }
                // prepare the close response
                RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse(this.Settings.SecurityStandardsManager);
                rstr.Context = rst.Context;
                rstr.IsRequestedTokenClosed = true;
                rstr.MakeReadOnly();
                Message response = null;
                if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                    response = Message.CreateMessage(message.Version, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction, message.Version.Addressing), rstr);
                else if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    List<RequestSecurityTokenResponse> rstrList = new List<RequestSecurityTokenResponse>();
                    rstrList.Add(rstr);
                    RequestSecurityTokenResponseCollection rstrCollection = new RequestSecurityTokenResponseCollection(rstrList, this.Settings.SecurityStandardsManager);
                    response = Message.CreateMessage(message.Version, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction, message.Version.Addressing), rstrCollection);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                PrepareReply(message, response);
                this.closeResponse = response;
            }

            bool ShouldWrapException(Exception e)
            {
                return ((e is FormatException) || (e is XmlException));
            }

            protected Message ProcessIncomingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, out MessageFault protocolFault)
            {
                protocolFault = null;
                lock (ThisLock)
                {
                    DoKeyRolloverIfNeeded();
                }

                try
                {
                    VerifyIncomingMessage(ref message, timeout, correlationState);

                    string action = message.Headers.Action;

                    if (action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction.Value)
                    {
                        SecurityTraceRecordHelper.TraceCloseResponseReceived(this.currentSessionToken, this.RemoteAddress);
                        this.ProcessCloseResponse(message);
                        this.OnCloseResponseReceived();
                    }
                    else if (action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction.Value)
                    {
                        SecurityTraceRecordHelper.TraceCloseMessageReceived(this.currentSessionToken, this.RemoteAddress);
                        this.ProcessCloseMessage(message);
                        this.OnCloseReceived();
                    }
                    else if (action == DotNetSecurityStrings.SecuritySessionFaultAction)
                    {
                        bool isKeyRenewalFault;
                        bool isSessionAbortedFault;
                        protocolFault = GetProtocolFault(ref message, out isKeyRenewalFault, out isSessionAbortedFault);
                        if (isKeyRenewalFault)
                        {
                            ProcessKeyRenewalFault();
                        }
                        else if (isSessionAbortedFault)
                        {
                            ProcessSessionAbortedFault(protocolFault);
                        }
                        else
                        {
                            return message;
                        }
                    }
                    else
                    {
                        return message;
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

                message.Close();
                return null;
            }

            protected Message ProcessRequestContext(RequestContext requestContext, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                if (requestContext == null)
                {
                    return null;
                }
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                Message message = requestContext.RequestMessage;
                Message unverifiedMessage = message;
                try
                {
                    Exception faultException = null;
                    try
                    {
                        MessageFault dummyProtocolFault;
                        return ProcessIncomingMessage(message, timeoutHelper.RemainingTime(), correlationState, out dummyProtocolFault);
                    }
                    catch (MessageSecurityException e)
                    {
                        // if the message is an unsecured security fault from the other party over the same connection then fault the session
                        if (!isCompositeDuplexConnection)
                        {
                            if (unverifiedMessage.IsFault)
                            {
                                MessageFault fault = MessageFault.CreateFault(unverifiedMessage, TransportDefaults.MaxSecurityFaultSize);
                                if (SecurityUtils.IsSecurityFault(fault, this.settings.sessionProtocolFactory.StandardsManager))
                                {
                                    faultException = SecurityUtils.CreateSecurityFaultException(fault);
                                }
                            }
                            else
                            {
                                faultException = e;
                            }
                        }
                    }
                    if (faultException != null)
                    {
                        this.Fault(faultException);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(faultException);
                    }
                    return null;
                }
                finally
                {
                    requestContext.Close(timeoutHelper.RemainingTime());
                }
            }

            /// <summary>
            /// This method removes the previous session key when the key rollover time is past.
            /// It must be called within a lock
            /// </summary>
            void DoKeyRolloverIfNeeded()
            {
                if (DateTime.UtcNow >= this.keyRolloverTime && this.previousSessionToken != null)
                {
                    SecurityTraceRecordHelper.TracePreviousSessionKeyDiscarded(this.previousSessionToken, this.currentSessionToken, this.RemoteAddress);
                    // forget the previous session token 
                    this.previousSessionToken = null;
                    List<SecurityToken> incomingTokens = new List<SecurityToken>(1);
                    incomingTokens.Add(this.currentSessionToken);
                    ((IInitiatorSecuritySessionProtocol)this.securityProtocol).SetIncomingSessionTokens(incomingTokens);
                }
            }

            DateTime GetKeyRenewalTime(SecurityToken token)
            {
                TimeSpan tokenValidityInterval = TimeSpan.FromTicks((long)(((token.ValidTo.Ticks - token.ValidFrom.Ticks) * this.settings.issuedTokenRenewalThreshold) / 100));
                DateTime keyRenewalTime1 = TimeoutHelper.Add(token.ValidFrom, tokenValidityInterval);
                DateTime keyRenewalTime2 = TimeoutHelper.Add(token.ValidFrom, this.settings.keyRenewalInterval);
                if (keyRenewalTime1 < keyRenewalTime2)
                {
                    return keyRenewalTime1;
                }
                else
                {
                    return keyRenewalTime2;
                }
            }

            /// <summary>
            /// This method returns true if key renewal is needed.
            /// It must be called within a lock
            /// </summary>
            bool IsKeyRenewalNeeded()
            {
                return DateTime.UtcNow >= this.keyRenewalTime;
            }

            /// <summary>
            /// When the new session token is obtained, mark the current token as previous and remove it
            /// after KeyRolloverTime. Mark the new current as pending and update the next key renewal time
            /// </summary>
            void UpdateSessionTokens(SecurityToken newToken)
            {
                lock (ThisLock)
                {
                    this.previousSessionToken = this.currentSessionToken;
                    this.keyRolloverTime = TimeoutHelper.Add(DateTime.UtcNow, this.Settings.KeyRolloverInterval);
                    this.currentSessionToken = newToken;
                    this.keyRenewalTime = GetKeyRenewalTime(newToken);
                    List<SecurityToken> incomingTokens = new List<SecurityToken>(2);
                    incomingTokens.Add(this.previousSessionToken);
                    incomingTokens.Add(this.currentSessionToken);
                    ((IInitiatorSecuritySessionProtocol)this.securityProtocol).SetIncomingSessionTokens(incomingTokens);
                    ((IInitiatorSecuritySessionProtocol)this.securityProtocol).SetOutgoingSessionToken(this.currentSessionToken);
                    SecurityTraceRecordHelper.TraceSessionKeyRenewed(this.currentSessionToken, this.previousSessionToken, this.RemoteAddress);
                }
            }

            void RenewKey(TimeSpan timeout)
            {
                if (!this.settings.CanRenewSession)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SessionKeyExpiredException(SR.GetString(SR.SessionKeyRenewalNotSupported)));
                }

                bool startKeyRenewal;
                lock (ThisLock)
                {
                    if (!this.isKeyRenewalOngoing)
                    {
                        this.isKeyRenewalOngoing = true;
                        this.keyRenewalCompletedEvent.Reset();
                        startKeyRenewal = true;
                    }
                    else
                    {
                        startKeyRenewal = false;
                    }
                }
                if (startKeyRenewal == true)
                {
                    try
                    {
                        using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ?
                            ServiceModelActivity.CreateBoundedActivity() : null)
                        {
                            if (DiagnosticUtility.ShouldUseActivity)
                            {
                                ServiceModelActivity.Start(activity, SR.GetString(SR.ActivitySecurityRenew), ActivityType.SecuritySetup);
                            }
                            SecurityToken renewedToken = this.sessionTokenProvider.RenewToken(timeout, this.currentSessionToken);
                            UpdateSessionTokens(renewedToken);
                        }
                    }
                    finally
                    {
                        lock (ThisLock)
                        {
                            this.isKeyRenewalOngoing = false;
                            this.keyRenewalCompletedEvent.Set();
                        }
                    }
                }
                else
                {
                    this.keyRenewalCompletedEvent.Wait(timeout);
                    lock (ThisLock)
                    {
                        if (IsKeyRenewalNeeded())
                        {
                            // the key renewal attempt failed. Throw an exception to the user
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SessionKeyExpiredException(SR.GetString(SR.UnableToRenewSessionKey)));
                        }
                    }
                }
            }

            bool CheckIfKeyRenewalNeeded()
            {
                bool doKeyRenewal = false;
                lock (ThisLock)
                {
                    doKeyRenewal = IsKeyRenewalNeeded();
                    DoKeyRolloverIfNeeded();
                }
                return doKeyRenewal;
            }

            protected IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                bool doKeyRenewal = CheckIfKeyRenewalNeeded();
                if (!doKeyRenewal)
                {
                    SecurityProtocolCorrelationState correlationState = this.securityProtocol.SecureOutgoingMessage(ref message, timeout, null);
                    return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, correlationState, callback, state);
                }
                else
                {
                    return new KeyRenewalAsyncResult(message, this, timeout, callback, state);
                }
            }

            protected Message EndSecureOutgoingMessage(IAsyncResult result, out SecurityProtocolCorrelationState correlationState)
            {
                if (result is CompletedAsyncResult<Message, SecurityProtocolCorrelationState>)
                {
                    return CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out correlationState);
                }
                else
                {
                    TimeSpan remainingTime;
                    Message message = KeyRenewalAsyncResult.End(result, out remainingTime);
                    correlationState = this.securityProtocol.SecureOutgoingMessage(ref message, remainingTime, null);
                    return message;
                }
            }

            protected SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout)
            {
                bool doKeyRenewal = CheckIfKeyRenewalNeeded();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (doKeyRenewal)
                {
                    RenewKey(timeoutHelper.RemainingTime());
                }
                return this.securityProtocol.SecureOutgoingMessage(ref message, timeoutHelper.RemainingTime(), null);
            }

            protected void VerifyIncomingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                this.securityProtocol.VerifyIncomingMessage(ref message, timeout, correlationState);
            }

            protected virtual void AbortCore()
            {
                if (this.channelBinder != null)
                {
                    this.channelBinder.Abort();
                }
                if (this.sessionTokenProvider != null)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(this.sessionTokenProvider);
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
                    if (this.sessionTokenProvider != null)
                    {
                        SecurityUtils.CloseTokenProviderIfRequired(this.sessionTokenProvider, timeoutHelper.RemainingTime());
                    }
                    this.keyRenewalCompletedEvent.Abort(this);
                    this.inputSessionClosedHandle.Abort(this);
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (this.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                }
            }

            protected virtual IAsyncResult BeginCloseCore(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseCoreAsyncResult(this, timeout, callback, state);
            }

            protected virtual void EndCloseCore(IAsyncResult result)
            {
                CloseCoreAsyncResult.End(result);
            }

            protected IAsyncResult BeginReceiveInternal(TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
            {
                return new ReceiveAsyncResult(this, timeout, correlationState, callback, state);
            }

            protected Message EndReceiveInternal(IAsyncResult result)
            {
                return ReceiveAsyncResult.End(result);
            }

            protected Message ReceiveInternal(TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                while (!this.isInputClosed)
                {
                    RequestContext requestContext;

                    if (this.ChannelBinder.TryReceive(timeoutHelper.RemainingTime(), out requestContext))
                    {
                        if (requestContext == null)
                        {
                            return null;
                        }

                        Message message = ProcessRequestContext(requestContext, timeoutHelper.RemainingTime(), correlationState);

                        if (message != null)
                        {
                            return message;
                        }
                    }

                    if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                    {
                        // we timed out
                        break;
                    }
                }

                return null;
            }

            protected bool CloseSession(TimeSpan timeout, out bool wasAborted)
            {
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ?
                    ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivitySecurityClose), ActivityType.SecuritySetup);
                    }

                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    wasAborted = false;
                    try
                    {
                        this.CloseOutputSession(timeoutHelper.RemainingTime());
                        return this.inputSessionClosedHandle.Wait(timeoutHelper.RemainingTime(), false);
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
            }

            protected IAsyncResult BeginCloseSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ?
                    ServiceModelActivity.CreateAsyncActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivitySecurityClose), ActivityType.SecuritySetup);
                    }
                    return new CloseSessionAsyncResult(timeout, this, callback, state);
                }
            }

            protected bool EndCloseSession(IAsyncResult result, out bool wasAborted)
            {
                return CloseSessionAsyncResult.End(result, out wasAborted);
            }

            void DetermineCloseMessageToSend(out bool sendClose, out bool sendCloseResponse)
            {
                sendClose = false;
                sendCloseResponse = false;
                lock (ThisLock)
                {
                    if (!this.isOutputClosed)
                    {
                        this.isOutputClosed = true;
                        if (this.receivedClose)
                        {
                            sendCloseResponse = true;
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

            protected virtual SecurityProtocolCorrelationState CloseOutputSession(TimeSpan timeout)
            {
                ThrowIfFaulted();
                if (!this.SendCloseHandshake)
                {
                    return null;
                }
                bool sendClose;
                bool sendCloseResponse;
                DetermineCloseMessageToSend(out sendClose, out sendCloseResponse);
                if (sendClose || sendCloseResponse)
                {
                    try
                    {
                        if (sendClose)
                        {
                            return this.SendCloseMessage(timeout);
                        }
                        else
                        {
                            this.SendCloseResponseMessage(timeout);
                            return null;
                        }
                    }
                    finally
                    {
                        this.outputSessionCloseHandle.Set();
                    }
                }
                else
                {
                    return null;
                }
            }

            protected virtual IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                ThrowIfFaulted();
                if (!this.SendCloseHandshake)
                {
                    return new CompletedAsyncResult(callback, state);
                }
                bool sendClose;
                bool sendCloseResponse;
                DetermineCloseMessageToSend(out sendClose, out sendCloseResponse);
                if (sendClose || sendCloseResponse)
                {
                    bool setOutputSessionCloseHandle = true;
                    try
                    {
                        IAsyncResult result;
                        if (sendClose)
                        {
                            result = this.BeginSendCloseMessage(timeout, callback, state);
                        }
                        else
                        {
                            result = this.BeginSendCloseResponseMessage(timeout, callback, state);
                        }
                        setOutputSessionCloseHandle = false;
                        return result;
                    }
                    finally
                    {
                        if (setOutputSessionCloseHandle)
                        {
                            this.outputSessionCloseHandle.Set();
                        }
                    }
                }
                else
                {
                    return new CompletedAsyncResult(callback, state);
                }
            }

            protected virtual SecurityProtocolCorrelationState EndCloseOutputSession(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                    return null;
                }
                bool sentCloseLocal;
                lock (ThisLock)
                {
                    sentCloseLocal = this.sentClose;
                }
                try
                {
                    if (sentCloseLocal)
                    {
                        return this.EndSendCloseMessage(result);
                    }
                    else
                    {
                        this.EndSendCloseResponseMessage(result);
                        return null;
                    }
                }
                finally
                {
                    this.outputSessionCloseHandle.Set();
                }
            }

            protected void CheckOutputOpen()
            {
                ThrowIfClosedOrNotOpen();
                lock (ThisLock)
                {
                    if (isOutputClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(SR.GetString(SR.OutputNotExpected)));
                    }
                }
            }

            protected override void OnAbort()
            {
                this.AbortCore();
                this.inputSessionClosedHandle.Abort(this);
                this.keyRenewalCompletedEvent.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (this.SendCloseHandshake)
                {
                    bool wasAborted;
                    bool wasSessionClosed = this.CloseSession(timeout, out wasAborted);
                    if (wasAborted)
                    {
                        return;
                    }
                    if (!wasSessionClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ClientSecurityCloseTimeout, timeout)));
                    }
                    // wait for any concurrent output session close to finish
                    try
                    {
                        if (!this.outputSessionCloseHandle.Wait(timeoutHelper.RemainingTime(), false))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ClientSecurityOutputSessionCloseTimeout, timeoutHelper.OriginalTimeout)));
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.State == CommunicationState.Closed)
                        {
                            return;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                this.CloseCore(timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseAsyncResult(this, timeout, callback, state);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CloseAsyncResult.End(result);
            }

            class CloseCoreAsyncResult : TraceAsyncResult
            {
                static AsyncCallback closeChannelBinderCallback = Fx.ThunkCallback(new AsyncCallback(ChannelBinderCloseCallback));
                static AsyncCallback closeTokenProviderCallback = Fx.ThunkCallback(new AsyncCallback(CloseTokenProviderCallback));

                TimeoutHelper timeoutHelper;
                ClientSecuritySessionChannel channel;

                public CloseCoreAsyncResult(ClientSecuritySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    bool completeSelf = false;
                    if (channel.channelBinder != null)
                    {
                        try
                        {
                            IAsyncResult result = this.channel.channelBinder.BeginClose(timeoutHelper.RemainingTime(), closeChannelBinderCallback, this);
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
                            completeSelf = true;
                        }
                    }
                    if (!completeSelf)
                    {
                        completeSelf = this.OnChannelBinderClosed();
                    }
                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                static void ChannelBinderCloseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseCoreAsyncResult self = (CloseCoreAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    bool completeSelf = false;
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
                    if (channel.sessionTokenProvider != null)
                    {
                        try
                        {
                            IAsyncResult result = SecurityUtils.BeginCloseTokenProviderIfRequired(this.channel.sessionTokenProvider, timeoutHelper.RemainingTime(), closeTokenProviderCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return false;
                            }
                            SecurityUtils.EndCloseTokenProviderIfRequired(result);
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
                    return this.OnTokenProviderClosed();
                }

                static void CloseTokenProviderCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseCoreAsyncResult self = (CloseCoreAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    bool completeSelf = false;
                    try
                    {
                        try
                        {
                            SecurityUtils.EndCloseTokenProviderIfRequired(result);
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
                            completeSelf = self.OnTokenProviderClosed();
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

                bool OnTokenProviderClosed()
                {
                    this.channel.keyRenewalCompletedEvent.Abort(this.channel);
                    this.channel.inputSessionClosedHandle.Abort(this.channel);
                    return true;
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseCoreAsyncResult>(result);
                }
            }

            class ReceiveAsyncResult : TraceAsyncResult
            {
                static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
                ClientSecuritySessionChannel channel;
                Message message;
                SecurityProtocolCorrelationState correlationState;
                TimeoutHelper timeoutHelper;

                public ReceiveAsyncResult(ClientSecuritySessionChannel channel, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.channel = channel;
                    this.correlationState = correlationState;
                    this.timeoutHelper = new TimeoutHelper(timeout);

                    IAsyncResult result = channel.ChannelBinder.BeginTryReceive(timeoutHelper.RemainingTime(), onReceive, this);

                    if (!result.CompletedSynchronously)
                        return;

                    bool completedSynchronously = CompleteReceive(result);
                    if (completedSynchronously)
                    {
                        Complete(true);
                    }
                }

                bool CompleteReceive(IAsyncResult result)
                {
                    while (!channel.isInputClosed)
                    {
                        RequestContext requestContext;
                        if (channel.ChannelBinder.EndTryReceive(result, out requestContext))
                        {
                            if (requestContext == null)
                                break;

                            message = channel.ProcessRequestContext(requestContext, timeoutHelper.RemainingTime(), this.correlationState);

                            if (message != null || channel.isInputClosed)
                                break;
                        }

                        TimeSpan timeout = timeoutHelper.RemainingTime();
                        if (timeout == TimeSpan.Zero)
                        {
                            // we timed out
                            break;
                        }

                        result = channel.ChannelBinder.BeginTryReceive(timeoutHelper.RemainingTime(), onReceive, this);

                        if (!result.CompletedSynchronously)
                            return false;
                    }

                    return true;
                }

                public static Message End(IAsyncResult result)
                {
                    ReceiveAsyncResult receiveResult = AsyncResult.End<ReceiveAsyncResult>(result);
                    return receiveResult.message;
                }

                static void OnReceive(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;
                    ReceiveAsyncResult self = (ReceiveAsyncResult)result.AsyncState;
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

            class OpenAsyncResult : TraceAsyncResult
            {
                static readonly AsyncCallback getTokenCallback = Fx.ThunkCallback(new AsyncCallback(GetTokenCallback));
                static readonly AsyncCallback openTokenProviderCallback = Fx.ThunkCallback(new AsyncCallback(OpenTokenProviderCallback));
                ClientSecuritySessionChannel sessionChannel;
                TimeoutHelper timeoutHelper;

                public OpenAsyncResult(ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    this.sessionChannel.SetupSessionTokenProvider();
                    IAsyncResult result = SecurityUtils.BeginOpenTokenProviderIfRequired(this.sessionChannel.sessionTokenProvider, timeoutHelper.RemainingTime(), openTokenProviderCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    SecurityUtils.EndOpenTokenProviderIfRequired(result);
                    bool completeSelf = this.OnTokenProviderOpened();
                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                static void OpenTokenProviderCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    OpenAsyncResult thisAsyncResult = (OpenAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        SecurityUtils.EndOpenTokenProviderIfRequired(result);
                        completeSelf = thisAsyncResult.OnTokenProviderOpened();
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
                        thisAsyncResult.Complete(false, completionException);
                    }
                }

                bool OnTokenProviderOpened()
                {
                    IAsyncResult result = this.sessionChannel.sessionTokenProvider.BeginGetToken(timeoutHelper.RemainingTime(), getTokenCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }

                    SecurityToken sessionToken = this.sessionChannel.sessionTokenProvider.EndGetToken(result);
                    return this.OnTokenObtained(sessionToken);
                }


                bool OnTokenObtained(SecurityToken sessionToken)
                {
                    // Token was issued, do send cancel on close;
                    this.sessionChannel.sendCloseHandshake = true;
                    this.sessionChannel.OpenCore(sessionToken, timeoutHelper.RemainingTime());
                    return true;
                }

                static void GetTokenCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    OpenAsyncResult thisAsyncResult = (OpenAsyncResult)result.AsyncState;
                    try
                    {
                        using (ServiceModelActivity.BoundOperation(thisAsyncResult.CallbackActivity))
                        {
                            bool completeSelf = false;
                            Exception completionException = null;
                            try
                            {
                                SecurityToken sessionToken = thisAsyncResult.sessionChannel.sessionTokenProvider.EndGetToken(result);
                                completeSelf = thisAsyncResult.OnTokenObtained(sessionToken);
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
                                thisAsyncResult.Complete(false, completionException);
                            }
                        }
                    }
                    finally
                    {
                        if (thisAsyncResult.CallbackActivity != null)
                        {
                            thisAsyncResult.CallbackActivity.Dispose();
                        }
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<OpenAsyncResult>(result);
                    ServiceModelActivity.Stop(((OpenAsyncResult)result).CallbackActivity);
                }
            }

            class CloseSessionAsyncResult : TraceAsyncResult
            {
                static readonly AsyncCallback closeOutputSessionCallback = Fx.ThunkCallback(new AsyncCallback(CloseOutputSessionCallback));
                static readonly AsyncCallback shutdownWaitCallback = Fx.ThunkCallback(new AsyncCallback(ShutdownWaitCallback));

                ClientSecuritySessionChannel sessionChannel;
                bool closeCompleted;
                bool wasAborted;
                TimeoutHelper timeoutHelper;

                public CloseSessionAsyncResult(TimeSpan timeout, ClientSecuritySessionChannel sessionChannel, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    bool completeSelf = false;
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
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        completeSelf = true;
                        wasAborted = true;
                    }
                    if (!wasAborted)
                    {
                        completeSelf = this.OnOutputSessionClosed();
                    }
                    if (completeSelf)
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
                    CloseSessionAsyncResult thisResult = (CloseSessionAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        try
                        {
                            thisResult.sessionChannel.EndCloseOutputSession(result);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (thisResult.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            thisResult.wasAborted = true;
                            completeSelf = true;
                        }
                        if (!thisResult.wasAborted)
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
                    try
                    {
                        IAsyncResult result = this.sessionChannel.inputSessionClosedHandle.BeginWait(this.timeoutHelper.RemainingTime(), true, shutdownWaitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.inputSessionClosedHandle.EndWait(result);
                        this.closeCompleted = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        // if the channel was aborted by a parallel thread, allow the Close to
                        // complete cleanly
                        this.wasAborted = true;
                    }
                    catch (TimeoutException)
                    {
                        this.closeCompleted = false;
                    }
                    return true;
                }

                static void ShutdownWaitCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseSessionAsyncResult thisResult = (CloseSessionAsyncResult)result.AsyncState;
                    Exception completionException = null;
                    try
                    {
                        thisResult.sessionChannel.inputSessionClosedHandle.EndWait(result);
                        thisResult.closeCompleted = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (thisResult.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        thisResult.wasAborted = true;
                    }
                    catch (TimeoutException)
                    {
                        thisResult.closeCompleted = false;
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
                    thisResult.Complete(false, completionException);
                }

                public static bool End(IAsyncResult result, out bool wasAborted)
                {
                    CloseSessionAsyncResult thisResult = AsyncResult.End<CloseSessionAsyncResult>(result);
                    wasAborted = thisResult.wasAborted;
                    ServiceModelActivity.Stop(thisResult.CallbackActivity);
                    return thisResult.closeCompleted;
                }
            }

            class CloseAsyncResult : TraceAsyncResult
            {
                static readonly AsyncCallback closeSessionCallback = Fx.ThunkCallback(new AsyncCallback(CloseSessionCallback));
                static readonly AsyncCallback outputSessionClosedCallback = Fx.ThunkCallback(new AsyncCallback(OutputSessionClosedCallback));
                static readonly AsyncCallback closeCoreCallback = Fx.ThunkCallback(new AsyncCallback(CloseCoreCallback));

                ClientSecuritySessionChannel sessionChannel;
                TimeoutHelper timeoutHelper;

                public CloseAsyncResult(ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    sessionChannel.ThrowIfFaulted();
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;

                    if (!sessionChannel.SendCloseHandshake)
                    {
                        if (this.CloseCore())
                        {
                            Complete(true);
                        }
                        return;
                    }

                    bool wasClosed;
                    bool wasAborted = false;
                    IAsyncResult result = this.sessionChannel.BeginCloseSession(this.timeoutHelper.RemainingTime(), closeSessionCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    wasClosed = this.sessionChannel.EndCloseSession(result, out wasAborted);
                    if (wasAborted)
                    {
                        Complete(true);
                        return;
                    }
                    if (!wasClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ClientSecurityCloseTimeout, timeout)));
                    }
                    bool completeSelf = this.OnWaitForOutputSessionClose(out wasAborted);
                    if (wasAborted || completeSelf)
                    {
                        Complete(true);
                    }
                }

                static void CloseSessionCallback(IAsyncResult result)
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
                        bool wasAborted;
                        bool wasClosed = thisResult.sessionChannel.EndCloseSession(result, out wasAborted);
                        if (wasAborted)
                        {
                            completeSelf = true;
                        }
                        else
                        {
                            if (!wasClosed)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ClientSecurityCloseTimeout, thisResult.timeoutHelper.OriginalTimeout)));
                            }
                            completeSelf = thisResult.OnWaitForOutputSessionClose(out wasAborted);
                            if (wasAborted)
                            {
                                completeSelf = true;
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

                bool OnWaitForOutputSessionClose(out bool wasAborted)
                {
                    wasAborted = false;
                    bool wasOutputSessionClosed = false;
                    // wait for pending output sessions to finish
                    try
                    {
                        IAsyncResult result = this.sessionChannel.outputSessionCloseHandle.BeginWait(timeoutHelper.RemainingTime(), true, outputSessionClosedCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.outputSessionCloseHandle.EndWait(result);
                        wasOutputSessionClosed = true;
                    }
                    catch (TimeoutException)
                    {
                        wasOutputSessionClosed = false;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State == CommunicationState.Closed)
                        {
                            wasAborted = true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    if (wasAborted)
                    {
                        return true;
                    }
                    if (!wasOutputSessionClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ClientSecurityOutputSessionCloseTimeout, timeoutHelper.OriginalTimeout)));
                    }
                    else
                    {
                        return this.CloseCore();
                    }
                }

                static void OutputSessionClosedCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseAsyncResult self = (CloseAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    bool completeSelf = false;
                    try
                    {
                        bool wasOutputSessionClosed = false;
                        bool wasAborted = false;
                        try
                        {
                            self.sessionChannel.outputSessionCloseHandle.EndWait(result);
                            wasOutputSessionClosed = true;
                        }
                        catch (TimeoutException)
                        {
                            wasOutputSessionClosed = false;
                        }
                        catch (CommunicationObjectFaultedException)
                        {
                            if (self.sessionChannel.State == CommunicationState.Closed)
                            {
                                wasAborted = true;
                            }
                            else
                            {
                                throw;
                            }
                        }
                        if (!wasAborted)
                        {
                            if (!wasOutputSessionClosed)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(SR.GetString(SR.ClientSecurityOutputSessionCloseTimeout, self.timeoutHelper.OriginalTimeout)));
                            }
                            completeSelf = self.CloseCore();
                        }
                        else
                        {
                            completeSelf = true;
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                        completeSelf = true;
                    }
                    if (completeSelf)
                    {
                        self.Complete(false, completionException);
                    }
                }

                bool CloseCore()
                {
                    IAsyncResult result = this.sessionChannel.BeginCloseCore(timeoutHelper.RemainingTime(), closeCoreCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.sessionChannel.EndCloseCore(result);
                    return true;
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

            class KeyRenewalAsyncResult : TraceAsyncResult
            {
                static readonly Action<object> renewKeyCallback = new Action<object>(RenewKeyCallback);
                Message message;
                ClientSecuritySessionChannel sessionChannel;
                TimeoutHelper timeoutHelper;

                public KeyRenewalAsyncResult(Message message, ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    timeoutHelper = new TimeoutHelper(timeout);
                    this.message = message;
                    this.sessionChannel = sessionChannel;
                    // its ok to start up a separate thread for this since this is a very rare event.
                    ActionItem.Schedule(renewKeyCallback, this);
                }

                static void RenewKeyCallback(object state)
                {
                    KeyRenewalAsyncResult thisResult = (KeyRenewalAsyncResult)state;
                    Exception completionException = null;
                    try
                    {
                        using (thisResult.CallbackActivity == null ? null : ServiceModelActivity.BoundOperation(thisResult.CallbackActivity))
                        {
                            thisResult.sessionChannel.RenewKey(thisResult.timeoutHelper.RemainingTime());
                        }
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
                    thisResult.Complete(false, completionException);
                }

                public static Message End(IAsyncResult result, out TimeSpan remainingTime)
                {
                    KeyRenewalAsyncResult thisResult = AsyncResult.End<KeyRenewalAsyncResult>(result);
                    remainingTime = thisResult.timeoutHelper.RemainingTime();
                    return thisResult.message;
                }
            }

            internal abstract class SecureSendAsyncResultBase : TraceAsyncResult
            {
                static readonly AsyncCallback secureOutgoingMessageCallback = Fx.ThunkCallback(new AsyncCallback(SecureOutgoingMessageCallback));
                Message message;
                SecurityProtocolCorrelationState correlationState;
                ClientSecuritySessionChannel sessionChannel;
                bool didSecureOutgoingMessageCompleteSynchronously = false;
                TimeoutHelper timeoutHelper;

                protected SecureSendAsyncResultBase(Message message, ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.message = message;
                    this.sessionChannel = sessionChannel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    IAsyncResult result = this.sessionChannel.BeginSecureOutgoingMessage(message, timeoutHelper.RemainingTime(), secureOutgoingMessageCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    this.message = this.sessionChannel.EndSecureOutgoingMessage(result, out this.correlationState);
                    this.didSecureOutgoingMessageCompleteSynchronously = true;
                }

                protected bool DidSecureOutgoingMessageCompleteSynchronously
                {
                    get
                    {
                        return this.didSecureOutgoingMessageCompleteSynchronously;
                    }
                }

                protected TimeoutHelper TimeoutHelper
                {
                    get
                    {
                        return this.timeoutHelper;
                    }
                }

                protected IClientReliableChannelBinder ChannelBinder
                {
                    get
                    {
                        return this.sessionChannel.ChannelBinder;
                    }
                }

                protected Message Message
                {
                    get
                    {
                        return this.message;
                    }
                }

                protected SecurityProtocolCorrelationState SecurityCorrelationState
                {
                    get
                    {
                        return this.correlationState;
                    }
                }

                protected abstract bool OnMessageSecured();

                static void SecureOutgoingMessageCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    SecureSendAsyncResultBase thisResult = (SecureSendAsyncResultBase)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        thisResult.message = thisResult.sessionChannel.EndSecureOutgoingMessage(result, out thisResult.correlationState);
                        completeSelf = thisResult.OnMessageSecured();
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
            }

            internal sealed class SecureSendAsyncResult : SecureSendAsyncResultBase
            {
                static readonly AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SendCallback));

                bool autoCloseMessage;

                public SecureSendAsyncResult(Message message, ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state, bool autoCloseMessage)
                    : base(message, sessionChannel, timeout, callback, state)
                {
                    this.autoCloseMessage = autoCloseMessage;

                    if (!this.DidSecureOutgoingMessageCompleteSynchronously)
                    {
                        return;
                    }
                    bool completeSelf = this.OnMessageSecured();
                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                protected override bool OnMessageSecured()
                {
                    bool closeMessage = true;
                    try
                    {
                        IAsyncResult result = this.ChannelBinder.BeginSend(this.Message, this.TimeoutHelper.RemainingTime(), sendCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            closeMessage = false;
                            return false;
                        }
                        this.ChannelBinder.EndSend(result);
                        return true;
                    }
                    finally
                    {
                        if (closeMessage && this.autoCloseMessage && this.Message != null)
                        {
                            this.Message.Close();
                        }
                    }
                }

                static void SendCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    SecureSendAsyncResult thisResult = (SecureSendAsyncResult)result.AsyncState;
                    Exception completionException = null;
                    try
                    {
                        thisResult.ChannelBinder.EndSend(result);
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
                        if (thisResult.autoCloseMessage && thisResult.Message != null)
                        {
                            thisResult.Message.Close();
                        }
                        if (thisResult.CallbackActivity != null && DiagnosticUtility.ShouldUseActivity)
                        {
                            thisResult.CallbackActivity.Stop();
                        }
                    }

                    thisResult.Complete(false, completionException);
                }

                public static SecurityProtocolCorrelationState End(IAsyncResult result)
                {
                    SecureSendAsyncResult thisResult = AsyncResult.End<SecureSendAsyncResult>(result);
                    return thisResult.SecurityCorrelationState;
                }
            }

            protected class SoapSecurityOutputSession : ISecureConversationSession, IOutputSession
            {
                ClientSecuritySessionChannel channel;
                EndpointIdentity remoteIdentity;
                UniqueId sessionId;
                SecurityKeyIdentifierClause sessionTokenIdentifier;
                SecurityStandardsManager standardsManager;

                public SoapSecurityOutputSession(ClientSecuritySessionChannel channel)
                {
                    this.channel = channel;
                }

                internal void Initialize(SecurityToken sessionToken, SecuritySessionClientSettings<TChannel> settings)
                {
                    if (sessionToken == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sessionToken");
                    }
                    if (settings == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
                    }
                    Claim identityClaim = SecurityUtils.GetPrimaryIdentityClaim(((GenericXmlSecurityToken)sessionToken).AuthorizationPolicies);
                    if (identityClaim != null)
                    {
                        this.remoteIdentity = EndpointIdentity.CreateIdentity(identityClaim);
                    }
                    this.standardsManager = settings.SessionProtocolFactory.StandardsManager;
                    this.sessionId = GetSessionId(sessionToken, this.standardsManager);
                    this.sessionTokenIdentifier = settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(sessionToken,
                        SecurityTokenReferenceStyle.External);
                }

                UniqueId GetSessionId(SecurityToken sessionToken, SecurityStandardsManager standardsManager)
                {
                    GenericXmlSecurityToken gxt = sessionToken as GenericXmlSecurityToken;
                    if (gxt == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SessionTokenIsNotGenericXmlToken, sessionToken, typeof(GenericXmlSecurityToken))));
                    }
                    return standardsManager.SecureConversationDriver.GetSecurityContextTokenId(XmlDictionaryReader.CreateDictionaryReader(new XmlNodeReader(gxt.TokenXml)));
                }

                public string Id
                {
                    get
                    {
                        if (this.sessionId == null)
                        {
                            // PreSharp Bug: Property get methods should not throw exceptions.
#pragma warning suppress 56503
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ChannelMustBeOpenedToGetSessionId)));
                        }
                        return this.sessionId.ToString();
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
                    SecurityContextKeyIdentifierClause incomingTokenIdentifier =
                        this.standardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(reader) as SecurityContextKeyIdentifierClause;
                    return incomingTokenIdentifier != null && incomingTokenIdentifier.Matches(sessionId, null);
                }
            }
        }

        abstract class ClientSecuritySimplexSessionChannel : ClientSecuritySessionChannel
        {
            SoapSecurityOutputSession outputSession;

            protected ClientSecuritySimplexSessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via)
                : base(settings, to, via)
            {
                this.outputSession = new SoapSecurityOutputSession(this);
            }

            public IOutputSession Session
            {
                get
                {
                    return this.outputSession;
                }
            }

            protected override bool ExpectClose
            {
                get { return false; }
            }

            protected override string SessionId
            {
                get { return this.Session.Id; }
            }

            protected override void InitializeSession(SecurityToken sessionToken)
            {
                this.outputSession.Initialize(sessionToken, this.Settings);
            }
        }

        sealed class SecurityRequestSessionChannel : ClientSecuritySimplexSessionChannel, IRequestSessionChannel
        {
            public SecurityRequestSessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via)
                : base(settings, to, via)
            {
            }

            protected override bool CanDoSecurityCorrelation
            {
                get
                {
                    return true;
                }
            }

            protected override SecurityProtocolCorrelationState CloseOutputSession(TimeSpan timeout)
            {
                ThrowIfFaulted();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                SecurityProtocolCorrelationState correlationState = base.CloseOutputSession(timeoutHelper.RemainingTime());

                Message message = ReceiveInternal(timeoutHelper.RemainingTime(), correlationState);

                if (message != null)
                {
                    using (message)
                    {
                        ProtocolException error = ProtocolException.ReceiveShutdownReturnedNonNull(message);
                        throw TraceUtility.ThrowHelperWarning(error, message);
                    }
                }
                return null;
            }

            protected override IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                ThrowIfFaulted();
                return new CloseOutputSessionAsyncResult(this, timeout, callback, state);
            }

            protected override SecurityProtocolCorrelationState EndCloseOutputSession(IAsyncResult result)
            {
                CloseOutputSessionAsyncResult.End(result);
                return null;
            }

            IAsyncResult BeginBaseCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.BeginCloseOutputSession(timeout, callback, state);
            }

            SecurityProtocolCorrelationState EndBaseCloseOutputSession(IAsyncResult result)
            {
                return base.EndCloseOutputSession(result);
            }

            public Message Request(Message message)
            {
                return this.Request(message, this.DefaultSendTimeout);
            }

            Message ProcessReply(Message reply, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                if (reply == null)
                {
                    return null;
                }
                Message unverifiedReply = reply;
                Message processedReply = null;
                MessageFault protocolFault = null;
                Exception faultException = null;
                try
                {
                    processedReply = this.ProcessIncomingMessage(reply, timeout, correlationState, out protocolFault);
                }
                catch (MessageSecurityException)
                {
                    if (unverifiedReply.IsFault)
                    {
                        MessageFault fault = MessageFault.CreateFault(unverifiedReply, TransportDefaults.MaxSecurityFaultSize);
                        if (SecurityUtils.IsSecurityFault(fault, this.Settings.standardsManager))
                        {
                            faultException = SecurityUtils.CreateSecurityFaultException(fault);
                        }
                    }
                    if (faultException == null)
                    {
                        throw;
                    }
                }
                if (faultException != null)
                {
                    Fault(faultException);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(faultException);
                }
                if (processedReply == null && protocolFault != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SecuritySessionFaultReplyWasSent), new FaultException(protocolFault)));
                }
                return processedReply;
            }


            public Message Request(Message message, TimeSpan timeout)
            {
                ThrowIfFaulted();
                CheckOutputOpen();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                SecurityProtocolCorrelationState correlationState = this.SecureOutgoingMessage(ref message, timeoutHelper.RemainingTime());
                Message reply = this.ChannelBinder.Request(message, timeoutHelper.RemainingTime());
                return ProcessReply(reply, timeoutHelper.RemainingTime(), correlationState);
            }

            public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
            {
                return this.BeginRequest(message, this.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                ThrowIfFaulted();
                CheckOutputOpen();
                return new SecureRequestAsyncResult(message, this, timeout, callback, state);
            }

            public Message EndRequest(IAsyncResult result)
            {
                SecurityProtocolCorrelationState requestCorrelationState;
                TimeSpan remainingTime;
                Message reply = SecureRequestAsyncResult.EndAsReply(result, out requestCorrelationState, out remainingTime);
                return ProcessReply(reply, remainingTime, requestCorrelationState);
            }

            sealed class SecureRequestAsyncResult : SecureSendAsyncResultBase
            {
                static readonly AsyncCallback requestCallback = Fx.ThunkCallback(new AsyncCallback(RequestCallback));
                Message reply;

                public SecureRequestAsyncResult(Message request, ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(request, sessionChannel, timeout, callback, state)
                {
                    if (!this.DidSecureOutgoingMessageCompleteSynchronously)
                    {
                        return;
                    }
                    bool completeSelf = OnMessageSecured();
                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                protected override bool OnMessageSecured()
                {
                    IAsyncResult result = this.ChannelBinder.BeginRequest(this.Message, this.TimeoutHelper.RemainingTime(), requestCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.reply = this.ChannelBinder.EndRequest(result);
                    return true;
                }

                static void RequestCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    SecureRequestAsyncResult thisAsyncResult = (SecureRequestAsyncResult)result.AsyncState;
                    Exception completionException = null;
                    try
                    {
                        thisAsyncResult.reply = thisAsyncResult.ChannelBinder.EndRequest(result);
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
                    thisAsyncResult.Complete(false, completionException);
                }

                public static Message EndAsReply(IAsyncResult result, out SecurityProtocolCorrelationState correlationState, out TimeSpan remainingTime)
                {
                    SecureRequestAsyncResult thisResult = AsyncResult.End<SecureRequestAsyncResult>(result);
                    correlationState = thisResult.SecurityCorrelationState;
                    remainingTime = thisResult.TimeoutHelper.RemainingTime();
                    return thisResult.reply;
                }
            }

            class CloseOutputSessionAsyncResult : TraceAsyncResult
            {
                static readonly AsyncCallback baseCloseOutputSessionCallback = Fx.ThunkCallback(new AsyncCallback(BaseCloseOutputSessionCallback));
                static readonly AsyncCallback receiveInternalCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveInternalCallback));
                SecurityRequestSessionChannel requestChannel;
                SecurityProtocolCorrelationState correlationState;
                TimeoutHelper timeoutHelper;

                public CloseOutputSessionAsyncResult(SecurityRequestSessionChannel requestChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.requestChannel = requestChannel;
                    IAsyncResult result = this.requestChannel.BeginBaseCloseOutputSession(timeoutHelper.RemainingTime(), baseCloseOutputSessionCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    this.correlationState = this.requestChannel.EndBaseCloseOutputSession(result);
                    bool completeSelf = this.OnBaseOutputSessionClosed();
                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                static void BaseCloseOutputSessionCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseOutputSessionAsyncResult thisAsyncResult = (CloseOutputSessionAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        thisAsyncResult.correlationState = thisAsyncResult.requestChannel.EndBaseCloseOutputSession(result);
                        completeSelf = thisAsyncResult.OnBaseOutputSessionClosed();
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
                        thisAsyncResult.Complete(false, completionException);
                    }
                }

                bool OnBaseOutputSessionClosed()
                {
                    IAsyncResult result = this.requestChannel.BeginReceiveInternal(this.timeoutHelper.RemainingTime(), this.correlationState, receiveInternalCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    Message message = this.requestChannel.EndReceiveInternal(result);
                    return this.OnMessageReceived(message);
                }

                static void ReceiveInternalCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseOutputSessionAsyncResult thisAsyncResult = (CloseOutputSessionAsyncResult)result.AsyncState;
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        Message message = thisAsyncResult.requestChannel.EndReceiveInternal(result);
                        completeSelf = thisAsyncResult.OnMessageReceived(message);
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
                        thisAsyncResult.Complete(false, completionException);
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
                    return true;
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseOutputSessionAsyncResult>(result);
                }
            }
        }

        class ClientSecurityDuplexSessionChannel : ClientSecuritySessionChannel, IDuplexSessionChannel
        {
            static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
            SoapSecurityClientDuplexSession session;
            InputQueue<Message> queue;
            Action startReceiving;
            Action<object> completeLater;

            public ClientSecurityDuplexSessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via)
                : base(settings, to, via)
            {
                this.session = new SoapSecurityClientDuplexSession(this);
                this.queue = TraceUtility.CreateInputQueue<Message>();
                this.startReceiving = new Action(StartReceiving);
                this.completeLater = new Action<object>(CompleteLater);
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return base.InternalLocalAddress;
                }
            }

            public IDuplexSession Session
            {
                get
                {
                    return this.session;
                }
            }

            protected override bool ExpectClose
            {
                get { return true; }
            }

            protected override string SessionId
            {
                get { return this.session.Id; }
            }

            public Message Receive()
            {
                return this.Receive(this.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return InputChannel.HelpReceive(this, timeout);
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return InputChannel.HelpBeginReceive(this, timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                return InputChannel.HelpEndReceive(result);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                ThrowIfFaulted();
                return queue.BeginDequeue(timeout, callback, state);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                bool wasDequeued = queue.EndDequeue(result, out message);
                if (message == null)
                {
                    // the channel could have faulted, shutting down the input queue
                    ThrowIfFaulted();
                }
                return wasDequeued;
            }

            protected override void OnOpened()
            {
                base.OnOpened();
                StartReceiving();
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                ThrowIfFaulted();
                bool wasDequeued = queue.Dequeue(timeout, out message);
                if (message == null)
                {
                    // the channel could have faulted, shutting down the input queue
                    ThrowIfFaulted();
                }
                return wasDequeued;
            }

            public void Send(Message message)
            {
                this.Send(message, this.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                ThrowIfFaulted();
                CheckOutputOpen();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.SecureOutgoingMessage(ref message, timeoutHelper.RemainingTime());
                this.ChannelBinder.Send(message, timeoutHelper.RemainingTime());
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                ThrowIfFaulted();
                CheckOutputOpen();
                return new SecureSendAsyncResult(message, this, timeout, callback, state, false);
            }

            public void EndSend(IAsyncResult result)
            {
                SecureSendAsyncResult.End(result);
            }

            protected override void InitializeSession(SecurityToken sessionToken)
            {
                this.session.Initialize(sessionToken, this.Settings);
            }

            void StartReceiving()
            {
                IAsyncResult result = this.IssueReceive();
                if (result != null && result.CompletedSynchronously)
                {
                    ActionItem.Schedule(completeLater, result);
                }
            }

            IAsyncResult IssueReceive()
            {
                while (true)
                {
                    // no need to receive anymore if in the closed state
                    if (this.State == CommunicationState.Closed || this.State == CommunicationState.Faulted || this.IsInputClosed)
                    {
                        return null;
                    }
                    try
                    {
                        return this.BeginReceiveInternal(TimeSpan.MaxValue, null, onReceive, this);

                    }
                    catch (CommunicationException e)
                    {
                        // BeginReceive failed. ignore the exception and start another receive
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        // BeginReceive failed. ignore the exception and start another receive
                        if (TD.ReceiveTimeoutIsEnabled())
                        {
                            TD.ReceiveTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                }
            }

            void CompleteLater(object obj)
            {
                CompleteReceive((IAsyncResult)obj);
            }

            static void OnReceive(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                ((ClientSecurityDuplexSessionChannel)result.AsyncState).CompleteReceive(result);
            }

            void CompleteReceive(IAsyncResult result)
            {
                Message message = null;
                bool issueAnotherReceive = false;
                try
                {
                    message = this.EndReceiveInternal(result);
                    issueAnotherReceive = true;
                }
                catch (MessageSecurityException)
                {
                    // a messagesecurityexception will only be thrown if the channel received a fault
                    // from the other side, in which case the channel would have faulted
                    issueAnotherReceive = false;
                }
                catch (CommunicationException e)
                {
                    issueAnotherReceive = true;
                    // EndReceive failed. ignore the exception and start another receive
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    issueAnotherReceive = true;
                    // EndReceive failed. ignore the exception and start another receive
                    if (TD.ReceiveTimeoutIsEnabled())
                    {
                        TD.ReceiveTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                IAsyncResult nextReceiveResult = null;
                if (issueAnotherReceive)
                {
                    nextReceiveResult = this.IssueReceive();
                    if (nextReceiveResult != null && nextReceiveResult.CompletedSynchronously)
                    {
                        ActionItem.Schedule(completeLater, nextReceiveResult);
                    }
                }
                if (message != null)
                {
                    // since we may be dispatching to user code ---- non-fatal exceptions
                    try
                    {
                        this.queue.EnqueueAndDispatch(message);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                    }
                }
            }

            protected override void AbortCore()
            {
                try
                {
                    this.queue.Dispose();
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
                base.AbortCore();
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.queue.WaitForItem(timeout);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.queue.BeginWaitForItem(timeout, callback, state);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.queue.EndWaitForItem(result);
            }

            protected override void OnFaulted()
            {
                queue.Shutdown(() => this.GetPendingException());
                base.OnFaulted();
            }

            protected override bool OnCloseResponseReceived()
            {
                if (base.OnCloseResponseReceived())
                {
                    this.queue.Shutdown();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            protected override bool OnCloseReceived()
            {
                if (base.OnCloseReceived())
                {
                    this.queue.Shutdown();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            class SoapSecurityClientDuplexSession : SoapSecurityOutputSession, IDuplexSession
            {
                ClientSecurityDuplexSessionChannel channel;
                bool initialized = false;

                public SoapSecurityClientDuplexSession(ClientSecurityDuplexSessionChannel channel)
                    : base(channel)
                {
                    this.channel = channel;
                }

                internal new void Initialize(SecurityToken sessionToken, SecuritySessionClientSettings<TChannel> settings)
                {
                    base.Initialize(sessionToken, settings);
                    this.initialized = true;
                }

                void CheckInitialized()
                {
                    if (!this.initialized)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ChannelNotOpen)));
                    }
                }

                public void CloseOutputSession()
                {
                    this.CloseOutputSession(this.channel.DefaultCloseTimeout);
                }

                public void CloseOutputSession(TimeSpan timeout)
                {
                    CheckInitialized();
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception pendingException = null;
                    try
                    {
                        this.channel.CloseOutputSession(timeout);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        pendingException = e;
                    }
                    if (pendingException != null)
                    {
                        this.channel.Fault(pendingException);
                        throw pendingException;
                    }
                }

                public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
                {
                    return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
                }

                public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    CheckInitialized();
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception pendingException = null;
                    try
                    {
                        return this.channel.BeginCloseOutputSession(timeout, callback, state);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        // another thread must have aborted the channel. Allow the close to complete
                        // gracefully
                        return new CompletedAsyncResult(callback, state);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
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
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(pendingException);
                        }
                    }
                    // we should never reach here
                    Fx.Assert("Unexpected control flow");
                    return null;
                }

                public void EndCloseOutputSession(IAsyncResult result)
                {
                    if (result is CompletedAsyncResult)
                    {
                        CompletedAsyncResult.End(result);
                        return;
                    }
                    Exception pendingException = null;
                    try
                    {
                        this.channel.EndCloseOutputSession(result);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        // another thread must have aborted the channel. Allow the close to complete
                        // gracefully
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
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
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(pendingException);
                        }
                    }
                }
            }
        }
    }
}
