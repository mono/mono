//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description 
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    public class UseRequestHeadersForMetadataAddressBehavior : IServiceBehavior
    {
        Dictionary<string, int> defaultPortsByScheme;

        public UseRequestHeadersForMetadataAddressBehavior()
        {
        }

        public IDictionary<string, int> DefaultPortsByScheme
        {
            get
            {
                if (this.defaultPortsByScheme == null)
                {
                    this.defaultPortsByScheme = new Dictionary<string, int>();
                }
                return this.defaultPortsByScheme;
            }
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
        
        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
