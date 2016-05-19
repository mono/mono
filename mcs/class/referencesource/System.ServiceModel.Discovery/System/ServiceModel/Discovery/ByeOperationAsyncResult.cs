//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xml;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    
    abstract class ByeOperationAsyncResult<TMessage> : AsyncResult
        where TMessage : class
    {
        static AsyncCompletion onOnOfflineAnnoucementCompletedCallback = 
            new AsyncCompletion(OnOnOfflineAnnouncementCompleted);

        IAnnouncementServiceImplementation announcementServiceImpl;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal ByeOperationAsyncResult(
            IAnnouncementServiceImplementation announcementServiceImpl, 
            TMessage message, 
            AsyncCallback callback, 
            object state)
            : base(callback, state)
        {
            this.announcementServiceImpl = announcementServiceImpl;

            if (this.IsInvalid(message))
            {
                this.Complete(true);
                return;
            }

            IAsyncResult innerAsyncResult =
                this.announcementServiceImpl.OnBeginOfflineAnnouncement(
                this.GetMessageSequence(message),
                this.GetEndpointDiscoveryMetadata(message),
                this.PrepareAsyncCompletion(onOnOfflineAnnoucementCompletedCallback),
                this);

            if (innerAsyncResult.CompletedSynchronously && OnOnOfflineAnnouncementCompleted(innerAsyncResult))
            {
                this.Complete(true);
                return;
            }
        }

        protected abstract bool ValidateContent(TMessage message);
        protected abstract DiscoveryMessageSequence GetMessageSequence(TMessage message);
        protected abstract EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(TMessage message);

        static bool OnOnOfflineAnnouncementCompleted(IAsyncResult result)
        {
            ByeOperationAsyncResult<TMessage> thisPtr = (ByeOperationAsyncResult<TMessage>)result.AsyncState;
            thisPtr.announcementServiceImpl.OnEndOfflineAnnouncement(result);

            return true;
        }

        bool IsInvalid(TMessage message)
        {
            UniqueId messageId = OperationContext.Current.IncomingMessageHeaders.MessageId;
            if (messageId == null)
            {                
                if (TD.DiscoveryMessageWithNullMessageIdIsEnabled())
                {
                    TD.DiscoveryMessageWithNullMessageId(null, ProtocolStrings.TracingStrings.Bye);
                }

                return true;
            }

            EventTraceActivity eventTraceActivity = null;
            if (Fx.Trace.IsEtwProviderEnabled)
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(OperationContext.Current.IncomingMessage);
            }

            if (this.announcementServiceImpl.IsDuplicate(messageId))
            {
                if (TD.DuplicateDiscoveryMessageIsEnabled())
                {
                    TD.DuplicateDiscoveryMessage(eventTraceActivity, ProtocolStrings.TracingStrings.Bye, messageId.ToString());
                }

                return true;
            }
            else if (this.ValidateContent(message))
            {
                return false;
            }
            else
            {
                if (TD.DiscoveryMessageWithInvalidContentIsEnabled())
                {
                    TD.DiscoveryMessageWithInvalidContent(eventTraceActivity, ProtocolStrings.TracingStrings.Bye, messageId.ToString());
                }

                return true;
            }
        }
    }
}
