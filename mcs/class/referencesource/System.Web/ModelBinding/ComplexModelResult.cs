namespace System.Web.ModelBinding {
    using System;

    public sealed class ComplexModelResult {

        public ComplexModelResult(object model, ModelValidationNode validationNode) {
            if (validationNode == null) {
                throw new ArgumentNullException("validationNode");
            }

            Model = model;
            ValidationNode = validationNode;
        }

        public object Model {
            get;
            private set;
        }

        public ModelValidationNode ValidationNode {
            get;
            private set;
        }

    }
}
