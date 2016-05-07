//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ChannelEndpointElement), AddItemName = ConfigurationStrings.Endpoint)]
    public sealed class ChannelEndpointElementCollection : ServiceModelEnhancedConfigurationElementCollection<ChannelEndpointElement>
    {
        public ChannelEndpointElementCollection()
            : base(ConfigurationStrings.Endpoint)
        { }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ChannelEndpointElement configElementKey = (ChannelEndpointElement)element;
            return string.Format(CultureInfo.InvariantCulture,
                "contractType:{0};name:{1}",
                configElementKey.Contract,
                configElementKey.Name);
        }
    }
}


