namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Threading;

    public class NameValueCollectionValueProvider : IValueProvider, IUnvalidatedValueProvider {

        private readonly HashSet<string> _prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ValueProviderResultPlaceholder> _values = new Dictionary<string, ValueProviderResultPlaceholder>(StringComparer.OrdinalIgnoreCase);

        public NameValueCollectionValueProvider(NameValueCollection collection, CultureInfo culture)
            : this(collection, null /* unvalidatedCollection */, culture) {
        }

        public NameValueCollectionValueProvider(NameValueCollection collection, NameValueCollection unvalidatedCollection, CultureInfo culture) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            AddValues(collection, unvalidatedCollection ?? collection, culture);
        }

        private void AddValues(NameValueCollection validatedCollection, NameValueCollection unvalidatedCollection, CultureInfo culture) {
            // Need to read keys from the unvalidated collection, as M.W.I's granular request validation is a bit touchy
            // and validated entries at the time the key or value is looked at. For example, GetKey() will throw if the
            // value fails request validation, even though the value's not being looked at (M.W.I can't tell the difference).

            if (unvalidatedCollection.Count > 0) {
                _prefixes.Add("");
            }

            foreach (string key in unvalidatedCollection) {
                if (key != null) {
                    _prefixes.UnionWith(ValueProviderUtil.GetPrefixes(key));

                    // need to look up values lazily, as eagerly looking at the collection might trigger validation
                    _values[key] = new ValueProviderResultPlaceholder(key, validatedCollection, unvalidatedCollection, culture);
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
            return GetValue(key, skipValidation: false);
        }

        public virtual ValueProviderResult GetValue(string key, bool skipValidation) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            ValueProviderResultPlaceholder placeholder;
            _values.TryGetValue(key, out placeholder);
            if (placeholder == null) {
                return null;
            }
            else {
                return (skipValidation) ? placeholder.UnvalidatedResult : placeholder.ValidatedResult;
            }
        }

        // Placeholder that can store a validated (in relation to request validation) or unvalidated
        // ValueProviderResult for a given key.
        private sealed class ValueProviderResultPlaceholder {
            private readonly Lazy<ValueProviderResult> _validatedResultPlaceholder;
            private readonly Lazy<ValueProviderResult> _unvalidatedResultPlaceholder;

            public ValueProviderResultPlaceholder(string key, NameValueCollection validatedCollection, NameValueCollection unvalidatedCollection, CultureInfo culture) {
                _validatedResultPlaceholder = new Lazy<ValueProviderResult>(() => GetResultFromCollection(key, validatedCollection, culture), LazyThreadSafetyMode.None);
                _unvalidatedResultPlaceholder = new Lazy<ValueProviderResult>(() => GetResultFromCollection(key, unvalidatedCollection, culture), LazyThreadSafetyMode.None);
            }

            private static ValueProviderResult GetResultFromCollection(string key, NameValueCollection collection, CultureInfo culture) {
                string[] rawValue = collection.GetValues(key);
                string attemptedValue = collection[key];
                return new ValueProviderResult(rawValue, attemptedValue, culture);
            }

            public ValueProviderResult ValidatedResult {
                get { return _validatedResultPlaceholder.Value; }
            }

            public ValueProviderResult UnvalidatedResult {
                get { return _unvalidatedResultPlaceholder.Value; }
            }
        }
    }
}
