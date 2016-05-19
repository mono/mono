//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Configuration;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Xml;
    using System.Globalization;

    public partial class ServiceMetadataEndpointElement : StandardEndpointElement
    {
        public ServiceMetadataEndpointElement() : base() { }

        protected internal override Type EndpointType
        {
            get { return typeof(ServiceMetadataEndpoint); }
        }

        protected internal override ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription)
        {
            return new ServiceMetadataEndpoint();
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            if (String.IsNullOrEmpty(channelEndpointElement.Binding))
            {
                channelEndpointElement.Binding = ConfigurationStrings.MexHttpBindingCollectionElementName;
            }
            channelEndpointElement.Contract = ServiceMetadataBehavior.MexContractName;
        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            if (String.IsNullOrEmpty(serviceEndpointElement.Binding))
            {
                serviceEndpointElement.Binding = ConfigurationStrings.MexHttpBindingCollectionElementName;
            }
            serviceEndpointElement.Contract = ServiceMetadataBehavior.MexContractName;
            serviceEndpointElement.IsSystemEndpoint = true;
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
            //no additional configuration is required for MEX.
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
        {
            //no additional configuration is required for MEX.
        }
    }
}
