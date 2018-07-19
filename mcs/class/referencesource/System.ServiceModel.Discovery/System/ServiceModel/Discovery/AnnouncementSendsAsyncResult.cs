//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Xml;
    using System.Runtime;
    using System.Collections.ObjectModel;


    class AnnouncementSendsAsyncResult : RandomDelaySendsAsyncResult
    {
        AnnouncementClient announcementClient;
        Collection<EndpointDiscoveryMetadata> publishedEndpoints;
        Collection<UniqueId> messageIds;
        bool online;

        internal AnnouncementSendsAsyncResult(
            AnnouncementClient announcementClient, 
            Collection<EndpointDiscoveryMetadata> publishedEndpoints, 
            Collection<UniqueId> messageIds,
            bool online,
            TimeSpan maxDelay,
            Random random,
            AsyncCallback callback,
            object state)
            : base(publishedEndpoints.Count, maxDelay, announcementClient, random, callback, state)
        {
            Fx.Assert(publishedEndpoints.Count == messageIds.Count, "There must be one message Ids for each EndpointDiscoveryMetadata.");
            this.announcementClient = announcementClient;
            this.publishedEndpoints = publishedEndpoints;
            this.messageIds = messageIds;
            this.online = online;
        }

        protected override IAsyncResult OnBeginSend(int index, TimeSpan timeout, AsyncCallback callback, object state)
        {
            using (new OperationContextScope(this.announcementClient.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.MessageId = this.messageIds[index];

                if (this.online)
                {
                    return this.announcementClient.BeginAnnounceOnline(this.publishedEndpoints[index], callback, state);
                }
                else
                {
                    return this.announcementClient.BeginAnnounceOffline(this.publishedEndpoints[index], callback, state);
                }
            }
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            if (this.online)
            {
                this.announcementClient.EndAnnounceOnline(result);
            }
            else
            {
                this.announcementClient.EndAnnounceOffline(result);
            }
        }
        public static void End(IAsyncResult result)
        {
            AsyncResult.End<AnnouncementSendsAsyncResult>(result);
        }
    }
}
