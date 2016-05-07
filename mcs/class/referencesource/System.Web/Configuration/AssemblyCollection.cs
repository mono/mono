//------------------------------------------------------------------------------
// <copyright file="AssemblyCollection.cs" company="Microsoft">
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
    using System.Web.Compilation;
    using System.Reflection;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.CodeDom.Compiler;
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(AssemblyInfo))]
    public sealed class AssemblyCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static AssemblyCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public AssemblyInfo this[int index] {
            get {
                return (AssemblyInfo)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new AssemblyInfo this[String assemblyName] {
            get {
                return (AssemblyInfo)BaseGet(assemblyName);
            }
        }

        public void Add(AssemblyInfo assemblyInformation) {
            BaseAdd(assemblyInformation);
        }
        
        public void Remove(String key) {
            BaseRemove(key);
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        protected override ConfigurationElement CreateNewElement() {
            return new AssemblyInfo();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((AssemblyInfo)element).Assembly;
        }

        public void Clear() {
            BaseClear();
        }

        internal bool IsRemoved(string key) {
            return BaseIsRemoved(key);
        }
    }
}
