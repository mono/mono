//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextOutputChannel : ContextOutputChannelBase<IOutputChannel>, IOutputChannel
    {
        ClientContextProtocol contextProtocol;

        public ContextOutputChannel(ChannelManagerBase channelManager, IOutputChannel innerChannel,
            ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled)
            : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ClientContextProtocol(contextExchangeMechanism, this.InnerChannel.Via, this, callbackAddress, contextManagementEnabled);
        }

        protected override ContextProtocol ContextProtocol
        {
            get { return this.contextProtocol; }
        }

        protected override bool IsClient
        {
            get { return true; }
        }
    }
}
