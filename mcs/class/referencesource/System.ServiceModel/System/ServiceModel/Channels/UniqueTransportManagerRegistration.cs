//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ServiceModel;

    class UniqueTransportManagerRegistration : TransportManagerRegistration
    {
        List<TransportManager> list;

        public UniqueTransportManagerRegistration(TransportManager uniqueManager, Uri listenUri, HostNameComparisonMode hostNameComparisonMode)
            : base(listenUri, hostNameComparisonMode)
        {
            this.list = new List<TransportManager>();
            this.list.Add(uniqueManager);
        }

        public override IList<TransportManager> Select(TransportChannelListener channelListener)
        {
            return list;
        }
    }
}
