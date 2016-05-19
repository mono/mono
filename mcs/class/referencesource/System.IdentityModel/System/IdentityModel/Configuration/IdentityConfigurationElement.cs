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
    public sealed partial class IdentityConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)this[ConfigurationStrings.Name]; }
            set { this[ConfigurationStrings.Name] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.AudienceUris, IsRequired = false)]
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

        [ConfigurationProperty(ConfigurationStrings.ClaimsAuthenticationManager, IsRequired = false)]
        public CustomTypeElement ClaimsAuthenticationManager
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.ClaimsAuthenticationManager]; }
            set { this[ConfigurationStrings.ClaimsAuthenticationManager] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ClaimsAuthorizationManager, IsRequired = false)]
        public CustomTypeElement ClaimsAuthorizationManager
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.ClaimsAuthorizationManager]; }
            set { this[ConfigurationStrings.ClaimsAuthorizationManager] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuerNameRegistry, IsRequired = false)]
        public IssuerNameRegistryElement IssuerNameRegistry
        {
            get { return (IssuerNameRegistryElement)this[ConfigurationStrings.IssuerNameRegistry]; }
            set { this[ConfigurationStrings.IssuerNameRegistry] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuerTokenResolver, IsRequired = false)]
        public CustomTypeElement IssuerTokenResolver
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.IssuerTokenResolver]; }
            set { this[ConfigurationStrings.IssuerTokenResolver] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.MaximumClockSkew, IsRequired = false, DefaultValue = IdentityConfiguration.DefaultMaxClockSkewString )]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [IdentityModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaximumClockSkew
        {
            get { return (TimeSpan)this[ConfigurationStrings.MaximumClockSkew]; }
            set { this[ConfigurationStrings.MaximumClockSkew] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SaveBootstrapContext, IsRequired = false, DefaultValue = false)]
        public bool SaveBootstrapContext
        {
            get { return (bool)this[ConfigurationStrings.SaveBootstrapContext]; }
            set { this[ConfigurationStrings.SaveBootstrapContext] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceTokenResolver, IsRequired = false)]
        public CustomTypeElement ServiceTokenResolver
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.ServiceTokenResolver]; }
            set { this[ConfigurationStrings.ServiceTokenResolver] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TokenReplayDetection, IsRequired = false)]
        public TokenReplayDetectionElement TokenReplayDetection
        {
            get { return (TokenReplayDetectionElement)this[ConfigurationStrings.TokenReplayDetection]; }
            set { this[ConfigurationStrings.TokenReplayDetection] = value; }
        }

        /// <summary>
        /// A collection of SecurityTokenHandlerCollection elements.
        /// </summary>
        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public SecurityTokenHandlerSetElementCollection SecurityTokenHandlerSets
        {
            get { return (SecurityTokenHandlerSetElementCollection)this[ConfigurationStrings.DefaultConfigurationElementName]; }
        }

        // This config element is being marked as internal cause we need this just to make the App registration
        // tool to work. We do not want the application to use it. 
        // FIP 6495
        [ConfigurationProperty(ConfigurationStrings.ApplicationService, IsRequired = false)]
        internal ApplicationServiceConfigurationElement ApplicationService
        {
            get { return (ApplicationServiceConfigurationElement)this[ConfigurationStrings.ApplicationService]; }
            set { this[ConfigurationStrings.ApplicationService] = value; }
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return ((ElementInformation.Properties[ConfigurationStrings.Name].ValueOrigin != PropertyValueOrigin.Default) ||
                         AudienceUris.IsConfigured ||
                         Caches.IsConfigured ||
                         CertificateValidation.IsConfigured ||
                         ClaimsAuthenticationManager.IsConfigured ||
                         ClaimsAuthorizationManager.IsConfigured ||
                         IssuerNameRegistry.IsConfigured ||
                         IssuerTokenResolver.IsConfigured ||
                         (ElementInformation.Properties[ConfigurationStrings.SaveBootstrapContext].ValueOrigin != PropertyValueOrigin.Default ) ||
                         (ElementInformation.Properties[ConfigurationStrings.MaximumClockSkew].ValueOrigin != PropertyValueOrigin.Default) ||
                         ServiceTokenResolver.IsConfigured ||
                         TokenReplayDetection.IsConfigured ||
                         SecurityTokenHandlerSets.IsConfigured);
            }
        }
    }
#pragma warning restore 1591
}
