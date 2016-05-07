//------------------------------------------------------------------------------
// <copyright file="BuildProviderCollection.cs" company="Microsoft">
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

    // class CompilationSection

    [ConfigurationCollection(typeof(BuildProvider))]
    public sealed class BuildProviderCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static BuildProviderCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }
        
        public BuildProviderCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public new BuildProvider this[string name] {
            get {
                return (BuildProvider)BaseGet(name);
            }
        }
        public BuildProvider this[int index] {
            get {
                return (BuildProvider)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(BuildProvider buildProvider) {
            BaseAdd(buildProvider);
        }
        
        public void Remove(String name) {
            BaseRemove(name);
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        public void Clear() {
            BaseClear();
        }
        
        protected override ConfigurationElement CreateNewElement() {
            return new BuildProvider();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((BuildProvider)element).Extension;
        }
    }
}
