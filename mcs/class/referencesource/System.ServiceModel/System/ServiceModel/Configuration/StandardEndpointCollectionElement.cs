//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.Security;
    using System.Collections.Generic;
    using System.ServiceModel.Description;

    public partial class StandardEndpointCollectionElement<TStandardEndpoint, TEndpointConfiguration> : EndpointCollectionElement
        where TStandardEndpoint : ServiceEndpoint
        where TEndpointConfiguration : StandardEndpointElement, new ()
    {

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public StandardEndpointElementCollection<TEndpointConfiguration> Endpoints
        {
            get { return (StandardEndpointElementCollection<TEndpointConfiguration>)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        public override Type EndpointType
        {
            get { return typeof(TStandardEndpoint); }
        }

        public override ReadOnlyCollection<StandardEndpointElement> ConfiguredEndpoints
        {
            get
            {
                List<StandardEndpointElement> configuredEndpoints = new List<StandardEndpointElement>();
                foreach (StandardEndpointElement configuredEndpoint in this.Endpoints)
                {
                    configuredEndpoints.Add(configuredEndpoint);
                }

                return new ReadOnlyCollection<StandardEndpointElement>(configuredEndpoints);
            }
        }

        public override bool ContainsKey(string name)
        {
            StandardEndpointCollectionElement<TStandardEndpoint, TEndpointConfiguration> me = (StandardEndpointCollectionElement<TStandardEndpoint, TEndpointConfiguration>)this;
#pragma warning suppress 56506 //[....]; me.Endpoints can never be null (underlying configuration system guarantees)
            return me.Endpoints.ContainsKey(name);
        }

        protected internal override StandardEndpointElement GetDefaultStandardEndpointElement()
        {
            return System.Activator.CreateInstance<TEndpointConfiguration>();
        }

        protected internal override bool TryAdd(string name, ServiceEndpoint endpoint, Configuration config)
        {
            // The configuration item needs to understand the ServiceEndpointType && be of type ServiceEndpoint
            bool retval = (endpoint.GetType() == typeof(TStandardEndpoint)) &&
                typeof(StandardEndpointElement).IsAssignableFrom(typeof(TEndpointConfiguration));
            if (retval)
            {
                TEndpointConfiguration endpointConfig = new TEndpointConfiguration();
                endpointConfig.Name = name;
                endpointConfig.InitializeFrom(endpoint);
                this.Endpoints.Add(endpointConfig);
            }
            return retval;
        }
    }
}



