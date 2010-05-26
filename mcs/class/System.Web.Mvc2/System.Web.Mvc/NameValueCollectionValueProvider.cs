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
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;

    public class NameValueCollectionValueProvider : IValueProvider {

        private readonly HashSet<string> _prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ValueProviderResult> _values = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);

        public NameValueCollectionValueProvider(NameValueCollection collection, CultureInfo culture) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            AddValues(collection, culture);
        }

        private void AddValues(NameValueCollection collection, CultureInfo culture) {
            if (collection.Count > 0) {
                _prefixes.Add("");
            }

            foreach (string key in collection) {
                if (key != null) {
                    _prefixes.UnionWith(ValueProviderUtil.GetPrefixes(key));

                    string[] rawValue = collection.GetValues(key);
                    string attemptedValue = collection[key];
                    _values[key] = new ValueProviderResult(rawValue, attemptedValue, culture);
                }
            }
        }

        public virtual bool ContainsPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }

            return _prefixes.Contains(prefix);
        }

        public virtual ValueProviderResult GetValue(string key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            ValueProviderResult vpResult;
            _values.TryGetValue(key, out vpResult);
            return vpResult;
        }

    }
}
