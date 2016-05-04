//------------------------------------------------------------------------------
// <copyright file="ServicePointManagerElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Security;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class ServicePointManagerElement : ConfigurationElement
    {
        public ServicePointManagerElement()
        {
            this.properties.Add(this.checkCertificateName);
            this.properties.Add(this.checkCertificateRevocationList);
            this.properties.Add(this.dnsRefreshTimeout);
            this.properties.Add(this.enableDnsRoundRobin);
            this.properties.Add(this.encryptionPolicy);
            this.properties.Add(this.expect100Continue);
            this.properties.Add(this.useNagleAlgorithm);
        }

        protected override void PostDeserialize()
        {
            // Perf optimization. If the configuration is coming from machine.config
            // It is safe and we don't need to check for permissions.
            if (EvaluationContext.IsMachineLevel)
                return;

            PropertyInformation[] protectedProperties = {
                ElementInformation.Properties[ConfigurationStrings.CheckCertificateName],
                ElementInformation.Properties[ConfigurationStrings.CheckCertificateRevocationList]
            };

            foreach (PropertyInformation property in protectedProperties)
                if (property.ValueOrigin == PropertyValueOrigin.SetHere)
                {
                    try {
                        ExceptionHelper.UnmanagedPermission.Demand();
                    } catch (Exception exception) {
                        throw new ConfigurationErrorsException(
                                      SR.GetString(SR.net_config_property_permission, 
                                                   property.Name),
                                      exception);
                    }
                }
        }

        [ConfigurationProperty(ConfigurationStrings.CheckCertificateName, DefaultValue = true)]
        public bool CheckCertificateName
        {
            get { return (bool)this[this.checkCertificateName]; }
            set { this[this.checkCertificateName] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.CheckCertificateRevocationList, DefaultValue = false)]
        public bool CheckCertificateRevocationList
        {
            get { return (bool)this[this.checkCertificateRevocationList]; }
            set { this[this.checkCertificateRevocationList] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.DnsRefreshTimeout, DefaultValue = (int)( 2 * 60 * 1000))]
        public int DnsRefreshTimeout
        {
            get { return (int)this[this.dnsRefreshTimeout]; }
            set { this[this.dnsRefreshTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.EnableDnsRoundRobin, DefaultValue = false)]
        public bool EnableDnsRoundRobin
        {
            get { return (bool)this[this.enableDnsRoundRobin]; }
            set { this[this.enableDnsRoundRobin] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.EncryptionPolicy, DefaultValue = EncryptionPolicy.RequireEncryption)]
        public EncryptionPolicy EncryptionPolicy
        {
            get { return (EncryptionPolicy)this[this.encryptionPolicy]; }
            set { this[this.encryptionPolicy] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Expect100Continue, DefaultValue = true)]
        public bool Expect100Continue
        {
            get { return (bool)this[this.expect100Continue]; }
            set { this[this.expect100Continue] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseNagleAlgorithm, DefaultValue=true)]
        public bool UseNagleAlgorithm
        {
            get { return (bool)this[this.useNagleAlgorithm]; }
            set { this[this.useNagleAlgorithm] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get 
            {
                return this.properties;
            }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty checkCertificateName =
            new ConfigurationProperty(ConfigurationStrings.CheckCertificateName, 
                                      typeof(bool), 
                                      true, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty checkCertificateRevocationList =
            new ConfigurationProperty(ConfigurationStrings.CheckCertificateRevocationList, 
                                      typeof(bool), 
                                      false, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty dnsRefreshTimeout =
            new ConfigurationProperty(ConfigurationStrings.DnsRefreshTimeout, 
                                      typeof(int), 2 * 60 * 1000, 
                                      null, 
                                      new TimeoutValidator(true), 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty enableDnsRoundRobin =
            new ConfigurationProperty(ConfigurationStrings.EnableDnsRoundRobin, 
                                      typeof(bool), 
                                      false, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty encryptionPolicy =
            new ConfigurationProperty(ConfigurationStrings.EncryptionPolicy, 
                                      typeof(EncryptionPolicy), 
                                      EncryptionPolicy.RequireEncryption, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty expect100Continue =
            new ConfigurationProperty(ConfigurationStrings.Expect100Continue, 
                                      typeof(bool), 
                                      true, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty useNagleAlgorithm =
            new ConfigurationProperty(ConfigurationStrings.UseNagleAlgorithm, 
                                      typeof(bool), 
                                      true, 
                                      ConfigurationPropertyOptions.None); 
    }
}

