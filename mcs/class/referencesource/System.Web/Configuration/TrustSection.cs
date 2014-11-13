//------------------------------------------------------------------------------
// <copyright file="TrustSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;

    /*****************************************************************************
     From machine.config
    <!--  level="[Full|High|Medium|Low|Minimal]" -->
        <trust level="Full" originUrl="" />
     [SectionComment("<!--  level=\"[Full|High|Medium|Low|Minimal]\" -->")]
    ******************************************************************************/

    public sealed class TrustSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        
        private static readonly ConfigurationProperty _propLevel =
            new ConfigurationProperty("level",
                                        typeof(string),
                                        "Full",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propOriginUrl =
            new ConfigurationProperty("originUrl", 
                                        typeof(string), 
                                        String.Empty, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propProcessRequestInApplicationTrust =
            new ConfigurationProperty("processRequestInApplicationTrust",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propLegacyCasModel =
            new ConfigurationProperty("legacyCasModel",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propPermissionSetName =
            new ConfigurationProperty("permissionSetName",
                                        typeof(string),
                                        "ASP.Net",
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propHostSecurityPolicyResolverType =
            new ConfigurationProperty("hostSecurityPolicyResolverType",
                                        typeof(string),
                                        String.Empty,
                                        ConfigurationPropertyOptions.None);

        static TrustSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propLevel);
            _properties.Add(_propOriginUrl);
            _properties.Add(_propProcessRequestInApplicationTrust);
            _properties.Add(_propLegacyCasModel);
            _properties.Add(_propPermissionSetName);
            _properties.Add(_propHostSecurityPolicyResolverType);
        }

        public TrustSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("level", IsRequired = true, DefaultValue = "Full")]
        [StringValidator(MinLength = 1)]
        public string Level {
            get {
                return (string)base[_propLevel];
            }
            set {
                base[_propLevel] = value;
            }
        }

        [ConfigurationProperty("originUrl", DefaultValue = "")]
        public string OriginUrl {
            get {
                return (string)base[_propOriginUrl];
            }
            set {
                base[_propOriginUrl] = value;
            }
        }

        [ConfigurationProperty("processRequestInApplicationTrust", DefaultValue = true)]
        public bool ProcessRequestInApplicationTrust {
            get {
                return (bool)base[_propProcessRequestInApplicationTrust];
            }
            set {
                base[_propProcessRequestInApplicationTrust] = value;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Cas as Identifier is already used in CLR")]
        [ConfigurationProperty("legacyCasModel", DefaultValue = false)]
        public bool LegacyCasModel {
            get {
                return (bool)base[_propLegacyCasModel];
            }
            set {
                base[_propLegacyCasModel] = value;
            }
        }

        [ConfigurationProperty("permissionSetName", DefaultValue = "ASP.Net")]
        public string PermissionSetName {
            get {
                return (string)base[_propPermissionSetName];
            }
            set {
                base[_propPermissionSetName] = value;
            }
        }

        [ConfigurationProperty("hostSecurityPolicyResolverType", DefaultValue = "")]
        public string HostSecurityPolicyResolverType {
            get {
                return (string)base[_propHostSecurityPolicyResolverType];
            }
            set {
                base[_propHostSecurityPolicyResolverType] = value;
            }
        }
    }
}
