//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    class DiscoveryOperationContext
    {
        [Fx.Tag.SynchronizationObject]
        readonly object thisLock;
        readonly OperationContext operationContext;
        readonly DiscoveryOperationContextExtension operationContextExtension;
        readonly DiscoveryMessageProperty messageProperty;

        MessageHeaders outgoingMessageHeaders;
        EventTraceActivity eventTraceActivity;

        public DiscoveryOperationContext(OperationContext operationContext)
        {
            Fx.Assert(operationContext != null, "The operationContext must be non null.");

            if (Fx.Trace.IsEtwProviderEnabled)
            {
                this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(operationContext.IncomingMessage);
            }

            this.operationContext = operationContext;
            this.operationContextExtension = DiscoveryOperationContext.GetDiscoveryOperationContextExtension(this.operationContext);
            this.messageProperty = DiscoveryOperationContext.GetDiscoveryMessageProperty(this.operationContext);

            this.thisLock = new object();
        }

        public ServiceDiscoveryMode DiscoveryMode
        {
            get
            {
                return this.operationContextExtension.DiscoveryMode;
            }
        }

        public EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.eventTraceActivity;
            }
        }

        public TimeSpan MaxResponseDelay
        {
            get
            {
                return this.operationContextExtension.MaxResponseDelay;
            }
        }

        public TResponseChannel GetCallbackChannel<TResponseChannel>()
        {
            return this.operationContext.GetCallbackChannel<TResponseChannel>();
        }

        public void AddressDuplexResponseMessage(OperationContext responseOperationContext)
        {
            EnsureOutgoingMessageHeaders();
            responseOperationContext.OutgoingMessageHeaders.CopyHeadersFrom(this.outgoingMessageHeaders);
            responseOperationContext.OutgoingMessageHeaders.MessageId = new UniqueId();
            this.AddDiscoveryMessageProperty(responseOperationContext);
        }

        public void AddressRequestResponseMessage(OperationContext responseOperationContext)
        {
            responseOperationContext.OutgoingMessageHeaders.MessageId = new UniqueId();
            this.AddDiscoveryMessageProperty(responseOperationContext);
        }

        static DiscoveryOperationContextExtension GetDiscoveryOperationContextExtension(OperationContext operationContext)
        {
            DiscoveryOperationContextExtension operationContextExtension =
                operationContext.Extensions.Find<DiscoveryOperationContextExtension>();

            if (operationContextExtension == null)
            {
                operationContextExtension = new DiscoveryOperationContextExtension();
            }

            return operationContextExtension;
        }

        static DiscoveryMessageProperty GetDiscoveryMessageProperty(OperationContext operationContext)
        {
            object messageProperty;
            if (operationContext.IncomingMessageProperties.TryGetValue(DiscoveryMessageProperty.Name, out messageProperty))
            {
                return messageProperty as DiscoveryMessageProperty;
            }
            else
            {
                return null;
            }
        }

        static MessageHeaders GetOutgoingMessageHeaders(OperationContext operationContext)
        {
            MessageHeaders outgoingMessageHeaders = new MessageHeaders(operationContext.IncomingMessageVersion);

            EndpointAddress replyTo = operationContext.IncomingMessageHeaders.ReplyTo;
            if (replyTo != null)
            {
                outgoingMessageHeaders.To = replyTo.Uri;
                foreach (AddressHeader addrHeader in replyTo.Headers)
                {
                    outgoingMessageHeaders.Add(addrHeader.ToMessageHeader());
                }
            }

            outgoingMessageHeaders.RelatesTo = operationContext.IncomingMessageHeaders.MessageId;

            return outgoingMessageHeaders;
        }

        void AddDiscoveryMessageProperty(OperationContext responseOperationContext)
        {
            if (this.messageProperty != null)
            {
                responseOperationContext.OutgoingMessageProperties.Add(
                    DiscoveryMessageProperty.Name,
                    this.messageProperty);
            }
        }

        void EnsureOutgoingMessageHeaders()
        {
            if (this.outgoingMessageHeaders == null)
            {
                lock (this.thisLock)
                {
                    if (this.outgoingMessageHeaders == null)
                    {
                        this.outgoingMessageHeaders = DiscoveryOperationContext.GetOutgoingMessageHeaders(this.operationContext);
                    }
                }
            }
        }
    }
}
