namespace System.Web.Routing {
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class RouteValueDictionary : IDictionary<string, object> {
        private Dictionary<string, object> _dictionary;

        public RouteValueDictionary() {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public RouteValueDictionary(object values) {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            AddValues(values);
        }

        public RouteValueDictionary(IDictionary<string, object> dictionary) {
            _dictionary = new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public int Count {
            get {
                return _dictionary.Count;
            }
        }

        public Dictionary<string, object>.KeyCollection Keys {
            get {
                return _dictionary.Keys;
            }
        }

        public Dictionary<string, object>.ValueCollection Values {
            get {
                return _dictionary.Values;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public object this[string key] {
            get {
                object value;
                TryGetValue(key, out value);
                return value;
            }
            set {
                _dictionary[key] = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Add(string key, object value) {
            _dictionary.Add(key, value);
        }

        private void AddValues(object values) {
            if (values != null) {
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(values);
                foreach (PropertyDescriptor prop in props) {
                    object val = prop.GetValue(values);
                    Add(prop.Name, val);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Clear() {
            _dictionary.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool ContainsKey(string key) {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(object value) {
            return _dictionary.ContainsValue(value);
        }

        public Dictionary<string, object>.Enumerator GetEnumerator() {
            return _dictionary.GetEnumerator();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Remove(string key) {
            return _dictionary.Remove(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool TryGetValue(string key, out object value) {
            return _dictionary.TryGetValue(key, out value);
        }

        #region IDictionary<string,object> Members
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        ICollection<string> IDictionary<string, object>.Keys {
            get {
                return _dictionary.Keys;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        ICollection<object> IDictionary<string, object>.Values {
            get {
                return _dictionary.Values;
            }
        }
        #endregion

        #region ICollection<KeyValuePair<string,object>> Members
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) {
            ((ICollection<KeyValuePair<string, object>>)_dictionary).Add(item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) {
            return ((ICollection<KeyValuePair<string, object>>)_dictionary).Contains(item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<string, object>>)_dictionary).CopyTo(array, arrayIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
            get {
                return ((ICollection<KeyValuePair<string, object>>)_dictionary).IsReadOnly;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) {
            return ((ICollection<KeyValuePair<string, object>>)_dictionary).Remove(item);
        }
        #endregion

        #region IEnumerable<KeyValuePair<string,object>> Members
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
    }
}
