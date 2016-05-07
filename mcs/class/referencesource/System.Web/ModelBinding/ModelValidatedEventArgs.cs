namespace System.Web.ModelBinding {
    using System;

    public sealed class ModelValidatedEventArgs : EventArgs {

        public ModelValidatedEventArgs(ModelBindingExecutionContext modelBindingExecutionContext, ModelValidationNode parentNode) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            ModelBindingExecutionContext = modelBindingExecutionContext;
            ParentNode = parentNode;
        }

        public ModelBindingExecutionContext ModelBindingExecutionContext {
            get;
            private set;
        }

        public ModelValidationNode ParentNode {
            get;
            private set;
        }

    }
}
