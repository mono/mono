//------------------------------------------------------------------------------
// <copyright file="ConfigurationPropertyCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;

namespace System.Configuration {

    public class ConfigurationPropertyCollection : ICollection {

        private ArrayList _items = new ArrayList();

        public int Count {
            get {
                return _items.Count;
            }
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public Object SyncRoot {
            get {
                return _items;
            }
        }

        internal ConfigurationProperty DefaultCollectionProperty {
            get {
                return this[ConfigurationProperty.DefaultCollectionPropertyName];
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            _items.CopyTo(array, index);
        }

        public void CopyTo(ConfigurationProperty[] array, int index) {
            ((ICollection)this).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator() {
            return _items.GetEnumerator();
        }

        public ConfigurationProperty this[String name] {
            get {
                for (int index = 0; index < _items.Count; index++) {
                    ConfigurationProperty cp = (ConfigurationProperty)_items[index];
                    if (cp.Name == name) {
                        return (ConfigurationProperty)_items[index];
                    }
                }
                return (ConfigurationProperty)null;
            }
        }

        public bool Contains(String name) {
            for (int index = 0; index < _items.Count; index++) {
                ConfigurationProperty cp = (ConfigurationProperty)_items[index];
                if (cp.Name == name) {
                    return true;
                }
            }
            return false;
        }

        public void Add(ConfigurationProperty property) {
            if (Contains(property.Name) != true) {
                _items.Add(property);
            }
        }

        public bool Remove(string name) {
            for (int index = 0; index < _items.Count; index++) {
                ConfigurationProperty cp = (ConfigurationProperty)_items[index];

                if (cp.Name == name) {
                    _items.RemoveAt(index);
                    return true;
                }
            }
            return false;
        }

        public void Clear() {
            _items.Clear();
        }
    }
}
