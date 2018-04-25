//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;

    public sealed partial class ChannelEndpointElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        [Fx.Tag.SecurityNote(Critical = "Stores information used in a security decision.")]
        [SecurityCritical]
        EvaluationContextHelper contextHelper;

        public ChannelEndpointElement()
            : base()
        {
        }

        public ChannelEndpointElement(EndpointAddress address, string contractType)
            : this()
        {
            if (address != null)
            {
                this.Address = address.Uri;
                this.Headers.Headers = address.Headers;
                if (null != address.Identity)
                {
                    this.Identity.InitializeFrom(address.Identity);
                }
            }
            this.Contract = contractType;
        }

        internal void Copy(ChannelEndpointElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }

            PropertyInformationCollection properties = source.ElementInformation.Properties;
            if (properties[ConfigurationStrings.Address].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Address = source.Address;
            }
            if (properties[ConfigurationStrings.BehaviorConfiguration].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BehaviorConfiguration = source.BehaviorConfiguration;
            }
            if (properties[ConfigurationStrings.Binding].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Binding = source.Binding;
            }
            if (properties[ConfigurationStrings.BindingConfiguration].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BindingConfiguration = source.BindingConfiguration;
            }
            if (properties[ConfigurationStrings.Name].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Name = source.Name;
            }
            if (properties[ConfigurationStrings.Contract].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Contract = source.Contract;
            }
            if (properties[ConfigurationStrings.Headers].ValueOrigin != PropertyValueOrigin.Default
                && source.Headers != null)
            {
                this.Headers.Copy(source.Headers);
            }
            if (properties[ConfigurationStrings.Identity].ValueOrigin != PropertyValueOrigin.Default
                && source.Identity != null)
            {
                this.Identity.Copy(source.Identity);
            }
            if (properties[ConfigurationStrings.Kind].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Kind = source.Kind;
            }
            if (properties[ConfigurationStrings.EndpointConfiguration].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.EndpointConfiguration = source.EndpointConfiguration;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Address, Options = ConfigurationPropertyOptions.None)]
        public Uri Address
        {
            get { return (Uri)base[ConfigurationStrings.Address]; }
            set { base[ConfigurationStrings.Address] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.BehaviorConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string BehaviorConfiguration
        {
            get { return (string)base[ConfigurationStrings.BehaviorConfiguration]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BehaviorConfiguration] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Binding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Binding
        {
            get { return (string)base[ConfigurationStrings.Binding]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[ConfigurationStrings.Binding] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string BindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.BindingConfiguration]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[ConfigurationStrings.BindingConfiguration] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Contract, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string Contract
        {
            get { return (string)base[ConfigurationStrings.Contract]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Contract] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Headers)]
        public AddressHeaderCollectionElement Headers
        {
            get { return (AddressHeaderCollectionElement)base[ConfigurationStrings.Headers]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Identity)]
        public IdentityElement Identity
        {
            get { return (IdentityElement)base[ConfigurationStrings.Identity]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Name, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.Name]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[ConfigurationStrings.Name] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Kind, DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string Kind
        {
            get { return (string)base[ConfigurationStrings.Kind]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Kind] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.EndpointConfiguration, DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string EndpointConfiguration
        {
            get { return (string)base[ConfigurationStrings.EndpointConfiguration]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.EndpointConfiguration] = value;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.")]
        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);

            base.Reset(parentElement);
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.",
            Miscellaneous = "RequiresReview -- the return value will be used for a security decision -- see comment in interface definition.")]
        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }
    }
}



