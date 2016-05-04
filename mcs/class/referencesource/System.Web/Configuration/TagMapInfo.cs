//------------------------------------------------------------------------------
// <copyright file="TagMapInfo.cs" company="Microsoft">
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

    public sealed class TagMapInfo : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        
        private static readonly ConfigurationProperty _propTagTypeName =
            new ConfigurationProperty("tagType",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | 
                                        ConfigurationPropertyOptions.IsKey);
        
        private static readonly ConfigurationProperty _propMappedTagTypeName =
            new ConfigurationProperty("mappedTagType",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        static TagMapInfo() {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propTagTypeName);
            _properties.Add(_propMappedTagTypeName);
        }

        internal TagMapInfo() {
        }

        public TagMapInfo(String tagTypeName, String mappedTagTypeName)
            : this() {
            TagType = tagTypeName;
            MappedTagType = mappedTagTypeName;
        }

        public override bool Equals(object o) {
            TagMapInfo tm = o as TagMapInfo;
            return StringUtil.Equals(TagType, tm.TagType) &&
                   StringUtil.Equals(MappedTagType, tm.MappedTagType);
        }

        public override int GetHashCode() {
            return TagType.GetHashCode() ^ MappedTagType.GetHashCode();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("mappedTagType")]
        [StringValidator(MinLength = 1)]
        public string MappedTagType {
            get {
                return (string)base[_propMappedTagTypeName];
            }
            set {
                base[_propMappedTagTypeName] = value;
            }
        }

        [ConfigurationProperty("tagType", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string TagType {
            get {
                return (string)base[_propTagTypeName];
            }
            set {
                base[_propTagTypeName] = value;
            }
        }

        void Verify() {
            if (String.IsNullOrEmpty(TagType)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Config_base_required_attribute_missing, 
                        "tagType"));
            }

            if (String.IsNullOrEmpty(MappedTagType)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Config_base_required_attribute_missing, 
                        "mappedTagType"));
            }
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey) {
            Verify();
            return base.SerializeElement(writer, serializeCollectionKey);
        }
    }
}
