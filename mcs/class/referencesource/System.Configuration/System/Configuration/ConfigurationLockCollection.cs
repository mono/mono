//------------------------------------------------------------------------------
// <copyright file="ConfigurationLockCollection.cs" company="Microsoft">
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

    public sealed class ConfigurationLockCollection : IEnumerable, ICollection {
        private HybridDictionary internalDictionary;
        private ArrayList internalArraylist;
        private bool _bModified = false;
        private bool _bExceptionList = false;
        private string _ignoreName = String.Empty;
        private ConfigurationElement _thisElement = null;
        private ConfigurationLockCollectionType _lockType;
        private string SeedList = String.Empty;
        private const string LockAll = "*";

        internal ConfigurationLockCollection(ConfigurationElement thisElement)
            : this(thisElement, ConfigurationLockCollectionType.LockedAttributes) {
        }

        internal ConfigurationLockCollection(ConfigurationElement thisElement, ConfigurationLockCollectionType lockType)
            : this(thisElement, lockType, String.Empty) {
        }

        internal ConfigurationLockCollection(ConfigurationElement thisElement, ConfigurationLockCollectionType lockType, string ignoreName)
            : this(thisElement, lockType, ignoreName, null) {
        }

        internal ConfigurationLockCollection(ConfigurationElement thisElement, ConfigurationLockCollectionType lockType,
                    string ignoreName, ConfigurationLockCollection parentCollection) {
            _thisElement = thisElement;
            _lockType = lockType;
            internalDictionary = new HybridDictionary();
            internalArraylist = new ArrayList();
            _bModified = false;

            _bExceptionList = _lockType == ConfigurationLockCollectionType.LockedExceptionList ||
                              _lockType == ConfigurationLockCollectionType.LockedElementsExceptionList;
            _ignoreName = ignoreName;

            if (parentCollection != null) {
                foreach (string key in parentCollection) // seed the new collection
                {
                    Add(key, ConfigurationValueFlags.Inherited);  // add the local copy
                    if (_bExceptionList) {
                        if (SeedList.Length != 0)
                            SeedList += ",";
                        SeedList += key;
                    }
                }
            }

        }

        internal void ClearSeedList()
        {
            SeedList = String.Empty;
        }

        internal ConfigurationLockCollectionType LockType {
            get { return _lockType; }
        }

        public void Add(string name) {

            if (((_thisElement.ItemLocked & ConfigurationValueFlags.Locked) != 0) &&
                ((_thisElement.ItemLocked & ConfigurationValueFlags.Inherited) != 0)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, name));
            }

            ConfigurationValueFlags flags = ConfigurationValueFlags.Modified;

            string attribToLockTrim = name.Trim(); 
            ConfigurationProperty propToLock = _thisElement.Properties[attribToLockTrim];
            if (propToLock == null && attribToLockTrim != LockAll) {
                ConfigurationElementCollection collection = _thisElement as ConfigurationElementCollection;
                if (collection == null && _thisElement.Properties.DefaultCollectionProperty != null) { // this is not a collection but it may contain a default collection
                    collection = _thisElement[_thisElement.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                }

                if (collection == null ||
                    _lockType == ConfigurationLockCollectionType.LockedAttributes || // If the collection type is not element then the lock is bogus
                    _lockType == ConfigurationLockCollectionType.LockedExceptionList) {
                    _thisElement.ReportInvalidLock(attribToLockTrim, _lockType, null, null);
                }
                else if (!collection.IsLockableElement(attribToLockTrim)) {
                    _thisElement.ReportInvalidLock(attribToLockTrim, _lockType, null, collection.LockableElements);
                }
            }
            else { // the lock is in the property bag but is it the correct type?
                if (propToLock != null && propToLock.IsRequired)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_required_attribute_lock_attempt, propToLock.Name));

                if (attribToLockTrim != LockAll) {
                    if ((_lockType == ConfigurationLockCollectionType.LockedElements) ||
                         (_lockType == ConfigurationLockCollectionType.LockedElementsExceptionList)) {
                        // If it is an element then it must be derived from ConfigurationElement
                        if (!typeof(ConfigurationElement).IsAssignableFrom(propToLock.Type)) {
                            _thisElement.ReportInvalidLock(attribToLockTrim, _lockType, null, null);
                        }
                    }
                    else {
                        // if it is a property then it cannot be derived from ConfigurationElement
                        if (typeof(ConfigurationElement).IsAssignableFrom(propToLock.Type)) {
                            _thisElement.ReportInvalidLock(attribToLockTrim, _lockType, null, null);
                        }
                    }
                }
            }

            if (internalDictionary.Contains(name)) {
                flags = ConfigurationValueFlags.Modified | (ConfigurationValueFlags)internalDictionary[name];
                internalDictionary.Remove(name); // not from parent
                internalArraylist.Remove(name);
            }
            internalDictionary.Add(name, flags); // not from parent
            internalArraylist.Add(name);
            _bModified = true;
        }


        internal void Add(string name, ConfigurationValueFlags flags) {
            if ((flags != ConfigurationValueFlags.Inherited) && (internalDictionary.Contains(name))) {
                // the user has an item declared as locked below a level where it is already locked
                // keep enough info so we can write out the lock if they save in modified mode
                flags = ConfigurationValueFlags.Modified | (ConfigurationValueFlags)internalDictionary[name];
                internalDictionary.Remove(name);
                internalArraylist.Remove(name);
            }

            internalDictionary.Add(name, flags); // not from parent
            internalArraylist.Add(name);
        }

        internal bool DefinedInParent(string name) {
            if (name == null)
                return false;
            if (_bExceptionList)
            {
                string ParentListEnclosed = "," + SeedList + ",";                
                if (name.Equals(_ignoreName) || ParentListEnclosed.IndexOf("," + name + ",", StringComparison.Ordinal) >= 0) {
                    return true;
                }
            }
            return (internalDictionary.Contains(name) &&
                ((ConfigurationValueFlags)internalDictionary[name] & ConfigurationValueFlags.Inherited) != 0);
        }

        internal bool IsValueModified(string name) {
            return (internalDictionary.Contains(name) &&
                ((ConfigurationValueFlags)internalDictionary[name] & ConfigurationValueFlags.Modified) != 0);
        }

        internal void RemoveInheritedLocks() {
            StringCollection removeList = new StringCollection();
            foreach (string key in this) {
                if (DefinedInParent(key)) {
                    removeList.Add(key);
                }
            }
            foreach (string key in removeList) {
                internalDictionary.Remove(key);
                internalArraylist.Remove(key);
            }
        }


        public void Remove(string name) {
            if (!internalDictionary.Contains(name)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_not_found, name));
            }
            // in a locked list you cannot remove items that were locked in the parent
            // in an exception list this is legal because it makes the list more restrictive

            if (_bExceptionList == false &&
                ((ConfigurationValueFlags)internalDictionary[name] & ConfigurationValueFlags.Inherited) != 0) {
                if (((ConfigurationValueFlags)internalDictionary[name] & ConfigurationValueFlags.Modified) != 0) {
                    // allow the local one to be "removed" so it won't write out but throw if they try and remove
                    // one that is only inherited
                    ConfigurationValueFlags flags = (ConfigurationValueFlags)internalDictionary[name];
                    flags &= ~ConfigurationValueFlags.Modified;
                    internalDictionary[name] = flags;
                    _bModified = true;
                    return;
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, name));
                }
            }

            internalDictionary.Remove(name);
            internalArraylist.Remove(name);
            _bModified = true;
        }
        
        public IEnumerator GetEnumerator() {
            return internalArraylist.GetEnumerator();
        }

        internal void ClearInternal(bool useSeedIfAvailble) {
            ArrayList removeList = new ArrayList();
            foreach (DictionaryEntry de in internalDictionary) {
                if ((((ConfigurationValueFlags)de.Value & ConfigurationValueFlags.Inherited) == 0)
                    || _bExceptionList == true) {
                    removeList.Add(de.Key);
                }
            }
            foreach (object removeKey in removeList) {
                internalDictionary.Remove(removeKey);
                internalArraylist.Remove(removeKey);
            }

            // Clearing an Exception list really means revert to parent
            if (useSeedIfAvailble && !String.IsNullOrEmpty(SeedList)) {
                string[] Keys = SeedList.Split(new char[] { ',' });
                foreach (string key in Keys) {
                    Add(key, ConfigurationValueFlags.Inherited); // 
                }
            }
            _bModified = true;
        }

        public void Clear() {
            ClearInternal(true);
        }

        public bool Contains(string name) {
            if (_bExceptionList && name.Equals(_ignoreName)) {
                return true;
            }
            return internalDictionary.Contains(name);
        }
        
        public int Count { 
            get { 
                return internalDictionary.Count; 
            } 
        }
        
        public bool IsSynchronized { 
            get { 
                return false; 
            } 
        }

        public object SyncRoot { 
            get { 
                return this; 
            } 
        }

        public void CopyTo(string[] array, int index) {
            ((ICollection)this).CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index) {
            internalArraylist.CopyTo(array, index);
        }

        public bool IsModified { 
            get { 
                return _bModified; 
            } 
        }
        
        internal void ResetModified() { 
            _bModified = false; 
        }
        
        public bool IsReadOnly(string name) {
            if (!internalDictionary.Contains(name)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_not_found, name));
            }
            return (bool)(((ConfigurationValueFlags)internalDictionary[name] & ConfigurationValueFlags.Inherited) != 0);
        }

        internal bool ExceptionList { 
            get { 
                return _bExceptionList; 
            } 
        }

        public string AttributeList {
            get {
                StringBuilder sb;
                sb = new StringBuilder();

                foreach (DictionaryEntry de in internalDictionary) {
                    if (sb.Length != 0) {
                        sb.Append(',');
                    }
                    sb.Append(de.Key);
                }
                return sb.ToString();
            }
        }

        public void SetFromList(string attributeList) {
            string[] splits = attributeList.Split(new char[] { ',', ';', ':' });
            Clear();
            foreach (string name in splits) {
                string attribTrim = name.Trim();
                if (!Contains(attribTrim)) {
                    Add(attribTrim);
                }
            }
        }
        public bool HasParentElements {
            get {
                // return true if there is at least one element that was defined in the parent
                bool result = false;
                // Check to see if the exception list is empty as a result of a merge from config
                // If so the there were some parent elements because empty string is invalid in config.
                // and the only way to get an empty list is for the merged config to have no elements
                // in common.
                if (ExceptionList && internalDictionary.Count == 0 && !String.IsNullOrEmpty(SeedList))
                    return true;

                foreach (DictionaryEntry de in internalDictionary) {
                    if ((bool)(((ConfigurationValueFlags)de.Value & ConfigurationValueFlags.Inherited) != 0)) {
                        result = true;
                        break;
                    }
                }

                return result;
            }
        }
    }
}
