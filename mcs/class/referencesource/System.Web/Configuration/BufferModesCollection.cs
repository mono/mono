//------------------------------------------------------------------------------
// <copyright file="BufferModesCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(BufferModeSettings))]
    public sealed class BufferModesCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static BufferModesCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        public BufferModesCollection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public void Add(BufferModeSettings bufferModeSettings) {
            BaseAdd(bufferModeSettings);
        }

        public void Remove(String s) {
            BaseRemove(s);
        }

        public void Clear() {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() {
            return new BufferModeSettings();
        }
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((BufferModeSettings)element).Name;
        }

        public new BufferModeSettings this[string key] {
            get {
                return (BufferModeSettings)BaseGet(key);
            }
        }
        public BufferModeSettings this[int index] {
            get {
                return (BufferModeSettings)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }
}
