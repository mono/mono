//------------------------------------------------------------------------------
// <copyright file="UrlMappingsSection.cs" company="Microsoft">
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
    using System.ComponentModel;
    using System.Web.Util;
    using System.Diagnostics;
    using System.Security.Permissions;

    public sealed class UrlMappingsSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled", 
                                        typeof(bool), 
                                        true, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMappings =
            new ConfigurationProperty(null, 
                                        typeof(UrlMappingCollection), 
                                        null,
                                        ConfigurationPropertyOptions.IsDefaultCollection);
        #endregion

        static UrlMappingsSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propMappings);
            _properties.Add(_propEnabled);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public UrlMappingCollection UrlMappings {
            get {
                return (UrlMappingCollection)base[_propMappings];
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool IsEnabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        internal string HttpResolveMapping(string path) {
            string result = null;

            // Convert the 'path' param to be a relative path
            string relative = UrlPath.MakeVirtualPathAppRelative(path);

            // Look it up in our map
            UrlMapping elem = UrlMappings[relative];

            if (elem != null) {
                result = elem.MappedUrl;
            }
            return result;
        }
    }
}
