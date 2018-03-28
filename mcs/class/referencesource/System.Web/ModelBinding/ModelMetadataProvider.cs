namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;

    public abstract class ModelMetadataProvider {
        public abstract IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType);

        public abstract ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName);

        public abstract ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType);
    }
}
