//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;

    class ReplyChannel : InputQueueChannel<RequestContext>, IReplyChannel
    {
        EndpointAddress localAddress;

        public ReplyChannel(ChannelManagerBase channelManager, EndpointAddress localAddress)
            : base(channelManager)
        {
            this.localAddress = localAddress;
        }

        public EndpointAddress LocalAddress
        {
            get { return localAddress; }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IReplyChannel))
            {
                return (T)(object)this;
            }

            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            return default(T);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        #region static Helpers to convert TryReceiveRequest to ReceiveRequest
        internal static RequestContext HelpReceiveRequest(IReplyChannel channel, TimeSpan timeout)
        {
            RequestContext requestContext;
            if (channel.TryReceiveRequest(timeout, out requestContext))
            {
                return requestContext;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    ReplyChannel.CreateReceiveRequestTimedOutException(channel, timeout));
            }
        }

        internal static IAsyncResult HelpBeginReceiveRequest(IReplyChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new HelpReceiveRequestAsyncResult(channel, timeout, callback, state);
        }

        internal static RequestContext HelpEndReceiveRequest(IAsyncResult result)
        {
            return HelpReceiveRequestAsyncResult.End(result);
        }

        class HelpReceiveRequestAsyncResult : AsyncResult
        {
            IReplyChannel channel;
            TimeSpan timeout;
            static AsyncCallback onReceiveRequest = Fx.ThunkCallback(new AsyncCallback(OnReceiveRequest));
            RequestContext requestContext;

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

                HandleReceiveRequestComplete(result);
                base.Complete(true);
            }

            public static RequestContext End(IAsyncResult result)
            {
                HelpReceiveRequestAsyncResult thisPtr = AsyncResult.End<HelpReceiveRequestAsyncResult>(result);
                return thisPtr.requestContext;
            }

            void HandleReceiveRequestComplete(IAsyncResult result)
            {
                if (!this.channel.EndTryReceiveRequest(result, out this.requestContext))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        ReplyChannel.CreateReceiveRequestTimedOutException(this.channel, this.timeout));
                }
            }

            static void OnReceiveRequest(IAsyncResult result)
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
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
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
        }

        static Exception CreateReceiveRequestTimedOutException(IReplyChannel channel, TimeSpan timeout)
        {
            if (channel.LocalAddress != null)
            {
                return new TimeoutException(SR.GetString(SR.ReceiveRequestTimedOut, channel.LocalAddress.Uri.AbsoluteUri, timeout));
            }
            else
            {
                return new TimeoutException(SR.GetString(SR.ReceiveRequestTimedOutNoLocalAddress, timeout));
            }
        }
        #endregion


        public RequestContext ReceiveRequest()
        {
            return this.ReceiveRequest(this.DefaultReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();
            return ReplyChannel.HelpReceiveRequest(this, timeout);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return this.BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));
            }

            this.ThrowPending();
            return ReplyChannel.HelpBeginReceiveRequest(this, timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return ReplyChannel.HelpEndReceiveRequest(result);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();
            return base.Dequeue(timeout, out context);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();
            return base.BeginDequeue(timeout, callback, state);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            return base.EndDequeue(result, out context);
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();
            return base.WaitForItem(timeout);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();
            return base.BeginWaitForItem(timeout, callback, state);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return base.EndWaitForItem(result);
        }
    }
}
