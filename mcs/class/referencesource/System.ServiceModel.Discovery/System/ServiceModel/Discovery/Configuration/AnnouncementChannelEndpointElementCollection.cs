//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel.Configuration;

    [ConfigurationCollection(typeof(ChannelEndpointElement), AddItemName = ConfigurationStrings.Endpoint)]
    public sealed class AnnouncementChannelEndpointElementCollection : ServiceModelConfigurationElementCollection<ChannelEndpointElement>
    {
        public AnnouncementChannelEndpointElementCollection()
        {
            this.AddElementName = ConfigurationStrings.Endpoint;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }

            ChannelEndpointElement channelEndpointElement = (ChannelEndpointElement)element;

            string address = channelEndpointElement.Address == null ? "" : channelEndpointElement.Address.ToString().ToUpperInvariant();

            return string.Format(CultureInfo.InvariantCulture,
                "kind:{0};endpointConfiguration:{1};address:{2};bindingConfiguration:{3};binding:{4};",
                channelEndpointElement.Kind,
                channelEndpointElement.EndpointConfiguration,
                address,
                channelEndpointElement.BindingConfiguration,
                channelEndpointElement.Binding);
        }
    }
}
