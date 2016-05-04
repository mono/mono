namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ValueProviderCollection : Collection<IValueProvider>, IValueProvider, IUnvalidatedValueProvider {

        public ValueProviderCollection() {
        }

        public ValueProviderCollection(IList<IValueProvider> list)
            : base(list) {
        }

        public virtual bool ContainsPrefix(string prefix) {
            return this.Any(vp => vp.ContainsPrefix(prefix));
        }

        public virtual ValueProviderResult GetValue(string key) {
            return GetValue(key, skipValidation: false);
        }

        public virtual ValueProviderResult GetValue(string key, bool skipValidation) {
            return (from provider in this
                    let result = GetValueFromProvider(provider, key, skipValidation)
                    where result != null
                    select result).FirstOrDefault();
        }

        internal static ValueProviderResult GetValueFromProvider(IValueProvider provider, string key, bool skipValidation) {
            // Since IUnvalidatedValueProvider is a superset of IValueProvider, it's always OK to use the
            // IUnvalidatedValueProvider-supplied members if they're present. Otherwise just call the
            // normal IValueProvider members.

            IUnvalidatedValueProvider unvalidatedProvider = provider as IUnvalidatedValueProvider;
            return (unvalidatedProvider != null) ? unvalidatedProvider.GetValue(key, skipValidation) : provider.GetValue(key);
        }

        protected override void InsertItem(int index, IValueProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IValueProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

    }
}
