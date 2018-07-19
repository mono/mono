//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.PeerResolvers;
    using System.ComponentModel;

    public sealed partial class PeerCustomResolverElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Address, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public Uri Address
        {
            get { return (Uri)base[ConfigurationStrings.Address]; }
            set { base[ConfigurationStrings.Address] = value; }
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

        [ConfigurationProperty(ConfigurationStrings.Binding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Binding
        {
            get { return (string)base[ConfigurationStrings.Binding]; }
            set { base[ConfigurationStrings.Binding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string BindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.BindingConfiguration]; }
            set { base[ConfigurationStrings.BindingConfiguration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.PeerResolverType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string ResolverType
        {
            get { return (string)base[ConfigurationStrings.PeerResolverType]; }
            set { base[ConfigurationStrings.PeerResolverType] = value; }
        }

        internal void ApplyConfiguration(PeerCustomResolverSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            if (this.Address != null)
            {
                settings.Address = new EndpointAddress(this.Address, ConfigLoader.LoadIdentity(this.Identity), this.Headers.Headers);
            }
            settings.BindingSection = this.Binding;
            settings.BindingConfiguration = this.BindingConfiguration;

            if (!String.IsNullOrEmpty(this.Binding) && !String.IsNullOrEmpty(this.BindingConfiguration))
                settings.Binding = ConfigLoader.LookupBinding(this.Binding, this.BindingConfiguration);
            if (!String.IsNullOrEmpty(this.ResolverType))
            {
                Type myResolverType = Type.GetType(this.ResolverType, false);
                if (myResolverType != null)
                {
                    settings.Resolver = Activator.CreateInstance(myResolverType) as PeerResolver;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.PeerResolverInvalid, this.ResolverType)));
                }
            }
        }

        internal void InitializeFrom(PeerCustomResolverSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            if (settings.Address != null)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Address, settings.Address.Uri);
                this.Identity.InitializeFrom(settings.Address.Identity);
            }
            if (settings.Resolver != null)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.PeerResolverType, settings.Resolver.GetType().AssemblyQualifiedName);
            }
            if (settings.Binding != null)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.BindingConfiguration, PeerStrings.PeerCustomResolver + Guid.NewGuid().ToString());
                string sectionName;
                BindingsSection.TryAdd(this.BindingConfiguration,
                    settings.Binding,
                    out sectionName);
                this.Binding = sectionName;
            }
        }
    }
}
