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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Mvc.Resources;

    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable",
        Justification = "It is not anticipated that users will need to serialize this type.")]
    [SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers",
        Justification = "It is not anticipated that users will call FormCollection.CopyTo().")]
    [FormCollectionBinder]
    public class FormCollection : NameValueCollection {

        public FormCollection() {
        }

        public FormCollection(NameValueCollection collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            Add(collection);
        }

        public IDictionary<string, ValueProviderResult> ToValueProvider() {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            Dictionary<string, ValueProviderResult> dict = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);
            string[] keys = AllKeys;
            foreach (string key in keys) {
                string[] rawValue = GetValues(key);
                string attemptedValue = this[key];
                ValueProviderResult vpResult = new ValueProviderResult(rawValue, attemptedValue, currentCulture);
                dict[key] = vpResult;
            }

            return dict;
        }

        public virtual ValueProviderResult GetValue(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "name");
            }

            string[] rawValue = GetValues(name);
            if (rawValue == null) {
                return null;
            }

            string attemptedValue = this[name];
            return new ValueProviderResult(rawValue, attemptedValue, CultureInfo.CurrentCulture);
        }

        private sealed class FormCollectionBinderAttribute : CustomModelBinderAttribute {

            // since the FormCollectionModelBinder.BindModel() method is thread-safe, we only need to keep
            // a single instance of the binder around
            private static readonly FormCollectionModelBinder _binder = new FormCollectionModelBinder();

            public override IModelBinder GetBinder() {
                return _binder;
            }

            // this class is used for generating a FormCollection object
            private sealed class FormCollectionModelBinder : IModelBinder {
                public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
                    if (controllerContext == null) {
                        throw new ArgumentNullException("controllerContext");
                    }

                    return new FormCollection(controllerContext.HttpContext.Request.Form);
                }
            }
        }

    }
}
