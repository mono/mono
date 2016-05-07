//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Configuration;       

    [Fx.Tag.XamlVisible(false)]
    public sealed class DiscoveryClientSettingsElement : ConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        [ConfigurationProperty(ConfigurationStrings.Endpoint)]
        [SuppressMessage(
            FxCop.Category.Configuration,
            FxCop.Rule.ConfigurationPropertyNameRule,
            Justification = "The configuration name for this element is 'endpoint'.")]
        public ChannelEndpointElement DiscoveryEndpoint
        {
            get
            {
                return (ChannelEndpointElement)base[ConfigurationStrings.Endpoint];
            }
        }


        [ConfigurationProperty(ConfigurationStrings.FindCriteria)]   
        public FindCriteriaElement FindCriteria
        {
            get
            {
                return (FindCriteriaElement)base[ConfigurationStrings.FindCriteria];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();


                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Endpoint,
                        typeof(ChannelEndpointElement),
                        null,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.FindCriteria,
                        typeof(FindCriteriaElement),
                        null,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));

                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}
