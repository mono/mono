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
    using System.Globalization;

    public class DictionaryValueProvider<TValue> : IValueProvider {

        private readonly HashSet<string> _prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ValueProviderResult> _values = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);

        public DictionaryValueProvider(IDictionary<string, TValue> dictionary, CultureInfo culture) {
            if (dictionary == null) {
                throw new ArgumentNullException("dictionary");
            }

            AddValues(dictionary, culture);
        }

        private void AddValues(IDictionary<string, TValue> dictionary, CultureInfo culture) {
            if (dictionary.Count > 0) {
                _prefixes.Add("");
            }

            foreach (var entry in dictionary) {
                _prefixes.UnionWith(ValueProviderUtil.GetPrefixes(entry.Key));

                object rawValue = entry.Value;
                string attemptedValue = Convert.ToString(rawValue, culture);
                _values[entry.Key] = new ValueProviderResult(rawValue, attemptedValue, culture);
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
