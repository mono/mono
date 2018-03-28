//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    //This is a perchannel instance context provider which returns non-throttled IC.
    class DurableInstanceContextProvider : IInstanceContextProvider
    {
        ServiceHostBase serviceHostBase;

        public DurableInstanceContextProvider(ServiceHostBase serviceHost)
        {
            this.serviceHostBase = serviceHost;
        }

        public InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            Fx.Assert(message != null, "Null message");
            Fx.Assert(channel != null, "Null channel");

            return new InstanceContext(this.serviceHostBase);
        }

        public void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {

        }

        public bool IsIdle(InstanceContext instanceContext)
        {
            return true;
        }

        public void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
            //Empty
        }
    }
}
