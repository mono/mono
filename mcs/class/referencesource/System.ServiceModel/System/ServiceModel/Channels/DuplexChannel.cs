//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;

    abstract class DuplexChannel : InputQueueChannel<Message>, IDuplexChannel
    {
        EndpointAddress localAddress;

        protected DuplexChannel(ChannelManagerBase channelManager, EndpointAddress localAddress)
            : base(channelManager)
        {
            this.localAddress = localAddress;
        }

        public virtual EndpointAddress LocalAddress
        {
            get { return localAddress; }
        }

        public abstract EndpointAddress RemoteAddress { get; }
        public abstract Uri Via { get; }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");

            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            ThrowIfDisposedOrNotOpen();
            AddHeadersTo(message);
            return OnBeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            OnEndSend(result);
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IDuplexChannel))
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

        protected abstract void OnSend(Message message, TimeSpan timeout);

        protected virtual IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnSend(message, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected virtual void OnEndSend(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");

            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            ThrowIfDisposedOrNotOpen();

            AddHeadersTo(message);
            OnSend(message, timeout);
        }

        protected virtual void AddHeadersTo(Message message)
        {
        }

        public Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

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

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            this.ThrowPending();
            return base.Dequeue(timeout, out message);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

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
    }
}
