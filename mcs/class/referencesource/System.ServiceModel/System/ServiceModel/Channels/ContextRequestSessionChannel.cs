//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextRequestSessionChannel : ContextRequestChannelBase<IRequestSessionChannel>, IRequestSessionChannel
    {
        public ContextRequestSessionChannel(ChannelManagerBase channelManager, IRequestSessionChannel innerChannel,
            ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled)
            : base(channelManager, innerChannel, contextExchangeMechanism, callbackAddress, contextManagementEnabled)
        {
        }

        public IOutputSession Session
        {
            get { return this.InnerChannel.Session; }
        }
    }
}
