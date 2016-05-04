namespace System.Web.ModelBinding {
    using System;
    using System.Collections.ObjectModel;

    [Serializable]
    public class ModelErrorCollection : Collection<ModelError> {

        public void Add(Exception exception) {
            Add(new ModelError(exception));
        }

        public void Add(string errorMessage) {
            Add(new ModelError(errorMessage));
        }
    }
}
