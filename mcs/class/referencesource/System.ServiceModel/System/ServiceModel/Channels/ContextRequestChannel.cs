//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    class ContextRequestChannel : ContextRequestChannelBase<IRequestChannel>, IRequestChannel
    {
        public ContextRequestChannel(ChannelManagerBase channelManager, IRequestChannel innerChannel,
            ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled)
            : base(channelManager, innerChannel, contextExchangeMechanism, callbackAddress, contextManagementEnabled)
        {
        }
    }
}
