//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;

    interface IDiscoveryRequestContext
    {
        TimeoutHelper TimeoutHelper { get; }
        ServiceDiscoveryMode DiscoveryMode { get; }

        IAsyncResult BeginSendFindResponse(Collection<EndpointDiscoveryMetadata> matchingEndpoints, AsyncCallback callback, object state);
        void EndSendFindResponse(IAsyncResult result);

        IAsyncResult BeginSendResolveResponse(EndpointDiscoveryMetadata matchingEndpoint, AsyncCallback callback, object state);
        void EndSendResolveResponse(IAsyncResult result);

        IAsyncResult BeginSendProxyAnnouncements(Collection<EndpointDiscoveryMetadata> proxyAnnouncementEndpoints, AsyncCallback callback, object state);
        void EndSendProxyAnnouncements(IAsyncResult result);        
    }

}
