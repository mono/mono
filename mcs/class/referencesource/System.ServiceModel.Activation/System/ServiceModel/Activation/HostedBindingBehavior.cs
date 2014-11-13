//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;

    class HostedBindingBehavior : IServiceBehavior
    {
        VirtualPathExtension virtualPathExtension;

        internal HostedBindingBehavior(VirtualPathExtension virtualPathExtension)
        {
            this.virtualPathExtension = virtualPathExtension;
        }

        public VirtualPathExtension VirtualPathExtension
        {
            get { return this.virtualPathExtension; }
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw FxTrace.Exception.ArgumentNull("parameters");
            }

            parameters.Add(this.virtualPathExtension);
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }
    }
}
