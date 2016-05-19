//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextInputSessionChannel : ContextInputChannelBase<IInputSessionChannel>, IInputSessionChannel
    {
        public ContextInputSessionChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism)
            : base(channelManager, innerChannel, contextExchangeMechanism)
        {
        }

        public IInputSession Session
        {
            get { return this.InnerChannel.Session; }
        }
    }
}
