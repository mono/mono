//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    class DefaultDiscoveryServiceExtension : DiscoveryServiceExtension
    {
        readonly DiscoveryService discoveryService;

        public DefaultDiscoveryServiceExtension(int duplicateMessageHistoryLength)
        {
            this.discoveryService = new DefaultDiscoveryService(
                this,
                new DiscoveryMessageSequenceGenerator(),
                duplicateMessageHistoryLength);
        }

        protected override DiscoveryService GetDiscoveryService()
        {
            return this.discoveryService;   
        }
    }
}
