//------------------------------------------------------------------------------
// <copyright file="ExpressionBuilderCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(ExpressionBuilder))]
    public sealed class ExpressionBuilderCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static ExpressionBuilderCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }
        public ExpressionBuilderCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }


        public new ExpressionBuilder this[string name] {
            get {
                return (ExpressionBuilder)BaseGet(name);
            }
        }
        
        public ExpressionBuilder this[int index] {
            get {
                return (ExpressionBuilder)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(ExpressionBuilder buildProvider) {
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
            return new ExpressionBuilder();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((ExpressionBuilder)element).ExpressionPrefix;
        }
    }
}
