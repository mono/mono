//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextInputChannel : ContextInputChannelBase<IInputChannel>, IInputChannel
    {
        public ContextInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism)
            : base(channelManager, innerChannel, contextExchangeMechanism)
        {
        }
    }
}
