//------------------------------------------------------------------------------
// <copyright file="UrlMappingCollection.cs" company="Microsoft">
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
    using System.ComponentModel;
    using System.Web.Util;
    using System.Diagnostics;
    using System.Security.Permissions;    

    [ConfigurationCollection(typeof(UrlMapping))]
    public sealed class UrlMappingCollection : ConfigurationElementCollection {
        private static readonly ConfigurationPropertyCollection _properties;

        static UrlMappingCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public UrlMappingCollection()
            :
            base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public String[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }

        public String GetKey(int index) {
            return (String)BaseGetKey(index);
        }

        public void Add(UrlMapping urlMapping) {
            BaseAdd(urlMapping);
        }

        public void Remove(string name) {
            BaseRemove(name);
        }

        public void Remove(UrlMapping urlMapping) {
            BaseRemove(GetElementKey(urlMapping));
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public new UrlMapping this[string name] {
            get {
                return (UrlMapping)BaseGet(name);
            }
        }

        public UrlMapping this[int index] {
            get {
                return (UrlMapping)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        public void Clear() {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() {
            return new UrlMapping();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((UrlMapping)element).Url;
        }
    }
}
