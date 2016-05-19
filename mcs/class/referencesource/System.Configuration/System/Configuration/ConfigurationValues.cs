//------------------------------------------------------------------------------
// <copyright file="ConfigurationValues.cs" company="Microsoft">
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

namespace System.Configuration {

    internal class ConfigurationValues : NameObjectCollectionBase {
        private BaseConfigurationRecord _configRecord;
        private volatile bool _containsElement;
        private volatile bool _containsInvalidValue;

        internal ConfigurationValues() : base(StringComparer.Ordinal) {
        }

        // AssociateContext
        //
        // Associate a collection of values with a configRecord
        //
        internal void AssociateContext(BaseConfigurationRecord configRecord) {
            _configRecord = configRecord;

            // Associate with children
            foreach (ConfigurationElement currentElement in ConfigurationElements) {
                currentElement.AssociateContext(_configRecord);
            }
        }

        internal bool Contains(string key) {
            return (BaseGet(key) != null);
        }

        internal string GetKey(int index) {
            return BaseGetKey(index);
        }

        /*
        internal ConfigurationValue GetConfigValue(int index)
        {
            return (ConfigurationValue)BaseGet(BaseGetKey(index));
        }
        */

        internal ConfigurationValue GetConfigValue(string key) {
            return (ConfigurationValue)BaseGet(key);
        }

        internal ConfigurationValue GetConfigValue(int index) {
            return (ConfigurationValue)BaseGet(index);
        }

        internal PropertySourceInfo GetSourceInfo(string key) {
            ConfigurationValue configurationValue = GetConfigValue(key);
            if (configurationValue != null) {
                return configurationValue.SourceInfo;
            }
            else {
                return null;
            }
        }

        internal void ChangeSourceInfo(string key, PropertySourceInfo sourceInfo) {
            ConfigurationValue configurationValue = GetConfigValue(key);
            if (configurationValue != null) {
                configurationValue.SourceInfo = sourceInfo;
            }
        }

        private ConfigurationValue CreateConfigValue(object value, ConfigurationValueFlags valueFlags, PropertySourceInfo sourceInfo) {
            if (value != null) {
                if (value is ConfigurationElement) {
                    _containsElement = true;
                    ((ConfigurationElement)value).AssociateContext(_configRecord);
                }
                else if (value is InvalidPropValue) {
                    _containsInvalidValue = true;
                }
            }

            ConfigurationValue configValue = new ConfigurationValue(value, valueFlags, sourceInfo);
            return configValue;
        }

        internal void SetValue(string key, object value, ConfigurationValueFlags valueFlags, PropertySourceInfo sourceInfo) {
            ConfigurationValue configValue = CreateConfigValue(value, valueFlags, sourceInfo);
            BaseSet(key, configValue);
        }

#if UNUSED_CODE
        private void SetValue(int index, object value, ConfigurationValueFlags valueFlags, PropertySourceInfo sourceInfo) {
            ConfigurationValue configValue = CreateConfigValue(value, valueFlags, sourceInfo);
            BaseSet(index, configValue);
        }
#endif

        internal object this[string key] {
            get {
                ConfigurationValue configValue = GetConfigValue(key);
                if (configValue != null) {
                    return configValue.Value;
                }
                else {
                    return null;
                }
            }
            set {
                SetValue(key, value, ConfigurationValueFlags.Modified, null);
            }
        }

        internal object this[int index] {
            get {
                ConfigurationValue configValue = GetConfigValue(index);
                if (configValue != null) {
                    return configValue.Value;
                }
                else {
                    return null;
                }
            }

#if UNUSED_CODE
            set {
                SetValue(index, value, ConfigurationValueFlags.Modified, null);
            }
#endif
        }

        internal void Clear() {
            BaseClear();
        }

        internal object SyncRoot { get { return this; } }

        internal ConfigurationValueFlags RetrieveFlags(string key) {
            ConfigurationValue configurationValue = (ConfigurationValue)BaseGet(key);
            if (configurationValue != null) {
                return configurationValue.ValueFlags;
            }
            else {
                return ConfigurationValueFlags.Default;
            }
        }

        internal bool IsModified(string key) {
            ConfigurationValue configurationValue = (ConfigurationValue)BaseGet(key);
            if (configurationValue != null) {
                return ((configurationValue.ValueFlags & ConfigurationValueFlags.Modified) != 0);
            }
            else {
                return false;
            }
        }

        internal bool IsInherited(string key) {
            ConfigurationValue configurationValue = (ConfigurationValue)BaseGet(key);
            if (configurationValue != null) {
                return ((configurationValue.ValueFlags & ConfigurationValueFlags.Inherited) != 0);
            }
            else {
                return false;
            }

        }

        internal IEnumerable ConfigurationElements {
            get {
                if (_containsElement) {
                    return new ConfigurationElementsCollection(this);
                }
                else {
                    return EmptyCollectionInstance;
                }
            }
        }

        internal IEnumerable InvalidValues {
            get {
                if (_containsInvalidValue) {
                    return new InvalidValuesCollection(this);
                }
                else {
                    return EmptyCollectionInstance;
                }
            }
        }

        static volatile IEnumerable s_emptyCollection;
        static IEnumerable EmptyCollectionInstance {
            get {
                if (s_emptyCollection == null) {
                    s_emptyCollection = new EmptyCollection();
                }

                return s_emptyCollection;
            }
        }

        private class EmptyCollection : IEnumerable {
            IEnumerator _emptyEnumerator;

            private class EmptyCollectionEnumerator : IEnumerator {
                bool IEnumerator.MoveNext() {
                    return false;
                }

                Object IEnumerator.Current {
                    get {
                        return null;
                    }
                }

                void IEnumerator.Reset() {
                }
            }

            internal EmptyCollection() {
                _emptyEnumerator = new EmptyCollectionEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _emptyEnumerator;
            }
        }

        private class ConfigurationElementsCollection : IEnumerable {
            ConfigurationValues _values;

            internal ConfigurationElementsCollection(ConfigurationValues values) {
                _values = values;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                if (_values._containsElement) {
                    for (int index = 0; index < _values.Count; index++) {
                        object value = _values[index];
                        if (value is ConfigurationElement) {
                            yield return value;
                        }
                    }
                }
            }
        }

        private class InvalidValuesCollection : IEnumerable {
            ConfigurationValues _values;

            internal InvalidValuesCollection(ConfigurationValues values) {
                _values = values;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                if (_values._containsInvalidValue) {
                    for (int index = 0; index < _values.Count; index++) {
                        object value = _values[index];
                        if (value is InvalidPropValue) {
                            yield return value;
                        }
                    }
                }
            }
        }

        /* 
        internal bool IsLocked(string key)
        {
            ConfigurationValue configurationValue = (ConfigurationValue)BaseGet(key);
            if (configurationValue != null)
                return ((configurationValue.ValueFlags & ConfigurationValueFlags.Locked) != 0);
            else
                return false;

        }
        internal bool IsReadOnly(string key)
        {
            ConfigurationValue configurationValue = (ConfigurationValue)BaseGet(key);
            if (configurationValue != null)
                return ((configurationValue.ValueFlags & ConfigurationValueFlags.ReadOnly) != 0);
            else
                return false;

        }
        */

    }
}
