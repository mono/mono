//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Threading;
    using System.ServiceModel.Dispatcher;

    sealed class SecurityChannelListener<TChannel> : DelegatingChannelListener<TChannel> where TChannel : class, IChannel
    {
        ChannelBuilder channelBuilder;
        SecurityProtocolFactory securityProtocolFactory;
        SecuritySessionServerSettings sessionServerSettings;
        bool sessionMode;
        // this will be disabled by negotiation when doing request/reply over composite duplex
        bool sendUnsecuredFaults = true;
        SecurityListenerSettingsLifetimeManager settingsLifetimeManager;
        bool hasSecurityStateReference;
        bool extendedProtectionPolicyHasSupport;
        ISecurityCapabilities securityCapabilities;
        EndpointIdentity identity;

        public SecurityChannelListener(SecurityBindingElement bindingElement, BindingContext context)
            : base(true, context.Binding)
        {
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
            extendedProtectionPolicyHasSupport = SecurityUtils.IsSecurityBindingSuitableForChannelBinding(bindingElement as TransportSecurityBindingElement);
        }

        // used by internal test code
        internal SecurityChannelListener(SecurityProtocolFactory protocolFactory, IChannelListener innerChannelListener)
            : base(true, null, innerChannelListener)
        {
            this.securityProtocolFactory = protocolFactory;
        }

        public ChannelBuilder ChannelBuilder
        {
            get
            {
                ThrowIfDisposed();
                return this.channelBuilder;
            }
        }

        public SecurityProtocolFactory SecurityProtocolFactory
        {
            get
            {
                ThrowIfDisposed();
                return this.securityProtocolFactory;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                ThrowIfDisposedOrImmutable();
                this.securityProtocolFactory = value;
            }
        }

        public bool SessionMode
        {
            get
            {
                return this.sessionMode;
            }
            set
            {
                ThrowIfDisposedOrImmutable();
                this.sessionMode = value;
            }
        }

        public SecuritySessionServerSettings SessionServerSettings
        {
            get
            {
                ThrowIfDisposed();
                if (this.sessionServerSettings == null)
                {
                    lock (ThisLock)
                    {
                        if (this.sessionServerSettings == null)
                        {
                            SecuritySessionServerSettings tmp = new SecuritySessionServerSettings();
                            System.Threading.Thread.MemoryBarrier();
                            this.sessionServerSettings = tmp;
                        }
                    }
                }
                return this.sessionServerSettings;
            }
        }

        bool SupportsDuplex
        {
            get
            {
                ThrowIfProtocolFactoryNotSet();
                return this.securityProtocolFactory.SupportsDuplex;
            }
        }

        bool SupportsRequestReply
        {
            get
            {
                ThrowIfProtocolFactoryNotSet();
                return this.securityProtocolFactory.SupportsRequestReply;
            }
        }

        public bool SendUnsecuredFaults
        {
            get
            {
                return this.sendUnsecuredFaults;
            }
            set
            {
                ThrowIfDisposedOrImmutable();
                this.sendUnsecuredFaults = value;
            }
        }

        // This method should only be called at Open time, since it looks up the identity based on the 
        // thread token
        void ComputeEndpointIdentity()
        {
            EndpointIdentity result = null;
            if (this.State == CommunicationState.Opened)
            {
                if (this.SecurityProtocolFactory != null)
                {
                    result = this.SecurityProtocolFactory.GetIdentityOfSelf();
                }
                else if (this.SessionServerSettings != null && this.SessionServerSettings.SessionProtocolFactory != null)
                {
                    result = this.SessionServerSettings.SessionProtocolFactory.GetIdentityOfSelf();
                }
            }
            if (result == null)
            {
                result = base.GetProperty<EndpointIdentity>();
            }
            this.identity = result;
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(SecurityProtocolFactory))
            {
                return (T)(object)this.SecurityProtocolFactory;
            }
            else if (this.SessionMode && (typeof(T) == typeof(IListenerSecureConversationSessionSettings)))
            {
                return (T)(object)this.SessionServerSettings;
            }
            else if (typeof(T) == typeof(EndpointIdentity))
            {
                return (T)(object)(this.identity);
            }
            else if (typeof(T) == typeof(Collection<ISecurityContextSecurityTokenCache>))
            {
                if (this.SecurityProtocolFactory != null)
                {
                    return (T)(object)this.SecurityProtocolFactory.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
                }
                else
                {
                    return (T)(object)base.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
                }
            }
            else if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)this.securityCapabilities;
            }
            else if (typeof(T) == typeof(ILogonTokenCacheManager))
            {
                List<ILogonTokenCacheManager> cacheManagers = new List<ILogonTokenCacheManager>();

                if (this.SecurityProtocolFactory != null && this.securityProtocolFactory.ChannelSupportingTokenAuthenticatorSpecification.Count > 0)
                {
                    foreach (SupportingTokenAuthenticatorSpecification spec in this.securityProtocolFactory.ChannelSupportingTokenAuthenticatorSpecification)
                    {
                        if (spec.TokenAuthenticator is ILogonTokenCacheManager)
                            cacheManagers.Add(spec.TokenAuthenticator as ILogonTokenCacheManager);
                    }
                }

                if (this.SessionServerSettings.SessionProtocolFactory != null && this.SessionServerSettings.SessionTokenAuthenticator is ILogonTokenCacheManager)
                    cacheManagers.Add(this.SessionServerSettings.SessionTokenAuthenticator as ILogonTokenCacheManager);

                return (T)(object)(new AggregateLogonTokenCacheManager(new ReadOnlyCollection<ILogonTokenCacheManager>(cacheManagers)));
            }

            return base.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            lock (ThisLock)
            {
                if (this.hasSecurityStateReference)
                {
                    hasSecurityStateReference = false;
                    if (this.settingsLifetimeManager != null)
                    {
                        this.settingsLifetimeManager.Abort();
                    }
                }
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.SessionMode)
            {
                if (this.sessionServerSettings != null)
                {
                    this.sessionServerSettings.StopAcceptingNewWork();
                }
            }

            return new ChainedAsyncResult(timeout, callback, state, this.OnBeginCloseSharedState, this.OnEndCloseSharedState, base.OnBeginClose, base.OnEndClose);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        IAsyncResult OnBeginCloseSharedState(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseSharedStateAsyncResult(this, timeout, callback, state);
        }

        void OnEndCloseSharedState(IAsyncResult result)
        {
            CloseSharedStateAsyncResult.End(result);
        }

        internal IAsyncResult OnBeginOpenListenerState(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenListenerStateAsyncResult(this, timeout, callback, state);
        }

        internal void OnEndOpenListenerState(IAsyncResult result)
        {
            OpenListenerStateAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfInnerListenerNotSet();
            EnableChannelBindingSupport();

            return new ChainedAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, this.OnBeginOpenListenerState, this.OnEndOpenListenerState);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            ComputeEndpointIdentity();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.sessionServerSettings != null)
            {
                this.sessionServerSettings.StopAcceptingNewWork();
            }
            lock (ThisLock)
            {
                if (this.hasSecurityStateReference)
                {
                    hasSecurityStateReference = false;
                    this.settingsLifetimeManager.Close(timeoutHelper.RemainingTime());
                }
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        internal void InitializeListener(ChannelBuilder channelBuilder)
        {
            this.channelBuilder = channelBuilder;

            if (this.SessionMode)
            {
                this.sessionServerSettings.ChannelBuilder = this.ChannelBuilder;
                this.InnerChannelListener = this.sessionServerSettings.CreateInnerChannelListener();
                this.Acceptor = this.sessionServerSettings.CreateAcceptor<TChannel>();
            }
            else
            {
                this.InnerChannelListener = this.ChannelBuilder.BuildChannelListener<TChannel>();
                this.Acceptor = (IChannelAcceptor<TChannel>)new SecurityChannelAcceptor(this,
                    (IChannelListener<TChannel>)InnerChannelListener, this.securityProtocolFactory.CreateListenerSecurityState());
            }
        }

        void InitializeListenerSecurityState()
        {
            if (this.SessionMode)
            {
                this.SessionServerSettings.SessionProtocolFactory.ListenUri = this.Uri;
                this.SessionServerSettings.SecurityChannelListener = this;
            }
            else
            {
                ThrowIfProtocolFactoryNotSet();
                this.securityProtocolFactory.ListenUri = this.Uri;
            }
            this.settingsLifetimeManager = new SecurityListenerSettingsLifetimeManager(this.securityProtocolFactory, this.sessionServerSettings, this.sessionMode, this.InnerChannelListener);
            if (this.sessionServerSettings != null)
            {
                this.sessionServerSettings.SettingsLifetimeManager = this.settingsLifetimeManager;
            }
            this.hasSecurityStateReference = true;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            ThrowIfInnerListenerNotSet();
            EnableChannelBindingSupport();

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            lock (ThisLock)
            {
                // if an abort happened before the Open, return
                if (this.State == CommunicationState.Closing && this.State == CommunicationState.Closed)
                {
                    return;
                }
                InitializeListenerSecurityState();
            }
            this.settingsLifetimeManager.Open(timeoutHelper.RemainingTime());
        }

        void EnableChannelBindingSupport()
        {

            ExtendedProtectionPolicy extendedProtectionPolicy = InnerChannelListener.GetProperty<ExtendedProtectionPolicy>();

            // If extendedProtectionPolicy is set we need to check if necessary pieces are in place
            if (extendedProtectionPolicy != null)
            {
                // we do not support custom channel bindings in Win7
                if (extendedProtectionPolicy.CustomChannelBinding != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ExtendedProtectionPolicyCustomChannelBindingNotSupported)));
                }

                if (extendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Never)
                    return;

                IChannelBindingProvider cbp = InnerChannelListener.GetProperty<IChannelBindingProvider>();

                if (extendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Always)
                {
                    // If the securityBinding does not support ExtendedProtectionPolicy OR { there is no channel-binding-provider and we are not in TrustedProxy scenario } then we throw.
                    // For NetFXw7, we only suppport SPNego and Kerb.
                    if (SecurityUtils.IsChannelBindingDisabled || !this.extendedProtectionPolicyHasSupport || (cbp == null && extendedProtectionPolicy.ProtectionScenario != ProtectionScenario.TrustedProxy))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityChannelListenerChannelExtendedProtectionNotSupported)));
                    }
                }

                // Do not enable channel binding if there is no reason as it sets http in chunking mode.
                if ((SecurityUtils.IsChannelBindingDisabled) || (!this.extendedProtectionPolicyHasSupport))
                    return;

                if (cbp != null)
                {
                    cbp.EnableChannelBindingSupport();
                }

            }

            if (this.securityProtocolFactory != null)
            {
                this.securityProtocolFactory.ExtendedProtectionPolicy = extendedProtectionPolicy;
            }

        }

        void ThrowIfProtocolFactoryNotSet()
        {
            if (this.securityProtocolFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityProtocolFactoryShouldBeSetBeforeThisOperation)));
            }
        }

        protected override void OnFaulted()
        {
            lock (ThisLock)
            {
                if (this.hasSecurityStateReference)
                {
                    this.hasSecurityStateReference = false;
                    if (this.settingsLifetimeManager != null)
                    {
                        this.settingsLifetimeManager.Abort();
                    }
                }
            }
            base.OnFaulted();
        }

        class AggregateLogonTokenCacheManager : ILogonTokenCacheManager
        {
            ReadOnlyCollection<ILogonTokenCacheManager> cacheManagers;

            public AggregateLogonTokenCacheManager(ReadOnlyCollection<ILogonTokenCacheManager> cacheManagers)
            {
                if (cacheManagers == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cacheManagers");

                this.cacheManagers = cacheManagers;
            }

            public bool RemoveCachedLogonToken(string username)
            {
                bool removed = false;

                if (!removed && this.cacheManagers != null)
                {
                    for (int i = 0; i < this.cacheManagers.Count; ++i)
                    {
                        removed = this.cacheManagers[i].RemoveCachedLogonToken(username);
                        if (removed)
                            break;
                    }
                }

                return removed;
            }

            public void FlushLogonTokenCache()
            {
                if (this.cacheManagers != null)
                {
                    for (int i = 0; i < this.cacheManagers.Count; ++i)
                    {
                        this.cacheManagers[i].FlushLogonTokenCache();
                    }
                }
            }
        }

        internal sealed class SecurityChannelAcceptor : LayeredChannelAcceptor<TChannel, TChannel>
        {
            readonly object listenerSecurityProtocolState;

            public SecurityChannelAcceptor(ChannelManagerBase channelManager, IChannelListener<TChannel> innerListener,
                object listenerSecurityProtocolState)
                : base(channelManager, innerListener)
            {
                this.listenerSecurityProtocolState = listenerSecurityProtocolState;
            }

            SecurityChannelListener<TChannel> SecurityChannelListener
            {
                get
                {
                    return (SecurityChannelListener<TChannel>)base.ChannelManager;
                }
            }

            protected override TChannel OnAcceptChannel(TChannel innerChannel)
            {
                SecurityChannelListener<TChannel> listener = this.SecurityChannelListener;
                SecurityProtocol securityProtocol = listener.SecurityProtocolFactory.CreateSecurityProtocol(
                    null,
                    null,
                    this.listenerSecurityProtocolState,
                    typeof(TChannel) == typeof(IReplyChannel) || typeof(TChannel) == typeof(IReplySessionChannel), TimeSpan.Zero);
                object securityChannel;
                if (typeof(TChannel) == typeof(IInputChannel))
                {
                    securityChannel = new SecurityInputChannel(listener, (IInputChannel)innerChannel, securityProtocol, listener.settingsLifetimeManager);
                }
                else if (typeof(TChannel) == typeof(IInputSessionChannel))
                {
                    securityChannel = new SecurityInputSessionChannel(listener, (IInputSessionChannel)innerChannel, securityProtocol, listener.settingsLifetimeManager);
                }
                else if (listener.SupportsDuplex && typeof(TChannel) == typeof(IDuplexChannel))
                {
                    securityChannel = new SecurityDuplexChannel(listener, (IDuplexChannel)innerChannel, securityProtocol, listener.settingsLifetimeManager);
                }
                else if (listener.SupportsDuplex && typeof(TChannel) == typeof(IDuplexSessionChannel))
                {
                    securityChannel = new SecurityDuplexSessionChannel(listener, (IDuplexSessionChannel)innerChannel, securityProtocol, listener.settingsLifetimeManager);
                }
                else if (listener.SupportsRequestReply && typeof(TChannel) == typeof(IReplyChannel))
                {
                    securityChannel = new SecurityReplyChannel(listener, (IReplyChannel)innerChannel, securityProtocol, listener.settingsLifetimeManager);
                }
                else if (listener.SupportsRequestReply && typeof(TChannel) == typeof(IReplySessionChannel))
                {
                    securityChannel = new SecurityReplySessionChannel(listener, (IReplySessionChannel)innerChannel, securityProtocol, listener.settingsLifetimeManager);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedChannelInterfaceType, typeof(TChannel))));
                }

                return (TChannel)securityChannel;
            }
        }

        class CloseSharedStateAsyncResult : AsyncResult
        {
            static AsyncCallback lifetimeManagerCloseCallback = Fx.ThunkCallback(new AsyncCallback(LifetimeManagerCloseCallback));
            SecurityChannelListener<TChannel> securityListener;

            public CloseSharedStateAsyncResult(SecurityChannelListener<TChannel> securityListener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.securityListener = securityListener;
                lock (this.securityListener.ThisLock)
                {
                    if (this.securityListener.hasSecurityStateReference)
                    {
                        this.securityListener.hasSecurityStateReference = false;
                        IAsyncResult result = this.securityListener.settingsLifetimeManager.BeginClose(timeout, lifetimeManagerCloseCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        this.securityListener.settingsLifetimeManager.EndClose(result);
                    }
                }
                Complete(true);
            }

            static void LifetimeManagerCloseCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                CloseSharedStateAsyncResult self = (CloseSharedStateAsyncResult)(result.AsyncState);
                Exception completionException = null;
                try
                {
                    self.securityListener.settingsLifetimeManager.EndClose(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    completionException = e;
                }
                self.Complete(false, completionException);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseSharedStateAsyncResult>(result);
            }
        }

        class OpenListenerStateAsyncResult : AsyncResult
        {
            static AsyncCallback lifetimeManagerOpenCallback = Fx.ThunkCallback(new AsyncCallback(LifetimeManagerOpenCallback));
            SecurityChannelListener<TChannel> securityListener;

            public OpenListenerStateAsyncResult(SecurityChannelListener<TChannel> securityListener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.securityListener = securityListener;

                bool openState;
                lock (this.securityListener.ThisLock)
                {
                    // if an abort happened during the Open, return
                    if (this.securityListener.State == CommunicationState.Closed || this.securityListener.State == CommunicationState.Closing)
                    {
                        openState = false;
                    }
                    else
                    {
                        openState = true;
                        this.securityListener.InitializeListenerSecurityState();
                    }
                }
                if (openState)
                {
                    IAsyncResult result = this.securityListener.settingsLifetimeManager.BeginOpen(timeout, lifetimeManagerOpenCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    this.securityListener.settingsLifetimeManager.EndOpen(result);
                }
                Complete(true);
            }

            static void LifetimeManagerOpenCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                OpenListenerStateAsyncResult self = (OpenListenerStateAsyncResult)(result.AsyncState);
                Exception completionException = null;
                try
                {
                    self.securityListener.settingsLifetimeManager.EndOpen(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    completionException = e;
                }
                self.Complete(false, completionException);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenListenerStateAsyncResult>(result);
            }
        }

        abstract class ServerSecurityChannel<UChannel> : SecurityChannel<UChannel> where UChannel : class, IChannel
        {
            static MessageFault secureConversationCloseNotSupportedFault;
            string secureConversationCloseAction;
            SecurityListenerSettingsLifetimeManager settingsLifetimeManager;
            bool hasSecurityStateReference;

            protected ServerSecurityChannel(ChannelManagerBase channelManager, UChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(channelManager, innerChannel, securityProtocol)
            {
                if (settingsLifetimeManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settingsLifetimeManager");
                }
                this.settingsLifetimeManager = settingsLifetimeManager;
            }

            internal void InternalThrowIfFaulted()
            {
                this.ThrowIfFaulted();
            }

            protected override void OnOpened()
            {
                base.OnOpened();
                this.secureConversationCloseAction = this.SecurityProtocol.SecurityProtocolFactory.StandardsManager.SecureConversationDriver.CloseAction.Value;
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.SecurityProtocol.Open(timeoutHelper.RemainingTime());
                base.OnOpen(timeoutHelper.RemainingTime());
                lock (ThisLock)
                {
                    // if an abort happened concurrently with the Open, then dont add a reference
                    if (this.State == CommunicationState.Closed || this.State == CommunicationState.Closing)
                    {
                        return;
                    }
                    this.hasSecurityStateReference = true;
                    this.settingsLifetimeManager.AddReference();
                }
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.SecurityProtocol.Open(timeoutHelper.RemainingTime());
                return base.OnBeginOpen(timeoutHelper.RemainingTime(), callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                base.OnEndOpen(result);
                lock (ThisLock)
                {
                    // if an abort happened concurrently with the Open, then dont add a reference
                    if (this.State == CommunicationState.Closed || this.State == CommunicationState.Closing)
                    {
                        return;
                    }
                    this.hasSecurityStateReference = true;
                    this.settingsLifetimeManager.AddReference();
                }
            }

            protected override void OnAbort()
            {
                lock (ThisLock)
                {
                    if (this.hasSecurityStateReference)
                    {
                        hasSecurityStateReference = false;
                        this.settingsLifetimeManager.Abort();
                    }
                }
                base.OnAbort();
            }

            protected override void OnFaulted()
            {
                lock (ThisLock)
                {
                    if (this.hasSecurityStateReference)
                    {
                        hasSecurityStateReference = false;
                        this.settingsLifetimeManager.Abort();
                    }
                }
                base.OnFaulted();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                lock (ThisLock)
                {
                    if (this.hasSecurityStateReference)
                    {
                        hasSecurityStateReference = false;
                        this.settingsLifetimeManager.Close(timeoutHelper.RemainingTime());
                    }
                }
                base.OnClose(timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedAsyncResult(timeout, callback, state, this.OnBeginCloseSharedState, this.OnEndCloseSharedState, base.OnBeginClose, base.OnEndClose);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            IAsyncResult OnBeginCloseSharedState(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseSharedStateAsyncResult(this, timeout, callback, state);
            }

            void OnEndCloseSharedState(IAsyncResult result)
            {
                CloseSharedStateAsyncResult.End(result);
            }

            static MessageFault GetSecureConversationCloseNotSupportedFault()
            {
                if (secureConversationCloseNotSupportedFault == null)
                {
                    FaultCode faultCode = FaultCode.CreateSenderFaultCode(DotNetSecurityStrings.SecureConversationCancelNotAllowedFault, DotNetSecurityStrings.Namespace);
                    FaultReason faultReason = new FaultReason(SR.GetString(SR.SecureConversationCancelNotAllowedFaultReason), System.Globalization.CultureInfo.InvariantCulture);
                    secureConversationCloseNotSupportedFault = MessageFault.CreateFault(faultCode, faultReason);
                }
                return secureConversationCloseNotSupportedFault;
            }

            void ThrowIfSecureConversationCloseMessage(Message message)
            {
                if (message.Headers.Action == this.secureConversationCloseAction)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.SecureConversationCancelNotAllowedFaultReason), null, GetSecureConversationCloseNotSupportedFault()));
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Delegates to a SecurityCritical method in HostedThreadData." +
                "Caller must ensure that function is called appropriately and result is guarded and Dispose()'d correctly.")]
            [SecurityCritical]
            IDisposable ApplyHostingIntegrationContext(Message message)
            {
                IDisposable hostingContext = null;
                IAspNetMessageProperty hostingProperty = AspNetEnvironment.Current.GetHostingProperty(message);
                if (hostingProperty != null)
                {
                    hostingContext = hostingProperty.ApplyIntegrationContext();
                }
                return hostingContext;
            }

            [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method ApplyHostingIntegrationContext.",
                Safe = "Does call properly and calls Dispose, doesn't leak control of the IDisposable out of the function.")]
            [SecuritySafeCritical]
            internal SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout, params SecurityProtocolCorrelationState[] correlationState)
            {
                if (message == null)
                {
                    return null;
                }
                ThrowIfSecureConversationCloseMessage(message);
                using (this.ApplyHostingIntegrationContext(message))
                {
                    return this.SecurityProtocol.VerifyIncomingMessage(ref message, timeout, correlationState);
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method ApplyHostingIntegrationContext.",
                Safe = "Does call properly and calls Dispose, doesn't leak control of the IDisposable out of the function.")]
            [SecuritySafeCritical]
            internal void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
            {
                if (message == null)
                {
                    return;
                }
                ThrowIfSecureConversationCloseMessage(message);
                using (this.ApplyHostingIntegrationContext(message))
                {
                    this.SecurityProtocol.VerifyIncomingMessage(ref message, timeout);
                }
            }

            class CloseSharedStateAsyncResult : AsyncResult
            {
                static AsyncCallback lifetimeManagerCloseCallback = Fx.ThunkCallback(new AsyncCallback(LifetimeManagerCloseCallback));
                ServerSecurityChannel<UChannel> securityChannel;

                public CloseSharedStateAsyncResult(ServerSecurityChannel<UChannel> securityChannel, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.securityChannel = securityChannel;
                    lock (this.securityChannel.ThisLock)
                    {
                        if (this.securityChannel.hasSecurityStateReference)
                        {
                            this.securityChannel.hasSecurityStateReference = false;
                            IAsyncResult result = this.securityChannel.settingsLifetimeManager.BeginClose(timeout, lifetimeManagerCloseCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return;
                            }
                            this.securityChannel.settingsLifetimeManager.EndClose(result);
                        }
                    }
                    Complete(true);
                }

                static void LifetimeManagerCloseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }
                    CloseSharedStateAsyncResult self = (CloseSharedStateAsyncResult)(result.AsyncState);
                    Exception completionException = null;
                    try
                    {
                        self.securityChannel.settingsLifetimeManager.EndClose(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        completionException = e;
                    }
                    self.Complete(false, completionException);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseSharedStateAsyncResult>(result);
                }
            }
        }

        class SecurityInputChannel : ServerSecurityChannel<IInputChannel>, IInputChannel
        {
            public SecurityInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
            }

            public EndpointAddress LocalAddress
            {
                get { return this.InnerChannel.LocalAddress; }
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

            public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (DoneReceivingInCurrentState())
                {
                    return new DoneReceivingAsyncResult(callback, state);
                }

                return new InputChannelReceiveMessageAndVerifySecurityAsyncResult(this, this.InnerChannel, timeout, callback, state);
            }

            public virtual bool EndTryReceive(IAsyncResult result, out Message message)
            {
                DoneReceivingAsyncResult doneRecevingResult = result as DoneReceivingAsyncResult;
                if (doneRecevingResult != null)
                {
                    return DoneReceivingAsyncResult.End(doneRecevingResult, out message);
                }

                return InputChannelReceiveMessageAndVerifySecurityAsyncResult.End(result, out message);
            }

            public virtual bool TryReceive(TimeSpan timeout, out Message message)
            {
                if (DoneReceivingInCurrentState())
                {
                    message = null;
                    return true;
                }

                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                while (true)
                {
                    if (this.State == CommunicationState.Closed || this.State == CommunicationState.Faulted)
                    {
                        message = null;
                        break;
                    }

                    if (!this.InnerChannel.TryReceive(timeoutHelper.RemainingTime(), out message))
                    {
                        return false;
                    }

                    try
                    {
                        this.VerifyIncomingMessage(ref message, timeoutHelper.RemainingTime());
                        break;
                    }
                    catch (MessageSecurityException)
                    {
                        message = null;
                        if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            return false;
                        }
                    }
                }
                ThrowIfFaulted();
                return true;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.InnerChannel.WaitForMessage(timeout);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.InnerChannel.BeginWaitForMessage(timeout, callback, state);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.InnerChannel.EndWaitForMessage(result);
            }
        }

        sealed class SecurityInputSessionChannel : SecurityInputChannel, IInputSessionChannel
        {
            public SecurityInputSessionChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
            }

            public IInputSession Session
            {
                get { return ((IInputSessionChannel)this.InnerChannel).Session; }
            }
        }

        class SecurityDuplexChannel : SecurityInputChannel, IDuplexChannel
        {
            readonly IDuplexChannel innerDuplexChannel;

            public SecurityDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
                this.innerDuplexChannel = innerChannel;
            }

            public EndpointAddress RemoteAddress
            {
                get { return this.innerDuplexChannel.RemoteAddress; }
            }

            public Uri Via
            {
                get { return this.innerDuplexChannel.Via; }
            }

            protected IDuplexChannel InnerDuplexChannel
            {
                get { return this.innerDuplexChannel; }
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                ThrowIfFaulted();
                ThrowIfDisposedOrNotOpen(message);
                return new OutputChannelSendAsyncResult(message, this.SecurityProtocol, this.innerDuplexChannel, timeout, callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                OutputChannelSendAsyncResult.End(result);
            }

            public void Send(Message message)
            {
                this.Send(message, this.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                ThrowIfFaulted();
                ThrowIfDisposedOrNotOpen(message);
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.SecurityProtocol.SecureOutgoingMessage(ref message, timeoutHelper.RemainingTime());
                this.innerDuplexChannel.Send(message, timeoutHelper.RemainingTime());
            }
        }

        sealed class SecurityDuplexSessionChannel : SecurityDuplexChannel, IDuplexSessionChannel
        {
            bool sendUnsecuredFaults;

            public SecurityDuplexSessionChannel(SecurityChannelListener<TChannel> channelManager, IDuplexSessionChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
                sendUnsecuredFaults = channelManager.SendUnsecuredFaults;
            }

            public IDuplexSession Session
            {
                get { return ((IDuplexSessionChannel)this.InnerChannel).Session; }
            }

            public bool SendUnsecuredFaults
            {
                get { return this.sendUnsecuredFaults; }
            }

            void SendFaultIfRequired(Exception e, Message unverifiedMessage, TimeSpan timeout)
            {
                if (!sendUnsecuredFaults)
                {
                    return;
                }
                MessageFault fault = SecurityUtils.CreateSecurityMessageFault(e, this.SecurityProtocol.SecurityProtocolFactory.StandardsManager);
                if (fault == null)
                {
                    return;
                }
                try
                {
                    using (Message faultMessage = Message.CreateMessage(unverifiedMessage.Version, fault, unverifiedMessage.Version.Addressing.DefaultFaultAction))
                    {
                        if (unverifiedMessage.Headers.MessageId != null)
                            faultMessage.InitializeReply(unverifiedMessage);

                        ((IDuplexChannel)this.InnerChannel).Send(faultMessage, timeout);
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                        throw;

                    // ---- exceptions
                }
            }

            public override bool TryReceive(TimeSpan timeout, out Message message)
            {
                if (DoneReceivingInCurrentState())
                {
                    message = null;
                    return true;
                }

                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                while (true)
                {
                    if (this.State == CommunicationState.Closed || this.State == CommunicationState.Faulted)
                    {
                        message = null;
                        break;
                    }

                    if (!this.InnerChannel.TryReceive(timeoutHelper.RemainingTime(), out message))
                    {
                        return false;
                    }

                    Message unverifiedMessage = message;
                    Exception securityException = null;
                    try
                    {
                        this.VerifyIncomingMessage(ref message, timeoutHelper.RemainingTime());
                        break;
                    }
                    catch (MessageSecurityException e)
                    {
                        message = null;
                        securityException = e;
                    }
                    if (securityException != null)
                    {
                        SendFaultIfRequired(securityException, unverifiedMessage, timeoutHelper.RemainingTime());
                        if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            return false;
                        }
                    }
                }
                ThrowIfFaulted();
                return true;
            }

            public override IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (DoneReceivingInCurrentState())
                {
                    return new DoneReceivingAsyncResult(callback, state);
                }

                return new DuplexSessionReceiveMessageAndVerifySecurityAsyncResult(this, this.InnerDuplexChannel, timeout, callback, state);
            }

            public override bool EndTryReceive(IAsyncResult result, out Message message)
            {
                DoneReceivingAsyncResult doneRecevingResult = result as DoneReceivingAsyncResult;
                if (doneRecevingResult != null)
                {
                    return DoneReceivingAsyncResult.End(doneRecevingResult, out message);
                }

                return DuplexSessionReceiveMessageAndVerifySecurityAsyncResult.End(result, out message);
            }
        }

        class SecurityReplyChannel : ServerSecurityChannel<IReplyChannel>, IReplyChannel
        {
            bool sendUnsecuredFaults;

            public SecurityReplyChannel(SecurityChannelListener<TChannel> channelManager, IReplyChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
                sendUnsecuredFaults = channelManager.SendUnsecuredFaults;
            }

            public EndpointAddress LocalAddress
            {
                get { return this.InnerChannel.LocalAddress; }
            }

            public bool SendUnsecuredFaults
            {
                get { return this.sendUnsecuredFaults; }
            }

            public RequestContext ReceiveRequest()
            {
                return this.ReceiveRequest(this.DefaultReceiveTimeout);
            }

            public RequestContext ReceiveRequest(TimeSpan timeout)
            {
                return ReplyChannel.HelpReceiveRequest(this, timeout);
            }

            public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
            {
                return this.BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ReplyChannel.HelpBeginReceiveRequest(this, timeout, callback, state);
            }

            public RequestContext EndReceiveRequest(IAsyncResult result)
            {
                return ReplyChannel.HelpEndReceiveRequest(result);
            }

            public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (DoneReceivingInCurrentState())
                {
                    return new DoneReceivingAsyncResult(callback, state);
                }

                return new ReceiveRequestAndVerifySecurityAsyncResult(this, this.InnerChannel, timeout, callback, state);
            }

            public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext requestContext)
            {
                DoneReceivingAsyncResult doneRecevingResult = result as DoneReceivingAsyncResult;
                if (doneRecevingResult != null)
                {
                    return DoneReceivingAsyncResult.End(doneRecevingResult, out requestContext);
                }

                return ReceiveRequestAndVerifySecurityAsyncResult.End(result, out requestContext);
            }

            internal RequestContext ProcessReceivedRequest(RequestContext requestContext, TimeSpan timeout)
            {
                if (requestContext == null)
                {
                    return null;
                }
                Message message = requestContext.RequestMessage;
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.ReceivedMessageInRequestContextNull, this.InnerChannel)));
                }
                SecurityProtocolCorrelationState correlationState = this.VerifyIncomingMessage(ref message, timeout, null);
                return new SecurityRequestContext(message, requestContext, this.SecurityProtocol, correlationState, this.DefaultSendTimeout, this.DefaultCloseTimeout);
            }

            void SendFaultIfRequired(Exception e, RequestContext innerContext, TimeSpan timeout)
            {
                if (!sendUnsecuredFaults)
                {
                    return;
                }
                MessageFault fault = SecurityUtils.CreateSecurityMessageFault(e, this.SecurityProtocol.SecurityProtocolFactory.StandardsManager);
                if (fault == null)
                {
                    return;
                }
                Message requestMessage = innerContext.RequestMessage;
                Message faultMessage = Message.CreateMessage(requestMessage.Version, fault, requestMessage.Version.Addressing.DefaultFaultAction);
                if (requestMessage.Headers.MessageId != null)
                    faultMessage.InitializeReply(requestMessage);

                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    innerContext.Reply(faultMessage, timeoutHelper.RemainingTime());
                    innerContext.Close(timeoutHelper.RemainingTime());
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    // eat up exceptions
                }
                finally
                {
                    faultMessage.Close();
                    innerContext.Abort();
                }
            }

            public bool TryReceiveRequest(TimeSpan timeout, out RequestContext requestContext)
            {
                if (DoneReceivingInCurrentState())
                {
                    requestContext = null;
                    return true;
                }

                requestContext = null;
                RequestContext innerContext;
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                while (true)
                {
                    if (this.State == CommunicationState.Closed || this.State == CommunicationState.Faulted)
                    {
                        requestContext = null;
                        break;
                    }

                    if (!this.InnerChannel.TryReceiveRequest(timeoutHelper.RemainingTime(), out innerContext))
                    {
                        requestContext = null;
                        return false;
                    }
                    Exception securityException = null;
                    try
                    {
                        requestContext = ProcessReceivedRequest(innerContext, timeoutHelper.RemainingTime());
                        break;
                    }
                    catch (MessageSecurityException e)
                    {
                        securityException = e;
                    }
                    if (securityException != null)
                    {
                        SendFaultIfRequired(securityException, innerContext, timeoutHelper.RemainingTime());
                        if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            return false;
                        }
                    }
                }
                ThrowIfFaulted();
                return true;
            }

            public bool WaitForRequest(TimeSpan timeout)
            {
                return this.InnerChannel.WaitForRequest(timeout);
            }

            public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.InnerChannel.BeginWaitForRequest(timeout, callback, state);
            }

            public bool EndWaitForRequest(IAsyncResult result)
            {
                return this.InnerChannel.EndWaitForRequest(result);
            }
        }

        sealed class SecurityReplySessionChannel : SecurityReplyChannel, IReplySessionChannel
        {
            public SecurityReplySessionChannel(SecurityChannelListener<TChannel> channelManager, IReplySessionChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager)
                : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
            }

            public IInputSession Session
            {
                get { return ((IReplySessionChannel)this.InnerChannel).Session; }
            }
        }

        sealed class SecurityRequestContext : RequestContextBase
        {
            readonly RequestContext innerContext;
            readonly SecurityProtocol securityProtocol;
            readonly SecurityProtocolCorrelationState correlationState;

            public SecurityRequestContext(Message requestMessage, RequestContext innerContext,
                SecurityProtocol securityProtocol, SecurityProtocolCorrelationState correlationState,
                TimeSpan defaultSendTimeout, TimeSpan defaultCloseTimeout)
                : base(requestMessage, defaultCloseTimeout, defaultSendTimeout)
            {
                this.innerContext = innerContext;
                this.securityProtocol = securityProtocol;
                this.correlationState = correlationState;
            }

            protected override void OnAbort()
            {
                this.innerContext.Abort();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.innerContext.Close(timeout);
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (message != null)
                {
                    return new RequestContextSendAsyncResult(message, this.securityProtocol, this.innerContext, timeout,
                        callback, state, correlationState);
                }
                else
                {
                    return this.innerContext.BeginReply(message, timeout, callback, state);
                }
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                if (result is RequestContextSendAsyncResult)
                {
                    RequestContextSendAsyncResult.End(result);
                }
                else
                {
                    this.innerContext.EndReply(result);
                }
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (message != null)
                {
                    this.securityProtocol.SecureOutgoingMessage(ref message, timeoutHelper.RemainingTime(), correlationState);
                }
                this.innerContext.Reply(message, timeoutHelper.RemainingTime());
            }

            sealed class RequestContextSendAsyncResult : ApplySecurityAndSendAsyncResult<RequestContext>
            {
                public RequestContextSendAsyncResult(Message message, SecurityProtocol protocol, RequestContext context, TimeSpan timeout,
                    AsyncCallback callback, object state, SecurityProtocolCorrelationState correlationState)
                    : base(protocol, context, timeout, callback, state)
                {
                    this.Begin(message, correlationState);
                }

                protected override IAsyncResult BeginSendCore(RequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return context.BeginReply(message, timeout, callback, state);
                }

                internal static void End(IAsyncResult result)
                {
                    RequestContextSendAsyncResult self = result as RequestContextSendAsyncResult;
                    OnEnd(self);
                }

                protected override void EndSendCore(RequestContext context, IAsyncResult result)
                {
                    context.EndReply(result);
                }

                protected override void OnSendCompleteCore(TimeSpan timeout)
                {
                }
            }
        }

        abstract class ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel> : AsyncResult
            where UChannel : class, IChannel
            where TItem : class
        {
            static AsyncCallback innerTryReceiveCompletedCallback = Fx.ThunkCallback(new AsyncCallback(InnerTryReceiveCompletedCallback));
            protected bool receiveCompleted;
            protected TimeoutHelper timeoutHelper;
            TItem innerItem;
            TItem item;
            ServerSecurityChannel<UChannel> channel;
            Message faultMessage;

            public ReceiveItemAndVerifySecurityAsyncResult(ServerSecurityChannel<UChannel> channel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.channel = channel;
            }

            protected void Start()
            {
                bool completeSelf = false;
                Exception completeException = null;

                try
                {
                    completeSelf = StartInnerReceive();
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    completeException = ex;
                }

                if (completeSelf || completeException != null)
                {
                    this.Complete(false, completeException);
                }
            }

            protected TItem Item
            {
                get { return this.item; }
            }

            protected bool ReceiveCompleted
            {
                get { return this.receiveCompleted; }
            }

            protected abstract bool CanSendFault { get; }
            protected abstract SecurityStandardsManager StandardsManager { get; }
            protected abstract IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state);
            protected abstract bool EndTryReceiveItem(IAsyncResult result, out TItem innerItem);
            protected abstract TItem ProcessInnerItem(TItem innerItem, TimeSpan timeout);
            protected abstract Message CreateFaultMessage(MessageFault fault, TItem innerItem);
            protected abstract IAsyncResult BeginSendFault(TItem innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state);
            protected abstract void EndSendFault(TItem innerItem, IAsyncResult result);
            protected abstract void CloseInnerItem(TItem innerItem, TimeSpan timeout);
            protected abstract void AbortInnerItem(TItem innerItem);

            bool StartInnerReceive()
            {
                try
                {
                    this.channel.InternalThrowIfFaulted();
                    if (this.channel.State == CommunicationState.Closed)
                    {
                        this.item = null;
                        this.receiveCompleted = true;
                        return true;
                    }

                    IAsyncResult asyncResult = BeginTryReceiveItem(timeoutHelper.RemainingTime(), innerTryReceiveCompletedCallback, this);
                    if (!asyncResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    bool innerReceiveCompleted = this.EndTryReceiveItem(asyncResult, out this.innerItem);
                    if (!innerReceiveCompleted)
                    {
                        receiveCompleted = false;
                        return true;
                    }
                    else
                    {
                        return this.OnInnerReceiveDone();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.Complete(false, e);
                    return false;
                }
            }

            static void InnerTryReceiveCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel> thisResult = (ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel>)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    bool innerReceiveCompleted = thisResult.EndTryReceiveItem(result, out thisResult.innerItem);
                    if (!innerReceiveCompleted)
                    {
                        thisResult.receiveCompleted = false;
                        completeSelf = true;
                    }
                    else
                    {
                        completeSelf = thisResult.OnInnerReceiveDone();
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

            bool OnInnerReceiveDone()
            {
                this.channel.InternalThrowIfFaulted();
                Exception securityException = null;
                try
                {
                    this.item = ProcessInnerItem(this.innerItem, this.timeoutHelper.RemainingTime());
                    this.receiveCompleted = true;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (MessageSecurityException e)
                {
                    securityException = e;
                }
                if (securityException != null)
                {
                    if (CanSendFault)
                    {
                        bool sentFaultSync = this.OnSecurityException(securityException);
                        if (!sentFaultSync)
                        {
                            return false;
                        }
                    }
                    return OnFaultSent();
                }
                else
                {
                    return true;
                }
            }

            bool OnFaultSent()
            {
                this.innerItem = null;
                if (this.timeoutHelper.RemainingTime() == TimeSpan.Zero)
                {
                    this.receiveCompleted = false;
                    return true;
                }
                else
                {
                    return this.StartInnerReceive();
                }
            }

            bool OnSecurityException(Exception e)
            {
                MessageFault fault = SecurityUtils.CreateSecurityMessageFault(e, this.StandardsManager);
                if (fault == null)
                {
                    return true;
                }
                else
                {
                    this.faultMessage = CreateFaultMessage(fault, this.innerItem);
                    return this.SendFault(faultMessage, e);
                }
            }

            bool SendFault(Message faultMessage, Exception e)
            {
                bool wasFaultSentSync = false;
                try
                {
                    IAsyncResult result = this.BeginSendFault(this.innerItem, faultMessage, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(SendFaultCallback)), e);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    wasFaultSentSync = true;
                    this.EndSendFault(innerItem, result);
                    CloseInnerItem(innerItem, timeoutHelper.RemainingTime());
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception ex)
                {
                    if (faultMessage != null)
                    {
                        faultMessage.Close();
                    }

                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    // ---- exceptions
                }
                finally
                {
                    if (wasFaultSentSync)
                    {
                        AbortInnerItem(innerItem);
                        if (faultMessage != null)
                        {
                            faultMessage.Close();
                        }
                    }
                }
                return true;
            }

            void SendFaultCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                Exception e = (Exception)result.AsyncState;
                try
                {
                    this.EndSendFault(innerItem, result);
                    this.CloseInnerItem(this.innerItem, timeoutHelper.RemainingTime());
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    // ---- exceptions
                }
                finally
                {
                    if (this.faultMessage != null)
                    {
                        this.faultMessage.Close();
                    }

                    this.AbortInnerItem(this.innerItem);
                }
                // start off another receive
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = this.OnFaultSent();
                }
                catch (Exception e2)
                {
                    if (Fx.IsFatal(e2))
                        throw;
                    completeSelf = true;
                    completionException = e2;
                }
                if (completeSelf)
                {
                    Complete(false, completionException);
                }
            }
        }


        sealed class ReceiveRequestAndVerifySecurityAsyncResult : ReceiveItemAndVerifySecurityAsyncResult<RequestContext, IReplyChannel>
        {
            SecurityReplyChannel channel;
            IReplyChannel innerChannel;

            public ReceiveRequestAndVerifySecurityAsyncResult(SecurityReplyChannel channel, IReplyChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(channel, timeout, callback, state)
            {
                this.channel = channel;
                this.innerChannel = innerChannel;
                ActionItem.Schedule(ReceiveMessage, this);
            }

            protected override bool CanSendFault
            {
                get { return this.channel.SendUnsecuredFaults; }
            }

            protected override SecurityStandardsManager StandardsManager
            {
                get { return this.channel.SecurityProtocol.SecurityProtocolFactory.StandardsManager; }
            }

            static void ReceiveMessage(object state)
            {
                ReceiveRequestAndVerifySecurityAsyncResult securityAsyncResult = state as ReceiveRequestAndVerifySecurityAsyncResult;
                if (securityAsyncResult == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException());
                }

                securityAsyncResult.Start();
            }

            protected override void AbortInnerItem(RequestContext innerItem)
            {
                innerItem.Abort();
            }

            protected override void CloseInnerItem(RequestContext innerItem, TimeSpan timeout)
            {
                innerItem.Close(timeout);
            }

            protected override IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginTryReceiveRequest(timeout, callback, state);
            }

            protected override bool EndTryReceiveItem(IAsyncResult result, out RequestContext innerItem)
            {
                return this.innerChannel.EndTryReceiveRequest(result, out innerItem);
            }

            protected override RequestContext ProcessInnerItem(RequestContext innerItem, TimeSpan timeout)
            {
                return this.channel.ProcessReceivedRequest(innerItem, timeout);
            }

            protected override Message CreateFaultMessage(MessageFault fault, RequestContext innerItem)
            {
                Message requestMessage = innerItem.RequestMessage;
                Message faultMessage = Message.CreateMessage(requestMessage.Version, fault, requestMessage.Version.Addressing.DefaultFaultAction);
                if (requestMessage.Headers.MessageId != null)
                    faultMessage.InitializeReply(requestMessage);
                return faultMessage;
            }

            protected override IAsyncResult BeginSendFault(RequestContext innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return innerItem.BeginReply(faultMessage, timeout, callback, state);
            }

            protected override void EndSendFault(RequestContext innerItem, IAsyncResult result)
            {
                innerItem.EndReply(result);
            }

            public static bool End(IAsyncResult result, out RequestContext requestContext)
            {
                ReceiveRequestAndVerifySecurityAsyncResult thisResult = AsyncResult.End<ReceiveRequestAndVerifySecurityAsyncResult>(result);
                requestContext = thisResult.Item;
                return thisResult.ReceiveCompleted;
            }
        }

        sealed class DuplexSessionReceiveMessageAndVerifySecurityAsyncResult : ReceiveItemAndVerifySecurityAsyncResult<Message, IInputChannel>
        {
            IDuplexChannel innerChannel;
            SecurityDuplexSessionChannel channel;

            public DuplexSessionReceiveMessageAndVerifySecurityAsyncResult(SecurityDuplexSessionChannel channel, IDuplexChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(channel, timeout, callback, state)
            {
                this.innerChannel = innerChannel;
                this.channel = channel;
                ActionItem.Schedule(ReceiveMessage, this);
            }

            protected override bool CanSendFault
            {
                get { return this.channel.SendUnsecuredFaults; }
            }

            protected override SecurityStandardsManager StandardsManager
            {
                get { return this.channel.SecurityProtocol.SecurityProtocolFactory.StandardsManager; }
            }

            static void ReceiveMessage(object state)
            {
                DuplexSessionReceiveMessageAndVerifySecurityAsyncResult securityAsyncResult = state as DuplexSessionReceiveMessageAndVerifySecurityAsyncResult;
                if (securityAsyncResult != null)
                {
                    securityAsyncResult.Start();
                }
            }

            protected override void AbortInnerItem(Message innerItem)
            {
            }

            protected override void CloseInnerItem(Message innerItem, TimeSpan timeout)
            {
                innerItem.Close();
            }

            protected override IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginTryReceive(timeout, callback, state);
            }

            protected override bool EndTryReceiveItem(IAsyncResult result, out Message innerItem)
            {
                return this.innerChannel.EndTryReceive(result, out innerItem);
            }

            protected override Message ProcessInnerItem(Message innerItem, TimeSpan timeout)
            {
                if (innerItem == null)
                {
                    return null;
                }
                Message item = innerItem;
                this.channel.VerifyIncomingMessage(ref item, timeout);
                return item;
            }

            protected override Message CreateFaultMessage(MessageFault fault, Message innerItem)
            {
                Message faultMessage = Message.CreateMessage(innerItem.Version, fault, innerItem.Version.Addressing.DefaultFaultAction);
                if (innerItem.Headers.MessageId != null)
                    faultMessage.InitializeReply(innerItem);
                return faultMessage;
            }

            protected override IAsyncResult BeginSendFault(Message innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(faultMessage, timeout, callback, state);
            }

            protected override void EndSendFault(Message innerItem, IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            public static bool End(IAsyncResult result, out Message message)
            {
                DuplexSessionReceiveMessageAndVerifySecurityAsyncResult thisResult = AsyncResult.End<DuplexSessionReceiveMessageAndVerifySecurityAsyncResult>(result);
                message = thisResult.Item;
                return thisResult.ReceiveCompleted;
            }
        }


        sealed class InputChannelReceiveMessageAndVerifySecurityAsyncResult : ReceiveItemAndVerifySecurityAsyncResult<Message, IInputChannel>
        {
            IInputChannel innerChannel;
            SecurityInputChannel channel;

            public InputChannelReceiveMessageAndVerifySecurityAsyncResult(SecurityInputChannel channel, IInputChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(channel, timeout, callback, state)
            {
                this.innerChannel = innerChannel;
                this.channel = channel;
                ActionItem.Schedule(ReceiveMessage, this);
            }

            protected override SecurityStandardsManager StandardsManager
            {
                get { return this.channel.SecurityProtocol.SecurityProtocolFactory.StandardsManager; }
            }

            static void ReceiveMessage(object state)
            {
                InputChannelReceiveMessageAndVerifySecurityAsyncResult securityAsyncResult = state as InputChannelReceiveMessageAndVerifySecurityAsyncResult;
                if (securityAsyncResult == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException());
                }

                securityAsyncResult.Start();
            }

            protected override bool CanSendFault
            {
                get
                {
                    return false;
                }
            }

            protected override void AbortInnerItem(Message innerItem)
            {
            }

            protected override void CloseInnerItem(Message innerItem, TimeSpan timeout)
            {
                innerItem.Close();
            }

            protected override IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginTryReceive(timeout, callback, state);
            }

            protected override bool EndTryReceiveItem(IAsyncResult result, out Message innerItem)
            {
                return this.innerChannel.EndTryReceive(result, out innerItem);
            }

            protected override Message ProcessInnerItem(Message innerItem, TimeSpan timeout)
            {
                if (innerItem == null)
                {
                    return null;
                }
                Message item = innerItem;
                this.channel.VerifyIncomingMessage(ref item, timeout);
                return item;
            }

            protected override Message CreateFaultMessage(MessageFault fault, Message innerItem)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override IAsyncResult BeginSendFault(Message innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override void EndSendFault(Message innerItem, IAsyncResult result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public static bool End(IAsyncResult result, out Message message)
            {
                InputChannelReceiveMessageAndVerifySecurityAsyncResult thisResult = AsyncResult.End<InputChannelReceiveMessageAndVerifySecurityAsyncResult>(result);
                message = thisResult.Item;
                return thisResult.ReceiveCompleted;
            }
        }
    }
}
