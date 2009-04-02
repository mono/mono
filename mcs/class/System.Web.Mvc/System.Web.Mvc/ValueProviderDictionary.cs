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
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Routing;

    public class ValueProviderDictionary : IDictionary<string, ValueProviderResult> {

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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool IsReadOnly {
            get {
                return ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).IsReadOnly;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public ICollection<string> Keys {
            get {
                return Dictionary.Keys;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public ICollection<ValueProviderResult> Values {
            get {
                return Dictionary.Values;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Add(KeyValuePair<string, ValueProviderResult> item) {
            ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Add(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Clear() {
            ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Clear();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Contains(KeyValuePair<string, ValueProviderResult> item) {
            return ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Contains(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool ContainsKey(string key) {
            return Dictionary.ContainsKey(key);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void CopyTo(KeyValuePair<string, ValueProviderResult>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).CopyTo(array, arrayIndex);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Remove(KeyValuePair<string, ValueProviderResult> item) {
            return ((ICollection<KeyValuePair<string, ValueProviderResult>>)Dictionary).Remove(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Remove(string key) {
            return Dictionary.Remove(key);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool TryGetValue(string key, out ValueProviderResult value) {
            return Dictionary.TryGetValue(key, out value);
        }

        #region IEnumerable Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)Dictionary).GetEnumerator();
        }
        #endregion

    }
}
