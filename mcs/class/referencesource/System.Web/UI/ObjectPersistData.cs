//------------------------------------------------------------------------------
// <copyright file="ObjectPersistData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Util;
    using System.Security.Permissions;

    public class ObjectPersistData {
        private Type _objectType;

        private bool _isCollection;
        private ArrayList _collectionItems;

        private bool _localize;
        private string _resourceKey;

        private IDictionary _propertyTableByFilter;
        private IDictionary _propertyTableByProperty;
        private ArrayList _allPropertyEntries;

        private ArrayList _eventEntries;

        private IDictionary _builtObjects;


        public ObjectPersistData(ControlBuilder builder, IDictionary builtObjects) {
            _objectType = builder.ControlType;
            _localize = builder.Localize;
            _resourceKey = builder.GetResourceKey();

            _builtObjects = builtObjects;

            if (typeof(ICollection).IsAssignableFrom(_objectType)) {
                _isCollection = true;
            }

            _collectionItems = new ArrayList();
            _propertyTableByFilter = new HybridDictionary(true);
            _propertyTableByProperty = new HybridDictionary(true);
            _allPropertyEntries = new ArrayList();
            _eventEntries = new ArrayList();

            foreach (PropertyEntry entry in builder.SimplePropertyEntries) {
                AddPropertyEntry(entry);
            }

            foreach (PropertyEntry entry in builder.ComplexPropertyEntries) {
                AddPropertyEntry(entry);
            }

            foreach (PropertyEntry entry in builder.TemplatePropertyEntries) {
                AddPropertyEntry(entry);
            }

            foreach (PropertyEntry entry in builder.BoundPropertyEntries) {
                AddPropertyEntry(entry);
            }

            foreach (EventEntry entry in builder.EventEntries) {
                AddEventEntry(entry);
            }
        }


        /// <devdoc>
        /// Get all property entries
        /// </devdoc>
        public ICollection AllPropertyEntries {
            get {
                return _allPropertyEntries;
            }
        }

        public IDictionary BuiltObjects {
            get {
                return _builtObjects;
            }
        }


        public ICollection CollectionItems {
            get {
                return _collectionItems;
            }
        }


        public ICollection EventEntries {
            get {
                return _eventEntries;
            }
        }


        /// <devdoc>
        /// True if this persistence data is for a collection
        /// </devdoc>
        public bool IsCollection {
            get {
                return _isCollection;
            }
        }

        public bool Localize {
            get {
                return _localize;
            }
        }


        /// <devdoc>
        /// The type of the object with these properties.
        /// </devdoc>
        public Type ObjectType {
            get {
                return _objectType;
            }
        }

        public string ResourceKey {
            get {
                return _resourceKey;
            }
        }

        /// <devdoc>
        /// Adds a property to this persistence data, adding it to all necessary
        /// data structures.
        /// </devdoc>
        private void AddPropertyEntry(PropertyEntry entry) {
            if (_isCollection && (entry is ComplexPropertyEntry && ((ComplexPropertyEntry)entry).IsCollectionItem)) {
                _collectionItems.Add(entry);
            }
            else {
                IDictionary filteredProperties = (IDictionary)_propertyTableByFilter[entry.Filter];
                if (filteredProperties == null) {
                    filteredProperties = new HybridDictionary(true);
                    _propertyTableByFilter[entry.Filter] = filteredProperties;
                }

                Debug.Assert((entry.Name != null) && (entry.Name.Length > 0));
                filteredProperties[entry.Name] = entry;

                ArrayList properties = (ArrayList)_propertyTableByProperty[entry.Name];
                if (properties == null) {
                    properties = new ArrayList();
                    _propertyTableByProperty[entry.Name] = properties;
                }

                properties.Add(entry);
            }


            _allPropertyEntries.Add(entry);
        }

        private void AddEventEntry(EventEntry entry) {
            _eventEntries.Add(entry);
        }


        /// <devdov>
        /// </devdoc>
        public void AddToObjectControlBuilderTable(IDictionary table) {
            if (_builtObjects != null) {
                foreach (DictionaryEntry entry in _builtObjects) {
                    table[entry.Key] = entry.Value;
                }
            }
        }


        /// <devdoc>
        /// Gets a PropertyEntry for the specified filter and property name
        /// </devdoc>
        public PropertyEntry GetFilteredProperty(string filter, string name) {
            IDictionary filteredProperties = GetFilteredProperties(filter);
            if (filteredProperties != null) {
                return (PropertyEntry)filteredProperties[name];
            }

            return null;
        }


        /// <devdoc>
        /// Gets all PropertyEntries for the specified filter
        /// </devdoc>
        public IDictionary GetFilteredProperties(string filter) {
            return (IDictionary)_propertyTableByFilter[filter];
        }


        /// <devdoc>
        /// Gets all filtered PropertiesEntries for a specified property (name uses dot-syntax e.g. Font.Bold)
        /// </devdoc>
        public ICollection GetPropertyAllFilters(string name) {
            ICollection properties = (ICollection)_propertyTableByProperty[name];
            if (properties == null) {
                return new ArrayList();
            }

            return properties;
        }
    }

}
