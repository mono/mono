//------------------------------------------------------------------------------
// <copyright file="TransformerInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System;
    using System.Configuration;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.Util;
    using System.Xml;
    using System.Security.Permissions;

    public sealed class TransformerInfo : ConfigurationElement {

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
            new ConfigurationProperty("type",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);

        static TransformerInfo() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propType);
        }

        internal TransformerInfo() {
        }

        public TransformerInfo(string name, string type)
            : this() {
            Name = name;
            Type = type;
        }

        [ConfigurationProperty("name", IsRequired = true, DefaultValue = "", IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        /// <internalonly />
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
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

        /// <internalonly />
        public override bool Equals(object o) {
            if (o == this) {
                return true;
            }

            TransformerInfo ti = o as TransformerInfo;
            return StringUtil.Equals(Name, ti.Name) &&
                   StringUtil.Equals(Type, ti.Type);
        }

        /// <internalonly />
        public override int GetHashCode() {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }
    }
}
