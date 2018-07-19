#if !SILVERLIGHT
namespace System.ComponentModel.DataAnnotations {
    using System;
    using System.ComponentModel.DataAnnotations.Resources;

    /// <summary>
    /// Used for associating a metadata class with the entity class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MetadataTypeAttribute : Attribute {

        private Type _metadataClassType;

        public Type MetadataClassType {
            get {
                if (_metadataClassType == null) {
                    throw new InvalidOperationException(DataAnnotationsResources.MetadataTypeAttribute_TypeCannotBeNull);
                }

                return _metadataClassType;
            }
        }

        public MetadataTypeAttribute(Type metadataClassType) {
            _metadataClassType = metadataClassType;
        }

    }
}
#endif
