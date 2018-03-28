//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xml;
    using System.ServiceModel.Diagnostics;
    using System.Runtime.Diagnostics;
    
    abstract class HelloOperationAsyncResult<TMessage> : AsyncResult
        where TMessage : class
    {
        static AsyncCompletion onOnOnlineAnnoucementCompletedCallback = 
            new AsyncCompletion(OnOnOnlineAnnouncementCompleted);

        IAnnouncementServiceImplementation announcementServiceImpl;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal HelloOperationAsyncResult(
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
                this.announcementServiceImpl.OnBeginOnlineAnnouncement(
                this.GetMessageSequence(message),
                this.GetEndpointDiscoveryMetadata(message),
                this.PrepareAsyncCompletion(onOnOnlineAnnoucementCompletedCallback),
                this);

            if (innerAsyncResult.CompletedSynchronously && OnOnOnlineAnnouncementCompleted(innerAsyncResult))
            {
                this.Complete(true);
                return;
            }
        }

        protected abstract bool ValidateContent(TMessage message);
        protected abstract DiscoveryMessageSequence GetMessageSequence(TMessage message);
        protected abstract EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(TMessage message);

        static bool OnOnOnlineAnnouncementCompleted(IAsyncResult result)
        {
            HelloOperationAsyncResult<TMessage> thisPtr = (HelloOperationAsyncResult<TMessage>)result.AsyncState;
            thisPtr.announcementServiceImpl.OnEndOnlineAnnouncement(result);

            return true;
        }

        bool IsInvalid(TMessage message)
        {
            EventTraceActivity eventTraceActivity = null;
            if (Fx.Trace.IsEtwProviderEnabled)
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(OperationContext.Current.IncomingMessage);
            }

            UniqueId messageId = OperationContext.Current.IncomingMessageHeaders.MessageId;
            if (messageId == null)
            {
                if (TD.DiscoveryMessageWithNullMessageIdIsEnabled())
                {
                    TD.DiscoveryMessageWithNullMessageId(eventTraceActivity, ProtocolStrings.TracingStrings.Hello);
                }

                return true;
            }
            else if (this.announcementServiceImpl.IsDuplicate(messageId))
            {
                if (TD.DuplicateDiscoveryMessageIsEnabled())
                {
                    TD.DuplicateDiscoveryMessage(eventTraceActivity, ProtocolStrings.TracingStrings.Hello, messageId.ToString());
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
                    TD.DiscoveryMessageWithInvalidContent(eventTraceActivity, ProtocolStrings.TracingStrings.Hello, messageId.ToString());
                }

                return true;
            }
        }
    }
}
