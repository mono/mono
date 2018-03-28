//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.ServiceModel.Description;

    interface IDiscoveryInnerClient
    {
        ClientCredentials ClientCredentials { get; }
        ChannelFactory ChannelFactory { get; }
        IClientChannel InnerChannel { get; }
        ServiceEndpoint Endpoint { get; }
        ICommunicationObject InnerCommunicationObject { get; }
        bool IsRequestResponse { get; }

        // The response is sent to DiscoveryClient through the IDiscoveryInnerClientResponse 
        // interface (even in request-response MEP)
        void ProbeOperation(FindCriteria findCriteria);
        void ResolveOperation(ResolveCriteria resolveCriteria);

        IAsyncResult BeginProbeOperation(FindCriteria findCriteria, AsyncCallback callback, object state);
        IAsyncResult BeginResolveOperation(ResolveCriteria resolveCriteria, AsyncCallback callback, object state);

        void EndProbeOperation(IAsyncResult result);
        void EndResolveOperation(IAsyncResult result);
    }
}
