namespace System.Web.ModelBinding {
    using System;

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class UserProfileAttribute : Attribute, IValueProviderSource {

        public IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            return new UserProfileValueProvider(modelBindingExecutionContext);
        }

    }
}
