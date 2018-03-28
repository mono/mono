//------------------------------------------------------------------------------
// <copyright file="WebPartsPersonalization.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System;
    using System.Configuration;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.Util;
    using System.Xml;
    using System.Security.Permissions;

    public sealed class WebPartsPersonalization : ConfigurationElement {

        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propDefaultProvider =
            new ConfigurationProperty(  "defaultProvider",
                                        typeof( string ),
                                        "AspNetSqlPersonalizationProvider",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None );

        private static readonly ConfigurationProperty _propProviders =
            new ConfigurationProperty("providers", 
                                        typeof(ProviderSettingsCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propAuthorization =
            new ConfigurationProperty("authorization", 
                                        typeof(WebPartsPersonalizationAuthorization), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        static WebPartsPersonalization() {
            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_propDefaultProvider);
            _properties.Add(_propProviders);
            _properties.Add(_propAuthorization);
        }

        public WebPartsPersonalization() {
        }

        [ConfigurationProperty("authorization")]
        public WebPartsPersonalizationAuthorization Authorization {
            get {
                return (WebPartsPersonalizationAuthorization)base[_propAuthorization];
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue = "AspNetSqlPersonalizationProvider")]
        [StringValidator(MinLength = 1)]
        public string DefaultProvider {
            get {
                return (string)base[_propDefaultProvider];
            }
            set {
                base[_propDefaultProvider] = value;
            }
        }

        /// <internalonly />
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers {
            get {
                return (ProviderSettingsCollection)base[_propProviders];
            }
        }

        internal void ValidateAuthorization() {
            foreach (AuthorizationRule rule in Authorization.Rules) {
                StringCollection verbs = rule.Verbs;
                if (verbs.Count == 0) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.WebPartsSection_NoVerbs),
                        rule.ElementInformation.Properties["verbs"].Source,
                        rule.ElementInformation.Properties["verbs"].LineNumber);
                }
                foreach (string verb in verbs) {
                    if (verb != "enterSharedScope" && verb != "modifyState") {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.WebPartsSection_InvalidVerb, verb),
                            rule.ElementInformation.Properties["verbs"].Source,
                            rule.ElementInformation.Properties["verbs"].LineNumber);
                    }
                }
            }
        }
    }
}
