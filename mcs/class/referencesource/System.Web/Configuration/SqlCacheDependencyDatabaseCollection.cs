//------------------------------------------------------------------------------
// <copyright file="SqlCacheDependencyDatabaseCollection.cs" company="Microsoft">
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
    using System.Diagnostics;
    using System.Web.Util;
    using System.Security.Permissions;


    [ConfigurationCollection(typeof(SqlCacheDependencyDatabase))]
    public sealed class SqlCacheDependencyDatabaseCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static SqlCacheDependencyDatabaseCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        public SqlCacheDependencyDatabaseCollection() {
        }
        
        // public properties
        public String[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }
        
        public new SqlCacheDependencyDatabase this[string name] {
            get {
                return (SqlCacheDependencyDatabase)BaseGet(name);
            }
            // Having a setter here would be strange in that you could write
            //  collection["Name1"] = new SqlCacheDependencyDatabase("differentName"...
            // 
        }
        
        public SqlCacheDependencyDatabase this[int index] {
            get {
                return (SqlCacheDependencyDatabase)BaseGet(index);
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
            return new SqlCacheDependencyDatabase();
        }
        
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((SqlCacheDependencyDatabase)element).Name;
        }

        // public methods
        public void Add(SqlCacheDependencyDatabase name) {
            BaseAdd(name);
        }
        
        public void Clear() {
            BaseClear();
        }
        
        public SqlCacheDependencyDatabase Get(int index) {
            return (SqlCacheDependencyDatabase)BaseGet(index);
        }
        
        public SqlCacheDependencyDatabase Get(string name) {
            return (SqlCacheDependencyDatabase)BaseGet(name);
        }

        public String GetKey(int index) {
            return (String)BaseGetKey(index);
        }
        
        public void Remove(string name) {
            BaseRemove(name);
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        public void Set(SqlCacheDependencyDatabase user) {
            BaseAdd(user, false);
        }
    }
}
