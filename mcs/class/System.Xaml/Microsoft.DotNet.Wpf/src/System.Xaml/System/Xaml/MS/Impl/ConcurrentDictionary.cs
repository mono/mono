using System;
using System.Collections.Generic;

namespace System.xaml.MS.Impl
{
    // For use on 3.5, where there's no ConcurrentDictionary in mscorlib
    internal class ConcurrentDictionary<K, V> : IDictionary<K, V>
    {
        private System.Collections.Hashtable _hashtable;

        public ConcurrentDictionary()
        {
            _hashtable = new System.Collections.Hashtable();
        }

        public void Add(K key, V value)
        {
            object val = CheckValue(value);
            lock (_hashtable)
            {
                _hashtable.Add(key, val);
            }
        }

        public bool ContainsKey(K key)
        {
            return _hashtable.ContainsKey(key);
        }

        public ICollection<K> Keys
        {
            get
            {
                List<K> result = new List<K>(_hashtable.Count);
                lock (_hashtable)
                {
                    foreach (object key in _hashtable.Keys)
                    {
                        result.Add((K)key);
                    }
                }
                return result.AsReadOnly();
            }
        }

        public bool Remove(K key)
        {
            lock (_hashtable)
            {
                if (_hashtable.ContainsKey(key))
                {
                    _hashtable.Remove(key);
                    return true;
                }
                return false;
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            object result = _hashtable[key];
            if (result == null)
            {
                value = default(V);
                return false;
            }
            value = (V)result;
            return true;
        }

        public ICollection<V> Values
        {
            get
            {
                List<V> result = new List<V>(_hashtable.Count);
                lock (_hashtable)
                {
                    foreach (object value in _hashtable.Values)
                    {
                        result.Add((V)value);
                    }
                }
                return result.AsReadOnly();
            }
        }

        public V this[K key]
        {
            get
            {
                object result = _hashtable[key];
                if (result == null)
                {
                    throw new KeyNotFoundException();
                }
                return (V)result;
            }
            set
            {
                object val = CheckValue(value);
                lock (_hashtable)
                {
                    _hashtable[key] = val;
                }
            }
        }

        public void Clear()
        {
            _hashtable.Clear();
        }

        public int Count
        {
            get { return _hashtable.Count; }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            List<KeyValuePair<K,V>> result = new List<KeyValuePair<K,V>>();
            lock (_hashtable)
            {
                foreach (System.Collections.DictionaryEntry entry in _hashtable)
                {
                    result.Add(new KeyValuePair<K,V>((K)entry.Key, (V)entry.Value));
                }
            }
            return result.GetEnumerator();
        }

        public bool TryAdd(K key, V value)
        {
            object val = CheckValue(value);
            lock (_hashtable)
            {
                if (_hashtable.ContainsKey(key))
                {
                    return false;
                }
                _hashtable.Add(key, val);
                return true;
            }
        }

        public bool TryUpdate(K key, V value, V comparand)
        {
            IEqualityComparer<V> comparer = EqualityComparer<V>.Default;
            object val = CheckValue(value);
            lock (_hashtable)
            {
                object existingValue = _hashtable[key];
                if (existingValue == null)
                {
                    return false;
                }
                if (comparer.Equals((V)existingValue, comparand))
                {
                    _hashtable[key] = val;
                    return true;
                }
                return false;
            }
        }

        // We don't allow null because otherwise we have no way of distinguishing null from unset
        // without taking a lock on read
        private object CheckValue(V value)
        {
            object result = value;
            if (result == null)
            {
                throw new ArgumentNullException("value");
            }
            return result;
        }

        #region ICollection<KeyValuePair<K,V>> Members

        public void Add(KeyValuePair<K, V> item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
