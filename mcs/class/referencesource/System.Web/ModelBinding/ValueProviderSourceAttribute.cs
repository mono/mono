namespace System.Web.ModelBinding {
    using System;

    /// <summary>
    /// This attribute is the base class for all the value provider attributes that can be specified on
    /// method parameters to be able to get values from alternate sources like Form, QueryString, ViewState.
    /// </summary>
    public abstract class ValueProviderSourceAttribute : Attribute, IValueProviderSource, IModelNameProvider {

        public abstract IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext);

        public virtual string GetModelName() {
            return null;
        }
    }
}
