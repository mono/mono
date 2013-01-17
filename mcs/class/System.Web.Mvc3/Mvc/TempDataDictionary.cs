namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;

    public class TempDataDictionary : IDictionary<string, object> {
        internal const string _tempDataSerializationKey = "__tempData";

        private Dictionary<string, object> _data;
        private HashSet<string> _initialKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _retainedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TempDataDictionary() {
            _data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public int Count {
            get {
                return _data.Count;
            }
        }

        public ICollection<string> Keys {
            get {
                return _data.Keys;
            }
        }

        public void Keep() {
            _retainedKeys.Clear();
            _retainedKeys.UnionWith(_data.Keys);
        }

        public void Keep(string key) {
            _retainedKeys.Add(key);
        }

        public void Load(ControllerContext controllerContext, ITempDataProvider tempDataProvider) {
            IDictionary<string, object> providerDictionary = tempDataProvider.LoadTempData(controllerContext);
            _data = (providerDictionary != null) ? new Dictionary<string, object>(providerDictionary, StringComparer.OrdinalIgnoreCase) :
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _initialKeys = new HashSet<string>(_data.Keys, StringComparer.OrdinalIgnoreCase);
            _retainedKeys.Clear();
        }

        public object Peek(string key) {
            object value;
            _data.TryGetValue(key, out value);
            return value;
        }

        public void Save(ControllerContext controllerContext, ITempDataProvider tempDataProvider) {
            string[] keysToKeep = _initialKeys.Union(_retainedKeys, StringComparer.OrdinalIgnoreCase).ToArray();
            string[] keysToRemove = _data.Keys.Except(keysToKeep, StringComparer.OrdinalIgnoreCase).ToArray();
            foreach (string key in keysToRemove) {
                _data.Remove(key);
            }
            tempDataProvider.SaveTempData(controllerContext, _data);
        }

        public ICollection<object> Values {
            get {
                return _data.Values;
            }
        }

        public object this[string key] {
            get {
                object value;
                if (TryGetValue(key, out value)) {
                    _initialKeys.Remove(key);
                    return value;
                }
                return null;
            }
            set {
                _data[key] = value;
                _initialKeys.Add(key);
            }
        }

        public void Add(string key, object value) {
            _data.Add(key, value);
            _initialKeys.Add(key);
        }

        public void Clear() {
            _data.Clear();
            _retainedKeys.Clear();
            _initialKeys.Clear();
        }

        public bool ContainsKey(string key) {
            return _data.ContainsKey(key);
        }

        public bool ContainsValue(object value) {
            return _data.ContainsValue(value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return new TempDataDictionaryEnumerator(this);
        }

        public bool Remove(string key) {
            _retainedKeys.Remove(key);
            _initialKeys.Remove(key);
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out object value) {
            _initialKeys.Remove(key);
            return _data.TryGetValue(key, out value);
        }

        #region ICollection<KeyValuePair<string, object>> Implementation
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
            get {
                return ((ICollection<KeyValuePair<string, object>>)_data).IsReadOnly;
            }
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int index) {
            ((ICollection<KeyValuePair<string, object>>)_data).CopyTo(array, index);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> keyValuePair) {
            _initialKeys.Add(keyValuePair.Key);
            ((ICollection<KeyValuePair<string, object>>)_data).Add(keyValuePair);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> keyValuePair) {
            return ((ICollection<KeyValuePair<string, object>>)_data).Contains(keyValuePair);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> keyValuePair) {
            _initialKeys.Remove(keyValuePair.Key);
            return ((ICollection<KeyValuePair<string, object>>)_data).Remove(keyValuePair);
        }
        #endregion

        #region IEnumerable Implementation
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)(new TempDataDictionaryEnumerator(this));
        }
        #endregion

        private sealed class TempDataDictionaryEnumerator : IEnumerator<KeyValuePair<string, object>> {
            private IEnumerator<KeyValuePair<string, object>> _enumerator;
            private TempDataDictionary _tempData;

            public TempDataDictionaryEnumerator(TempDataDictionary tempData) {
                _tempData = tempData;
                _enumerator = _tempData._data.GetEnumerator();
            }

            public KeyValuePair<string, object> Current {
                get {
                    KeyValuePair<string, object> kvp = _enumerator.Current;
                    _tempData._initialKeys.Remove(kvp.Key);
                    return kvp;
                }
            }

            public bool MoveNext() {
                return _enumerator.MoveNext();
            }

            public void Reset() {
                _enumerator.Reset();
            }

            #region IEnumerator Implementation
            object IEnumerator.Current {
                get {
                    return Current;
                }
            }
            #endregion

            #region IDisposable Implementation
            void IDisposable.Dispose() {
                _enumerator.Dispose();
            }
            #endregion
        }
    }
}
