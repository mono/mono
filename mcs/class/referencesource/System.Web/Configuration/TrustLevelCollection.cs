//------------------------------------------------------------------------------
// <copyright file="TrustLevelCollection.cs" company="Microsoft">
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
    using System.ComponentModel;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(TrustLevel), AddItemName = "trustLevel",
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class TrustLevelCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;
        static TrustLevelCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public TrustLevelCollection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }
        // public properties

        public TrustLevel this[int index] {
            get {
                return (TrustLevel)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new TrustLevel this[string key] {
            get {
                return (TrustLevel)BaseGet(key);
            }
        }

        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new TrustLevel();
        }
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((TrustLevel)element).Name;
        }
        protected override string ElementName {
            get {
                return "trustLevel";
            }
        }

        protected override bool ThrowOnDuplicate { get { return true; } }

        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        
        protected override bool IsElementName(string elementname) {
            bool IsElement = false;
            switch (elementname) {
                case "trustLevel":
                    IsElement = true;
                    break;
            }
            return IsElement;
        }
        
        // public methods
        public void Add(TrustLevel trustLevel) {
            BaseAdd(trustLevel);
        }
        
        public void Clear() {
            BaseClear();
        }
        
        public TrustLevel Get(int index) {
            return (TrustLevel)BaseGet(index);
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        public void Remove(TrustLevel trustLevel) {
            BaseRemove(GetElementKey(trustLevel));
        }

        public void Set(int index, TrustLevel trustLevel) {
            BaseAdd(index, trustLevel);
        }
    }
}
