//------------------------------------------------------------------------------
// <copyright file="FilteredAttributeDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;


    /// <devdoc>
    /// Contains a filtered (by device filter) view of the attributes parsed from a tag
    /// </devdoc>
    internal sealed class FilteredAttributeDictionary : IDictionary {
        private string _filter;
        private IDictionary _data;

        private ParsedAttributeCollection _owner;

        internal FilteredAttributeDictionary(ParsedAttributeCollection owner, string filter) {
            _filter = filter;
            _owner = owner;
            _data = new ListDictionary(StringComparer.OrdinalIgnoreCase);
        }

        /// <devdoc>
        /// The actual dictionary used for storing the data
        /// </devdoc>
        internal IDictionary Data {
             get {
                 return _data;
             }
        }


        /// <devdoc>
        /// The filter that this collection is filtering on
        /// </devdoc>
        public string Filter {
            get {
                return _filter;
            }
        }


        /// <devdoc>
        /// Returns the value of a particular attribute for this filter
        /// </devdoc>
        public string this[string key] {
            get {
                return (string)_data[key];
            }
            set {
                _owner.ReplaceFilteredAttribute(_filter, key, value);
            }
        }


        /// <devdoc>
        /// Adds a new attribute for this filter
        /// </devdoc>
        public void Add(string key, string value) {
            _owner.AddFilteredAttribute(_filter, key, value);
        }


        /// <devdoc>
        /// Clears all attributes for this filter
        /// </devdoc>
        public void Clear() {
            _owner.ClearFilter(_filter);
        }


        /// <devdoc>
        /// Returns true if this filtered view contains the specified attribute
        /// </devdoc>
        public bool Contains(string key) {
            return _data.Contains(key);
        }


        /// <devdoc>
        /// Removes the specified attribute for this filter
        /// </devdoc>
        public void Remove(string key) {
            _owner.RemoveFilteredAttribute(_filter, key);
        }

        #region IDictionary implementation

        /// <internalonly/>
        bool IDictionary.IsFixedSize {
            get {
                return false;
            }
        }


        /// <internalonly/>
        bool IDictionary.IsReadOnly {
            get {
                return false;
            }
        }


        /// <internalonly/>
        object IDictionary.this[object key] {
            get {
                if (!(key is string)) {
                    throw new ArgumentException(SR.GetString(SR.FilteredAttributeDictionary_ArgumentMustBeString), "key");
                }

                return this[key.ToString()];
            }
            set {
                if (!(key is string)) {
                    throw new ArgumentException(SR.GetString(SR.FilteredAttributeDictionary_ArgumentMustBeString), "key");
                }

                if (!(value is string)) {
                    throw new ArgumentException(SR.GetString(SR.FilteredAttributeDictionary_ArgumentMustBeString), "value");
                }

                this[key.ToString()] = value.ToString();
            }
        }


        /// <internalonly/>
        ICollection IDictionary.Keys {
            get {
                return _data.Keys;
            }
        }


        /// <internalonly/>
        ICollection IDictionary.Values {
            get {
                return _data.Values;
            }
        }


        /// <internalonly/>
        void IDictionary.Add(object key, object value) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if (!(key is string)) {
                throw new ArgumentException(SR.GetString(SR.FilteredAttributeDictionary_ArgumentMustBeString), "key");
            }

            if (!(value is string)) {
                throw new ArgumentException(SR.GetString(SR.FilteredAttributeDictionary_ArgumentMustBeString), "value");
            }

            if (value == null) {
                value = String.Empty;
            }

            Add(key.ToString(), value.ToString());
        }


        /// <internalonly/>
        bool IDictionary.Contains(object key) {
            if (!(key is string)) {
                throw new ArgumentException(SR.GetString(SR.FilteredAttributeDictionary_ArgumentMustBeString), "key");
            }

            return Contains(key.ToString());
        }


        /// <internalonly/>
        void IDictionary.Clear() {
            Clear();
        }


        /// <internalonly/>
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return _data.GetEnumerator();
        }


        /// <internalonly/>
        void IDictionary.Remove(object key) {
            Remove(key.ToString());
        }
        #endregion IDictionary implementation

        #region ICollection implementation

        /// <internalonly/>
        int ICollection.Count {
            get {
                return _data.Count;
            }
        }


        /// <internalonly/>
        bool ICollection.IsSynchronized {
            get {
                return ((ICollection)_data).IsSynchronized;
            }
        }


        /// <internalonly/>
        object ICollection.SyncRoot {
            get {
                return _data.SyncRoot;
            }
        }


        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            _data.CopyTo(array, index);
        }


        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return _data.GetEnumerator();
        }
        #endregion ICollection implementation
    }
}
