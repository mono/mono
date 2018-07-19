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
    using System.Runtime.Diagnostics;

    class ContextChannelFactory<TChannel> : LayeredChannelFactory<TChannel>
    {
        ContextExchangeMechanism contextExchangeMechanism;
        Uri callbackAddress;
        bool contextManagementEnabled;

        public ContextChannelFactory(BindingContext context, ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled)
            : base(context == null ? null : context.Binding, context == null ? null : context.BuildInnerChannelFactory<TChannel>())
        {
            if (!ContextExchangeMechanismHelper.IsDefined(contextExchangeMechanism))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("contextExchangeMechanism"));
            }

            this.contextExchangeMechanism = contextExchangeMechanism;
            this.callbackAddress = callbackAddress;
            this.contextManagementEnabled = contextManagementEnabled;
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR.GetString(SR.ContextChannelFactoryChannelCreatedDetail, address, via);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.ContextChannelFactoryChannelCreated, SR.GetString(SR.TraceCodeContextChannelFactoryChannelCreated),
                    new StringTraceRecord("ChannelDetail", traceText),
                    this, null);
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel)(object)new ContextOutputChannel(this, ((IChannelFactory<IOutputChannel>)this.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            else if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                return (TChannel)(object)new ContextOutputSessionChannel(this, ((IChannelFactory<IOutputSessionChannel>)this.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel)(object)new ContextRequestChannel(this, ((IChannelFactory<IRequestChannel>)this.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            else if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (TChannel)(object)new ContextRequestSessionChannel(this, ((IChannelFactory<IRequestSessionChannel>)this.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            else // IDuplexSessionChannel
            {
                return (TChannel)(object)new ContextDuplexSessionChannel(this, ((IChannelFactory<IDuplexSessionChannel>)this.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, via, this.callbackAddress, this.contextManagementEnabled);
            }
        }
    }
}
