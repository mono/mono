//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;      

    [Fx.Tag.XamlVisible(false)]
    public sealed class DynamicEndpointElement : StandardEndpointElement
    {
        ConfigurationPropertyCollection properties;

        [ConfigurationProperty(ConfigurationStrings.DiscoveryClientSettings)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "No validator requiered.")]        
        public DiscoveryClientSettingsElement DiscoveryClientSettings
        {
            get
            {
                return (DiscoveryClientSettingsElement)base[ConfigurationStrings.DiscoveryClientSettings];
            }
        }

        protected internal override Type EndpointType
        {
            get { return typeof(DynamicEndpoint); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    
                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.DiscoveryClientSettings,
                        typeof(DiscoveryClientSettingsElement),
                        null,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));                  

                    this.properties = properties;
                }

                return this.properties;
            }
        }

        protected internal override ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription)
        {
            return new DynamicEndpoint(contractDescription);
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            if (string.IsNullOrEmpty(channelEndpointElement.Contract))
            {
                throw FxTrace.Exception.AsError(
                    new ConfigurationErrorsException(
                        SR.DiscoveryConfigContractNotSpecified(channelEndpointElement.Kind)));
            }

            if (channelEndpointElement.Address != null && !channelEndpointElement.Address.Equals(DiscoveryClientBindingElement.DiscoveryEndpointAddress.Uri))
            {
                throw FxTrace.Exception.AsError(
                    new ConfigurationErrorsException(
                            SR.DiscoveryEndpointAddressIncorrect(
                            "address",
                            channelEndpointElement.Address, 
                            DiscoveryClientBindingElement.DiscoveryEndpointAddress.Uri)));
            }
        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {            
            throw FxTrace.Exception.AsError(
                new InvalidOperationException(
                    SR.DiscoveryConfigDynamicEndpointInService(serviceEndpointElement.Kind)));
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {                        
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
        {
            DynamicEndpoint dynamicEndpoint = (DynamicEndpoint)endpoint;            
            
            if (!dynamicEndpoint.ValidateAndInsertDiscoveryClientBindingElement(dynamicEndpoint.Binding))
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.DiscoveryClientBindingElementPresentInDynamicEndpoint));
            }            

            if (PropertyValueOrigin.Default == this.DiscoveryClientSettings.ElementInformation.Properties[ConfigurationStrings.Endpoint].ValueOrigin)
            {
                dynamicEndpoint.DiscoveryEndpointProvider = new ConfigurationDiscoveryEndpointProvider();
            }
            else
            {                
                dynamicEndpoint.DiscoveryEndpointProvider = new ConfigurationDiscoveryEndpointProvider(this.DiscoveryClientSettings.DiscoveryEndpoint);
            }

            this.DiscoveryClientSettings.FindCriteria.ApplyConfiguration(dynamicEndpoint.FindCriteria);

            if (dynamicEndpoint.FindCriteria.ContractTypeNames.Count == 0)
            {
                dynamicEndpoint.FindCriteria.ContractTypeNames.Add(
                    new XmlQualifiedName(dynamicEndpoint.Contract.Name, dynamicEndpoint.Contract.Namespace));                
            }
        }
    }
}
