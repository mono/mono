//---------------------------------------------------------------------
// <copyright file="KeyToListMap.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.Collections.ObjectModel;

namespace System.Data.Common.Utils {

    // This class contains an abstraction that map a key of type TKey to a
    // list of values (of type TValue). This is really a convenience abstraction
    internal class KeyToListMap<TKey, TValue> : InternalBase {

        #region Constructors

        // effects: Creates an empty map with keys compared using comparer
        internal KeyToListMap(IEqualityComparer<TKey> comparer) {
            Debug.Assert(null != comparer);
            m_map = new Dictionary<TKey, List<TValue>>(comparer);
        }
        #endregion

        #region Fields
        // Just a regular dictionary
        private Dictionary<TKey, List<TValue>> m_map;
        #endregion

        #region Properties
        // effects: Yields all the keys in this
        internal IEnumerable<TKey> Keys {
            get { return m_map.Keys; }
        }

        // effects: Returns all the values for all keys with all the values
        // of a particular key adjacent to each other
        internal IEnumerable<TValue> AllValues {
            get {
                foreach (TKey key in Keys) {
                    foreach (TValue value in ListForKey(key)) {
                        yield return value;
                    }
                }
            }
        }

        // effects: Returns all the Dictionary Entries in this Map.
        internal IEnumerable<KeyValuePair<TKey, List<TValue>>> KeyValuePairs {
            get {
                return m_map;
            }
        }
        #endregion

        #region Methods
        internal bool ContainsKey(TKey key) {
            return m_map.ContainsKey(key);
        }
        
        // effects: Adds <key, value> to this. If the entry already exists, another one is added
        internal void Add(TKey key, TValue value) {
            // If entry for key already exists, add value to the list, else
            // create a new list and add the value to it
            List<TValue> valueList;
            if (!m_map.TryGetValue(key, out valueList)) {
                valueList = new List<TValue>();
                m_map[key] = valueList;
            }
            valueList.Add(value);
        }

        // effects: Adds <key, value> for each value in values to this. If the entry already exists, another one is added
        internal void AddRange(TKey key, IEnumerable<TValue> values) {
            foreach (TValue value in values) {
                Add(key, value);
            }
        }

        // effects: Removes all entries corresponding to key
        // Returns true iff the key was removed
        internal bool RemoveKey(TKey key) {
            return m_map.Remove(key);
        }
        
        // requires: key exist in this
        // effects: Returns the values associated with key
        internal System.Collections.ObjectModel.ReadOnlyCollection<TValue> ListForKey(TKey key) {
            Debug.Assert(m_map.ContainsKey(key), "key not registered in map");
            return new System.Collections.ObjectModel.ReadOnlyCollection<TValue>(m_map[key]);
        }

        // effects: Returns true if the key exists and false if not.
        //          In case the Key exists, the out parameter is assigned the List for that key,
        //          otherwise it is assigned a null value
        internal bool TryGetListForKey(TKey key, out System.Collections.ObjectModel.ReadOnlyCollection<TValue> valueCollection)
        {
            List<TValue> list;
            valueCollection = null;
            if (m_map.TryGetValue(key, out list))
            {
                valueCollection = new System.Collections.ObjectModel.ReadOnlyCollection<TValue>(list);
                return true;
            }
            return false;
        }


        // Returns all values for the given key. If no values have been added for the key,
        // yields no values.
        internal IEnumerable<TValue> EnumerateValues(TKey key) {
            List<TValue> values;
            if (m_map.TryGetValue(key, out values)) {
                foreach (TValue value in values) { yield return value; }
            }
        }


        internal override void ToCompactString(StringBuilder builder) {
            foreach (TKey key in Keys) {
                // Calling key's ToString here
                StringUtil.FormatStringBuilder(builder, "{0}", key);
                builder.Append(": ");
                IEnumerable<TValue> values = ListForKey(key);
                StringUtil.ToSeparatedString(builder, values, ",", "null");
                builder.Append("; ");
            }
        }
        #endregion
    }
}
