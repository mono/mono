//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Activation;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    using System.ServiceModel.Diagnostics;

    class SharedTcpTransportManager : TcpTransportManager, ITransportManagerRegistration
    {
        SharedConnectionListener listener;
        ConnectionDemuxer connectionDemuxer;
        HostNameComparisonMode hostNameComparisonMode;
        Uri listenUri;
        int queueId;
        Guid token;
        Func<Uri, int> onDuplicatedViaCallback;
        bool demuxerCreated;

        public SharedTcpTransportManager(Uri listenUri, TcpChannelListener channelListener)
        {
            this.HostNameComparisonMode = channelListener.HostNameComparisonMode;
            this.listenUri = listenUri;

            // For port sharing, we apply all of the settings from channel listener to the transport manager.
            this.ApplyListenerSettings(channelListener);
        }

        protected SharedTcpTransportManager(Uri listenUri)
        {
            this.listenUri = listenUri;
        }

        protected override bool IsCompatible(TcpChannelListener channelListener)
        {
            if (channelListener.HostedVirtualPath == null && !channelListener.PortSharingEnabled)
            {
                return false;
            }

            return base.IsCompatible(channelListener);
        }

        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
            set
            {
                HostNameComparisonModeHelper.Validate(value);
                lock (base.ThisLock)
                {
                    ThrowIfOpen();
                    this.hostNameComparisonMode = value;
                }
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        internal override void OnOpen()
        {
            OnOpenInternal(0, Guid.Empty);
        }

        protected virtual Action<Uri> GetOnViaCallback()
        {
            return null;
        }

        // This method is called only for the first via of the current proxy.
        int OnDuplicatedVia(Uri via)
        {
            Action<Uri> onVia = GetOnViaCallback();
            if (onVia != null)
            {
                onVia(via);
            }

            if (!demuxerCreated)
            {
                lock (ThisLock)
                {
                    if (listener == null)
                    {
                        // The listener has been stopped.
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(
                            SR.GetString(SR.Sharing_ListenerProxyStopped)));
                    }

                    if (!demuxerCreated)
                    {
                        CreateConnectionDemuxer();
                        demuxerCreated = true;
                    }
                }
            }

            return this.ConnectionBufferSize;
        }

        void CreateConnectionDemuxer()
        {
            IConnectionListener connectionListener = new BufferedConnectionListener(listener, MaxOutputDelay, ConnectionBufferSize);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                connectionListener = new TracingConnectionListener(connectionListener, this.ListenUri);
            }

            connectionDemuxer = new ConnectionDemuxer(connectionListener,
                MaxPendingAccepts, MaxPendingConnections, ChannelInitializationTimeout,
                IdleTimeout, MaxPooledConnections,
                OnGetTransportFactorySettings,
                OnGetSingletonMessageHandler,
                OnHandleServerSessionPreamble,
                OnDemuxerError);
            connectionDemuxer.StartDemuxing(this.GetOnViaCallback());
        }

        internal void OnOpenInternal(int queueId, Guid token)
        {
            lock (ThisLock)
            {
                this.queueId = queueId;
                this.token = token;

                BaseUriWithWildcard path = new BaseUriWithWildcard(this.ListenUri, this.HostNameComparisonMode);

                if (this.onDuplicatedViaCallback == null)
                {
                    this.onDuplicatedViaCallback = new Func<Uri, int>(OnDuplicatedVia);
                }

                listener = new SharedConnectionListener(path, queueId, token, this.onDuplicatedViaCallback);

                // Delay the creation of the demuxer on the first request.
            }
        }

        protected void CleanUp(bool aborting, TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (listener != null)
                {
                    if (!aborting)
                    {
                        listener.Stop(timeout);
                    }
                    else
                    {
                        listener.Abort();
                    }

                    // The listener will be closed by the demuxer.
                    listener = null;
                }

                if (connectionDemuxer != null)
                {
                    connectionDemuxer.Dispose();
                }

                demuxerCreated = false;
            }
        }

        void Unregister()
        {
            TcpChannelListener.StaticTransportManagerTable.UnregisterUri(this.ListenUri, this.HostNameComparisonMode);
        }

        internal override void OnAbort()
        {
            CleanUp(true, TimeSpan.Zero);
            Unregister();
            base.OnAbort();
        }

        internal override void OnClose(TimeSpan timeout)
        {
            CleanUp(false, timeout);
            Unregister();
        }

        protected virtual void OnSelecting(TcpChannelListener channelListener)
        {
        }

        IList<TransportManager> ITransportManagerRegistration.Select(TransportChannelListener channelListener)
        {
            if (!channelListener.IsScopeIdCompatible(this.hostNameComparisonMode, this.listenUri))
            {
                return null;
            }

            OnSelecting((TcpChannelListener)channelListener);

            IList<TransportManager> result = null;
            if (this.IsCompatible((TcpChannelListener)channelListener))
            {
                result = new List<TransportManager>();
                result.Add(this);
            }
            return result;
        }
    }
}
