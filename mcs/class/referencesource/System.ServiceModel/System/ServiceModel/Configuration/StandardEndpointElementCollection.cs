//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    public sealed class StandardEndpointElementCollection<TEndpointConfiguration> : ServiceModelEnhancedConfigurationElementCollection<TEndpointConfiguration>
        where TEndpointConfiguration : StandardEndpointElement, new()
    {
        public StandardEndpointElementCollection()
            : base(ConfigurationStrings.StandardEndpoint)
        { }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            TEndpointConfiguration configElementKey = (TEndpointConfiguration)element;
            return configElementKey.Name;
        }
    }

}


