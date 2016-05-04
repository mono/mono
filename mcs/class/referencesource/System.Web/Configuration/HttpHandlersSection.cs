//------------------------------------------------------------------------------
// <copyright file="HttpHandlersSection.cs" company="Microsoft">
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
    using System.Web.Util;
    using System.Web.Compilation;
    using System.Globalization;
    using System.Security.Permissions;

    public sealed class HttpHandlersSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propHandlers =
            new ConfigurationProperty(null, typeof(HttpHandlerActionCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        private bool _validated;

        static HttpHandlersSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propHandlers);
        }

        public HttpHandlersSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public HttpHandlerActionCollection Handlers {
            get {
                return (HttpHandlerActionCollection)base[_propHandlers];
            }
        }

        internal bool ValidateHandlers() {
            if (!_validated) {
                lock (this) {
                    if (!_validated) {
                        foreach (HttpHandlerAction ha in Handlers) {
                            ha.InitValidateInternal();
                        }

                        _validated = true;
                    }
                }
            }

            return _validated;
        }

        internal HttpHandlerAction FindMapping(String verb, VirtualPath path) {
            ValidateHandlers();

            for (int i = 0; i < Handlers.Count; i++) {
                HttpHandlerAction m = (HttpHandlerAction)Handlers[i];

                if (m.IsMatch(verb, path)) {
                    return m;
                }
            }

            return null;
        }
    }
}

