//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Xml;

    interface IAnnouncementServiceImplementation
    {
        bool IsDuplicate(UniqueId messageId);

        IAsyncResult OnBeginOnlineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state);
        void OnEndOnlineAnnouncement(IAsyncResult result);

        IAsyncResult OnBeginOfflineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state);
        void OnEndOfflineAnnouncement(IAsyncResult result);
    }
}
