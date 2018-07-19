// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;

    internal sealed class UdpReplyChannel : UdpChannelBase<RequestContext>, IReplyChannel
    {
        public UdpReplyChannel(UdpReplyChannelListener listener, UdpSocket[] sockets, EndpointAddress localAddress, Uri via, bool isMulticast)
            : base(
            listener, 
            listener.MessageEncoderFactory.Encoder, 
            listener.BufferManager,
            sockets, 
            listener.UdpTransportBindingElement.RetransmissionSettings,
            listener.UdpTransportBindingElement.MaxPendingMessagesTotalSize,
            localAddress,
            via,
            isMulticast,
            (int)listener.UdpTransportBindingElement.MaxReceivedMessageSize)
        {
            UdpOutputChannel udpOutputChannel = new ServerUdpOutputChannel(
                listener,
                listener.MessageEncoderFactory.Encoder,
                listener.BufferManager,
                sockets,
                listener.UdpTransportBindingElement.RetransmissionSettings,
                via,
                isMulticast);

            this.SetOutputChannel(udpOutputChannel);
        }

        protected override bool IgnoreSerializationException
        {
            get
            {
                return true;
            }
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1100:DoNotPrefixCallsWithBaseUnlessLocalImplementationExists", Justification = "StyleCop 4.5 does not validate this rule properly.")]
        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IReplyChannel))
            {
                return (T)(object)this;
            }

            return base.GetProperty<T>();
        }

        public RequestContext ReceiveRequest()
        {
            return this.ReceiveRequest(this.DefaultReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return UdpReplyChannel.HelpReceiveRequest(this, timeout);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return this.BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return UdpReplyChannel.HelpBeginReceiveRequest(this, timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return UdpReplyChannel.HelpEndReceiveRequest(result);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return this.Dequeue(timeout, out context);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return this.BeginDequeue(timeout, callback, state);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            return this.EndDequeue(result, out context);
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return this.WaitForItem(timeout);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("timeout", timeout, SR.TimeoutOutOfRange0));
            }

            this.ThrowPending();
            return this.BeginWaitForItem(timeout, callback, state);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return this.EndWaitForItem(result);
        }

        internal override void FinishEnqueueMessage(Message message, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            UdpRequestContext udpRequestContext = new UdpRequestContext(this.UdpOutputChannel, message);
            this.EnqueueAndDispatch(udpRequestContext, dequeuedCallback, canDispatchOnThisThread);
        }

        private static RequestContext HelpReceiveRequest(IReplyChannel channel, TimeSpan timeout)
        {
            RequestContext requestContext;
            if (channel.TryReceiveRequest(timeout, out requestContext))
            {
                return requestContext;
            }
            else
            {
                throw FxTrace.Exception.AsError(UdpReplyChannel.CreateReceiveRequestTimedOutException(channel, timeout));
            }
        }

        private static IAsyncResult HelpBeginReceiveRequest(IReplyChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new HelpReceiveRequestAsyncResult(channel, timeout, callback, state);
        }

        private static RequestContext HelpEndReceiveRequest(IAsyncResult result)
        {
            return HelpReceiveRequestAsyncResult.End(result);
        }

        private static Exception CreateReceiveRequestTimedOutException(IReplyChannel channel, TimeSpan timeout)
        {
            if (channel.LocalAddress != null)
            {
                return new TimeoutException(SR.ReceiveRequestTimedOut(channel.LocalAddress.Uri.AbsoluteUri, timeout));
            }
            else
            {
                return new TimeoutException(SR.ReceiveRequestTimedOutNoLocalAddress(timeout));
            }
        }

        private class HelpReceiveRequestAsyncResult : AsyncResult
        {
            private static AsyncCallback onReceiveRequest = Fx.ThunkCallback(new AsyncCallback(OnReceiveRequest));
            private IReplyChannel channel;
            private TimeSpan timeout;
            private RequestContext requestContext;

            public HelpReceiveRequestAsyncResult(IReplyChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.timeout = timeout;
                IAsyncResult result = channel.BeginTryReceiveRequest(timeout, onReceiveRequest, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                this.HandleReceiveRequestComplete(result);
                this.Complete(true);
            }

            public static RequestContext End(IAsyncResult result)
            {
                HelpReceiveRequestAsyncResult thisPtr = AsyncResult.End<HelpReceiveRequestAsyncResult>(result);
                return thisPtr.requestContext;
            }

            private static void OnReceiveRequest(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                HelpReceiveRequestAsyncResult thisPtr = (HelpReceiveRequestAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    thisPtr.HandleReceiveRequestComplete(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Complete(false, completionException);
            }

            private void HandleReceiveRequestComplete(IAsyncResult result)
            {
                if (!this.channel.EndTryReceiveRequest(result, out this.requestContext))
                {
                    throw FxTrace.Exception.AsError(UdpReplyChannel.CreateReceiveRequestTimedOutException(this.channel, this.timeout));
                }
            }
        }
    }
}
