//------------------------------------------------------------------------------
// <copyright file="WebControlsSection.cs" company="Microsoft">
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

    public sealed class WebControlsSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty _propClientScriptsLocation =
            new ConfigurationProperty("clientScriptsLocation",
                                        typeof(string),
                                        "/aspnet_client/{0}/{1}/",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);
        #endregion

        static WebControlsSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propClientScriptsLocation);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        protected override object GetRuntimeObject() {
            // Legacy section returned a Hashtable and people are depenant on that implimentation.
            Hashtable runtimeHashTable = new Hashtable();
            foreach (ConfigurationProperty prop in Properties) {
                runtimeHashTable[prop.Name] = base[prop];
            }

            return runtimeHashTable;            // return the read only object
        }

        [ConfigurationProperty("clientScriptsLocation", IsRequired = true, DefaultValue = "/aspnet_client/{0}/{1}/")]
        [StringValidator(MinLength = 1)]
        public string ClientScriptsLocation {
            get {
                return (string)base[_propClientScriptsLocation];
            }
        }
    }
}
