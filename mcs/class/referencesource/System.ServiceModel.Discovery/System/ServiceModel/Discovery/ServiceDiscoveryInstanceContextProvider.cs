//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    class ServiceDiscoveryInstanceContextProvider : IInstanceContextProvider, IInstanceProvider
    {
        DiscoveryService discoveryService;

        internal ServiceDiscoveryInstanceContextProvider(DiscoveryService discoveryService)
        {
            Fx.Assert(discoveryService != null, "The discoveryService must be non null.");
            this.discoveryService = discoveryService;
        }

        InstanceContext IInstanceContextProvider.GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            // per call instance context
            return null;
        }

        void IInstanceContextProvider.InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
        }

        bool IInstanceContextProvider.IsIdle(InstanceContext instanceContext)
        {
            return true;
        }

        void IInstanceContextProvider.NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
        }

        object IInstanceProvider.GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.discoveryService;
        }

        object IInstanceProvider.GetInstance(InstanceContext instanceContext)
        {
            return this.discoveryService;
        }

        void IInstanceProvider.ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}
