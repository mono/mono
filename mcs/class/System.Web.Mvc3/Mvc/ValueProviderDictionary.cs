namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Routing;

    [Obsolete("The recommended alternative is to use one of the specific ValueProvider types, such as FormValueProvider.")]
    public class ValueProviderDictionary : IDictionary<string, ValueProviderResult>, IValueProvider {

        private readonly Dictionary<string, ValueProviderResult> _dictionary = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);

        public ValueProviderDictionary(ControllerContext controllerContext) {
            ControllerContext = controllerContext;
            if (controllerContext != null) {
                PopulateDictionary();
            }
        }

        public ControllerContext ControllerContext {
            get;
            private set;
        }

        public int Count {
            get {
                return ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Count;
            }
        }

        internal Dictionary<string, ValueProviderResult> Dictionary {
            get {
                return _dictionary;
            }
        }

        public bool IsReadOnly {
            get {
                return ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).IsReadOnly;
            }
        }

        public ICollection<string> Keys {
            get {
                return Dictionary.Keys;
            }
        }

        public ValueProviderResult this[string key] {
            get {
                ValueProviderResult result;
                Dictionary.TryGetValue(key, out result);
                return result;
            }
            set {
                Dictionary[key] = value;
            }
        }

        public ICollection<ValueProviderResult> Values {
            get {
                return Dictionary.Values;
            }
        }

        public void Add(KeyValuePair<string, ValueProviderResult> item) {
            ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Add(item);
        }

        public void Add(string key, object value) {
            string attemptedValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            ValueProviderResult vpResult = new ValueProviderResult(value, attemptedValue, CultureInfo.InvariantCulture);
            Add(key, vpResult);
        }

        public void Add(string key, ValueProviderResult value) {
            Dictionary.Add(key, value);
        }

        private void AddToDictionaryIfNotPresent(string key, ValueProviderResult result) {
            if (!String.IsNullOrEmpty(key)) {
                if (!Dictionary.ContainsKey(key)) {
                    Dictionary.Add(key, result);
                }
            }
        }

        public void Clear() {
            ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Clear();
        }

        public bool Contains(KeyValuePair<string, ValueProviderResult> item) {
            return ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Contains(item);
        }

        public bool ContainsKey(string key) {
            return Dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, ValueProviderResult>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, ValueProviderResult>> GetEnumerator() {
            return ((IEnumerable<KeyValuePair<string, ValueProviderResult>>)Dictionary).GetEnumerator();
        }

        private void PopulateDictionary() {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;

            // We use this order of precedence to populate the dictionary:
            // 1. Request form submission (should be culture-aware)
            // 2. Values from the RouteData (could be from the typed-in URL or from the route's default values)
            // 3. URI query string

            NameValueCollection form = ControllerContext.HttpContext.Request.Form;
            if (form != null) {
                string[] keys = form.AllKeys;
                foreach (string key in keys) {
                    string[] rawValue = form.GetValues(key);
                    string attemptedValue = form[key];
                    ValueProviderResult result = new ValueProviderResult(rawValue, attemptedValue, currentCulture);
                    AddToDictionaryIfNotPresent(key, result);
                }
            }

            RouteValueDictionary routeValues = ControllerContext.RouteData.Values;
            if (routeValues != null) {
                foreach (var kvp in routeValues) {
                    string key = kvp.Key;
                    object rawValue = kvp.Value;
                    string attemptedValue = Convert.ToString(rawValue, invariantCulture);
                    ValueProviderResult result = new ValueProviderResult(rawValue, attemptedValue, invariantCulture);
                    AddToDictionaryIfNotPresent(key, result);
                }
            }

            NameValueCollection queryString = ControllerContext.HttpContext.Request.QueryString;
            if (queryString != null) {
                string[] keys = queryString.AllKeys;
                foreach (string key in keys) {
                    string[] rawValue = queryString.GetValues(key);
                    string attemptedValue = queryString[key];
                    ValueProviderResult result = new ValueProviderResult(rawValue, attemptedValue, invariantCulture);
                    AddToDictionaryIfNotPresent(key, result);
                }
            }
        }

        public bool Remove(KeyValuePair<string, ValueProviderResult> item) {
            return ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Remove(item);
        }

        public bool Remove(string key) {
            return Dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out ValueProviderResult value) {
            return Dictionary.TryGetValue(key, out value);
        }

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)Dictionary).GetEnumerator();
        }
        #endregion

        #region IValueProvider Members
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "The declaring type is obsolete, so there is little benefit to exposing this as a virtual method.")]
        bool IValueProvider.ContainsPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }

            return ValueProviderUtil.CollectionContainsPrefix(Keys, prefix);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "The declaring type is obsolete, so there is little benefit to exposing this as a virtual method.")]
        ValueProviderResult IValueProvider.GetValue(string key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            ValueProviderResult vpResult;
            TryGetValue(key, out vpResult);
            return vpResult;
        }
        #endregion

    }
}
