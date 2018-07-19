//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    class ContextChannelListener<TChannel> : LayeredChannelListener<TChannel>
        where TChannel : class, IChannel
    {
        ContextExchangeMechanism contextExchangeMechanism;
        Uri listenBaseAddress;

        public ContextChannelListener(BindingContext context, ContextExchangeMechanism contextExchangeMechanism)
            : base(context == null ? null : context.Binding, context == null ? null : context.BuildInnerChannelListener<TChannel>())
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!ContextExchangeMechanismHelper.IsDefined(contextExchangeMechanism))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("contextExchangeMechanism"));
            }

            this.contextExchangeMechanism = contextExchangeMechanism;
            this.listenBaseAddress = context.ListenUriBaseAddress;
        }

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            return this.InternalAcceptChannel(((IChannelListener<TChannel>)this.InnerChannelListener).AcceptChannel(timeout));
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ((IChannelListener<TChannel>)this.InnerChannelListener).BeginAcceptChannel(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return this.InternalAcceptChannel(((IChannelListener<TChannel>)this.InnerChannelListener).EndAcceptChannel(result));
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.InnerChannelListener.EndWaitForChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.InnerChannelListener.WaitForChannel(timeout);
        }

        TChannel InternalAcceptChannel(TChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return innerChannel;
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ContextChannelListenerChannelAccepted,
                     SR.GetString(SR.TraceCodeContextChannelListenerChannelAccepted), this);
            }

            if (typeof(TChannel) == typeof(IInputChannel))
            {
                return (TChannel)(object)new ContextInputChannel(this, (IInputChannel)innerChannel, this.contextExchangeMechanism);
            }
            else if (typeof(TChannel) == typeof(IInputSessionChannel))
            {
                return (TChannel)(object)new ContextInputSessionChannel(this, (IInputSessionChannel)innerChannel, this.contextExchangeMechanism);
            }
            else if (typeof(TChannel) == typeof(IReplyChannel))
            {
                return (TChannel)(object)new ContextReplyChannel(this, (IReplyChannel)innerChannel, this.contextExchangeMechanism);
            }
            else if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                return (TChannel)(object)new ContextReplySessionChannel(this, (IReplySessionChannel)innerChannel, this.contextExchangeMechanism);
            }
            else // IDuplexSessionChannel
            {
                return (TChannel)(object)new ContextDuplexSessionChannel(this, (IDuplexSessionChannel)innerChannel, this.contextExchangeMechanism);
            }
        }
    }
}
