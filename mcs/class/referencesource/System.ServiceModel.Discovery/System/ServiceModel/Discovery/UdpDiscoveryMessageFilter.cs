//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Runtime.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;

    // This class provides a mitigration to the DDOS threat when using Discovery APIs with 
    // UDP multicast transport.
    //
    // The Probe and Resolve request are sent multicast and are not secure. An attacker can launch 
    // a third party distributed DOS attack by setting the address of the third party in the ReplyTo
    // header of the Probe and Resolve requests. To mitigate this threat this behavior drops the 
    // message that have ReplyTo set to a value that is not annonymous by setting appropriate 
    // message filter.
    //
    class UdpDiscoveryMessageFilter : MessageFilter
    {
        MessageFilter innerFilter;

        public UdpDiscoveryMessageFilter(MessageFilter innerFilter)
        {
            if (innerFilter == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerFilter");
            }

            this.innerFilter = innerFilter;
        }

        public MessageFilter InnerFilter
        {
            get
            {
                return this.innerFilter;
            }
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }

            if (InnerFilter.Match(message))
            {
                bool isMatch = ((message.Headers.ReplyTo == null) ||
                    (message.Headers.ReplyTo.IsAnonymous));

                if (!isMatch && TD.DiscoveryMessageWithInvalidReplyToIsEnabled())
                {
                    EventTraceActivity eventTraceActivity = null;
                    if (Fx.Trace.IsEtwProviderEnabled)
                    {
                        eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                    }

                    TD.DiscoveryMessageWithInvalidReplyTo(eventTraceActivity, message.Headers.MessageId.ToString());
                }

                return isMatch;
            }

            return false;
        }

        public override bool Match(MessageBuffer buffer)
        {
            if (buffer == null)
            {
                throw FxTrace.Exception.ArgumentNull("buffer");
            }

            return this.Match(buffer.CreateMessage());
        }
    }
}
