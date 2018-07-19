namespace System.Web.ModelBinding {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class ModelStateDictionary : IDictionary<string, ModelState> {

        private readonly Dictionary<string, ModelState> _innerDictionary = new Dictionary<string, ModelState>(StringComparer.OrdinalIgnoreCase);

        public ModelStateDictionary() {
        }

        public ModelStateDictionary(ModelStateDictionary dictionary) {
            if (dictionary == null) {
                throw new ArgumentNullException("dictionary");
            }

            foreach (var entry in dictionary) {
                _innerDictionary.Add(entry.Key, entry.Value);
            }
        }

        public int Count {
            get {
                return _innerDictionary.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return ((IDictionary<string, ModelState>)_innerDictionary).IsReadOnly;
            }
        }

        public bool IsValid {
            get {
                return Values.All(modelState => modelState.Errors.Count == 0);
            }
        }

        public ICollection<string> Keys {
            get {
                return _innerDictionary.Keys;
            }
        }

        public ModelState this[string key] {
            get {
                ModelState value;
                _innerDictionary.TryGetValue(key, out value);
                return value;
            }
            set {
                _innerDictionary[key] = value;
            }
        }

        public ICollection<ModelState> Values {
            get {
                return _innerDictionary.Values;
            }
        }

        public void Add(KeyValuePair<string, ModelState> item) {
            ((IDictionary<string, ModelState>)_innerDictionary).Add(item);
        }

        public void Add(string key, ModelState value) {
            _innerDictionary.Add(key, value);
        }

        public void AddModelError(string key, Exception exception) {
            GetModelStateForKey(key).Errors.Add(exception);
        }

        public void AddModelError(string key, string errorMessage) {
            GetModelStateForKey(key).Errors.Add(errorMessage);
        }

        public void Clear() {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, ModelState> item) {
            return ((IDictionary<string, ModelState>)_innerDictionary).Contains(item);
        }

        public bool ContainsKey(string key) {
            return _innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, ModelState>[] array, int arrayIndex) {
            ((IDictionary<string, ModelState>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, ModelState>> GetEnumerator() {
            return _innerDictionary.GetEnumerator();
        }

        private ModelState GetModelStateForKey(string key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            ModelState modelState;
            if (!TryGetValue(key, out modelState)) {
                modelState = new ModelState();
                this[key] = modelState;
            }

            return modelState;
        }

        public bool IsValidField(string key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            // if the key is not found in the dictionary, we just say that it's valid (since there are no errors)
            return DictionaryHelpers.FindKeysWithPrefix(this, key).All(entry => entry.Value.Errors.Count == 0);
        }

        public void Merge(ModelStateDictionary dictionary) {
            if (dictionary == null) {
                return;
            }

            foreach (var entry in dictionary) {
                this[entry.Key] = entry.Value;
            }
        }

        public bool Remove(KeyValuePair<string, ModelState> item) {
            return ((IDictionary<string, ModelState>)_innerDictionary).Remove(item);
        }

        public bool Remove(string key) {
            return _innerDictionary.Remove(key);
        }

        public void SetModelValue(string key, ValueProviderResult value) {
            GetModelStateForKey(key).Value = value;
        }

        public bool TryGetValue(string key, out ModelState value) {
            return _innerDictionary.TryGetValue(key, out value);
        }

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_innerDictionary).GetEnumerator();
        }
        #endregion

    }
}
