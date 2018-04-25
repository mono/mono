//------------------------------------------------------------------------------
// <copyright file="OutputCacheSettingsSection.cs" company="Microsoft">
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
    using System.Web.UI;
    using System.ComponentModel;
    using System.Security.Permissions;

    /*             <outputCacheSettings>
                <!--
                outputCacheProfiles Attributes:
                  name="string" - Name of the output cache profile
                  enabled="[true|false]" - Enable or disables caching of the content
                  duration="int" - Seconds to store the data
                  location="[]" - Location where the data is permitted
                  shared="[true|false]" -
                  sqlDependency="string" - Sql dependency string
                  varyByControl="string" - semi-colon separated list of controls to vary by
                  varyByCustom="string" - list of custom strings to vary by
                  varyByContentEncoding="string" - semi-colon separated list of codings to vary by
                  varyByHeader="string" - semi-colon separated list of headers to vary by
                  varyByParam="string" - semi-colon separated list of parameters to vary by
                  noStore="[true|false]" - Set the No-Store header on the response
                -->
                <outputCacheProfiles>
                </outputCacheProfiles>
                <fragmentCacheProfiles>
                </fragmentCacheProfiles>
            </outputCacheSettings>

 */
    public sealed class OutputCacheSettingsSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propOutputCacheProfiles;
#if NOT_UNTIL_LATER
        private static readonly ConfigurationProperty _propFragmentCacheProfiles;
#endif
        static OutputCacheSettingsSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _propOutputCacheProfiles = new ConfigurationProperty("outputCacheProfiles", 
                                            typeof(OutputCacheProfileCollection), 
                                            null, 
                                            ConfigurationPropertyOptions.None);
            _properties.Add(_propOutputCacheProfiles);

#if NOT_UNTIL_LATER
            _propFragmentCacheProfiles = new ConfigurationProperty("fragmentCacheProfiles", 
                                        typeof(FragmentCacheProfileCollection), 
                                        new FragmentCacheProfileCollection(), 
                                        ConfigurationPropertyOptions.None);
            _properties.Add(_propFragmentCacheProfiles);
#endif
        }

        public OutputCacheSettingsSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("outputCacheProfiles")]
        public OutputCacheProfileCollection OutputCacheProfiles {
            get {
                return (OutputCacheProfileCollection)base[_propOutputCacheProfiles];
            }
        }

#if NOT_UNTIL_LATER
        [ConfigurationProperty("FragmentCacheProfiles")]
        public FragmentCacheProfileCollection FragmentCacheProfiles {
            get {
                return (FragmentCacheProfileCollection) base[_propFragmentCacheProfiles];
            }
        }
#endif

    }
}
