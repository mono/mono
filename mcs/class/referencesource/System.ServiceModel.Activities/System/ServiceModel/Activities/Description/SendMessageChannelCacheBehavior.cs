//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    class SendMessageChannelCacheBehavior : IServiceBehavior
    {
        public SendMessageChannelCacheBehavior()
        {
        }

        public bool AllowUnsafeCaching
        {
            get;
            set;
        }

        public ChannelCacheSettings FactorySettings
        {
            get;
            set;
        }

        public ChannelCacheSettings ChannelSettings
        {
            get;
            set;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost workflowServiceHost = serviceHostBase as WorkflowServiceHost;
            if (workflowServiceHost != null)
            {
                workflowServiceHost.WorkflowExtensions.Add(new SendMessageChannelCache(this.FactorySettings, this.ChannelSettings, this.AllowUnsafeCaching));
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
