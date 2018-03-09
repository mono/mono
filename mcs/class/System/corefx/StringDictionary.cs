// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;

namespace System.Collections.Specialized
{
    partial class StringDictionary
    {
        internal IDictionary<string, string> AsGenericDictionary() => new GenericAdapter(this);

        internal void ReplaceHashtable(Hashtable newContents) => contents = newContents;

        // Copied from mono/mcs/class/referencesource/System/compmod/system/collections/specialized/stringdictionary.cs
        // This class is used to make StringDictionary implement IDictionary<string,string> indirectly. 
        // This is done to prevent StringDictionary be serialized as IDictionary<string,string> and break its serialization by DataContractSerializer due to a bug in the serialization code.
        private class GenericAdapter : IDictionary<string, string>
        {

            StringDictionary m_stringDictionary;

            internal GenericAdapter(StringDictionary stringDictionary) {
                m_stringDictionary = stringDictionary;
            }

            #region IDictionary<string, string> Members
            public void Add(string key, string value) {

                // GenericAdapter.Add has the semantics of Item property to make ProcessStartInfo.Environment deserializable by DataContractSerializer.
                // ProcessStartInfo.Environment property does not have a setter
                // and so during deserialization the serializer initializes the property by calling get_Environment and 
                // then populates it via IDictionary<,>.Add per item.
                // However since get_Environment gives the current snapshot of environment variables we might try to insert a key that already exists.
                // (For Example 'PATH') causing an exception. This implementation ensures that we overwrite values in case of duplication.

                this[key] =  value;
            }

            public bool ContainsKey(string key) {
                return m_stringDictionary.ContainsKey(key);
            }

            public void Clear() {
                m_stringDictionary.Clear();
            }

            public int Count {
                get {
                    return m_stringDictionary.Count;
                }
            }

            // Items added to allow StringDictionary to provide IDictionary<string, string> support.
            ICollectionToGenericCollectionAdapter _values;
            ICollectionToGenericCollectionAdapter _keys;

            // IDictionary<string,string>.Item vs StringDictioanry.Item
            // IDictionary<string,string>.get_Item         i. KeyNotFoundException when the property is retrieved and key is not found.
            // StringBuilder.get_Item                      i. Returns null in case the key is not found.
            public string this[string key] {
                get {
                    if (key == null) {
                        throw new ArgumentNullException("key");
                    }

                    if (!m_stringDictionary.ContainsKey(key)) throw new KeyNotFoundException();

                    return m_stringDictionary[key];
                }
                set {
                    if (key == null) {
                        throw new ArgumentNullException("key");
                    }

                    m_stringDictionary[key] = value;
                }
            }

            // This method returns a read-only view of the Keys in the StringDictinary.
            public ICollection<string> Keys {
                get {
                    if( _keys == null ) {
                        _keys = new ICollectionToGenericCollectionAdapter(m_stringDictionary, KeyOrValue.Key);
                    }
                    return _keys;
                }
            }

            // This method returns a read-only view of the Values in the StringDictionary.
            public ICollection<string> Values {
                get {
                    if( _values == null ) {
                        _values = new ICollectionToGenericCollectionAdapter(m_stringDictionary, KeyOrValue.Value);
                    }
                    return _values;
                }
            }

            // IDictionary<string,string>.Remove vs StringDictionary.Remove.
            // IDictionary<string,string>.Remove-  i. Returns a bool status that represents success\failure.
            //                                    ii. Returns false in case key is not found.
            // StringDictionary.Remove             i. Does not return the status and does nothing in case key is not found.
            public bool Remove(string key) {

                // Check if the key is not present and return false.
                if (!m_stringDictionary.ContainsKey(key)) return false;

                // We call the virtual StringDictionary.Remove method to ensure any subClass gets the expected behavior.
                m_stringDictionary.Remove(key);

                // If the above call has succeeded we simply return true.
                return true;
            }


            public bool TryGetValue(string key, out string value) {
                if (!m_stringDictionary.ContainsKey(key)) {
                    value = null;
                    return false;
                }

                value = m_stringDictionary[key];
                return true;
            }

            void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) {
                m_stringDictionary.Add(item.Key, item.Value);
            }

            bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item) {
                string value;
                return TryGetValue(item.Key, out value) && value.Equals(item.Value);
            }

