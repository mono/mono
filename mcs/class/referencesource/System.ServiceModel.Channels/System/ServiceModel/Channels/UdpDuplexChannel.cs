//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Xml;

    abstract class UdpDuplexChannel : UdpChannelBase<Message>, IDuplexChannel
    {        
        protected UdpDuplexChannel(
            ChannelManagerBase channelMananger, 
            MessageEncoder encoder, 
            BufferManager bufferManager,
            UdpSocket[] sendSockets, 
            UdpRetransmissionSettings retransmissionSettings,
            long maxPendingMessagesTotalSize, 
            EndpointAddress localAddress, 
            Uri via, 
            bool isMulticast, 
            int maxReceivedMessageSize)
            : base(channelMananger, encoder, bufferManager, sendSockets, retransmissionSettings, maxPendingMessagesTotalSize, localAddress, via, isMulticast, maxReceivedMessageSize)
        {
        }

        public virtual EndpointAddress RemoteAddress
        {
            get { return null; }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IDuplexChannel))
            {
                return (T)(object)this;
            }

            return base.GetProperty<T>();
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfDisposedOrNotOpen();
            
            if (message is NullMessage)
            {
                return new CompletedAsyncResult(callback, state); 
            }
            AddHeadersTo(message);
            return this.UdpOutputChannel.BeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result); 
            }
            else 
            {
                this.UdpOutputChannel.EndSend(result);
            }
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            if (message is NullMessage)
            {
                return;
            }

            this.UdpOutputChannel.Send(message, timeout);
        }

        public Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return InputChannel.HelpReceive(this, timeout);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return InputChannel.HelpBeginReceive(this, timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return InputChannel.HelpEndReceive(result);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return base.Dequeue(timeout, out message);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return base.BeginDequeue(timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return base.EndDequeue(result, out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return base.WaitForItem(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return base.BeginWaitForItem(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return base.EndWaitForItem(result);
        }

        internal override void FinishEnqueueMessage(Message message, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            if (!this.IsMulticast)
            {
                //When using Multicast, we can't assume that receiving one message means that we are done receiving messages.
                //For example, Discovery will send one message out and receive n responses that match.  Because of this, we 
                //can only short circuit retransmission when using unicast.
                this.UdpOutputChannel.CancelRetransmission(message.Headers.RelatesTo);
            }
            this.EnqueueAndDispatch(message, dequeuedCallback, canDispatchOnThisThread);
        }
    }
}
