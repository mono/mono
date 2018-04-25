//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System.Security.Principal;

    sealed class ExclusiveNamedPipeTransportManager : NamedPipeTransportManager
    {
        ConnectionDemuxer connectionDemuxer;
        IConnectionListener connectionListener;

        public ExclusiveNamedPipeTransportManager(Uri listenUri, NamedPipeChannelListener channelListener)
            : base(listenUri)
        {
            ApplyListenerSettings(channelListener);
            SetHostNameComparisonMode(channelListener.HostNameComparisonMode);
            SetAllowedUsers(channelListener.AllowedUsers);
        }

        internal override void OnOpen()
        {
            connectionListener = new BufferedConnectionListener(
                new PipeConnectionListener(ListenUri, HostNameComparisonMode, ConnectionBufferSize,
                    AllowedUsers, true, int.MaxValue),
                    MaxOutputDelay, ConnectionBufferSize);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                connectionListener = new TracingConnectionListener(connectionListener, this.ListenUri.ToString(), false);
            }

            connectionDemuxer = new ConnectionDemuxer(connectionListener,
                MaxPendingAccepts, MaxPendingConnections, ChannelInitializationTimeout,
                IdleTimeout, MaxPooledConnections,
                OnGetTransportFactorySettings,
                OnGetSingletonMessageHandler,
                OnHandleServerSessionPreamble,
                OnDemuxerError);

            bool startedDemuxing = false;
            try
            {
                connectionDemuxer.StartDemuxing();
                startedDemuxing = true;
            }
            finally
            {
                if (!startedDemuxing)
                {
                    connectionDemuxer.Dispose();
                }
            }
        }

        internal override void OnClose(TimeSpan timeout)
        {
            connectionDemuxer.Dispose();
            connectionListener.Dispose();
            base.OnClose(timeout);
        }

        internal override void OnAbort()
        {
            if (connectionDemuxer != null)
            {
                connectionDemuxer.Dispose();
            }

            if (connectionListener != null)
            {
                connectionListener.Dispose();
            }

            base.OnAbort();
        }
    }
}
