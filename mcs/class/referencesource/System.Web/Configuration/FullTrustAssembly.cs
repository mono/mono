//------------------------------------------------------------------------------
// <copyright file="FullTrustAssembly.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
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

    public sealed class FullTrustAssembly : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propAssemblyName =
            new ConfigurationProperty("assemblyName",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

        private static readonly ConfigurationProperty _propVersion =
            new ConfigurationProperty("version",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

        private static readonly ConfigurationProperty _propPublicKey =
            new ConfigurationProperty("publicKey",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        static FullTrustAssembly() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propAssemblyName);
            _properties.Add(_propVersion);
            _properties.Add(_propPublicKey);
        }


        internal FullTrustAssembly() {
        }

        public FullTrustAssembly(string assemblyName, string version, string publicKey) {
            AssemblyName = assemblyName;
            Version = version;
            PublicKey = publicKey;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("assemblyName", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string AssemblyName {
            get {
                return (string)base[_propAssemblyName];
            }
            set {
                base[_propAssemblyName] = value;
            }
        }

        [ConfigurationProperty("version", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Version {
            get {
                return (string)base[_propVersion];
            }
            set {
                base[_propVersion] = value;
            }
        }

        [ConfigurationProperty("publicKey", IsRequired = true, IsKey = false, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string PublicKey {
            get {
                return (string)base[_propPublicKey];
            }
            set {
                base[_propPublicKey] = value;
            }
        }
    }
}
