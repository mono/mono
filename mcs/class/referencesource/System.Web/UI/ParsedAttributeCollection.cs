//------------------------------------------------------------------------------
// <copyright file="ParsedAttributeCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.Util;


    /// <devdoc>
    /// Contains parsed attributes organized by filter.
    /// The IDictionary implementation uses the combination of all filters using filter:attrName as the attribute names
    /// </devdoc>
    internal sealed class ParsedAttributeCollection : IDictionary {
        private IDictionary _filterTable;
        private IDictionary _allFiltersDictionary;
        private IDictionary<String, Pair> _attributeValuePositionInfo;

        internal ParsedAttributeCollection() {
            _filterTable = new ListDictionary(StringComparer.OrdinalIgnoreCase);
        }

        /// <devdoc>
        /// Returns the combination of all filters using filter:attrName as the attribute names
        /// </devdoc>
        private IDictionary AllFiltersDictionary {
            get {
                if (_allFiltersDictionary == null) {
                    _allFiltersDictionary = new ListDictionary(StringComparer.OrdinalIgnoreCase);
                    foreach (FilteredAttributeDictionary fac in _filterTable.Values) {
                        foreach (DictionaryEntry entry in fac) {
                            Debug.Assert(entry.Key != null);
                            _allFiltersDictionary[Util.CreateFilteredName(fac.Filter, entry.Key.ToString())] = entry.Value;
                        }
                    }
                }

                return _allFiltersDictionary;
            }
        }


        /// <devdoc>
        /// Adds a filtered attribute
        /// </devdoc>
        public void AddFilteredAttribute(string filter, string name, string value) {
            if (String.IsNullOrEmpty(name)) {
                throw ExceptionUtil.ParameterNullOrEmpty("name");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            if (filter == null) {
                filter = String.Empty;
            }

            if (_allFiltersDictionary != null) {
                _allFiltersDictionary.Add(Util.CreateFilteredName(filter, name), value);
            }

            FilteredAttributeDictionary filteredAttributes = (FilteredAttributeDictionary)_filterTable[filter];
            if (filteredAttributes == null) {
                filteredAttributes = new FilteredAttributeDictionary(this, filter);
                _filterTable[filter] = filteredAttributes;
            }
            filteredAttributes.Data.Add(name, value);
        }

        /// <summary>
        /// This adds an entry for the attribute name and the starting column of attribute value within the text.
        /// This information is later used for generating line pragmas for intellisense to work.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="line">The line number where the attribute value expression is present.</param>
        /// <param name="column">The column value where the attribute value expression begins. Note that this is actually after the attribute name itself.</param>
        public void AddAttributeValuePositionInformation(string name, int line, int column) {
            Debug.Assert(!String.IsNullOrEmpty(name));
            Pair pair = new Pair(line, column);
            AttributeValuePositionsDictionary[name] = pair;
        }

        public IDictionary<String, Pair> AttributeValuePositionsDictionary {
            get {
                if (_attributeValuePositionInfo == null) {
                    _attributeValuePositionInfo = new Dictionary<String, Pair>(StringComparer.OrdinalIgnoreCase);
                }
                return _attributeValuePositionInfo;
            }
        }



        /// <devdoc>
        /// Clears all attributes from the specified filter
        /// </devdoc>
        public void ClearFilter(string filter) {
            if (filter == null) {
                filter = String.Empty;
            }

            if (_allFiltersDictionary != null) {
                ArrayList removeList = new ArrayList();
                foreach (string key in _allFiltersDictionary.Keys) {
                    string attrName;
                    string currentFilter = Util.ParsePropertyDeviceFilter(key, out attrName);
                    if (StringUtil.EqualsIgnoreCase(currentFilter, filter)) {
                        removeList.Add(key);
                    }
                }

                foreach (string key in removeList) {
                    _allFiltersDictionary.Remove(key);
                }
            }

            _filterTable.Remove(filter);
        }


        /// <devdoc>
        /// Gets the collection of FilteredAttributeDictionaries used by this collection.
        /// </devdoc>
        public ICollection GetFilteredAttributeDictionaries() {
            return _filterTable.Values;
        }


        /// <devdoc>
        /// Removes the specified attribute from the specified filter.
        /// </devdoc>
        public void RemoveFilteredAttribute(string filter, string name) {
            if (String.IsNullOrEmpty(name)) {
                throw ExceptionUtil.ParameterNullOrEmpty("name");
            }

            if (filter == null) {
                filter = String.Empty;
            }

            if (_allFiltersDictionary != null) {
                _allFiltersDictionary.Remove(Util.CreateFilteredName(filter, name));
            }

            FilteredAttributeDictionary filteredAttributes = (FilteredAttributeDictionary)_filterTable[filter];
            if (filteredAttributes != null) {
                filteredAttributes.Data.Remove(name);
            }
        }


        /// <devdoc>
        /// Replaces the specified attribute's value from the specified filter.
        /// </devdoc>
        public void ReplaceFilteredAttribute(string filter, string name, string value) {
            if (String.IsNullOrEmpty(name)) {
                throw ExceptionUtil.ParameterNullOrEmpty("name");
            }

            if (filter == null) {
                filter = String.Empty;
            }

            if (_allFiltersDictionary != null) {
                _allFiltersDictionary[Util.CreateFilteredName(filter, name)] = value;
            }

            FilteredAttributeDictionary filteredAttributes = (FilteredAttributeDictionary)_filterTable[filter];
            if (filteredAttributes == null) {
                filteredAttributes = new FilteredAttributeDictionary(this, filter);
                _filterTable[filter] = filteredAttributes;
            }
            filteredAttributes.Data[name] = value;
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
                return AllFiltersDictionary[key];
            }
            set {
                if (key == null) {
                    throw new ArgumentNullException("key");
                }

                string attrName;
                string filter = Util.ParsePropertyDeviceFilter(key.ToString(), out attrName);

                ReplaceFilteredAttribute(filter, attrName, value.ToString());
            }
        }


        /// <internalonly/>
        ICollection IDictionary.Keys {
            get {
                return AllFiltersDictionary.Keys;
            }
        }


        /// <internalonly/>
        ICollection IDictionary.Values {
            get {
                return AllFiltersDictionary.Values;
            }
        }


        /// <internalonly/>
        void IDictionary.Add(object key, object value) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if (value == null) {
                value = String.Empty;
            }

            string attrName;
            string filter = Util.ParsePropertyDeviceFilter(key.ToString(), out attrName);

            AddFilteredAttribute(filter, attrName, value.ToString());
        }


        /// <internalonly/>
        bool IDictionary.Contains(object key) {
            return AllFiltersDictionary.Contains(key);
        }


        /// <internalonly/>
        void IDictionary.Clear() {
            AllFiltersDictionary.Clear();
            _filterTable.Clear();
        }


        /// <internalonly/>
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return AllFiltersDictionary.GetEnumerator();
        }


        /// <internalonly/>
        void IDictionary.Remove(object key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            string attrName;
            string filter = Util.ParsePropertyDeviceFilter(key.ToString(), out attrName);

            RemoveFilteredAttribute(filter, attrName);
        }
        #endregion IDictionary implementation

        #region ICollection implementation

        /// <internalonly/>
        int ICollection.Count {
            get {
                return AllFiltersDictionary.Count;
            }
        }


        /// <internalonly/>
        bool ICollection.IsSynchronized {
            get {
                return ((ICollection)AllFiltersDictionary).IsSynchronized;
            }
        }


        /// <internalonly/>
        object ICollection.SyncRoot {
            get {
                return AllFiltersDictionary.SyncRoot;
            }
        }


        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            AllFiltersDictionary.CopyTo(array, index);
        }


        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return AllFiltersDictionary.GetEnumerator();
        }
        #endregion ICollection implementation
    }
}

