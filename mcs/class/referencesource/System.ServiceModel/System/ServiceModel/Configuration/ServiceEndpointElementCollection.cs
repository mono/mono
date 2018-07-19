//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ServiceEndpointElement), AddItemName = ConfigurationStrings.Endpoint)]
    public sealed class ServiceEndpointElementCollection : ServiceModelEnhancedConfigurationElementCollection<ServiceEndpointElement>
    {
        public ServiceEndpointElementCollection()
            : base(ConfigurationStrings.Endpoint)
        { }

        protected override bool ThrowOnDuplicate
        {
            get { return false; }
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ServiceEndpointElement configElementKey = (ServiceEndpointElement)element;

            // We need to provide something sufficiently unique for the underlying system.
            // Conceptually, this is an ever-expanding collection. 
            // There is no logical object key for this collection.
            return string.Format(CultureInfo.InvariantCulture,
                "address:{0};bindingConfiguration{1};bindingName:{2};bindingNamespace:{3};bindingSectionName:{4};contractType:{5};kind:{6};endpointConfiguration:{7};",
                (configElementKey.Address == null) ? null : configElementKey.Address.ToString().ToUpperInvariant(),
                configElementKey.BindingConfiguration,
                configElementKey.BindingName,
                configElementKey.BindingNamespace,
                configElementKey.Binding,
                configElementKey.Contract,
                configElementKey.Kind,
                configElementKey.EndpointConfiguration);
        }

    }
}


