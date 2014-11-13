//------------------------------------------------------------------------------
// <copyright file="HttpModulesSection.cs" company="Microsoft">
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
    using System.Web.Configuration;
    using System.Globalization;
    using System.Web.Security;
    using System.Security.Permissions;

    public sealed class HttpModulesSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propHttpModules =
            new ConfigurationProperty(null, 
                                        typeof(HttpModuleActionCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.IsDefaultCollection);

        static HttpModulesSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propHttpModules);
        }

        public HttpModulesSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public HttpModuleActionCollection Modules {
            get {
                return (HttpModuleActionCollection)base[_propHttpModules];
            }
        }

        internal HttpModuleCollection CreateModules() {
            HttpModuleCollection modules = new HttpModuleCollection();

            foreach (HttpModuleAction module in Modules) {
                modules.AddModule(module.Entry.ModuleName, module.Entry.Create());
            }

            modules.AddModule("DefaultAuthentication", DefaultAuthenticationModule.CreateDefaultAuthenticationModuleWithAssert());

            return modules;
        }
    }
}
