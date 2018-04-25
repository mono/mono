//------------------------------------------------------------------------------
// <copyright file="ClientTarget.cs" company="Microsoft">
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

    public sealed class ClientTarget : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty _propAlias =
            new ConfigurationProperty("alias",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propUserAgent =
            new ConfigurationProperty("userAgent",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);
        #endregion

        static ClientTarget() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propAlias);
            _properties.Add(_propUserAgent);
        }
        internal ClientTarget() {
        }
        public ClientTarget(string alias, string userAgent) {
            base[_propAlias] = alias;
            base[_propUserAgent] = userAgent;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("alias", IsRequired = true, IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Alias {
            get {
                return (string)base[_propAlias];
            }
        }

        [ConfigurationProperty("userAgent", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string UserAgent {
            get {
                return (string)base[_propUserAgent];
            }
        }
    }
}
