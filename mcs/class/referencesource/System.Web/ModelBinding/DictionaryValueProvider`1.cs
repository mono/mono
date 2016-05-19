namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class DictionaryValueProvider<TValue> : IValueProvider {

        private readonly PrefixContainer _prefixes;
        private readonly Dictionary<string, ValueProviderResult> _values = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);

        public DictionaryValueProvider(IDictionary<string, TValue> dictionary, CultureInfo culture) {
            if (dictionary == null) {
                throw new ArgumentNullException("dictionary");
            }

            _prefixes = new PrefixContainer(dictionary.Keys);
            AddValues(dictionary, culture);
        }

        private void AddValues(IDictionary<string, TValue> dictionary, CultureInfo culture) {
            foreach (var entry in dictionary) {
                object rawValue = entry.Value;
                string attemptedValue = Convert.ToString(rawValue, culture);
                _values[entry.Key] = new ValueProviderResult(rawValue, attemptedValue, culture);
            }
        }

        public virtual bool ContainsPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }

            return _prefixes.ContainsPrefix(prefix);
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
