//------------------------------------------------------------------------------
// <copyright file="KeyValueInternalCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;

    // class AppSettingsSection

    class KeyValueInternalCollection : NameValueCollection {
        private AppSettingsSection _root = null;
        public KeyValueInternalCollection(AppSettingsSection root) {
            _root = root;
            foreach (KeyValueConfigurationElement element in _root.Settings) {
                base.Add(element.Key, element.Value);
            }
        }

        public override void Add(String key, String value) {
            _root.Settings.Add(new KeyValueConfigurationElement(key, value));
            base.Add(key, value);
        }

        public override void Clear() {
            _root.Settings.Clear();
            base.Clear();
        }

        public override void Remove(string key) {
            _root.Settings.Remove(key);
            base.Remove(key);
        }

#if DONT_COMPILE
        // Unfortunately this is not virtual and we hand out the base collection
        public new string this[string key] {
            get {
                return base[key];
            }
            set {
                _root.Settings[key] = new KeyValueConfigurationElement(key, value);
                base[key] = value;
            }
        }
#endif

    }
}
