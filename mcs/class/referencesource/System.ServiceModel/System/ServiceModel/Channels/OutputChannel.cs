//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    abstract class OutputChannel : ChannelBase, IOutputChannel
    {
        protected OutputChannel(ChannelManagerBase manager)
            : base(manager)
        {
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
            this.EmitTrace(message);
            return OnBeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            OnEndSend(result);
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IOutputChannel))
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

        protected abstract IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);

        protected abstract void OnEndSend(IAsyncResult result);

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
            this.EmitTrace(message);
            OnSend(message, timeout);
        }

        protected virtual TraceRecord CreateSendTrace(Message message)
        {
            return MessageTransmitTraceRecord.CreateSendTraceRecord(message, this.RemoteAddress);
        }

        void EmitTrace(Message message)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageSent,
                    SR.GetString(SR.TraceCodeMessageSent),
                    this.CreateSendTrace(message), this, null);
            }
        }

        protected virtual void AddHeadersTo(Message message)
        {
        }
    }
}
