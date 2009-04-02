/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public int Count {
            get {
                return _innerDictionary.Count;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public ICollection<string> Keys {
            get {
                return _innerDictionary.Keys;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public ICollection<ModelState> Values {
            get {
                return _innerDictionary.Values;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Add(KeyValuePair<string, ModelState> item) {
            ((IDictionary<string, ModelState>)_innerDictionary).Add(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Add(string key, ModelState value) {
            _innerDictionary.Add(key, value);
        }

        public void AddModelError(string key, Exception exception) {
            GetModelStateForKey(key).Errors.Add(exception);
        }

        public void AddModelError(string key, string errorMessage) {
            GetModelStateForKey(key).Errors.Add(errorMessage);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Clear() {
            _innerDictionary.Clear();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Contains(KeyValuePair<string, ModelState> item) {
            return ((IDictionary<string, ModelState>)_innerDictionary).Contains(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool ContainsKey(string key) {
            return _innerDictionary.ContainsKey(key);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void CopyTo(KeyValuePair<string, ModelState>[] array, int arrayIndex) {
            ((IDictionary<string, ModelState>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Remove(KeyValuePair<string, ModelState> item) {
            return ((IDictionary<string, ModelState>)_innerDictionary).Remove(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Remove(string key) {
            return _innerDictionary.Remove(key);
        }

        public void SetModelValue(string key, ValueProviderResult value) {
            GetModelStateForKey(key).Value = value;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool TryGetValue(string key, out ModelState value) {
            return _innerDictionary.TryGetValue(key, out value);
        }

        #region IEnumerable Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_innerDictionary).GetEnumerator();
        }
        #endregion

    }
}
