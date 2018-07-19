namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    // Describes a complex model, but uses a collection rather than individual properties as the data store.
    public class ComplexModel {

        public ComplexModel(ModelMetadata modelMetadata, IEnumerable<ModelMetadata> propertyMetadata) {
            if (modelMetadata == null) {
                throw new ArgumentNullException("modelMetadata");
            }
            if (propertyMetadata == null) {
                throw new ArgumentNullException("propertyMetadata");
            }

            ModelMetadata = modelMetadata;
            PropertyMetadata = new ReadOnlyCollection<ModelMetadata>(propertyMetadata.ToList());
            Results = new Dictionary<ModelMetadata, ComplexModelResult>();
        }

        public ModelMetadata ModelMetadata {
            get;
            private set;
        }

        public ReadOnlyCollection<ModelMetadata> PropertyMetadata {
            get;
            private set;
        }

        // Contains entries corresponding to each property against which binding was
        // attempted. If binding failed, the entry's value will be null. If binding
        // was never attempted, this dictionary will not contain a corresponding
        // entry.
        public IDictionary<ModelMetadata, ComplexModelResult> Results {
            get;
            private set;
        }

    }
}
