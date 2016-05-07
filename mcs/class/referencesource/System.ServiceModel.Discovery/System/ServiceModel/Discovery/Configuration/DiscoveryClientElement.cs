//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public sealed class DiscoveryClientElement : BindingElementExtensionElement
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

        [SuppressMessage(
            FxCop.Category.Configuration, 
            FxCop.Rule.ConfigurationPropertyAttributeRule, 
            Justification = "This property only overrides the base property.")]
        public override Type BindingElementType
        {
            get { return typeof(DiscoveryClientBindingElement); }
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

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);

            DiscoveryClientBindingElement discoveryClientBindingElement = (DiscoveryClientBindingElement)bindingElement;
            
            if (PropertyValueOrigin.Default == this.ElementInformation.Properties[ConfigurationStrings.Endpoint].ValueOrigin)
            {                
                discoveryClientBindingElement.DiscoveryEndpointProvider = new ConfigurationDiscoveryEndpointProvider();
            }
            else
            {
                discoveryClientBindingElement.DiscoveryEndpointProvider = new ConfigurationDiscoveryEndpointProvider(this.DiscoveryEndpoint);                
            }

            this.FindCriteria.ApplyConfiguration(discoveryClientBindingElement.FindCriteria);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            DiscoveryClientElement source = (DiscoveryClientElement)from;

            if (PropertyValueOrigin.Default == this.ElementInformation.Properties[ConfigurationStrings.Endpoint].ValueOrigin)
            {
                ChannelEndpointElement udpChannelEndpointElement = ConfigurationUtility.GetDefaultDiscoveryEndpointElement();
                udpChannelEndpointElement.Copy(source.DiscoveryEndpoint);
            }
            else
            {
                this.DiscoveryEndpoint.Copy(source.DiscoveryEndpoint);
            }
            this.FindCriteria.CopyFrom(source.FindCriteria);
        }

        protected internal override BindingElement CreateBindingElement()
        {
            DiscoveryClientBindingElement discoveryClientBindingElement = new DiscoveryClientBindingElement();
            this.ApplyConfiguration(discoveryClientBindingElement);

            return discoveryClientBindingElement;
        }        

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            throw FxTrace.Exception.AsError(
                new NotSupportedException(SR2.DiscoveryConfigInitializeFromNotSupported));             
        }        
    }
}
