namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    public class NameValueCollectionValueProvider : IValueProvider, IUnvalidatedValueProvider {

        private readonly CultureInfo _culture;
        private readonly PrefixContainer _prefixes;
        private readonly Dictionary<string, ValueProviderResultPlaceholder> _values = new Dictionary<string, ValueProviderResultPlaceholder>(StringComparer.OrdinalIgnoreCase);
        private readonly NameValueCollection _validatedCollection;
        private readonly NameValueCollection _unvalidatedCollection;

        public NameValueCollectionValueProvider(NameValueCollection collection, CultureInfo culture)
            : this(collection, null /* unvalidatedCollection */, culture) {
        }

        public NameValueCollectionValueProvider(NameValueCollection collection, NameValueCollection unvalidatedCollection, CultureInfo culture) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            _culture = culture;
            _prefixes = new PrefixContainer(collection.Keys.Cast<string>());
            _validatedCollection = collection;
            _unvalidatedCollection = unvalidatedCollection ?? collection;

            foreach (string key in collection) {
                if (key != null) {
                    _values[key] = new ValueProviderResultPlaceholder(key, this);
                }
            }
        }

        public virtual bool ContainsPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }

            return _prefixes.ContainsPrefix(prefix);
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
            private readonly Func<ValueProviderResult> _validatedResultAccessor;
            private readonly Func<ValueProviderResult> _unvalidatedResultAccessor;
            private ValueProviderResult _validatedResult;
            private ValueProviderResult _unvalidatedResult;

            public ValueProviderResultPlaceholder(string key, NameValueCollectionValueProvider valueProvider) {
                _validatedResultAccessor = () => GetResultFromCollection(key, valueProvider, useValidatedCollection: true);
                _unvalidatedResultAccessor = () => GetResultFromCollection(key, valueProvider, useValidatedCollection: false);
            }

            private static ValueProviderResult GetResultFromCollection(string key, NameValueCollectionValueProvider valueProvider, bool useValidatedCollection) {
                NameValueCollection collection = (useValidatedCollection) ? valueProvider._validatedCollection : valueProvider._unvalidatedCollection;
                string[] rawValue = collection.GetValues(key);
                string attemptedValue = collection[key];
                return new ValueProviderResult(rawValue, attemptedValue, valueProvider._culture);
            }

            public ValueProviderResult ValidatedResult {
                get { return LazyInitializer.EnsureInitialized(ref _validatedResult, _validatedResultAccessor); }
            }

            public ValueProviderResult UnvalidatedResult {
                get { return LazyInitializer.EnsureInitialized(ref _unvalidatedResult, _unvalidatedResultAccessor); }
            }
        }

    }
}
