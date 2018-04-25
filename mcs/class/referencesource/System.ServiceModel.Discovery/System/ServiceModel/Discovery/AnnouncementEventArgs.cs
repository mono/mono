//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class AnnouncementEventArgs : EventArgs
    {
        internal AnnouncementEventArgs(
            DiscoveryMessageSequence messageSequence, 
            EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            this.MessageSequence = messageSequence;
            this.EndpointDiscoveryMetadata = endpointDiscoveryMetadata;
        }

        public DiscoveryMessageSequence MessageSequence { get; private set; }

        public EndpointDiscoveryMetadata EndpointDiscoveryMetadata { get; private set; }
    }
}
