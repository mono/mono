//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;

    class InputChannel : InputQueueChannel<Message>, IInputChannel
    {
        EndpointAddress localAddress;

        public InputChannel(ChannelManagerBase channelManager, EndpointAddress localAddress)
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
            if (typeof(T) == typeof(IInputChannel))
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

        public virtual Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public virtual Message Receive(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();

            return InputChannel.HelpReceive(this, timeout);
        }

        public virtual IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public virtual IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));
            }

            this.ThrowPending();

            return InputChannel.HelpBeginReceive(this, timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return InputChannel.HelpEndReceive(result);
        }

        public virtual bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));
            }

            this.ThrowPending();
            return base.Dequeue(timeout, out message);
        }

        public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));
            }

            this.ThrowPending();
            return base.BeginDequeue(timeout, callback, state);
        }

        public virtual bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return base.EndDequeue(result, out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));
            }

            this.ThrowPending();
            return base.WaitForItem(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();
            return base.BeginWaitForItem(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return base.EndWaitForItem(result);
        }

        #region static Helpers to convert TryReceive to Receive
        internal static Message HelpReceive(IInputChannel channel, TimeSpan timeout)
        {
            Message message;
            if (channel.TryReceive(timeout, out message))
            {
                return message;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateReceiveTimedOutException(channel, timeout));
            }
        }

        internal static IAsyncResult HelpBeginReceive(IInputChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new HelpReceiveAsyncResult(channel, timeout, callback, state);
        }

        internal static Message HelpEndReceive(IAsyncResult result)
        {
            return HelpReceiveAsyncResult.End(result);
        }

        class HelpReceiveAsyncResult : AsyncResult
        {
            IInputChannel channel;
            TimeSpan timeout;
            static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
            Message message;

            public HelpReceiveAsyncResult(IInputChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.timeout = timeout;
                IAsyncResult result = channel.BeginTryReceive(timeout, onReceive, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                HandleReceiveComplete(result);
                base.Complete(true);
            }

            public static Message End(IAsyncResult result)
            {
                HelpReceiveAsyncResult thisPtr = AsyncResult.End<HelpReceiveAsyncResult>(result);
                return thisPtr.message;
            }

            void HandleReceiveComplete(IAsyncResult result)
            {
                if (!this.channel.EndTryReceive(result, out this.message))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        InputChannel.CreateReceiveTimedOutException(this.channel, this.timeout));
                }
            }

            static void OnReceive(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                HelpReceiveAsyncResult thisPtr = (HelpReceiveAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    thisPtr.HandleReceiveComplete(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
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

        static Exception CreateReceiveTimedOutException(IInputChannel channel, TimeSpan timeout)
        {
            if (channel.LocalAddress != null)
            {
                return new TimeoutException(SR.GetString(SR.ReceiveTimedOut, channel.LocalAddress.Uri.AbsoluteUri, timeout));
            }
            else
            {
                return new TimeoutException(SR.GetString(SR.ReceiveTimedOutNoLocalAddress, timeout));
            }
        }
        #endregion
    }
}
