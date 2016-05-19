//------------------------------------------------------------------------------
// <copyright file="UrlMapping.cs" company="Microsoft">
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

    public sealed class UrlMapping : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty _propUrl =
            new ConfigurationProperty("url",
                                        typeof(string),
                                        null,
                                        StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                        new CallbackValidator(typeof(string), ValidateUrl),
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

        private static readonly ConfigurationProperty _propMappedUrl =
            new ConfigurationProperty("mappedUrl",
                                        typeof(string),
                                        null,
                                        StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);
        #endregion

        static UrlMapping() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propUrl);
            _properties.Add(_propMappedUrl);
        }

        internal UrlMapping() {
        }

        public UrlMapping(string url, string mappedUrl) {
            base[_propUrl] = url;
            base[_propMappedUrl] = mappedUrl;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("url", IsRequired = true, IsKey = true)]
        public string Url {
            get {
                return (string)base[_propUrl];
            }
        }

        [ConfigurationProperty("mappedUrl", IsRequired = true)]
        public string MappedUrl {
            get {
                return (string)base[_propMappedUrl];
            }
        }

        static private void ValidateUrl(object value) {
            // The Url cannot be an empty string. Use the std validator for that
            StdValidatorsAndConverters.NonEmptyStringValidator.Validate(value);

            string url = (string)value;

            if (!UrlPath.IsAppRelativePath(url)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.UrlMappings_only_app_relative_url_allowed, url));
            }
        }
    }
}
