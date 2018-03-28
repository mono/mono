namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    public sealed class CookieValueProvider : IValueProvider, IUnvalidatedValueProvider {

        private readonly CultureInfo _culture;
        private readonly PrefixContainer _prefixes;
        private readonly Dictionary<string, ValueProviderResultPlaceholder> _values = new Dictionary<string, ValueProviderResultPlaceholder>(StringComparer.OrdinalIgnoreCase);
        private readonly HttpCookieCollection _validatedCollection;
        private readonly HttpCookieCollection _unvalidatedCollection;

        public CookieValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
            : this(modelBindingExecutionContext, modelBindingExecutionContext.HttpContext.Request.Unvalidated) {
        }

        internal CookieValueProvider(ModelBindingExecutionContext modelBindingExecutionContext, UnvalidatedRequestValuesBase unvalidatedValues)
            : this(modelBindingExecutionContext.HttpContext.Request.Cookies, unvalidatedValues.Cookies, CultureInfo.CurrentCulture) {
        }

        internal CookieValueProvider(HttpCookieCollection collection, HttpCookieCollection unvalidatedCollection, CultureInfo culture) {
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

        public bool ContainsPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }

            return _prefixes.ContainsPrefix(prefix);
        }

        public ValueProviderResult GetValue(string key) {
            return GetValue(key, skipValidation: false);
        }

        public ValueProviderResult GetValue(string key, bool skipValidation) {
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

            public ValueProviderResultPlaceholder(string key, CookieValueProvider valueProvider) {
                _validatedResultAccessor = () => GetResultFromCollection(key, valueProvider, useValidatedCollection: true);
                _unvalidatedResultAccessor = () => GetResultFromCollection(key, valueProvider, useValidatedCollection: false);
            }

            private static ValueProviderResult GetResultFromCollection(string key, CookieValueProvider valueProvider, bool useValidatedCollection) {
                HttpCookieCollection collection = (useValidatedCollection) ? valueProvider._validatedCollection : valueProvider._unvalidatedCollection;
                string value = collection[key].Value;
                return new ValueProviderResult(value, value, valueProvider._culture);

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
