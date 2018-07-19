//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Configuration;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Configuration
{
#pragma warning disable 1591
    /// <summary>
    /// Manages configuration for all the Security Token Handlers.
    /// </summary>
    public sealed partial class SecurityTokenHandlerConfigurationElement : ConfigurationElement
    {
        public SecurityTokenHandlerConfigurationElement()
        {
        }

        protected override void Init()
        {
            Name = SecurityTokenHandlerCollectionManager.Usage.Default;
        }

        [ConfigurationProperty( ConfigurationStrings.AudienceUris, IsRequired = false )]
        public AudienceUriElementCollection AudienceUris
        {
            get { return (AudienceUriElementCollection)this[ConfigurationStrings.AudienceUris]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Caches, IsRequired = false)]
        public IdentityModelCachesElement Caches
        {
            get { return (IdentityModelCachesElement)this[ConfigurationStrings.Caches]; }
            set { this[ConfigurationStrings.Caches] = value; }
        }
        
        [ConfigurationProperty(ConfigurationStrings.X509CertificateValidation, IsRequired = false)]
        public X509CertificateValidationElement CertificateValidation
        {
            get { return (X509CertificateValidationElement)this[ConfigurationStrings.X509CertificateValidation]; }
            set { this[ConfigurationStrings.X509CertificateValidation] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.IssuerNameRegistry, IsRequired = false )]
        public IssuerNameRegistryElement IssuerNameRegistry
        {
            get { return (IssuerNameRegistryElement)this[ConfigurationStrings.IssuerNameRegistry]; }
            set { this[ConfigurationStrings.IssuerNameRegistry] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.IssuerTokenResolver, IsRequired = false )]
        public CustomTypeElement IssuerTokenResolver
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.IssuerTokenResolver]; }
            set { this[ConfigurationStrings.IssuerTokenResolver] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.Name, IsRequired = false, Options = ConfigurationPropertyOptions.IsKey )]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)this[ConfigurationStrings.Name]; }
            set { this[ConfigurationStrings.Name] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.SaveBootstrapContext, IsRequired = false, DefaultValue = false )]
        public bool SaveBootstrapContext
        {
            get { return (bool)this[ConfigurationStrings.SaveBootstrapContext]; }
            set { this[ConfigurationStrings.SaveBootstrapContext] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.MaximumClockSkew, IsRequired = false, DefaultValue = IdentityConfiguration.DefaultMaxClockSkewString )]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [IdentityModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaximumClockSkew
        {
            get { return (TimeSpan)this[ConfigurationStrings.MaximumClockSkew]; }
            set { this[ConfigurationStrings.MaximumClockSkew] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.ServiceTokenResolver, IsRequired = false )]
        public CustomTypeElement ServiceTokenResolver
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.ServiceTokenResolver]; }
            set { this[ConfigurationStrings.ServiceTokenResolver] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.TokenReplayDetection, IsRequired = false )]
        public TokenReplayDetectionElement TokenReplayDetection
        {
            get { return (TokenReplayDetectionElement)this[ConfigurationStrings.TokenReplayDetection]; }
            set { this[ConfigurationStrings.TokenReplayDetection] = value; }
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return ( AudienceUris.IsConfigured ||
                         Caches.IsConfigured || 
                         CertificateValidation.IsConfigured ||
                         IssuerNameRegistry.IsConfigured ||
                         IssuerTokenResolver.IsConfigured ||
                         ( ElementInformation.Properties[ConfigurationStrings.Name].ValueOrigin != PropertyValueOrigin.Default ) ||
                         ( ElementInformation.Properties[ConfigurationStrings.SaveBootstrapContext].ValueOrigin != PropertyValueOrigin.Default ) ||
                         ( ElementInformation.Properties[ConfigurationStrings.MaximumClockSkew].ValueOrigin != PropertyValueOrigin.Default ) ||
                         ServiceTokenResolver.IsConfigured ||
                         TokenReplayDetection.IsConfigured );
            }
        }
    }
#pragma warning restore 1591
}
