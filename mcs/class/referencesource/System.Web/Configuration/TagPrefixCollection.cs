//------------------------------------------------------------------------------
// <copyright file="TagPrefixCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(TagPrefixInfo), AddItemName = "add", 
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class TagPrefixCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;


        static TagPrefixCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }
        public TagPrefixCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public TagPrefixInfo this[int index] {
            get {
                return (TagPrefixInfo)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override bool ThrowOnDuplicate {
            get {
                return false;
            }
        }

        public void Add(TagPrefixInfo tagPrefixInformation) {
            BaseAdd(tagPrefixInformation);
        }

        public void Remove(TagPrefixInfo tagPrefixInformation) {
            BaseRemove(GetElementKey(tagPrefixInformation));
        }

        public void Clear() {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() {
            return new TagPrefixInfo();
        }

        protected override string ElementName {
            get {
                return "add";
            }
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            TagPrefixInfo info = (TagPrefixInfo)element;

            if (String.IsNullOrEmpty(info.TagName)) {
                return info.TagPrefix + ":" + info.Namespace + ":" +  
                      (String.IsNullOrEmpty(info.Assembly) ? string.Empty : info.Assembly);
            }
            else {
                return info.TagPrefix + ":" + info.TagName;
            }
        }
    }
}
