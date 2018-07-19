//------------------------------------------------------------------------------
// <copyright file="FragmentCacheProfileCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if NOT_UNTIL_LATER
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

    [ConfigurationCollection(typeof(FragmentCacheProfile))]
    public sealed class FragmentCacheProfileCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static FragmentCacheProfileCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public FragmentCacheProfileCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        // public properties
        public String[] AllKeys {
            get { 
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }

        public new FragmentCacheProfile this[string name] {
            get {
                return (FragmentCacheProfile)BaseGet(name);
            }
            // Having a setter here would be strange in that you could write
            //  collection["Name1"] = new FragmentCacheProfile("differentName"...
            // 
        }

        public FragmentCacheProfile this[int index] {
            get {
                return (FragmentCacheProfile)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new FragmentCacheProfile();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((FragmentCacheProfile)element).Name;
        }

        // public methods
        public void Add(FragmentCacheProfile user) {
            BaseAdd(user);
        }

        public void Clear() {
            BaseClear();
        }

        public FragmentCacheProfile Get(int index) {
            return (FragmentCacheProfile)BaseGet(index);
        }

        public FragmentCacheProfile Get(string name) {
            return (FragmentCacheProfile)BaseGet(name);
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

        public void Set(FragmentCacheProfile user) {
            BaseAdd(user, false);
        }

    }
}
#endif

