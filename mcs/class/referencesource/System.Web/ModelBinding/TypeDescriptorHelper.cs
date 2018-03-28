namespace System.Web.ModelBinding {
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    internal static class TypeDescriptorHelper {

        public static ICustomTypeDescriptor Get(Type type) {
            return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
        }

    }
}
