//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;

    abstract class TcpTransportManager : ConnectionOrientedTransportManager<TcpChannelListener>
    {
        internal TcpTransportManager()
        {
        }

        internal override string Scheme
        {
            get { return Uri.UriSchemeNetTcp; }
        }

        protected virtual bool IsCompatible(TcpChannelListener channelListener)
        {
            return base.IsCompatible(channelListener);
        }
    }
}
