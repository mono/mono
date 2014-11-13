//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel.Dispatcher;

    sealed class BufferedReceiveMessageProperty
    {
        const string PropertyName = "BufferedReceiveMessageProperty";
        MessageBuffer messageBuffer;
        static MessageBuffer dummyMessageBuffer = BufferedMessage.CreateMessage(MessageVersion.Default, string.Empty).CreateBufferedCopy(1);

        internal BufferedReceiveMessageProperty(ref MessageRpc rpc)
        {
            this.RequestContext = new BufferedRequestContext(rpc.RequestContext);
            rpc.RequestContext = this.RequestContext;
            this.Notification = rpc.InvokeNotification;
        }

        public static string Name
        {
            get { return PropertyName; }
        }

        // Implementation specifc storage
        public object UserState
        {
            get;
            set;
        }

        // The original RequestContext that was created by the ChannelHandler
        public BufferedRequestContext RequestContext
        {
            get;
            private set;
        }

        // The 'Manual Concurrency' notification which allows higher layers to notify the dispatcher of an event
        // e.g. The event associated with re-pumping the above buffered RequestContext again
        internal IInvokeReceivedNotification Notification
        {
            get;
            private set;
        }

        public void RegisterForReplay(OperationContext operationContext)
        {
            this.messageBuffer = (MessageBuffer)operationContext.IncomingMessageProperties[ChannelHandler.MessageBufferPropertyName];
            // cannot remove the MessageBufferProperty from messageProperties because it causes the message buffer associated with the property
            // to be disposed of.  Assigning null to the property has the same effect, so we assign dummyMessageBuffer to the property.
            operationContext.IncomingMessageProperties[ChannelHandler.MessageBufferPropertyName] = dummyMessageBuffer;
        }

        public void ReplayRequest()
        {
            Message requestMessage = this.messageBuffer.CreateMessage();
            // re-injecting the MessageBufferProperty here so that it can be reused by the dispatch layer 
            // (ChannelHandler.DispatchAndReleasePump to be specific) instead of recreating the buffer for replay.
            requestMessage.Properties[ChannelHandler.MessageBufferPropertyName] = this.messageBuffer;
            this.RequestContext.ReInitialize(requestMessage);
        }

        public static bool TryGet(Message message, out BufferedReceiveMessageProperty property)
        {
            Fx.Assert(message != null, "The Message parameter is null");

            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out BufferedReceiveMessageProperty property)
        {
            Fx.Assert(properties != null, "The MessageProperties parameter is null");

            object value = null;
            if (properties.TryGetValue(PropertyName, out value))
            {
                property = value as BufferedReceiveMessageProperty;
            }
            else
            {
                property = null;
            }
            return property != null;
        }
    }
}
