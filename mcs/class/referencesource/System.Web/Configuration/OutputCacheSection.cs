//------------------------------------------------------------------------------
// <copyright file="OutputCacheSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;
    using System.Web.Caching;

    /*             <!--
            outputCache Attributes:
              enableOutputCache="[true|false]" - Enable or disable the output cache
              enableFragmentCache="[true|false]" - Enable or disable the fragment (user control) cache
              sendCacheControlHeader="[true|false]" - Enables automatic insertion of "Cache-Control: private" header
              omitVaryStar="[true|false]" - Enables automatic insertion of "Vary: *" header
              enableKernelCacheForVaryByStar="[true|false]" - Enable kernel caching for "Vary: *" header even with non-empty query string
            -->
            <outputCache enableOutputCache="true" enableFragmentCache="true" sendCacheControlHeader = "true" omitVaryStar="false">
            </outputCache>
 */
    public sealed class OutputCacheSection : ConfigurationSection {
        internal const bool DefaultOmitVaryStar = false;

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propEnableOutputCache =
            new ConfigurationProperty("enableOutputCache", 
                                        typeof(bool), 
                                        true, 
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableFragmentCache =
            new ConfigurationProperty("enableFragmentCache", 
                                        typeof(bool), 
                                        true, 
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSendCacheControlHeader =
            new ConfigurationProperty("sendCacheControlHeader", 
                                        typeof(bool), 
                                        HttpRuntimeSection.DefaultSendCacheControlHeader, 
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propOmitVaryStar =
            new ConfigurationProperty("omitVaryStar", 
                                        typeof(bool), 
                                        DefaultOmitVaryStar, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propEnableKernelCacheForVaryByStar =
            new ConfigurationProperty("enableKernelCacheForVaryByStar", 
                                        typeof(bool), 
                                        false,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propDefaultProviderName =
            new ConfigurationProperty("defaultProvider",
                                      typeof(String),
                                      OutputCache.ASPNET_INTERNAL_PROVIDER_NAME, //defaultValue
                                      null,
                                      StdValidatorsAndConverters.NonEmptyStringValidator,
                                      ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propProviders = 
            new ConfigurationProperty("providers", 
                                      typeof(ProviderSettingsCollection), 
                                      null, // defaultValue
                                      ConfigurationPropertyOptions.None);


        private bool sendCacheControlHeaderCached = false;
        private bool sendCacheControlHeaderCache;
        private bool omitVaryStarCached = false;
        private bool omitVaryStar;
        private bool enableKernelCacheForVaryByStarCached = false;
        private bool enableKernelCacheForVaryByStar;
        private bool enableOutputCacheCached = false;
        private bool enableOutputCache;

        static OutputCacheSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_propEnableOutputCache);
            _properties.Add(_propEnableFragmentCache);
            _properties.Add(_propSendCacheControlHeader);
            _properties.Add(_propOmitVaryStar);
            _properties.Add(_propEnableKernelCacheForVaryByStar);
            _properties.Add(_propDefaultProviderName);
            _properties.Add(_propProviders);
        }

        public OutputCacheSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("enableOutputCache", DefaultValue = true)]
        public bool EnableOutputCache {
            get {
                if (enableOutputCacheCached == false) {
                    enableOutputCache = (bool)base[_propEnableOutputCache];
                    enableOutputCacheCached = true;
                }

                return enableOutputCache;
            }
            set {
                base[_propEnableOutputCache] = value;
                enableOutputCache = value;
            }
        }

        [ConfigurationProperty("enableFragmentCache", DefaultValue = true)]
        public bool EnableFragmentCache {
            get {
                return (bool)base[_propEnableFragmentCache];
            }
            set {
                base[_propEnableFragmentCache] = value;
            }
        }

        [ConfigurationProperty("sendCacheControlHeader", DefaultValue = HttpRuntimeSection.DefaultSendCacheControlHeader)]
        public bool SendCacheControlHeader {
            get {
                if (sendCacheControlHeaderCached == false) {
                    sendCacheControlHeaderCache = (bool)base[_propSendCacheControlHeader];
                    sendCacheControlHeaderCached = true;
                }
                return sendCacheControlHeaderCache;
            }
            set {
                base[_propSendCacheControlHeader] = value;
                sendCacheControlHeaderCache = value;
            }
        }

        [ConfigurationProperty("omitVaryStar", DefaultValue = DefaultOmitVaryStar)]
        public bool OmitVaryStar {
            get {
                if (omitVaryStarCached == false) {
                    omitVaryStar = (bool)base[_propOmitVaryStar];
                    omitVaryStarCached = true;
                }
                return omitVaryStar;
            }
            set {
                base[_propOmitVaryStar] = value;
                omitVaryStar = value;
            }
        }

        [ConfigurationProperty("enableKernelCacheForVaryByStar", DefaultValue = false)]
        public bool EnableKernelCacheForVaryByStar {
            get {
                if (enableKernelCacheForVaryByStarCached == false) {
                    enableKernelCacheForVaryByStar = (bool)base[_propEnableKernelCacheForVaryByStar];
                    enableKernelCacheForVaryByStarCached = true;
                }
                return enableKernelCacheForVaryByStar;
            }
            set {
                base[_propEnableKernelCacheForVaryByStar] = value;
                enableKernelCacheForVaryByStar = value;
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue = OutputCache.ASPNET_INTERNAL_PROVIDER_NAME)]
        [StringValidator(MinLength = 1)]
        public String DefaultProviderName {
            get {
                return (string) base[_propDefaultProviderName];
            }
            set {
                base[_propDefaultProviderName] = value;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers {
            get {
                return (ProviderSettingsCollection)base[_propProviders];
            }
        }
        
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal OutputCacheProviderCollection CreateProviderCollection() {
            // if there are no providers defined, we'll default to the v2.0 OutputCache
            ProviderSettingsCollection providers = Providers;
            if (providers == null || providers.Count == 0) {
                return null;
            }
            OutputCacheProviderCollection collection = new OutputCacheProviderCollection();
            ProvidersHelper.InstantiateProviders(providers, collection, typeof(OutputCacheProvider));
            collection.SetReadOnly();
            return collection;
        }

        internal OutputCacheProvider GetDefaultProvider(OutputCacheProviderCollection providers) {
            // if defaultProvider is undefined, we'll default to the v2.0 OutputCache
            string defaultProviderName = DefaultProviderName;
            if (defaultProviderName == OutputCache.ASPNET_INTERNAL_PROVIDER_NAME) {
                return null;
            }

            // if the defaultProvider is defined, it must be in the providers collection
            OutputCacheProvider defaultProvider = (providers == null) ? null : providers[defaultProviderName];
            if (defaultProvider == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Def_provider_not_found),
                                                       ElementInformation.Properties["defaultProvider"].Source,
                                                       ElementInformation.Properties["defaultProvider"].LineNumber);
            }
            return defaultProvider;
        }
    }
}
