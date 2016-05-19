//------------------------------------------------------------------------------
// <copyright file="TagMapCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(TagMapInfo))]
    public sealed class TagMapCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        private Hashtable _tagMappings;

        static TagMapCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }
        
        public TagMapCollection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public TagMapInfo this[int index] {
            get {
                return (TagMapInfo)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(TagMapInfo tagMapInformation) {
            BaseAdd(tagMapInformation);
        }

        public void Remove(TagMapInfo tagMapInformation) {
            BaseRemove(GetElementKey(tagMapInformation));
        }

        public void Clear() {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() {
            return new TagMapInfo();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((TagMapInfo)element).TagType;
        }

        internal Hashtable TagTypeMappingInternal {
            get {
                if (_tagMappings == null) {
                    lock (this) {
                        if (_tagMappings == null) {
                            Hashtable tagMappings = new Hashtable(StringComparer.OrdinalIgnoreCase);

                            foreach (TagMapInfo tmi in this) {
                                Type tagType = ConfigUtil.GetType(tmi.TagType, "tagType", tmi);
                                Type mappedTagType = ConfigUtil.GetType(tmi.MappedTagType, "mappedTagType", tmi);

                                if (tagType.IsAssignableFrom(mappedTagType) == false) {
                                    throw new ConfigurationErrorsException(
                                        SR.GetString(SR.Mapped_type_must_inherit, 
                                            tmi.MappedTagType, 
                                            tmi.TagType), 
                                        tmi.ElementInformation.Properties["mappedTagType"].Source, 
                                        tmi.ElementInformation.Properties["mappedTagType"].LineNumber);
                                }

                                tagMappings[tagType] = mappedTagType;
                            }
                            _tagMappings = tagMappings;
                        }
                    }
                }
                return _tagMappings;
            }
        }
    }
}
