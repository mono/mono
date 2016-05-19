//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    class DurableInstanceProvider : IInstanceProvider
    {
        object singletonDurableInstance;
        ServiceHostBase serviceHost;

        public DurableInstanceProvider(ServiceHostBase serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        //Dummy Instance stuffed onto InstanceContext        
        object Instance
        {
            get
            {
                if (singletonDurableInstance == null)
                {
                    singletonDurableInstance = new object();
                }
                return singletonDurableInstance;
            }
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.Instance;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.Instance;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {

        }                
    }
}
