//------------------------------------------------------------------------------
// <copyright file="OutputCacheProfileCollection.cs" company="Microsoft">
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
    using System.Web.UI;
    using System.ComponentModel;
    using System.Web.Util;
    using System.Security.Permissions;


    [ConfigurationCollection(typeof(OutputCacheProfile))]
    public sealed class OutputCacheProfileCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static OutputCacheProfileCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }
        public OutputCacheProfileCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        // public properties
        public String[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }

        public new OutputCacheProfile this[string name] {
            get {
                return (OutputCacheProfile)BaseGet(name);
            }
            // Having a setter here would be strange in that you could write
            //  collection["Name1"] = new OutputCacheProfile("differentName"...
            // 
        }

        public OutputCacheProfile this[int index] {
            get {
                return (OutputCacheProfile)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new OutputCacheProfile();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((OutputCacheProfile)element).Name;
        }

        // public methods
        public void Add(OutputCacheProfile name) {
            BaseAdd(name);
        }

        public void Clear() {
            BaseClear();
        }

        public OutputCacheProfile Get(int index) {
            return (OutputCacheProfile)BaseGet(index);
        }

        public OutputCacheProfile Get(string name) {
            return (OutputCacheProfile)BaseGet(name);
        }

        public String GetKey(int index) {
            return (String) BaseGetKey(index);
        }

        public void Remove(string name) {
            BaseRemove(name);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public void Set(OutputCacheProfile user) {
            BaseAdd(user, false);
        }

    }
}
