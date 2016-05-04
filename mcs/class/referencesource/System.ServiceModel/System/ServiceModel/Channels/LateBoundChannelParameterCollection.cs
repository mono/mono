//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;

    class LateBoundChannelParameterCollection : ChannelParameterCollection
    {
        IChannel channel;

        protected override IChannel Channel
        {
            get { return this.channel; }
        }

        internal void SetChannel(IChannel channel)
        {
            this.channel = channel;
        }
    }
}
