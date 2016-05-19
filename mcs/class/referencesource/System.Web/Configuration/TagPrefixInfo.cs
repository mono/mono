//------------------------------------------------------------------------------
// <copyright file="TagPrefixInfo.cs" company="Microsoft">
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
    using System.Web.UI;
    using System.Web.Compilation;
    using System.Threading;
    using System.Web.Configuration;
    using System.Security.Permissions;

    public sealed class TagPrefixInfo : ConfigurationElement {
        private static readonly ConfigurationElementProperty s_elemProperty =
            new ConfigurationElementProperty(new CallbackValidator(typeof(TagPrefixInfo), Validate));

        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propTagPrefix =
            new ConfigurationProperty("tagPrefix",
                                        typeof(string),
                                        "/",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propTagName =
            new ConfigurationProperty("tagName",
                                        typeof(string),
                                        String.Empty,
                                        null,
                                        null,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propNamespace =
            new ConfigurationProperty("namespace",
                                        typeof(string),
                                        String.Empty,
                                        null,
                                        null,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propAssembly =
            new ConfigurationProperty("assembly",
                                        typeof(string),
                                        String.Empty,
                                        null,
                                        null,
                                        ConfigurationPropertyOptions.IsAssemblyStringTransformationRequired);

        private static readonly ConfigurationProperty _propSource =
            new ConfigurationProperty("src",
                                        typeof(string),
                                        String.Empty,
                                        null,
                                        null,
                                        ConfigurationPropertyOptions.None);

        static TagPrefixInfo() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propTagPrefix);
            _properties.Add(_propTagName);
            _properties.Add(_propNamespace);
            _properties.Add(_propAssembly);
            _properties.Add(_propSource);

        }

        internal TagPrefixInfo() {
        }

        public TagPrefixInfo(String tagPrefix, String nameSpace, String assembly, String tagName, String source)
            : this() {
            TagPrefix = tagPrefix;
            Namespace = nameSpace;
            Assembly = assembly;
            TagName = tagName;
            Source = source;
        }

        public override bool Equals(object prefix) {
            TagPrefixInfo ns = prefix as TagPrefixInfo;
            return StringUtil.Equals(TagPrefix, ns.TagPrefix) &&
                    StringUtil.Equals(TagName, ns.TagName) &&
                    StringUtil.Equals(Namespace, ns.Namespace) &&
                    StringUtil.Equals(Assembly, ns.Assembly) &&
                    StringUtil.Equals(Source, ns.Source);
        }
        public override int GetHashCode() {
            return TagPrefix.GetHashCode() ^ TagName.GetHashCode() ^
                   Namespace.GetHashCode() ^ Assembly.GetHashCode() ^
                   Source.GetHashCode();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("tagPrefix", IsRequired = true, DefaultValue = "/")]
        [StringValidator(MinLength = 1)]
        public string TagPrefix {
            get {
                return (string)base[_propTagPrefix];
            }
            set {
                base[_propTagPrefix] = value;
            }
        }

        [ConfigurationProperty("tagName")]
        public string TagName {
            get {
                return (string)base[_propTagName];
            }
            set {
                base[_propTagName] = value;
            }
        }

        [ConfigurationProperty("namespace")]
        public string Namespace {
            get {
                return (string)base[_propNamespace];
            }
            set {
                base[_propNamespace] = value;
            }
        }

        [ConfigurationProperty("assembly")]
        public string Assembly {
            get {
                return (string)base[_propAssembly];
            }
            set {
                base[_propAssembly] = value;
            }
        }

        [ConfigurationProperty("src")]
        public string Source {
            get {
                return (string)base[_propSource];
            }
            set {
                if (!String.IsNullOrEmpty(value)) {
                    base[_propSource] = value;
                }
                else {
                    base[_propSource] = null;
                }

            }
        }
        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }
        private static void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("control");
            }

            TagPrefixInfo elem = (TagPrefixInfo)value;

            if (System.Web.UI.Util.ContainsWhiteSpace(elem.TagPrefix)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Space_attribute, "tagPrefix"));
            }

            bool invalid = false;

            if (!String.IsNullOrEmpty(elem.Namespace)) {
                // It is a custom control
                if (!(String.IsNullOrEmpty(elem.TagName) && String.IsNullOrEmpty(elem.Source))) {
                    invalid = true;
                }
            }
            else if (!String.IsNullOrEmpty(elem.TagName)) {
                // It is a user control
                if (!(String.IsNullOrEmpty(elem.Namespace) &&
                      String.IsNullOrEmpty(elem.Assembly) &&
                      !String.IsNullOrEmpty(elem.Source))) {
                    invalid = true;
                }
            }
            else {
                invalid = true;
            }

            if (invalid) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_tagprefix_entry));
            }
        }
    }
}
