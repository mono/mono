//------------------------------------------------------------------------------
// <copyright file="RuleSettingsCollection.cs" company="Microsoft">
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
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Compilation;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(RuleSettings))]
    public sealed class RuleSettingsCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static RuleSettingsCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public RuleSettingsCollection() {
        }

        // public properties
        public RuleSettings this[int index] {
            get {
                return (RuleSettings)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new RuleSettings this[string key] {
            get {
                return (RuleSettings)BaseGet(key);
            }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new RuleSettings();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((RuleSettings)element).Name;
        }

        // public methods
        public void Add(RuleSettings ruleSettings) {
            BaseAdd(ruleSettings); // add to the end of the list and dont overwrite dups!
        }

        public void Clear() {
            BaseClear();
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public void Insert(int index, RuleSettings eventSettings) {
            BaseAdd(index, eventSettings);
        }

        public void Remove(String name) {
            BaseRemove(name);
        }


        public int IndexOf(String name) {
            ConfigurationElement element = BaseGet((Object)name);
            return (element != null) ? BaseIndexOf(element) : -1;
        }

        public bool Contains(String name) {
            return (IndexOf(name) != -1);
        }
    }
}
