//------------------------------------------------------------------------------
// <copyright file="ScriptingScriptResourceHandlerSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Script.Serialization;

    public sealed class ScriptingScriptResourceHandlerSection : ConfigurationSection {
        private static readonly ConfigurationProperty _propEnableCaching =
            new ConfigurationProperty("enableCaching",
                                    typeof(bool),
                                    true,
                                    ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propEnableCompression =
            new ConfigurationProperty("enableCompression",
                                    typeof(bool),
                                    true,
                                    ConfigurationPropertyOptions.None);

        private static ConfigurationPropertyCollection _properties = BuildProperties();

        private static ConfigurationPropertyCollection BuildProperties() {
            ConfigurationPropertyCollection props = new ConfigurationPropertyCollection();
            props.Add(_propEnableCaching);
            props.Add(_propEnableCompression);
            return props;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("enableCaching", DefaultValue = true)]
        public bool EnableCaching {
            get {
                return (bool)base[_propEnableCaching];
            }
            set {
                base[_propEnableCaching] = value;
            }
        }

        [ConfigurationProperty("enableCompression", DefaultValue = true)]
        public bool EnableCompression {
            get {
                return (bool)base[_propEnableCompression];
            }
            set {
                base[_propEnableCompression] = value;
            }
        }

        internal static class ApplicationSettings {
            private volatile static bool s_sectionLoaded;
            private static bool s_enableCaching;
            private static bool s_enableCompression;

            private static void EnsureSectionLoaded() {
                if (!s_sectionLoaded) {
                    ScriptingScriptResourceHandlerSection section = (ScriptingScriptResourceHandlerSection)
                        WebConfigurationManager.GetWebApplicationSection("system.web.extensions/scripting/scriptResourceHandler");

                    if (section != null) {
                        s_enableCaching = section.EnableCaching;
                        s_enableCompression = section.EnableCompression;
                    }
                    else {
                        s_enableCaching = (bool)_propEnableCaching.DefaultValue;
                        s_enableCompression = (bool)_propEnableCompression.DefaultValue;
                    }

                    s_sectionLoaded = true;
                }
            }

            internal static bool EnableCaching {
                get {
                    EnsureSectionLoaded();
                    return s_enableCaching;
                }
            }

            internal static bool EnableCompression {
                get {
                    EnsureSectionLoaded();
                    return s_enableCompression;
                }
            }
        }
    }
}
