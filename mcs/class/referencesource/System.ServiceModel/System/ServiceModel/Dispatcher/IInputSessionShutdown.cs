//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    public interface IInputSessionShutdown
    {
        void ChannelFaulted(IDuplexContextChannel channel);
        void DoneReceiving(IDuplexContextChannel channel);
    }
}
