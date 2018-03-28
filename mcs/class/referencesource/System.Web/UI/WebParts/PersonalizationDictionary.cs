//------------------------------------------------------------------------------
// <copyright file="PersonalizationDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Util;

    // This class is unsealed so it can be extended by developers who extend the personalization infrastructure
    public class PersonalizationDictionary : IDictionary {
        private HybridDictionary _dictionary;

        public PersonalizationDictionary() {
            _dictionary = new HybridDictionary(/* caseInsensitive */ true);
        }

        public PersonalizationDictionary(int initialSize) {
            _dictionary = new HybridDictionary(initialSize, /* caseInsensitive */ true);
        }

        public virtual int Count {
            get {
                return _dictionary.Count;
            }
        }

        public virtual bool IsFixedSize {
            get {
                return false;
            }
        }

        public virtual bool IsReadOnly {
            get {
                return false;
            }
        }

        public virtual bool IsSynchronized {
            get {
                return false;
            }
        }

        public virtual ICollection Keys {
            get {
                return _dictionary.Keys;
            }
        }

        public virtual object SyncRoot {
            get {
                return this;
            }
        }

        public virtual ICollection Values {
            get {
                return _dictionary.Values;
            }
        }

        public virtual PersonalizationEntry this[string key] {
            get{
                key = StringUtil.CheckAndTrimString(key, "key");
                return (PersonalizationEntry)_dictionary[key];
            }
            set {
                key = StringUtil.CheckAndTrimString(key, "key");
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _dictionary[key] = value;
            }
        }

        public virtual void Add(string key, PersonalizationEntry value) {
            key = StringUtil.CheckAndTrimString(key, "key");
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            _dictionary.Add(key, value);
        }

        public virtual void Clear() {
            _dictionary.Clear();
        }

        public virtual bool Contains(string key) {
            key = StringUtil.CheckAndTrimString(key, "key");
            return _dictionary.Contains(key);
        }

        public virtual void CopyTo(DictionaryEntry[] array, int index) {
            _dictionary.CopyTo(array, index);
        }

        public virtual IDictionaryEnumerator GetEnumerator() {
            return _dictionary.GetEnumerator();
        }

        public virtual void Remove(string key) {
            key = StringUtil.CheckAndTrimString(key, "key");
            _dictionary.Remove(key);
        }

        internal void RemoveSharedProperties() {
            DictionaryEntry[] entries = new DictionaryEntry[Count];
            CopyTo(entries, 0);
            foreach (DictionaryEntry entry in entries) {
                if (((PersonalizationEntry)entry.Value).Scope == PersonalizationScope.Shared) {
                    Remove((string)entry.Key);
                }
            }
        }

        #region Implementation of IDictionary
        object IDictionary.this[object key] {
            get {
                if (!(key is string)) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationDictionary_MustBeTypeString), "key");
                }
                return this[(string)key];
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                if (!(key is string)) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationDictionary_MustBeTypeString), "key");
                }
                if (!(value is PersonalizationEntry)) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationDictionary_MustBeTypePersonalizationEntry), "value");
                }
                this[(string)key] = (PersonalizationEntry)value;
            }
        }

        void IDictionary.Add(object key, object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            if (!(key is string)) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationDictionary_MustBeTypeString), "key");
            }
            if (!(value is PersonalizationEntry)) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationDictionary_MustBeTypePersonalizationEntry), "value");
            }
            Add((string)key, (PersonalizationEntry)value);
        }

        bool IDictionary.Contains(object key) {
            if (!(key is string)) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationDictionary_MustBeTypeString), "key");
            }
            return Contains((string)key);
        }

        void IDictionary.Remove(object key) {
            if (!(key is string)) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationDictionary_MustBeTypeString), "key");
            }
            Remove((string)key);
        }
        #endregion

        #region Implementation of ICollection
        void ICollection.CopyTo(Array array, int index) {
            if (!(array is DictionaryEntry[])) {
                throw new ArgumentException(
                    SR.GetString(SR.PersonalizationDictionary_MustBeTypeDictionaryEntryArray), "array");
            }
            CopyTo((DictionaryEntry[])array, index);
        }
        #endregion

        #region Implementation of IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
    }
}
