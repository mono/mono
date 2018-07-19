//------------------------------------------------------------------------------
// <copyright file="NamespaceCollection.cs" company="Microsoft">
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

    // class PagesSection

    [ConfigurationCollection(typeof(NamespaceInfo))]
    public sealed class NamespaceCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propAutoImportVBNamespace =
            new ConfigurationProperty("autoImportVBNamespace", typeof(bool), true, ConfigurationPropertyOptions.None);

        private Hashtable _namespaceEntries;

        static NamespaceCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propAutoImportVBNamespace);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("autoImportVBNamespace", DefaultValue = true)]
        public bool AutoImportVBNamespace {
            get {
                return (bool)base[_propAutoImportVBNamespace];
            }
            set {
                base[_propAutoImportVBNamespace] = value;
            }
        }

        public NamespaceInfo this[int index] {
            get {
                return (NamespaceInfo)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
                _namespaceEntries = null;
            }
        }
        public void Add(NamespaceInfo namespaceInformation) {
            BaseAdd(namespaceInformation);
            _namespaceEntries = null;
        }
        public void Remove(String s) {
            BaseRemove(s);
            _namespaceEntries = null;
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
            _namespaceEntries = null;
        }
        protected override ConfigurationElement CreateNewElement() {
            return new NamespaceInfo();
        }
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((NamespaceInfo)element).Namespace;
        }

        public void Clear() {
            BaseClear();
            _namespaceEntries = null;
        }

        internal Hashtable NamespaceEntries {
            get {
                if (_namespaceEntries == null) {
                    lock (this) {
                        if (_namespaceEntries == null) {
                            _namespaceEntries = new Hashtable(StringComparer.OrdinalIgnoreCase);

                            foreach (NamespaceInfo ni in this) {
                                NamespaceEntry namespaceEntry = new NamespaceEntry();
                                namespaceEntry.Namespace = ni.Namespace;

                                // Remember the config file location info, in case an error
                                // occurs later when we use this data
                                namespaceEntry.Line = ni.ElementInformation.Properties["namespace"].LineNumber;

                                // 
                                namespaceEntry.VirtualPath = ni.ElementInformation.Properties["namespace"].Source;
                                // If the namespace was given Programactically it needs to still have a
                                // valid line number of the compiler chokes (1 based).
                                if (namespaceEntry.Line == 0) {
                                    namespaceEntry.Line = 1;
                                }
                                _namespaceEntries[ni.Namespace] = namespaceEntry;
                            }
                        }
                    }
                }
                return _namespaceEntries;
            }
        }
    }
}
