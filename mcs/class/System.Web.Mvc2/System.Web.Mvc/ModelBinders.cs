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
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Linq;
    using System.Reflection;
    using System.Web;

    public static class ModelBinders {

        private static readonly ModelBinderDictionary _binders = CreateDefaultBinderDictionary();

        public static ModelBinderDictionary Binders {
            get {
                return _binders;
            }
        }

        internal static IModelBinder GetBinderFromAttributes(Type type, Func<string> errorMessageAccessor) {
            AttributeCollection allAttrs = TypeDescriptorHelper.Get(type).GetAttributes();
            CustomModelBinderAttribute[] filteredAttrs = allAttrs.OfType<CustomModelBinderAttribute>().ToArray();
            return GetBinderFromAttributesImpl(filteredAttrs, errorMessageAccessor);
        }

        internal static IModelBinder GetBinderFromAttributes(ICustomAttributeProvider element, Func<string> errorMessageAccessor) {
            CustomModelBinderAttribute[] attrs = (CustomModelBinderAttribute[])element.GetCustomAttributes(typeof(CustomModelBinderAttribute), true /* inherit */);
            return GetBinderFromAttributesImpl(attrs, errorMessageAccessor);
        }

        private static IModelBinder GetBinderFromAttributesImpl(CustomModelBinderAttribute[] attrs, Func<string> errorMessageAccessor) {
            // this method is used to get a custom binder based on the attributes of the element passed to it.
            // it will return null if a binder cannot be detected based on the attributes alone.

            if (attrs == null) {
                return null;
            }

            switch (attrs.Length) {
                case 0:
                    return null;

                case 1:
                    IModelBinder binder = attrs[0].GetBinder();
                    return binder;

                default:
                    string errorMessage = errorMessageAccessor();
                    throw new InvalidOperationException(errorMessage);
            }
        }

        private static ModelBinderDictionary CreateDefaultBinderDictionary() {
            // We can't add a binder to the HttpPostedFileBase type as an attribute, so we'll just
            // prepopulate the dictionary as a convenience to users.

            ModelBinderDictionary binders = new ModelBinderDictionary() {
                { typeof(HttpPostedFileBase), new HttpPostedFileBaseModelBinder() },
                { typeof(byte[]), new ByteArrayModelBinder() },
                { typeof(Binary), new LinqBinaryModelBinder() }
            };
            return binders;
        }

    }
}
