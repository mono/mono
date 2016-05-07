//------------------------------------------------------------------------------
// <copyright file="HttpModuleAction.cs" company="Microsoft">
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
    using System.Web.Configuration.Common;
    using System.Web.Util;
    using System.Globalization;
    using System.Security.Permissions;

    public sealed class HttpModuleAction : ConfigurationElement {
        private static readonly ConfigurationElementProperty s_elemProperty =
            new ConfigurationElementProperty(new CallbackValidator(typeof(HttpModuleAction), Validate));

        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired |
                                        ConfigurationPropertyOptions.IsKey);

        private static readonly ConfigurationProperty _propType =
            new ConfigurationProperty("type", typeof(string), String.Empty, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);

        private ModulesEntry _modualEntry;

        static HttpModuleAction() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propType);
        }

        internal HttpModuleAction() {
        }

        public HttpModuleAction(String name, String type)
            : this() {
            Name = name;
            Type = type;
            _modualEntry = null;
        }

        internal string Key {
            get {
                return Name;
            }
        }

        protected override ConfigurationPropertyCollection Properties  {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true, DefaultValue = "")]
        public string Type {
            get {
                return (string)base[_propType];
            }
            set {
                base[_propType] = value;
            }
        }

        internal string FileName  {
            get {
                return ElementInformation.Properties["name"].Source;
            }

        }

        internal int LineNumber {
            get {
                return ElementInformation.Properties["name"].LineNumber;
            }
        }

        internal ModulesEntry Entry {
            get {
                try {
                    if (_modualEntry == null) {
                        _modualEntry = new ModulesEntry(Name, Type, _propType.Name, this);
                    }
                    return _modualEntry;
                }
                catch (Exception ex) {
                    throw new ConfigurationErrorsException(ex.Message,
                        ElementInformation.Properties[_propType.Name].Source, ElementInformation.Properties[_propType.Name].LineNumber);
                 }

            }
        }

        internal static bool IsSpecialModule(String className) {
            return ModulesEntry.IsTypeMatch(typeof(System.Web.Security.DefaultAuthenticationModule), className);
        }

        internal static bool IsSpecialModuleName(String name) {
            return (StringUtil.EqualsIgnoreCase(name, "DefaultAuthentication"));
        }

        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }

        private static void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("httpModule");
            }

            HttpModuleAction elem = (HttpModuleAction)value;

            if (HttpModuleAction.IsSpecialModule(elem.Type)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Special_module_cannot_be_added_manually, elem.Type),
                    elem.ElementInformation.Properties["type"].Source,
                    elem.ElementInformation.Properties["type"].LineNumber);
            }

            if (HttpModuleAction.IsSpecialModuleName(elem.Name)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Special_module_cannot_be_added_manually, elem.Name),
                    elem.ElementInformation.Properties["name"].Source,
                    elem.ElementInformation.Properties["name"].LineNumber);
            }
        }
    } // class HttpModule
}