            void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
                if( array == null )
                    throw new ArgumentNullException("array", SR.GetString(SR.ArgumentNull_Array));
                if( arrayIndex < 0 )
                    throw new ArgumentOutOfRangeException("arrayIndex", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
                if( array.Length - arrayIndex < Count )
                    throw new ArgumentException(SR.GetString(SR.Arg_ArrayPlusOffTooSmall));

                int index = arrayIndex;

                foreach (DictionaryEntry entry in m_stringDictionary) {
                    array[index++] = new KeyValuePair<string, string>((string)entry.Key, (string)entry.Value);
                }
            }

            bool ICollection<KeyValuePair<string,string>>.IsReadOnly {
                get {
                    return false;
                }
            }

            // ICollection<KeyValuePair<string, string>>.Remove vs StringDictionary.Remove
            // ICollection<KeyValuePair<string, string>>.Remove - i. Return status.
            //                                                   ii. Returns false in case the items is not found.
            // StringDictionary.Remove                            i. Does not return a status and does nothing in case the key is not found.
            bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) {

                // If the item is not found return false.
                ICollection<KeyValuePair<string, string>> iCollection = this;
                if( !iCollection.Contains(item) ) return false;

                // We call the virtual StringDictionary.Remove method to ensure any subClass gets the expected behavior.
                m_stringDictionary.Remove(item.Key);

                // If the above call has succeeded we simply return true.
                return true;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }

            // The implementation asummes that this.GetEnumerator().Current can be casted to DictionaryEntry.
            // and although StringDictionary.GetEnumerator() returns IEnumerator and is a virtual method
            // it should be ok to take that assumption since it is an implicit contract.
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                foreach (DictionaryEntry dictionaryEntry in m_stringDictionary)
                    yield return new KeyValuePair<string, string>((string)dictionaryEntry.Key, (string)dictionaryEntry.Value);
            }

            internal enum KeyOrValue // Used internally for IDictionary<string, string> support 
            {
                Key,
                Value
            }

            // This Adapter converts StringDictionary.Keys and StringDictionary.Values to ICollection<string>
            // Since StringDictionary implements a virtual StringDictionary.Keys and StringDictionary.Values 
            private class ICollectionToGenericCollectionAdapter : ICollection<string> {

                StringDictionary _internal;
                KeyOrValue _keyOrValue;

                public ICollectionToGenericCollectionAdapter(StringDictionary source, KeyOrValue keyOrValue) {
                    if (source == null) throw new ArgumentNullException("source");

                    _internal = source;
                    _keyOrValue = keyOrValue;
                }

                public void Add(string item) {
                    ThrowNotSupportedException();
                }

                public void Clear() {
                    ThrowNotSupportedException();
                }

                public void ThrowNotSupportedException() {
                    if( _keyOrValue == KeyOrValue.Key ) {
                        throw new NotSupportedException(SR.GetString(SR.NotSupported_KeyCollectionSet)); //Same as KeyCollection/ValueCollection
                    }
                    throw new NotSupportedException(SR.GetString(SR.NotSupported_ValueCollectionSet)); //Same as KeyCollection/ValueCollection
                }


                public bool Contains(string item) {
                    // The underlying backing store for the StringDictionary is a HashTable so we 
                    // want to delegate Contains to respective ContainsKey/ContainsValue functionality 
                    // depending upon whether we are using Keys or Value collections.            

                    if( _keyOrValue == KeyOrValue.Key ) {
                        return _internal.ContainsKey(item);
                    }
                    return _internal.ContainsValue(item);
                }

                public void CopyTo(string[] array, int arrayIndex) {
                    var collection = GetUnderlyingCollection();
                    collection.CopyTo(array, arrayIndex);
                }

                public int Count {
                    get {
                        return _internal.Count;  // hashtable count is same as key/value count.
                    }
                }

                public bool IsReadOnly {
                    get {
                        return true; //Same as KeyCollection/ValueCollection
                    }
                }

                public bool Remove(string item) {
                    ThrowNotSupportedException();
                    return false;
                }

                private ICollection GetUnderlyingCollection() {
                    if( _keyOrValue == KeyOrValue.Key ) {
                        return (ICollection) _internal.Keys;
                    }
                    return (ICollection) _internal.Values;
                }

                public IEnumerator<string> GetEnumerator() {
                    ICollection collection = GetUnderlyingCollection();

                    // This is doing the same as collection.Cast<string>()
                    foreach (string entry in collection) {
                        yield return entry;
                    }
                }

                IEnumerator IEnumerable.GetEnumerator() {
                    return GetUnderlyingCollection().GetEnumerator();
                }
            }
            #endregion
        }
    }
}