//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;
    using System.Xml;

    interface IDiscoveryInnerClientResponse
    {
        void HelloOperation(UniqueId relatesTo, DiscoveryMessageSequence proxyMessageSequence, EndpointDiscoveryMetadata proxyEndpointMetadata);
        void ProbeMatchOperation(UniqueId relatesTo, DiscoveryMessageSequence discoveryMessageSequence, Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadataCollection, bool findCompleted);
        void ResolveMatchOperation(UniqueId relatesTo, DiscoveryMessageSequence discoveryMessageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata);
        void PostFindCompletedAndRemove(UniqueId operationId, bool cancelled, Exception error);
        void PostResolveCompletedAndRemove(UniqueId operationId, bool cancelled, Exception error);
    }
}
