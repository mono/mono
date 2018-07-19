//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Description;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    
    public class WorkflowControlEndpointElement : StandardEndpointElement
    {
        ConfigurationPropertyCollection properties;
        bool shouldLetConfigLoaderOverwriteAddress;

        protected internal override Type EndpointType
        {
            get { return typeof(WorkflowControlEndpoint); }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            Justification = "Value will be validated when converted into a Uri.")]
        [ConfigurationProperty(System.ServiceModel.Configuration.ConfigurationStrings.Address, DefaultValue = "")]
        public Uri Address
        {
            get { return (Uri)base[System.ServiceModel.Configuration.ConfigurationStrings.Address]; }
            set { base[System.ServiceModel.Configuration.ConfigurationStrings.Address] = value; }
        }

        [ConfigurationProperty(System.ServiceModel.Configuration.ConfigurationStrings.Binding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Binding
        {
            get { return (string)base[System.ServiceModel.Configuration.ConfigurationStrings.Binding]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[System.ServiceModel.Configuration.ConfigurationStrings.Binding] = value;
            }
        }

        [ConfigurationProperty(System.ServiceModel.Configuration.ConfigurationStrings.BindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string BindingConfiguration
        {
            get { return (string)base[System.ServiceModel.Configuration.ConfigurationStrings.BindingConfiguration]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[System.ServiceModel.Configuration.ConfigurationStrings.BindingConfiguration] = value;
            }
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
                        System.ServiceModel.Configuration.ConfigurationStrings.Binding,
                        typeof(string),
                        string.Empty,
                        null,
                        new StringValidator(0, 2147483647, null),
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        System.ServiceModel.Configuration.ConfigurationStrings.BindingConfiguration,
                        typeof(string),
                        string.Empty,
                        null,
                        new StringValidator(0, 2147483647, null),
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        System.ServiceModel.Configuration.ConfigurationStrings.Address,
                        typeof(Uri),
                        string.Empty,
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
            WorkflowControlEndpoint result = new WorkflowControlEndpoint();

            if (!string.IsNullOrEmpty(this.Binding))
            {
                Binding binding = ConfigLoader.LookupBinding(this.Binding, this.BindingConfiguration);

                // we need to add validation here
                if (binding == null)
                {
                    throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.FailedToLoadBindingInControlEndpoint(this.Binding, this.BindingConfiguration, this.Name)));
                }

                result.Binding = binding;
            }

            // This is only for client side
            if (this.shouldLetConfigLoaderOverwriteAddress)
            {
                // ConfigLoader will check for null and overwrite it with the address from ChannelEndpointElement
                result.Address = null;
            }

            return result;
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement)
        {

        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {

        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            // Override serviceEndpointElement.Address with this.Address when serviceEndpointElement.Address == null.
            // This condition (serviceEndpointElement.Address == null) should only be true when used with the SqlWorkflowInstanceStoreBehavior.
            // Setting the address here so that ConfigLoader is able to set the EndpointAddress correctly, especially when this.Address is 
            // a relative address and can only be made absolute using the baseAddresses configured on the serviceHost.

            // Server side address inference goes by the following order:
            // 1. ServiceEndpointElement.Address if it is not-null and non-default
            // 2. WorkflowControlEndpointElement.Address
            // 3. Host base address
            
            if (serviceEndpointElement.Address == null ||
                (!HasAddressSetByUser(serviceEndpointElement) && HasAddressSetByUser(this)))
            {
                serviceEndpointElement.Address = this.Address;
            }
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            // Client side address inference goes by the following order:
            // 1. ChannelEndpointElement.Address
            // 2. WorkflowControlEndpointElement.Address
            // 3. Default address from WorkflowControlEndpoint

            if (HasAddressSetByUser(channelEndpointElement))
            {
                this.shouldLetConfigLoaderOverwriteAddress = true;
            }
            else if (HasAddressSetByUser(this))
            {
                channelEndpointElement.Address = this.Address;
                this.shouldLetConfigLoaderOverwriteAddress = true;
            }
        }

        bool HasAddressSetByUser(ConfigurationElement configurationElement)
        {
            return configurationElement.ElementInformation.Properties[System.ServiceModel.Configuration.ConfigurationStrings.Address].ValueOrigin != PropertyValueOrigin.Default;
        }
    }
}
