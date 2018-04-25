//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime;
    using System.ServiceModel.Channels;

    abstract class SecurityChannel<TChannel> :
        LayeredChannel<TChannel>
        where TChannel : class, IChannel
    {
        SecurityProtocol securityProtocol;

        protected SecurityChannel(ChannelManagerBase channelManager, TChannel innerChannel)
            : this(channelManager, innerChannel, null)
        {
        }

        protected SecurityChannel(ChannelManagerBase channelManager, TChannel innerChannel, SecurityProtocol securityProtocol)
            : base(channelManager, innerChannel)
        {
            this.securityProtocol = securityProtocol;
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(FaultConverter))
            {
                return new SecurityChannelFaultConverter(this.InnerChannel) as T;
            }

            return base.GetProperty<T>();
        }

        public SecurityProtocol SecurityProtocol
        {
            get
            {
                return this.securityProtocol;
            }
            protected set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.securityProtocol = value;
            }
        }

        protected override void OnAbort()
        {
            if (this.securityProtocol != null)
            {
                this.securityProtocol.Close(true, TimeSpan.Zero);
            }

            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, this.BeginCloseSecurityProtocol, this.EndCloseSecurityProtocol,
                base.OnBeginClose, base.OnEndClose);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        IAsyncResult BeginCloseSecurityProtocol(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.securityProtocol != null)
            {
                return this.securityProtocol.BeginClose(timeout, callback, state);
            }
            else
            {
                return new NullSecurityProtocolCloseAsyncResult(callback, state);
            }
        }

        void EndCloseSecurityProtocol(IAsyncResult result)
        {
            if (result is NullSecurityProtocolCloseAsyncResult)
            {
                NullSecurityProtocolCloseAsyncResult.End(result);
            }
            else
            {
                this.securityProtocol.EndClose(result);
            }
        }


        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.securityProtocol != null)
            {
                this.securityProtocol.Close(false, timeoutHelper.RemainingTime());
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected void ThrowIfDisposedOrNotOpen(Message message)
        {
            ThrowIfDisposedOrNotOpen();
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
        }

        class NullSecurityProtocolCloseAsyncResult : CompletedAsyncResult
        {
            public NullSecurityProtocolCloseAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            new public static void End(IAsyncResult result)
            {
                AsyncResult.End<NullSecurityProtocolCloseAsyncResult>(result);
            }
        }

        protected sealed class OutputChannelSendAsyncResult : ApplySecurityAndSendAsyncResult<IOutputChannel>
        {
            public OutputChannelSendAsyncResult(Message message, SecurityProtocol binding, IOutputChannel channel, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(binding, channel, timeout, callback, state)
            {
                this.Begin(message, null);
            }

            protected override IAsyncResult BeginSendCore(IOutputChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginSend(message, timeout, callback, state);
            }

            internal static void End(IAsyncResult result)
            {
                OutputChannelSendAsyncResult self = result as OutputChannelSendAsyncResult;
                OnEnd(self);
            }

            protected override void EndSendCore(IOutputChannel channel, IAsyncResult result)
            {
                channel.EndSend(result);
            }

            protected override void OnSendCompleteCore(TimeSpan timeout)
            {
            }
        }
    }
}
