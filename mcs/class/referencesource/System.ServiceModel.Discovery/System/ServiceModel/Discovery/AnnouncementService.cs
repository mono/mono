//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Xml;
    using System.Runtime;
    using System.ServiceModel.Discovery.Version11;
    using System.ServiceModel.Discovery.VersionApril2005;
    using System.ServiceModel.Discovery.VersionCD1;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AnnouncementService : 
        IAnnouncementContractApril2005, 
        IAnnouncementContract11,
        IAnnouncementContractCD1,
        IAnnouncementServiceImplementation
    {
        DuplicateDetector<UniqueId> duplicateDetector;

        public AnnouncementService()
            : this(DiscoveryDefaults.DuplicateMessageHistoryLength)
        {
        }

        public AnnouncementService(int duplicateMessageHistoryLength)
        {
            if (duplicateMessageHistoryLength < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange(
                    "duplicateMessageHistoryLength", 
                    duplicateMessageHistoryLength, 
                    SR.DiscoveryNegativeDuplicateMessageHistoryLength);
            }

            if (duplicateMessageHistoryLength > 0)
            {
                this.duplicateDetector = new DuplicateDetector<UniqueId>(duplicateMessageHistoryLength);
            }
        }

        public event EventHandler<AnnouncementEventArgs> OnlineAnnouncementReceived;
        public event EventHandler<AnnouncementEventArgs> OfflineAnnouncementReceived;

        void IAnnouncementContractApril2005.HelloOperation(HelloMessageApril2005 message)
        {
            Fx.Assert("The sync method IAnnouncementContractApril2005.HelloOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractApril2005.BeginHelloOperation(HelloMessageApril2005 message, AsyncCallback callback, object state)
        {
            return new HelloOperationApril2005AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractApril2005.EndHelloOperation(IAsyncResult result)
        {
            HelloOperationApril2005AsyncResult.End(result);
        }

        void IAnnouncementContractApril2005.ByeOperation(ByeMessageApril2005 message)
        {
            Fx.Assert("The sync method IAnnouncementContractApril2005.ByeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractApril2005.BeginByeOperation(ByeMessageApril2005 message, AsyncCallback callback, object state)
        {
            return new ByeOperationApril2005AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractApril2005.EndByeOperation(IAsyncResult result)
        {
            ByeOperationApril2005AsyncResult.End(result);
        }

        void IAnnouncementContract11.HelloOperation(HelloMessage11 message)
        {
            Fx.Assert("The sync method IAnnouncementContract11.HelloOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContract11.BeginHelloOperation(HelloMessage11 message, AsyncCallback callback, object state)
        {
            return new HelloOperation11AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContract11.EndHelloOperation(IAsyncResult result)
        {
            HelloOperation11AsyncResult.End(result);
        }

        void IAnnouncementContract11.ByeOperation(ByeMessage11 message)
        {
            Fx.Assert("The sync method IAnnouncementContract11.ByeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContract11.BeginByeOperation(ByeMessage11 message, AsyncCallback callback, object state)
        {
            return new ByeOperation11AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContract11.EndByeOperation(IAsyncResult result)
        {
            ByeOperation11AsyncResult.End(result);
        }

        void IAnnouncementContractCD1.HelloOperation(HelloMessageCD1 message)
        {
            Fx.Assert("The sync method IAnnouncementContractCD1.HelloOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractCD1.BeginHelloOperation(HelloMessageCD1 message, AsyncCallback callback, object state)
        {
            return new HelloOperationCD1AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractCD1.EndHelloOperation(IAsyncResult result)
        {
            HelloOperationCD1AsyncResult.End(result);
        }

        void IAnnouncementContractCD1.ByeOperation(ByeMessageCD1 message)
        {
            Fx.Assert("The sync method IAnnouncementContractCD1.ByeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractCD1.BeginByeOperation(ByeMessageCD1 message, AsyncCallback callback, object state)
        {
            return new ByeOperationCD1AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractCD1.EndByeOperation(IAsyncResult result)
        {
            ByeOperationCD1AsyncResult.End(result);
        }

        bool IAnnouncementServiceImplementation.IsDuplicate(UniqueId messageId)
        {
            return (this.duplicateDetector != null) && (!this.duplicateDetector.AddIfNotDuplicate(messageId));
        }

        IAsyncResult IAnnouncementServiceImplementation.OnBeginOnlineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            return this.OnBeginOnlineAnnouncement(messageSequence, endpointDiscoveryMetadata, callback, state);
        }

        void IAnnouncementServiceImplementation.OnEndOnlineAnnouncement(IAsyncResult result)
        {
            this.OnEndOnlineAnnouncement(result);
        }

        IAsyncResult IAnnouncementServiceImplementation.OnBeginOfflineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            return this.OnBeginOfflineAnnouncement(messageSequence, endpointDiscoveryMetadata, callback, state);
        }

        void IAnnouncementServiceImplementation.OnEndOfflineAnnouncement(IAsyncResult result)
        {
            this.OnEndOfflineAnnouncement(result);
        }

        protected virtual IAsyncResult OnBeginOnlineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            EventHandler<AnnouncementEventArgs> handler = this.OnlineAnnouncementReceived;

            if (handler != null)
            {
                handler(this, new AnnouncementEventArgs(messageSequence, endpointDiscoveryMetadata));
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected virtual void OnEndOnlineAnnouncement(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected virtual IAsyncResult OnBeginOfflineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {

            EventHandler<AnnouncementEventArgs> handler = this.OfflineAnnouncementReceived;

            if (handler != null)
            {
                handler(this, new AnnouncementEventArgs(messageSequence, endpointDiscoveryMetadata));
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected virtual void OnEndOfflineAnnouncement(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }
    }
}
