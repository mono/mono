namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    public abstract class AssociatedValidatorProvider : ModelValidatorProvider {
        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type) {
            return TypeDescriptorHelper.Get(type);
        }

        public override sealed IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ModelBindingExecutionContext context) {
            if (metadata == null) {
                throw new ArgumentNullException("metadata");
            }
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            if (metadata.ContainerType != null && !String.IsNullOrEmpty(metadata.PropertyName)) {
                return GetValidatorsForProperty(metadata, context);
            }

            return GetValidatorsForType(metadata, context);
        }

        protected abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ModelBindingExecutionContext context, IEnumerable<Attribute> attributes);

        private IEnumerable<ModelValidator> GetValidatorsForProperty(ModelMetadata metadata, ModelBindingExecutionContext context) {
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(metadata.ContainerType);
            PropertyDescriptor property = typeDescriptor.GetProperties().Find(metadata.PropertyName, true);
            if (property == null) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.Common_PropertyNotFound),
                        metadata.ContainerType.FullName, metadata.PropertyName),
                    "metadata");
            }

            return GetValidators(metadata, context, property.Attributes.OfType<Attribute>());
        }

        private IEnumerable<ModelValidator> GetValidatorsForType(ModelMetadata metadata, ModelBindingExecutionContext context) {
            return GetValidators(metadata, context, GetTypeDescriptor(metadata.ModelType).GetAttributes().Cast<Attribute>());
        }
    }
}
