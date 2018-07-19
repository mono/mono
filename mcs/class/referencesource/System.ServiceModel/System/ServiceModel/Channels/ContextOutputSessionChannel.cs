//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextOutputSessionChannel : ContextOutputChannelBase<IOutputSessionChannel>, IOutputSessionChannel
    {
        ClientContextProtocol contextProtocol;

        public ContextOutputSessionChannel(ChannelManagerBase channelManager, IOutputSessionChannel innerChannel,
            ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled)
            : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ClientContextProtocol(contextExchangeMechanism, this.InnerChannel.Via, this, callbackAddress, contextManagementEnabled);
        }

        public IOutputSession Session
        {
            get { return this.InnerChannel.Session; }
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
