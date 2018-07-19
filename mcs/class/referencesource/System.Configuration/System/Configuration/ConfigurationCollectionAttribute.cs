//------------------------------------------------------------------------------
// <copyright file="ConfigurationCollectionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;
using System.Configuration;

namespace System.Configuration {

    // This attribute is expected on section properties of type derivied from ConfigurationElementCollection
    // or on the itself
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class ConfigurationCollectionAttribute : Attribute {
        private string _addItemName = null;
        private string _removeItemName = null;
        private string _clearItemsName = null;
        private Type _itemType = null;
        private ConfigurationElementCollectionType _collectionType = ConfigurationElementCollectionType.AddRemoveClearMap;

        public ConfigurationCollectionAttribute(Type itemType) {
            if (itemType == null) {
                throw new ArgumentNullException("itemType");
            }

            _itemType = itemType;
        }
        public Type ItemType {
            get {
                return _itemType;
            }
        }
        public string AddItemName {
            get {
                if (_addItemName == null) {
                    return ConfigurationElementCollection.DefaultAddItemName;
                }
                else {
                    return _addItemName;
                }
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    value = null;
                }
                _addItemName = value;
            }
        }
        public string RemoveItemName {
            get {
                if (_removeItemName == null) {
                    return ConfigurationElementCollection.DefaultRemoveItemName;
                }
                else {
                    return _removeItemName;
                }
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    value = null;
                }
                _removeItemName = value;
            }
        }
        public string ClearItemsName {
            get {
                if (_clearItemsName == null) {
                    return ConfigurationElementCollection.DefaultClearItemsName;
                }
                else {
                    return _clearItemsName;
                }
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    value = null;
                }
                _clearItemsName = value;
            }
        }

        public ConfigurationElementCollectionType CollectionType {
            get {
                return _collectionType;
            }
            set {
                _collectionType = value;
            }            
        }
    }
}
