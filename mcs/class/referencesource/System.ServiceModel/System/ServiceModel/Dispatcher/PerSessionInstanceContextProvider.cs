//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class PerSessionInstanceContextProvider : InstanceContextProviderBase
    {

        internal PerSessionInstanceContextProvider(DispatchRuntime dispatchRuntime)
            : base(dispatchRuntime)
        {
        }

        public override InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            // Here is the flow for a Sessionful channel
            //  1. First request comes in on new channel.
            //  2. ServiceChannel.InstanceContext is returned which is null.
            //  3. InstanceBehavior.EnsureInstanceContext will create a new InstanceContext.
            //  4. this.InitializeInstanceContext is called with the newly created InstanceContext and the channel.
            //  5. If the channel is sessionful then its bound to the InstanceContext.
            //  6. Bind channel to the InstanceContext.
            //  7. For all further requests on the same channel, we will return ServiceChannel.InstanceContext which will be non null.
            ServiceChannel serviceChannel = this.GetServiceChannelFromProxy(channel);
            Fx.Assert((serviceChannel != null), "System.ServiceModel.Dispatcher.PerSessionInstanceContextProvider.GetExistingInstanceContext(), serviceChannel != null");
            return (serviceChannel != null) ? serviceChannel.InstanceContext : null;
        }

        public override void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            ServiceChannel serviceChannel = GetServiceChannelFromProxy(channel);
            if (serviceChannel != null && serviceChannel.HasSession)
            {
                instanceContext.BindIncomingChannel(serviceChannel);
            }
        }


        public override bool IsIdle(InstanceContext instanceContext)
        {
            //By default return true
            return true;
        }

        public override void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
            //no-op
        }
    }
}
