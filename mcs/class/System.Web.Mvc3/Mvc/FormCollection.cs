namespace System.Web.Mvc {
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Helpers;

    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "It is not anticipated that users will need to serialize this type.")]
    [SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers", Justification = "It is not anticipated that users will call FormCollection.CopyTo().")]
    [FormCollectionBinder]
    public sealed class FormCollection : NameValueCollection, IValueProvider {

        public FormCollection() {
        }

        public FormCollection(NameValueCollection collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            Add(collection);
        }

        internal FormCollection(ControllerBase controller, Func<NameValueCollection> validatedValuesThunk, Func<NameValueCollection> unvalidatedValuesThunk) {
            Add(controller == null || controller.ValidateRequest ? validatedValuesThunk() : unvalidatedValuesThunk());
        }

        public ValueProviderResult GetValue(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            string[] rawValue = GetValues(name);
            if (rawValue == null) {
                return null;
            }

            string attemptedValue = this[name];
            return new ValueProviderResult(rawValue, attemptedValue, CultureInfo.CurrentCulture);
        }

        public IValueProvider ToValueProvider() {
            return this;
        }

        #region IValueProvider Members
        bool IValueProvider.ContainsPrefix(string prefix) {
            return ValueProviderUtil.CollectionContainsPrefix(AllKeys, prefix);
        }

        ValueProviderResult IValueProvider.GetValue(string key) {
            return GetValue(key);
        }
        #endregion

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

                    return new FormCollection(controllerContext.Controller,
                                              () => controllerContext.HttpContext.Request.Form,
                                              () => controllerContext.HttpContext.Request.Unvalidated().Form);
                }
            }
        }

    }
}
