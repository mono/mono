namespace System.Web.ModelBinding {
    using System;
    using System.ComponentModel;

    public sealed class ModelValidatingEventArgs : CancelEventArgs {

        public ModelValidatingEventArgs(ModelBindingExecutionContext modelBindingExecutionContext, ModelValidationNode parentNode) {
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
