//------------------------------------------------------------------------------
// <copyright file="GlobalizationSection.cs" company="Microsoft">
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
    using System.Threading;
    using System.Globalization;
    using System.Web.Util;
    using System.Security.Permissions;

    /* 
        <!--
        globalization Attributes:
          requestEncoding="[Encoding value]" - Encoding to use for request
          responseEncoding="[Encoding value]" - Encoding to use for response
          enableBestFitResponseEncoding="[true|false]" - Enable best fit character encoding for response
          responseHeaderEncoding="[Encoding value]" - Encoding to use for response headers (default is utf-8)
          fileEncoding="[Encoding value]" - Encoding to use for files
          culture="[Culture]" - default Thread.CurrentCulture
          uiCulture="[Culture]" - default Thread.CurrentUICulture
          resourceProviderFactoryType="[type]"
        -->
        <globalization
                requestEncoding="utf-8"
                responseEncoding="utf-8"
        />

    */

    public sealed class GlobalizationSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propRequestEncoding =
            new ConfigurationProperty("requestEncoding", typeof(string), Encoding.UTF8.WebName, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResponseEncoding =
            new ConfigurationProperty("responseEncoding", typeof(string), Encoding.UTF8.WebName, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propFileEncoding =
            new ConfigurationProperty("fileEncoding", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCulture =
            new ConfigurationProperty("culture", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUICulture =
            new ConfigurationProperty("uiCulture", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableClientBasedCulture =
            new ConfigurationProperty("enableClientBasedCulture", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResponseHeaderEncoding =
            new ConfigurationProperty("responseHeaderEncoding", typeof(string), Encoding.UTF8.WebName, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResourceProviderFactoryType =
            new ConfigurationProperty("resourceProviderFactoryType", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableBestFitResponseEncoding =
            new ConfigurationProperty("enableBestFitResponseEncoding", typeof(bool), false, ConfigurationPropertyOptions.None);

        private Encoding responseEncodingCache = null;
        private Encoding responseHeaderEncodingCache = null;
        private Encoding requestEncodingCache = null;
        private Encoding fileEncodingCache = null;

        private String cultureCache = null;
        private String uiCultureCache = null;
        private Type _resourceProviderFactoryType;

        static GlobalizationSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propRequestEncoding);
            _properties.Add(_propResponseEncoding);
            _properties.Add(_propFileEncoding);
            _properties.Add(_propCulture);
            _properties.Add(_propUICulture);
            _properties.Add(_propEnableClientBasedCulture);
            _properties.Add(_propResponseHeaderEncoding);
            _properties.Add(_propResourceProviderFactoryType);
            _properties.Add(_propEnableBestFitResponseEncoding);
        }

        public GlobalizationSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("requestEncoding", DefaultValue = "utf-8")]
        public Encoding RequestEncoding {
            get {
                if (requestEncodingCache == null) {
                    requestEncodingCache = Encoding.UTF8;
                }
                return requestEncodingCache;
            }
            set {
                if (value != null) {
                    base[_propRequestEncoding] = value.WebName;
                    requestEncodingCache = value;
                }
                else {
                    base[_propRequestEncoding] = value;
                    requestEncodingCache = Encoding.UTF8;
                }
            }
        }

        [ConfigurationProperty("responseEncoding", DefaultValue = "utf-8")]
        public Encoding ResponseEncoding {
            get {
                if (responseEncodingCache == null)
                    responseEncodingCache = Encoding.UTF8;
                return responseEncodingCache;
            }
            set {
                if (value != null) {
                    base[_propResponseEncoding] = value.WebName;
                    responseEncodingCache = value;
                }
                else {
                    base[_propResponseEncoding] = value;
                    responseEncodingCache = Encoding.UTF8;
                }
            }
        }

        [ConfigurationProperty("responseHeaderEncoding", DefaultValue = "utf-8")]
        public Encoding ResponseHeaderEncoding {
            get {
                if (responseHeaderEncodingCache == null) {
                    responseHeaderEncodingCache = Encoding.UTF8;
                }
                return responseHeaderEncodingCache;
            }
            set {
                if (value != null) {
                    base[_propResponseHeaderEncoding] = value.WebName;
                    responseHeaderEncodingCache = value;
                }
                else {
                    base[_propResponseHeaderEncoding] = value;
                    responseHeaderEncodingCache = Encoding.UTF8;
                }
            }
        }

        [ConfigurationProperty("fileEncoding")]
        public Encoding FileEncoding {
            get {
                if (fileEncodingCache == null) {
                    fileEncodingCache = Encoding.Default;
                }
                return fileEncodingCache;
            }
            set {
                if (value != null) {
                    base[_propFileEncoding] = value.WebName;
                    fileEncodingCache = value;
                }
                else {
                    base[_propFileEncoding] = value;
                    fileEncodingCache = Encoding.Default;
                }
            }
        }

        [ConfigurationProperty("culture", DefaultValue = "")]
        public string Culture {
            get {
                if (cultureCache == null) {
                    cultureCache = (string)base[_propCulture];
                }
                return cultureCache;
            }
            set {
                base[_propCulture] = value;
                cultureCache = value;
            }
        }

        [ConfigurationProperty("uiCulture", DefaultValue = "")]
        public string UICulture {
            get {
                if (uiCultureCache == null) {
                    uiCultureCache = (string)base[_propUICulture];
                }
                return uiCultureCache;
            }
            set {
                base[_propUICulture] = value;
                uiCultureCache = value;
            }
        }

        [ConfigurationProperty("enableClientBasedCulture", DefaultValue = false)]
        public bool EnableClientBasedCulture {
            get {
                return (bool)base[_propEnableClientBasedCulture];
            }
            set {
                base[_propEnableClientBasedCulture] = value;
            }
        }

        [ConfigurationProperty("resourceProviderFactoryType", DefaultValue = "")]
        public string ResourceProviderFactoryType {
            get {
                return (string)base[_propResourceProviderFactoryType];
            }
            set {
                base[_propResourceProviderFactoryType] = value;
            }
        }

        [ConfigurationProperty("enableBestFitResponseEncoding", DefaultValue = false)]
        public bool EnableBestFitResponseEncoding {
            get {
                return (bool)base[_propEnableBestFitResponseEncoding];
            }
            set {
                base[_propEnableBestFitResponseEncoding] = value;
            }
        }

        internal Type ResourceProviderFactoryTypeInternal {
            get {
                if (_resourceProviderFactoryType == null && !String.IsNullOrEmpty(ResourceProviderFactoryType)) {
                    lock (this) {
                        if (_resourceProviderFactoryType == null) {
                            Type resourceProviderFactoryType = ConfigUtil.GetType(ResourceProviderFactoryType, "resourceProviderFactoryType", this);
                            ConfigUtil.CheckBaseType(typeof(System.Web.Compilation.ResourceProviderFactory), resourceProviderFactoryType, "resourceProviderFactoryType", this);
                            _resourceProviderFactoryType = resourceProviderFactoryType;
                        }
                    }
                }

                return _resourceProviderFactoryType;
            }
        }

        private void CheckCulture(string configCulture) {
            if (StringUtil.EqualsIgnoreCase(configCulture, HttpApplication.AutoCulture)) {
                return;
            }
            else if (StringUtil.StringStartsWithIgnoreCase(configCulture, HttpApplication.AutoCulture)) {
                // This will throw if bad
                CultureInfo dummyCultureInfo = new CultureInfo(configCulture.Substring(5));
                return;
            }
            // This will throw if bad
            new CultureInfo(configCulture);
        }

        protected override void PreSerialize(XmlWriter writer) {
            PostDeserialize();
        }

        protected override void PostDeserialize() {
            ConfigurationPropertyCollection props = Properties;

            // Need to check that the encodings provided are valid here
            ConfigurationProperty errorProperty = null;
            int errorLine = Int32.MaxValue;

            try {
                if (!String.IsNullOrEmpty((string)base[_propResponseEncoding]))
                    responseEncodingCache = (Encoding)Encoding.GetEncoding((string)base[_propResponseEncoding]);
            }
            catch {
                errorProperty = _propResponseEncoding;
                errorLine = ElementInformation.Properties[errorProperty.Name].LineNumber;
            }

            try {
                if (!String.IsNullOrEmpty((string)base[_propResponseHeaderEncoding])) {
                    responseHeaderEncodingCache = (Encoding)Encoding.GetEncoding((string)base[_propResponseHeaderEncoding]);
                }
            }
            catch {
                if (errorLine > ElementInformation.Properties[_propResponseHeaderEncoding.Name].LineNumber) {
                    errorProperty = _propResponseHeaderEncoding;
                    errorLine = ElementInformation.Properties[errorProperty.Name].LineNumber;
                }
            }

            try {
                if (!String.IsNullOrEmpty((string)base[_propRequestEncoding])) {
                    requestEncodingCache = (Encoding)Encoding.GetEncoding((string)base[_propRequestEncoding]);
                }
            }
            catch {
                if (errorLine > ElementInformation.Properties[_propRequestEncoding.Name].LineNumber) {
                    errorProperty = _propRequestEncoding;
                    errorLine = ElementInformation.Properties[errorProperty.Name].LineNumber;
                }
            }

            try {
                if (!String.IsNullOrEmpty((string)base[_propFileEncoding])) {
                    fileEncodingCache = (Encoding)Encoding.GetEncoding((string)base[_propFileEncoding]);
                }
            }
            catch {
                if (errorLine > ElementInformation.Properties[_propFileEncoding.Name].LineNumber) {
                    errorProperty = _propFileEncoding;
                    errorLine = ElementInformation.Properties[errorProperty.Name].LineNumber;
                }
            }
            try {
                if (!String.IsNullOrEmpty((string)base[_propCulture])) {
                    CheckCulture((string)base[_propCulture]);
                }
            }
            catch {
                if (errorLine > ElementInformation.Properties[_propCulture.Name].LineNumber) {
                    errorProperty = _propCulture;
                    errorLine = ElementInformation.Properties[_propCulture.Name].LineNumber;
                }
            }

            try {
                if (!String.IsNullOrEmpty((string)base[_propUICulture])) {
                    CheckCulture((string)base[_propUICulture]);
                }
            }
            catch {
                if (errorLine > ElementInformation.Properties[_propUICulture.Name].LineNumber) {
                    errorProperty = _propUICulture;
                    errorLine = ElementInformation.Properties[_propUICulture.Name].LineNumber;
                }
            }

            if (errorProperty != null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_value_for_globalization_attr, errorProperty.Name),
                    ElementInformation.Properties[errorProperty.Name].Source, ElementInformation.Properties[errorProperty.Name].LineNumber);

            }
        }
    }
}
