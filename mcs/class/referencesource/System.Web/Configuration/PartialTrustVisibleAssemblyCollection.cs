//------------------------------------------------------------------------------
// <copyright file="PartialTrustVisibleAssemblyCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.CodeDom.Compiler;
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(String))]
    public sealed class PartialTrustVisibleAssemblyCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static PartialTrustVisibleAssemblyCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public PartialTrustVisibleAssembly this[int index] {
            get {
                return (PartialTrustVisibleAssembly)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PartialTrustVisibleAssembly partialTrustVisibleAssembly) {
            BaseAdd(partialTrustVisibleAssembly);
        }

        public void Remove(String key) {
            BaseRemove(key);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement() {
            return new PartialTrustVisibleAssembly();
        }

        public void Clear() {
            BaseClear();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((PartialTrustVisibleAssembly)element).AssemblyName;
        }

        internal bool IsRemoved(string key) {
            return BaseIsRemoved(key);
        }
    }
}

