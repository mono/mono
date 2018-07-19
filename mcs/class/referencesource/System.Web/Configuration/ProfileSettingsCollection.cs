//------------------------------------------------------------------------------
// <copyright file="ProfileSettingsCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(ProfileSettings))]
    public sealed class ProfileSettingsCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static ProfileSettingsCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public ProfileSettingsCollection() {
        }

        // public properties
        public ProfileSettings this[int index] {
            get {
                return (ProfileSettings)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new ProfileSettings this[string key] {
            get {
                return (ProfileSettings)BaseGet(key);
            }
        }

        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new ProfileSettings();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((ProfileSettings)element).Name;
        }

        // public methods
        public void Add(ProfileSettings profilesSettings) {
            BaseAdd(profilesSettings); // add to the end of the list and dont overwrite dups!
        }

        public void Clear() {
            BaseClear();
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public void Insert(int index, ProfileSettings authorizationSettings) {
            BaseAdd(index, authorizationSettings);
        }

        public int IndexOf(String name) {
            ConfigurationElement element = BaseGet((Object)name);
            return (element != null) ? BaseIndexOf(element) : -1;
        }

        public void Remove(String name) {
            BaseRemove(name);
        }

        public bool Contains(String name) {
            return (IndexOf(name) != -1);
        }
    }
}
