//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Text;

    abstract class ConnectionOrientedTransportChannelListener
        : TransportChannelListener, 
          IConnectionOrientedTransportFactorySettings, 
          IConnectionOrientedListenerSettings
    {
        int connectionBufferSize;
        bool exposeConnectionProperty;
        TimeSpan channelInitializationTimeout;
        int maxBufferSize;
        int maxPendingConnections;
        TimeSpan maxOutputDelay;
        int maxPendingAccepts;
        TimeSpan idleTimeout;
        int maxPooledConnections;
        TransferMode transferMode;
        ISecurityCapabilities securityCapabilities;
        StreamUpgradeProvider upgrade;
        bool ownUpgrade;
        EndpointIdentity identity;

        protected ConnectionOrientedTransportChannelListener(ConnectionOrientedTransportBindingElement bindingElement,
            BindingContext context)
            : base(bindingElement, context, bindingElement.HostNameComparisonMode)
        {
            if (bindingElement.TransferMode == TransferMode.Buffered)
            {
                if (bindingElement.MaxReceivedMessageSize > int.MaxValue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("bindingElement.MaxReceivedMessageSize",
                        SR.GetString(SR.MaxReceivedMessageSizeMustBeInIntegerRange)));
                }

                if (bindingElement.MaxBufferSize != bindingElement.MaxReceivedMessageSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement",
                        SR.GetString(SR.MaxBufferSizeMustMatchMaxReceivedMessageSize));
                }
            }
            else
            {
                if (bindingElement.MaxBufferSize > bindingElement.MaxReceivedMessageSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement",
                        SR.GetString(SR.MaxBufferSizeMustNotExceedMaxReceivedMessageSize));
                }
            }


            this.connectionBufferSize = bindingElement.ConnectionBufferSize;
            this.exposeConnectionProperty = bindingElement.ExposeConnectionProperty;
            this.InheritBaseAddressSettings = bindingElement.InheritBaseAddressSettings;
            this.channelInitializationTimeout = bindingElement.ChannelInitializationTimeout;
            this.maxBufferSize = bindingElement.MaxBufferSize;
            this.maxPendingConnections = bindingElement.MaxPendingConnections;
            this.maxOutputDelay = bindingElement.MaxOutputDelay;
            this.maxPendingAccepts = bindingElement.MaxPendingAccepts;
            this.transferMode = bindingElement.TransferMode;

            Collection<StreamUpgradeBindingElement> upgradeBindingElements =
                context.BindingParameters.FindAll<StreamUpgradeBindingElement>();

            if (upgradeBindingElements.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleStreamUpgradeProvidersInParameters)));
            }
            else if ((upgradeBindingElements.Count == 1) && this.SupportsUpgrade(upgradeBindingElements[0]))
            {
                this.upgrade = upgradeBindingElements[0].BuildServerStreamUpgradeProvider(context);
                this.ownUpgrade = true;
                context.BindingParameters.Remove<StreamUpgradeBindingElement>();
                this.securityCapabilities = upgradeBindingElements[0].GetProperty<ISecurityCapabilities>(context);
            }
        }

        public int ConnectionBufferSize
        {
            get
            {
                return this.connectionBufferSize;
            }
        }

        public TimeSpan IdleTimeout
        {
            get { return this.idleTimeout; }
        }

        public int MaxPooledConnections
        {
            get { return this.maxPooledConnections; }
        }

        internal void SetIdleTimeout(TimeSpan idleTimeout)
        {
            this.idleTimeout = idleTimeout;
        }

        internal void InitializeMaxPooledConnections(int maxOutboundConnectionsPerEndpoint)
        {
            if (maxOutboundConnectionsPerEndpoint == ConnectionOrientedTransportDefaults.MaxOutboundConnectionsPerEndpoint)
            {
                this.maxPooledConnections = ConnectionOrientedTransportDefaults.GetMaxConnections();
            }
            else
            {
                this.maxPooledConnections = maxOutboundConnectionsPerEndpoint;
            }
        }

        internal bool ExposeConnectionProperty
        {
            get { return this.exposeConnectionProperty; }
        }

        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.HostNameComparisonModeInternal;
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(EndpointIdentity))
            {
                return (T)(object)(this.identity);
            }
            else if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)this.securityCapabilities;
            }
            else
            {
                T result = base.GetProperty<T>();

                if (result == null && this.upgrade != null)
                {
                    result = this.upgrade.GetProperty<T>();
                }

                return result;
            }
        }

        public TimeSpan ChannelInitializationTimeout
        {
            get
            {
                return this.channelInitializationTimeout;
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return maxBufferSize;
            }
        }

        public int MaxPendingConnections
        {
            get
            {
                return this.maxPendingConnections;
            }
        }

        public TimeSpan MaxOutputDelay
        {
            get
            {
                return maxOutputDelay;
            }
        }

        public int MaxPendingAccepts
        {
            get
            {
                return this.maxPendingAccepts;
            }
        }

        public StreamUpgradeProvider Upgrade
        {
            get
            {
                return this.upgrade;
            }
        }

        public TransferMode TransferMode
        {
            get
            {
                return transferMode;
            }
        }

        int IConnectionOrientedTransportFactorySettings.MaxBufferSize
        {
            get { return MaxBufferSize; }
        }

        TransferMode IConnectionOrientedTransportFactorySettings.TransferMode
        {
            get { return TransferMode; }
        }

        StreamUpgradeProvider IConnectionOrientedTransportFactorySettings.Upgrade
        {
            get { return Upgrade; }
        }

        ServiceSecurityAuditBehavior IConnectionOrientedTransportFactorySettings.AuditBehavior
        {
            get { return base.AuditBehavior; }
        }

        internal override int GetMaxBufferSize()
        {
            return MaxBufferSize;
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            StreamUpgradeProvider localUpgrade = this.Upgrade;
            if (localUpgrade != null)
            {
                return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, localUpgrade);
            }
            else
            {
                return base.OnBeginOpen(timeout, callback, state);
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (result is ChainedOpenAsyncResult)
            {
                ChainedOpenAsyncResult.End(result);
            }
            else
            {
                base.OnEndOpen(result);
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeout);
            StreamUpgradeProvider localUpgrade = this.Upgrade;
            if (localUpgrade != null)
            {
                localUpgrade.Open(timeoutHelper.RemainingTime());
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            StreamSecurityUpgradeProvider security = this.Upgrade as StreamSecurityUpgradeProvider;
            if (security != null)
            {
                this.identity = security.Identity;
            }
        }

        protected override void OnAbort()
        {
            StreamUpgradeProvider localUpgrade = GetUpgrade();
            if (localUpgrade != null)
            {
                localUpgrade.Abort();
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            StreamUpgradeProvider localUpgrade = GetUpgrade();
            if (localUpgrade != null)
            {
                return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, localUpgrade);
            }
            else
            {
                return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedCloseAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            StreamUpgradeProvider localUpgrade = GetUpgrade();
            if (localUpgrade != null)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                localUpgrade.Close(timeoutHelper.RemainingTime());
                base.OnClose(timeoutHelper.RemainingTime());
            }
            else
            {
                base.OnClose(timeout);
            }
        }

        StreamUpgradeProvider GetUpgrade()
        {
            StreamUpgradeProvider result = null;

            lock (ThisLock)
            {
                if (this.ownUpgrade)
                {
                    result = this.upgrade;
                    this.ownUpgrade = false;
                }
            }

            return result;
        }

        protected override void ValidateUri(Uri uri)
        {
            base.ValidateUri(uri);
            int maxViaSize = ConnectionOrientedTransportDefaults.MaxViaSize;
            int encodedSize = Encoding.UTF8.GetByteCount(uri.AbsoluteUri);
            if (encodedSize > maxViaSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new QuotaExceededException(SR.GetString(SR.UriLengthExceedsMaxSupportedSize, uri, encodedSize, maxViaSize)));
            }
        }

        protected virtual bool SupportsUpgrade(StreamUpgradeBindingElement upgradeBindingElement)
        {
            return true;
        }

        // transfers around the StreamUpgradeProvider from an ownership perspective
        protected class ConnectionOrientedTransportReplyChannelAcceptor : TransportReplyChannelAcceptor
        {
            StreamUpgradeProvider upgrade;

            public ConnectionOrientedTransportReplyChannelAcceptor(ConnectionOrientedTransportChannelListener listener)
                : base(listener)
            {
                this.upgrade = listener.GetUpgrade();
            }

            protected override ReplyChannel OnCreateChannel()
            {
                return new ConnectionOrientedTransportReplyChannel(this.ChannelManager, null);
            }

            protected override void OnAbort()
            {
                base.OnAbort();
                if (this.upgrade != null && !TransferUpgrade())
                {
                    this.upgrade.Abort();
                }
            }

            IAsyncResult DummyBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            void DummyEndClose(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                ChainedBeginHandler begin1 = DummyBeginClose;
                ChainedEndHandler end1 = DummyEndClose;
                if (this.upgrade != null && !TransferUpgrade())
                {
                    begin1 = this.upgrade.BeginClose;
                    end1 = this.upgrade.EndClose;
                }

                return new ChainedAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, begin1, end1);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.OnClose(timeoutHelper.RemainingTime());
                if (this.upgrade != null && !TransferUpgrade())
                {
                    this.upgrade.Close(timeoutHelper.RemainingTime());
                }
            }

            // used to decouple our channel and listener lifetimes
            bool TransferUpgrade()
            {
                ConnectionOrientedTransportReplyChannel singletonChannel = (ConnectionOrientedTransportReplyChannel)base.GetCurrentChannel();
                if (singletonChannel == null)
                {
                    return false;
                }
                else
                {
                    return singletonChannel.TransferUpgrade(this.upgrade);
                }
            }

            // tracks StreamUpgradeProvider so that the channel can outlive the Listener
            class ConnectionOrientedTransportReplyChannel : TransportReplyChannel
            {
                StreamUpgradeProvider upgrade;

                public ConnectionOrientedTransportReplyChannel(ChannelManagerBase channelManager, EndpointAddress localAddress)
                    : base(channelManager, localAddress)
                {
                }

                public bool TransferUpgrade(StreamUpgradeProvider upgrade)
                {
                    lock (ThisLock)
                    {
                        if (this.State != CommunicationState.Opened)
                        {
                            return false;
                        }

                        this.upgrade = upgrade;
                        return true;
                    }
                }

                protected override void OnAbort()
                {
                    if (this.upgrade != null)
                    {
                        this.upgrade.Abort();
                    }
                    base.OnAbort();
                }

                protected override void OnClose(TimeSpan timeout)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    if (this.upgrade != null)
                    {
                        this.upgrade.Close(timeoutHelper.RemainingTime());
                    }
                    base.OnClose(timeoutHelper.RemainingTime());
                }

                IAsyncResult DummyBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return new CompletedAsyncResult(callback, state);
                }

                void DummyEndClose(IAsyncResult result)
                {
                    CompletedAsyncResult.End(result);
                }

                protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    ChainedBeginHandler begin1 = DummyBeginClose;
                    ChainedEndHandler end1 = DummyEndClose;
                    if (this.upgrade != null)
                    {
                        begin1 = this.upgrade.BeginClose;
                        end1 = this.upgrade.EndClose;
                    }

                    return new ChainedAsyncResult(timeout, callback, state, begin1, end1,
                            base.OnBeginClose, base.OnEndClose);
                }

                protected override void OnEndClose(IAsyncResult result)
                {
                    ChainedAsyncResult.End(result);
                }
            }
        }
    }
}
