//------------------------------------------------------------------------------
// <copyright file="NamespaceInfo.cs" company="Microsoft">
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

    public sealed class NamespaceInfo : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propNamespace =
            new ConfigurationProperty("namespace",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | 
                                        ConfigurationPropertyOptions.IsKey);

        static NamespaceInfo() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propNamespace);
        }



        internal NamespaceInfo() {
        }

        public NamespaceInfo(String name)
            : this() {
            Namespace = name;
        }

        public override bool Equals(object namespaceInformation) {
            NamespaceInfo ns = namespaceInformation as NamespaceInfo;
            return (ns != null && Namespace == ns.Namespace);
        }
        public override int GetHashCode() {
            return Namespace.GetHashCode();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("namespace", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Namespace {
            get {
                return (string)base[_propNamespace];
            }
            set {
                base[_propNamespace] = value;
            }
        }
    }
}
