//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.ComponentModel;
    using System.ServiceModel.Description;

    interface IAnnouncementInnerClient
    {
        event EventHandler<AsyncCompletedEventArgs> HelloOperationCompleted;
        event EventHandler<AsyncCompletedEventArgs> ByeOperationCompleted;

        DiscoveryMessageSequenceGenerator DiscoveryMessageSequenceGenerator { get; set; }
        ClientCredentials ClientCredentials { get; }
        ChannelFactory ChannelFactory { get; }
        IClientChannel InnerChannel { get; }
        ServiceEndpoint Endpoint { get; }
        ICommunicationObject InnerCommunicationObject { get; }

        IAsyncResult BeginHelloOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state);
        void EndHelloOperation(IAsyncResult result);

        IAsyncResult BeginByeOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state);
        void EndByeOperation(IAsyncResult result);

        void HelloOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata);
        void ByeOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata);

        void HelloOperationAsync(EndpointDiscoveryMetadata endpointDiscoveryMetadata, object userState);
        void ByeOperationAsync(EndpointDiscoveryMetadata endpointDiscoveryMetadata, object userState);
    }
}
