//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public sealed partial class ServiceEndpointElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        public ServiceEndpointElement() : base() { }

        public ServiceEndpointElement(Uri address, string contractType)
            : this()
        {
            this.Address = address;
            this.Contract = contractType;
        }

        internal void Copy(ServiceEndpointElement source)
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
            if (properties[ConfigurationStrings.BindingName].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BindingName = source.BindingName;
            }
            if (properties[ConfigurationStrings.BindingNamespace].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BindingNamespace = source.BindingNamespace;
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
            if (properties[ConfigurationStrings.ListenUriMode].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ListenUriMode = source.ListenUriMode;
            }
            if (properties[ConfigurationStrings.ListenUri].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ListenUri = source.ListenUri;
            }
            if (properties[ConfigurationStrings.IsSystemEndpoint].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.IsSystemEndpoint = source.IsSystemEndpoint;
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

        [ConfigurationProperty(ConfigurationStrings.Address, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
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
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BehaviorConfiguration] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Binding, Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string Binding
        {
            get { return (string)base[ConfigurationStrings.Binding]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Binding] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingConfiguration, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string BindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.BindingConfiguration]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BindingConfiguration] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Name, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.Name]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[ConfigurationStrings.Name] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingName, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string BindingName
        {
            get { return (string)base[ConfigurationStrings.BindingName]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BindingName] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingNamespace, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string BindingNamespace
        {
            get { return (string)base[ConfigurationStrings.BindingNamespace]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BindingNamespace] = value;
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

        [ConfigurationProperty(ConfigurationStrings.ListenUriMode, DefaultValue = ListenUriMode.Explicit)]
        [ServiceModelEnumValidator(typeof(ListenUriModeHelper))]
        public ListenUriMode ListenUriMode
        {
            get { return (ListenUriMode)base[ConfigurationStrings.ListenUriMode]; }
            set { base[ConfigurationStrings.ListenUriMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ListenUri, DefaultValue = null)]
        public Uri ListenUri
        {
            get { return (Uri)base[ConfigurationStrings.ListenUri]; }
            set { base[ConfigurationStrings.ListenUri] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IsSystemEndpoint, DefaultValue = false)]
        public bool IsSystemEndpoint
        {
            get { return (bool)base[ConfigurationStrings.IsSystemEndpoint]; }
            set { base[ConfigurationStrings.IsSystemEndpoint] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Kind, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
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

        [ConfigurationProperty(ConfigurationStrings.EndpointConfiguration, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
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

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Miscellaneous =
            "RequiresReview - the return value will be used for a security decision -- see comment in interface definition")]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            Fx.Assert("Not implemented: IConfigurationContextProviderInternal.GetOriginalEvaluationContext");
            return null;
        }
    }
}



