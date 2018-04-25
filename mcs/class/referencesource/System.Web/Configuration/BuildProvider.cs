//------------------------------------------------------------------------------
// <copyright file="BuildProvider.cs" company="Microsoft">
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
    using System.Web.Compilation;
    using System.Reflection;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.CodeDom.Compiler;
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    public sealed class BuildProvider : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propExtension =
            new ConfigurationProperty("extension",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propType =
            new ConfigurationProperty("type",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);

        private readonly BuildProviderInfo _info;

        static BuildProvider() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propExtension);
            _properties.Add(_propType);
        }

        public BuildProvider(String extension, String type)
            : this() {
            Extension = extension;
            Type = type;
        }
        internal BuildProvider() {
            _info = new ConfigurationBuildProviderInfo(this);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        // this override is required because AppliesTo may be in any order in the
        // property string but it still and the default equals operator would consider
        // them different depending on order in the persisted string.
        public override bool Equals(object provider) {
            BuildProvider o = provider as BuildProvider;
            return (o != null && StringUtil.EqualsIgnoreCase(Extension, o.Extension) && Type == o.Type);
        }
        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(StringUtil.GetNonRandomizedHashCode(Extension.ToLower(CultureInfo.InvariantCulture)),
                                                     Type.GetHashCode());
        }


        [ConfigurationProperty("extension", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Extension {
            get {
                return (string)base[_propExtension];
            }
            set {
                base[_propExtension] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Type {
            get {
                return (string)base[_propType];
            }
            set {
                base[_propType] = value;
            }
        }

        internal BuildProviderInfo BuildProviderInfo {
            get {
                Debug.Assert(_info != null);
                return _info;
            }
        }

        private class ConfigurationBuildProviderInfo : BuildProviderInfo {
            private readonly BuildProvider _buildProvider;
            object _lock = new object();
            private Type _type;

            public ConfigurationBuildProviderInfo(BuildProvider buildProvider) {
                Debug.Assert(buildProvider != null);
                _buildProvider = buildProvider;
            }

            internal override Type Type {
                get {
                    if (_type == null) {
                        lock (_lock) {
                            if (_type == null) {
                                _type = CompilationUtil.LoadTypeWithChecks(_buildProvider.Type, typeof(System.Web.Compilation.BuildProvider), null, _buildProvider, "type");
                            }
                        }
                    }

                    return _type;
                }
            }
        }
    }
}
