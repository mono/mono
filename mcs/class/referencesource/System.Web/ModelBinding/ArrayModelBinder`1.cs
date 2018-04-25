namespace System.Web.ModelBinding {
    using System.Collections.Generic;
    using System.Linq;

    public class ArrayModelBinder<TElement> : CollectionModelBinder<TElement> {

        protected override bool CreateOrReplaceCollection(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext, IList<TElement> newCollection) {
            bindingContext.Model = newCollection.ToArray();
            return true;
        }

    }
}
