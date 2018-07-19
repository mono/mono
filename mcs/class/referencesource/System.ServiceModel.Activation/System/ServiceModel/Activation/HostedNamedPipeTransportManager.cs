//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    class HostedNamedPipeTransportManager : NamedPipeTransportManager
    {
        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool settingsApplied;
        Action<Uri> onViaCallback;
        SharedConnectionListener listener;
        ConnectionDemuxer connectionDemuxer;
        int queueId;
        Guid token;
        Func<Uri, int> onDuplicatedViaCallback;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool demuxerCreated;

        public HostedNamedPipeTransportManager(BaseUriWithWildcard baseAddress)
            : base(baseAddress.BaseAddress)
        {
            this.HostNameComparisonMode = baseAddress.HostNameComparisonMode;
            this.onViaCallback = new Action<Uri>(OnVia);
            this.onDuplicatedViaCallback = new Func<Uri, int>(OnDuplicatedVia);
        }

        protected override bool IsCompatible(NamedPipeChannelListener channelListener)
        {
            if (channelListener.HostedVirtualPath == null)
            {
                return false;
            }

            return base.IsCompatible(channelListener);
        }

        internal void Start(int queueId, Guid token, Action messageReceivedCallback)
        {
            SetMessageReceivedCallback(messageReceivedCallback);
            OnOpenInternal(queueId, token);
        }

        internal override void OnOpen()
        {
            // This is intentionally empty.
        }

        internal override void OnAbort()
        {
        }

        internal void Stop(TimeSpan timeout)
        {
            Cleanup(false, timeout);
        }

        void Cleanup(bool aborting, TimeSpan timeout)
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
                settingsApplied = false;
            }
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

            connectionDemuxer.StartDemuxing(onViaCallback);
        }

        void OnOpenInternal(int queueId, Guid token)
        {
            lock (ThisLock)
            {
                this.queueId = queueId;
                this.token = token;

                BaseUriWithWildcard path = new BaseUriWithWildcard(this.ListenUri, this.HostNameComparisonMode);

                listener = new SharedConnectionListener(path, queueId, token, this.onDuplicatedViaCallback);
            }
        }

        internal override void OnClose(TimeSpan timeout)
        {
        }

        void OnVia(Uri address)
        {
            Debug.Print("HostedNamedPipeTransportManager.OnVia() address: " + address + " calling EnsureServiceAvailable()");
            ServiceHostingEnvironment.EnsureServiceAvailable(address.LocalPath);
        }

        protected override void OnSelecting(NamedPipeChannelListener channelListener)
        {
            if (settingsApplied)
            {
                return;
            }

            lock (ThisLock)
            {
                if (settingsApplied)
                {
                    // Use the setting for the first one.
                    return;
                }

                this.ApplyListenerSettings(channelListener);
                settingsApplied = true;
            }
        }

        // This method is called only for the first via of the current proxy.
        int OnDuplicatedVia(Uri via)
        {
            OnVia(via);
            
            if (!demuxerCreated)
            {
                lock (ThisLock)
                {
                    if (listener == null)
                    {
                        // The listener has been stopped.
                        throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.PipeListenerProxyStopped));
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
    }
}
