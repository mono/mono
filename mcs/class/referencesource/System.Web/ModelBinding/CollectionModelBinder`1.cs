namespace System.Web.ModelBinding {
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    public class CollectionModelBinder<TElement> : IModelBinder {

        // Used when the ValueProvider contains the collection to be bound as multiple elements, e.g. foo[0], foo[1].
        private static List<TElement> BindComplexCollection(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            string indexPropertyName = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, "index");
            ValueProviderResult vpResultIndex = bindingContext.UnvalidatedValueProvider.GetValue(indexPropertyName);
            IEnumerable<string> indexNames = CollectionModelBinderUtil.GetIndexNamesFromValueProviderResult(vpResultIndex);
            return BindComplexCollectionFromIndexes(modelBindingExecutionContext, bindingContext, indexNames);
        }

        internal static List<TElement> BindComplexCollectionFromIndexes(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext, IEnumerable<string> indexNames) {
            bool indexNamesIsFinite;
            if (indexNames != null) {
                indexNamesIsFinite = true;
            }
            else {
                indexNamesIsFinite = false;
                indexNames = CollectionModelBinderUtil.GetZeroBasedIndexes();
            }

            List<TElement> boundCollection = new List<TElement>();
            foreach (string indexName in indexNames) {
                string fullChildName = ModelBinderUtil.CreateIndexModelName(bindingContext.ModelName, indexName);
                ModelBindingContext childBindingContext = new ModelBindingContext(bindingContext) {
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(TElement)),
                    ModelName = fullChildName
                };

                object boundValue = null;
                IModelBinder childBinder = bindingContext.ModelBinderProviders.GetBinder(modelBindingExecutionContext, childBindingContext);
                if (childBinder != null) {
                    if (childBinder.BindModel(modelBindingExecutionContext, childBindingContext)) {
                        boundValue = childBindingContext.Model;

                        // merge validation up
                        bindingContext.ValidationNode.ChildNodes.Add(childBindingContext.ValidationNode);
                    }
                }
                else {
                    // should we even bother continuing?
                    if (!indexNamesIsFinite) {
                        break;
                    }
                }

                boundCollection.Add(ModelBinderUtil.CastOrDefault<TElement>(boundValue));
            }

            return boundCollection;
        }

        public virtual bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            ValueProviderResult vpResult = bindingContext.UnvalidatedValueProvider.GetValue(bindingContext.ModelName, skipValidation: !bindingContext.ValidateRequest);
            List<TElement> boundCollection = (vpResult != null)
                ? BindSimpleCollection(modelBindingExecutionContext, bindingContext, vpResult.RawValue, vpResult.Culture)
                : BindComplexCollection(modelBindingExecutionContext, bindingContext);

            bool retVal = CreateOrReplaceCollection(modelBindingExecutionContext, bindingContext, boundCollection);
            return retVal;
        }

        // Used when the ValueProvider contains the collection to be bound as a single element, e.g. the raw value
        // is [ "1", "2" ] and needs to be converted to an int[].
        internal static List<TElement> BindSimpleCollection(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext, object rawValue, CultureInfo culture) {
            if (rawValue == null) {
                return null; // nothing to do
            }

            List<TElement> boundCollection = new List<TElement>();

            object[] rawValueArray = ModelBinderUtil.RawValueToObjectArray(rawValue);
            foreach (object rawValueElement in rawValueArray) {
                ModelBindingContext innerBindingContext = new ModelBindingContext(bindingContext) {
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(TElement)),
                    ModelName = bindingContext.ModelName,
                    ValueProvider = new ValueProviderCollection() { // aggregate value provider
                        new ElementalValueProvider(bindingContext.ModelName, rawValueElement, culture), // our temporary provider goes at the front of the list
                        bindingContext.ValueProvider
                    }
                };

                object boundValue = null;
                IModelBinder childBinder = bindingContext.ModelBinderProviders.GetBinder(modelBindingExecutionContext, innerBindingContext);
                if (childBinder != null) {
                    if (childBinder.BindModel(modelBindingExecutionContext, innerBindingContext)) {
                        boundValue = innerBindingContext.Model;
                        bindingContext.ValidationNode.ChildNodes.Add(innerBindingContext.ValidationNode);
                    }
                }
                boundCollection.Add(ModelBinderUtil.CastOrDefault<TElement>(boundValue));
            }

            return boundCollection;
        }

        // Extensibility point that allows the bound collection to be manipulated or transformed before
        // being returned from the binder.
        protected virtual bool CreateOrReplaceCollection(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext, IList<TElement> newCollection) {
            CollectionModelBinderUtil.CreateOrReplaceCollection(bindingContext, newCollection, () => new List<TElement>());
            return true;
        }

    }
}
