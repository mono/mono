namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;

    public sealed class ModelBinderProviderCollection : Collection<ModelBinderProvider> {

        public ModelBinderProviderCollection() {
        }

        public ModelBinderProviderCollection(IList<ModelBinderProvider> list)
            : base(list) {
        }

        public IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }
            if (bindingContext == null) {
                throw new ArgumentNullException("bindingContext");
            }

            ModelBinderProvider providerFromAttr;
            if (TryGetProviderFromAttributes(bindingContext.ModelType, out providerFromAttr)) {
                return providerFromAttr.GetBinder(modelBindingExecutionContext, bindingContext);
            }

            return (from provider in this
                    let binder = provider.GetBinder(modelBindingExecutionContext, bindingContext)
                    where binder != null
                    select binder).FirstOrDefault();
        }

        internal IModelBinder GetRequiredBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            IModelBinder binder = GetBinder(modelBindingExecutionContext, bindingContext);
            if (binder == null) {
                throw Error.ModelBinderProviderCollection_BinderForTypeNotFound(bindingContext.ModelType);
            }
            return binder;
        }

        protected override void InsertItem(int index, ModelBinderProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            base.InsertItem(index, item);
        }

        private void InsertSimpleProviderAtFront(ModelBinderProvider provider) {
            // Don't want to insert simple providers before any that are marked as "should go first,"
            // as that might throw off other providers like the exact type match provider.

            int i = 0;
            for (; i < Count; i++) {
                if (!ShouldProviderGoFirst(this[i])) {
                    break;
                }
            }

            base.InsertItem(i, provider);
        }

        public void RegisterBinderForGenericType(Type modelType, IModelBinder modelBinder) {
            InsertSimpleProviderAtFront(new GenericModelBinderProvider(modelType, modelBinder));
        }

        public void RegisterBinderForGenericType(Type modelType, Func<Type[], IModelBinder> modelBinderFactory) {
            InsertSimpleProviderAtFront(new GenericModelBinderProvider(modelType, modelBinderFactory));
        }

        public void RegisterBinderForGenericType(Type modelType, Type modelBinderType) {
            InsertSimpleProviderAtFront(new GenericModelBinderProvider(modelType, modelBinderType));
        }

        public void RegisterBinderForType(Type modelType, IModelBinder modelBinder) {
            RegisterBinderForType(modelType, modelBinder, false /* suppressPrefixCheck */);
        }

        internal void RegisterBinderForType(Type modelType, IModelBinder modelBinder, bool suppressPrefixCheck) {
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(modelType, modelBinder) {
                SuppressPrefixCheck = suppressPrefixCheck
            };
            InsertSimpleProviderAtFront(provider);
        }

        public void RegisterBinderForType(Type modelType, Func<IModelBinder> modelBinderFactory) {
            InsertSimpleProviderAtFront(new SimpleModelBinderProvider(modelType, modelBinderFactory));
        }

        protected override void SetItem(int index, ModelBinderProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            base.SetItem(index, item);
        }

        private static bool ShouldProviderGoFirst(ModelBinderProvider provider) {
            ModelBinderProviderOptionsAttribute options = provider.GetType()
                .GetCustomAttributes(typeof(ModelBinderProviderOptionsAttribute), true /* inherit */)
                .OfType<ModelBinderProviderOptionsAttribute>()
                .FirstOrDefault();

            return (options != null) ? options.FrontOfList : false;
        }

        private static bool TryGetProviderFromAttributes(Type modelType, out ModelBinderProvider provider) {
            ExtensibleModelBinderAttribute attr = TypeDescriptorHelper.Get(modelType).GetAttributes().OfType<ExtensibleModelBinderAttribute>().FirstOrDefault();
            if (attr == null) {
                provider = null;
                return false;
            }

            if (typeof(ModelBinderProvider).IsAssignableFrom(attr.BinderType)) {
                provider = (ModelBinderProvider)SecurityUtils.SecureCreateInstance(attr.BinderType);
            }
            else if (typeof(IModelBinder).IsAssignableFrom(attr.BinderType)) {
                Type closedBinderType = (attr.BinderType.IsGenericTypeDefinition) ? attr.BinderType.MakeGenericType(modelType.GetGenericArguments()) : attr.BinderType;
                IModelBinder binderInstance = (IModelBinder)SecurityUtils.SecureCreateInstance(closedBinderType);
                provider = new SimpleModelBinderProvider(modelType, binderInstance) { SuppressPrefixCheck = attr.SuppressPrefixCheck };
            }
            else {
                string errorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ModelBinderProviderCollection_InvalidBinderType),
                    attr.BinderType, typeof(ModelBinderProvider), typeof(IModelBinder));
                throw new InvalidOperationException(errorMessage);
            }

            return true;
        }

    }
}
