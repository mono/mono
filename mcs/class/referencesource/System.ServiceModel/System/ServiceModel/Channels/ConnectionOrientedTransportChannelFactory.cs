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
    using System.Threading;

    abstract class ConnectionOrientedTransportChannelFactory<TChannel> : TransportChannelFactory<TChannel>, IConnectionOrientedTransportChannelFactorySettings
    {
        int connectionBufferSize;
        IConnectionInitiator connectionInitiator;
        ConnectionPool connectionPool;
        string connectionPoolGroupName;
        bool exposeConnectionProperty;
        TimeSpan idleTimeout;
        int maxBufferSize;
        int maxOutboundConnectionsPerEndpoint;
        TimeSpan maxOutputDelay;
        TransferMode transferMode;
        ISecurityCapabilities securityCapabilities;
        StreamUpgradeProvider upgrade;
        bool flowIdentity;

        internal ConnectionOrientedTransportChannelFactory(
            ConnectionOrientedTransportBindingElement bindingElement, BindingContext context,
            string connectionPoolGroupName, TimeSpan idleTimeout, int maxOutboundConnectionsPerEndpoint, bool supportsImpersonationDuringAsyncOpen)
            : base(bindingElement, context)
        {
            if (bindingElement.TransferMode == TransferMode.Buffered && bindingElement.MaxReceivedMessageSize > int.MaxValue)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("bindingElement.MaxReceivedMessageSize",
                    SR.GetString(SR.MaxReceivedMessageSizeMustBeInIntegerRange)));
            }

            this.connectionBufferSize = bindingElement.ConnectionBufferSize;
            this.connectionPoolGroupName = connectionPoolGroupName;
            this.exposeConnectionProperty = bindingElement.ExposeConnectionProperty;
            this.idleTimeout = idleTimeout;
            this.maxBufferSize = bindingElement.MaxBufferSize;
            this.maxOutboundConnectionsPerEndpoint = maxOutboundConnectionsPerEndpoint;
            this.maxOutputDelay = bindingElement.MaxOutputDelay;
            this.transferMode = bindingElement.TransferMode;

            Collection<StreamUpgradeBindingElement> upgradeBindingElements =
                context.BindingParameters.FindAll<StreamUpgradeBindingElement>();

            if (upgradeBindingElements.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleStreamUpgradeProvidersInParameters)));
            }
            else if ((upgradeBindingElements.Count == 1) && this.SupportsUpgrade(upgradeBindingElements[0]))
            {
                this.upgrade = upgradeBindingElements[0].BuildClientStreamUpgradeProvider(context);
                context.BindingParameters.Remove<StreamUpgradeBindingElement>();
                this.securityCapabilities = upgradeBindingElements[0].GetProperty<ISecurityCapabilities>(context);
                // flow the identity only if the channel factory supports impersonating during an async open AND
                // there is the binding is configured with security
                this.flowIdentity = supportsImpersonationDuringAsyncOpen;
            }
        }

        public int ConnectionBufferSize
        {
            get
            {
                return this.connectionBufferSize;
            }
        }

        internal IConnectionInitiator ConnectionInitiator
        {
            get
            {
                if (this.connectionInitiator == null)
                {
                    lock (ThisLock)
                    {
                        if (this.connectionInitiator == null)
                        {
                            this.connectionInitiator = GetConnectionInitiator();
                            if (DiagnosticUtility.ShouldUseActivity)
                            {
                                this.connectionInitiator = new TracingConnectionInitiator(this.connectionInitiator,
                                    ServiceModelActivity.Current != null && ServiceModelActivity.Current.ActivityType == ActivityType.OpenClient);
                            }
                        }
                    }
                }

                return this.connectionInitiator;
            }
        }

        public string ConnectionPoolGroupName
        {
            get
            {
                return connectionPoolGroupName;
            }
        }

        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return maxBufferSize;
            }
        }

        public int MaxOutboundConnectionsPerEndpoint
        {
            get
            {
                return maxOutboundConnectionsPerEndpoint;
            }
        }

        public TimeSpan MaxOutputDelay
        {
            get
            {
                return maxOutputDelay;
            }
        }

        public StreamUpgradeProvider Upgrade
        {
            get
            {
                StreamUpgradeProvider localUpgrade = this.upgrade;
                ThrowIfDisposed();
                return localUpgrade;
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
#pragma warning suppress 56503 // Internal method.
            get { throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityAuditNotSupportedOnChannelFactory))); }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)this.securityCapabilities;
            }

            T result = base.GetProperty<T>();
            if (result == null && this.upgrade != null)
            {
                result = this.upgrade.GetProperty<T>();
            }

            return result;
        }

        internal override int GetMaxBufferSize()
        {
            return this.MaxBufferSize;
        }

        internal abstract IConnectionInitiator GetConnectionInitiator();

        internal abstract ConnectionPool GetConnectionPool();

        internal abstract void ReleaseConnectionPool(ConnectionPool pool, TimeSpan timeout);

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            base.ValidateScheme(via);

            if (TransferMode == TransferMode.Buffered)
            {
                // typeof(TChannel) == typeof(IDuplexSessionChannel)
                return (TChannel)(object)new ClientFramingDuplexSessionChannel(this, this, address, via,
                    ConnectionInitiator, connectionPool, exposeConnectionProperty, this.flowIdentity);
            }

            // typeof(TChannel) == typeof(IRequestChannel)
            return (TChannel)(object)new StreamedFramingRequestChannel(this, this, address, via,
                ConnectionInitiator, connectionPool);
        }

        bool GetUpgradeAndConnectionPool(out StreamUpgradeProvider upgradeCopy, out ConnectionPool poolCopy)
        {
            if (this.upgrade != null || this.connectionPool != null)
            {
                lock (ThisLock)
                {
                    if (this.upgrade != null || this.connectionPool != null)
                    {
                        upgradeCopy = this.upgrade;
                        poolCopy = this.connectionPool;
                        this.upgrade = null;
                        this.connectionPool = null;
                        return true;
                    }
                }
            }

            upgradeCopy = null;
            poolCopy = null;
            return false;
        }

        protected override void OnAbort()
        {
            StreamUpgradeProvider localUpgrade;
            ConnectionPool localConnectionPool;
            if (GetUpgradeAndConnectionPool(out localUpgrade, out localConnectionPool))
            {
                if (localConnectionPool != null)
                {
                    ReleaseConnectionPool(localConnectionPool, TimeSpan.Zero);
                }

                if (localUpgrade != null)
                {
                    localUpgrade.Abort();
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

        protected override void OnClose(TimeSpan timeout)
        {
            StreamUpgradeProvider localUpgrade;
            ConnectionPool localConnectionPool;

            if (GetUpgradeAndConnectionPool(out localUpgrade, out localConnectionPool))
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                if (localConnectionPool != null)
                {
                    ReleaseConnectionPool(localConnectionPool, timeoutHelper.RemainingTime());
                }

                if (localUpgrade != null)
                {
                    localUpgrade.Close(timeoutHelper.RemainingTime());
                }
            }
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.connectionPool = GetConnectionPool(); // returns an already opened pool
            Fx.Assert(this.connectionPool != null, "ConnectionPool should always be found");
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this.Upgrade, timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        class OpenAsyncResult : AsyncResult
        {
            ICommunicationObject communicationObject;
            static AsyncCallback onOpenComplete = Fx.ThunkCallback(new AsyncCallback(OnOpenComplete));

            public OpenAsyncResult(ICommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = communicationObject;

                if (this.communicationObject == null)
                {
                    this.Complete(true);
                    return;
                }

                IAsyncResult result = this.communicationObject.BeginOpen(timeout, onOpenComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.communicationObject.EndOpen(result);
                    this.Complete(true);
                }
            }

            static void OnOpenComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr.communicationObject.EndOpen(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            StreamUpgradeProvider localUpgrade = this.Upgrade;
            if (localUpgrade != null)
            {
                localUpgrade.Open(timeout);
            }
        }

        protected virtual bool SupportsUpgrade(StreamUpgradeBindingElement upgradeBindingElement)
        {
            return true;
        }

        class CloseAsyncResult : AsyncResult
        {
            ConnectionOrientedTransportChannelFactory<TChannel> parent;
            ConnectionPool connectionPool;
            StreamUpgradeProvider upgradeProvider;
            TimeoutHelper timeoutHelper;
            static AsyncCallback onCloseComplete = Fx.ThunkCallback(new AsyncCallback(OnCloseComplete));
            static Action<object> onReleaseConnectionPoolScheduled;

            public CloseAsyncResult(ConnectionOrientedTransportChannelFactory<TChannel> parent, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.timeoutHelper = new TimeoutHelper(timeout);

                this.parent.GetUpgradeAndConnectionPool(out this.upgradeProvider, out this.connectionPool);

                if (this.connectionPool == null)
                {
                    if (this.HandleReleaseConnectionPoolComplete())
                    {
                        this.Complete(true);
                    }
                }
                else
                {
                    if (onReleaseConnectionPoolScheduled == null)
                    {
                        onReleaseConnectionPoolScheduled = new Action<object>(OnReleaseConnectionPoolScheduled);
                    }
                    ActionItem.Schedule(onReleaseConnectionPoolScheduled, this);
                }
            }

            bool HandleReleaseConnectionPoolComplete()
            {
                if (this.upgradeProvider == null)
                {
                    return true;
                }
                else
                {
                    IAsyncResult result = this.upgradeProvider.BeginClose(this.timeoutHelper.RemainingTime(),
                        onCloseComplete, this);

                    if (result.CompletedSynchronously)
                    {
                        this.upgradeProvider.EndClose(result);
                        return true;
                    }
                }
                return false;
            }

            bool OnReleaseConnectionPoolScheduled()
            {
                this.parent.ReleaseConnectionPool(this.connectionPool, this.timeoutHelper.RemainingTime());
                return this.HandleReleaseConnectionPoolComplete();
            }

            static void OnReleaseConnectionPoolScheduled(object state)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)state;
                bool completeSelf;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.OnReleaseConnectionPoolScheduled();
                }
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
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnCloseComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr.upgradeProvider.EndClose(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }
    }
}
