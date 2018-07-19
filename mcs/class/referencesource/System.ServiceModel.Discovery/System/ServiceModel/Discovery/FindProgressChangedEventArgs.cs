//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.ComponentModel;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class FindProgressChangedEventArgs : ProgressChangedEventArgs
    {
        EndpointDiscoveryMetadata endpointDiscoveryMetadata;
        DiscoveryMessageSequence messageSequence;

        internal FindProgressChangedEventArgs(int progressPercentage, object userState,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata, DiscoveryMessageSequence messageSequence)
            : base(progressPercentage, userState)
        {
            this.endpointDiscoveryMetadata = endpointDiscoveryMetadata;
            this.messageSequence = messageSequence;
        }

        public EndpointDiscoveryMetadata EndpointDiscoveryMetadata
        {
            get
            {
                return this.endpointDiscoveryMetadata;
            }
        }

        public DiscoveryMessageSequence MessageSequence
        {
            get
            {
                return this.messageSequence;
            }
        }
    }
}
