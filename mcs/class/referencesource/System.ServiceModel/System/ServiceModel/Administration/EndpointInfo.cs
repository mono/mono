//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    sealed class EndpointInfo
    {
        Uri address;
        KeyedByTypeCollection<IEndpointBehavior> behaviors;
        EndpointIdentity identity;
        AddressHeaderCollection headers;
        CustomBinding binding;
        ContractDescription contract;
        ServiceEndpoint endpoint;
        string serviceName;

        internal EndpointInfo(ServiceEndpoint endpoint, string serviceName)
        {
            Fx.Assert(null != endpoint, "endpoint cannot be null");

            this.endpoint = endpoint;
            this.address = endpoint.Address.Uri;
            this.headers = endpoint.Address.Headers;
            this.identity = endpoint.Address.Identity;
            this.behaviors = endpoint.Behaviors;
            this.serviceName = serviceName;

            this.binding = null == endpoint.Binding ? new CustomBinding() : new CustomBinding(endpoint.Binding);
            this.contract = endpoint.Contract;
        }

        public Uri Address
        {
            get { return this.address; }
        }

        public Uri ListenUri
        {
            get { return null != this.Endpoint.ListenUri ? this.Endpoint.ListenUri : this.Address; }
        }

        public KeyedByTypeCollection<IEndpointBehavior> Behaviors
        {
            get
            {
                return this.behaviors;
            }
        }

        public ContractDescription Contract
        {
            get { return this.contract; }
        }

        public CustomBinding Binding
        {
            get { return this.binding; }
        }

        public ServiceEndpoint Endpoint
        {
            get { return this.endpoint; }
        }

        public AddressHeaderCollection Headers
        {
            get { return this.headers; }
        }

        public EndpointIdentity Identity
        {
            get { return this.identity; }
        }

        public string Name
        {
            get
            {
                return this.ServiceName + "." + this.Contract.Name + "@" + this.Address.AbsoluteUri;
            }
        }

        public string ServiceName
        {
            get { return this.serviceName; }
        }
    }
}
