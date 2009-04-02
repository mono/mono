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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Mvc.Resources;

    public class ModelBinderDictionary : IDictionary<Type, IModelBinder> {

        private IModelBinder _defaultBinder;
        private readonly Dictionary<Type, IModelBinder> _innerDictionary = new Dictionary<Type, IModelBinder>();

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public int Count {
            get {
                return _innerDictionary.Count;
            }
        }

        public IModelBinder DefaultBinder {
            get {
                if (_defaultBinder == null) {
                    _defaultBinder = new DefaultModelBinder();
                }
                return _defaultBinder;
            }
            set {
                _defaultBinder = value;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool IsReadOnly {
            get {
                return ((IDictionary<Type, IModelBinder>)_innerDictionary).IsReadOnly;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public ICollection<Type> Keys {
            get {
                return _innerDictionary.Keys;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public IModelBinder this[Type key] {
            get {
                IModelBinder binder;
                _innerDictionary.TryGetValue(key, out binder);
                return binder;
            }
            set {
                _innerDictionary[key] = value;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public ICollection<IModelBinder> Values {
            get {
                return _innerDictionary.Values;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Add(KeyValuePair<Type, IModelBinder> item) {
            ((IDictionary<Type, IModelBinder>)_innerDictionary).Add(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Add(Type key, IModelBinder value) {
            _innerDictionary.Add(key, value);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Clear() {
            _innerDictionary.Clear();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Contains(KeyValuePair<Type, IModelBinder> item) {
            return ((IDictionary<Type, IModelBinder>)_innerDictionary).Contains(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool ContainsKey(Type key) {
            return _innerDictionary.ContainsKey(key);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void CopyTo(KeyValuePair<Type, IModelBinder>[] array, int arrayIndex) {
            ((IDictionary<Type, IModelBinder>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        public IModelBinder GetBinder(Type modelType) {
            return GetBinder(modelType, true /* fallbackToDefault */);
        }

        public virtual IModelBinder GetBinder(Type modelType, bool fallbackToDefault) {
            if (modelType == null) {
                throw new ArgumentNullException("modelType");
            }

            return GetBinder(modelType, (fallbackToDefault) ? DefaultBinder : null);
        }

        private IModelBinder GetBinder(Type modelType, IModelBinder fallbackBinder) {
            // Try to look up a binder for this type. We use this order of precedence:
            // 1. Binder registered in the global table
            // 2. Binder attribute defined on the type
            // 3. Supplied fallback binder

            IModelBinder binder;
            if (_innerDictionary.TryGetValue(modelType, out binder)) {
                return binder;
            }

            binder = ModelBinders.GetBinderFromAttributes(modelType,
                () => String.Format(CultureInfo.CurrentUICulture, MvcResources.ModelBinderDictionary_MultipleAttributes, modelType.FullName));

            return binder ?? fallbackBinder;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public IEnumerator<KeyValuePair<Type, IModelBinder>> GetEnumerator() {
            return _innerDictionary.GetEnumerator();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Remove(KeyValuePair<Type, IModelBinder> item) {
            return ((IDictionary<Type, IModelBinder>)_innerDictionary).Remove(item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Remove(Type key) {
            return _innerDictionary.Remove(key);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool TryGetValue(Type key, out IModelBinder value) {
            return _innerDictionary.TryGetValue(key, out value);
        }

        #region IEnumerable Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_innerDictionary).GetEnumerator();
        }
        #endregion

    }
}
