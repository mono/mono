//------------------------------------------------------------------------------
// <copyright file="SiteMapSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;

    /*         <!-- Configuration for siteMap:
               Attributes:
                  defaultProvider="string"  Name of provider to use by default
                  enabled="[true|false]"    Determine if the feature is enabled.

                <providers>              Providers (class must inherit from SiteMapProvider)

                    <add                 Add a provider
                        name="string"    Required string by which the SiteMap class identifies this provider
                        type="string"    Required string which represents the type to instantiate: type must inherit from SiteMapProvider
                        securityTrimmingEnabled="[true|false]"   Determine if security trimming is enabled. (default is false)
                        provider-specific-configuration />

                    <remove              Remove a provider
                        name="string" /> Name of provider to remove

                    <clear/>             Remove all providers
        -->
        <siteMap defaultProvider="AspNetXmlSiteMapProvider" enabled="true">
            <providers>
                <add name="AspNetXmlSiteMapProvider"
                     description="SiteMap provider which reads in .sitemap XML files."
                     type="System.Web.XmlSiteMapProvider, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                     siteMapFile="web.sitemap" />
            </providers>
        </siteMap>
*/
    public sealed class SiteMapSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        
        private static readonly ConfigurationProperty _propDefaultProvider =
            new ConfigurationProperty("defaultProvider",
                                        typeof(string),
                                        "AspNetXmlSiteMapProvider",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled", 
                                        typeof(bool), 
                                        true, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propProviders =
            new ConfigurationProperty("providers", 
                                        typeof(ProviderSettingsCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        private SiteMapProviderCollection _siteMapProviders;

        static SiteMapSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propDefaultProvider);
            _properties.Add(_propEnabled);
            _properties.Add(_propProviders);
        }

        public SiteMapSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue = "AspNetXmlSiteMapProvider")]
        [StringValidator(MinLength = 1)]
        public string DefaultProvider {
            get {
                return (string)base[_propDefaultProvider];
            }
            set {
                base[_propDefaultProvider] = value;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers {
            get {
                return (ProviderSettingsCollection)base[_propProviders];
            }
        }

        internal SiteMapProviderCollection ProvidersInternal {
            get {
                if (_siteMapProviders == null) {
                    lock (this) {
                        if (_siteMapProviders == null) {
                            SiteMapProviderCollection siteMapProviders = new SiteMapProviderCollection();
                            ProvidersHelper.InstantiateProviders(Providers, siteMapProviders, typeof(SiteMapProvider));

                            _siteMapProviders = siteMapProviders;
                        }
                    }
                }

                return _siteMapProviders;
            }
        }

        internal void ValidateDefaultProvider() {
            if (!String.IsNullOrEmpty(DefaultProvider)) // make sure the specified provider has a provider entry in the collection
            {
                if (Providers[DefaultProvider] == null) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Config_provider_must_exist, DefaultProvider),
                        ElementInformation.Properties[_propDefaultProvider.Name].Source, 
                        ElementInformation.Properties[_propDefaultProvider.Name].LineNumber);
                }
            }
        }
    } // class SiteMapSection
}
