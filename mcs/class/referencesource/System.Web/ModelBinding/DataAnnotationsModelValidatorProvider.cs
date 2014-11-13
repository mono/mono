namespace System.Web.ModelBinding {
    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

    // A factory for validators based on ValidationAttribute
    public delegate ModelValidator DataAnnotationsModelValidationFactory(ModelMetadata metadata, ModelBindingExecutionContext context, ValidationAttribute attribute);

    // A factory for validators based on IValidatableObject
    public delegate ModelValidator DataAnnotationsValidatableObjectAdapterFactory(ModelMetadata metadata, ModelBindingExecutionContext context);

    /// <summary>
    /// An implementation of <see cref="ModelValidatorProvider"/> which providers validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>. To support
    /// client side validation, you can either register adapters through the static methods
    /// on this class, or by having your validation attributes implement
    /// <see cref="IClientValidatable"/>. The logic to support IClientValidatable
    /// is implemented in <see cref="DataAnnotationsModelValidator"/>.
    /// </summary>
    public class DataAnnotationsModelValidatorProvider : AssociatedValidatorProvider {
        private static bool _addImplicitRequiredAttributeForValueTypes = true;
        private static ReaderWriterLockSlim _adaptersLock = new ReaderWriterLockSlim();

        // Factories for validation attributes

        internal static DataAnnotationsModelValidationFactory DefaultAttributeFactory =
            (metadata, context, attribute) => new DataAnnotationsModelValidator(metadata, context, attribute);

        internal static Dictionary<Type, DataAnnotationsModelValidationFactory> AttributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>() {
            {
                typeof(RangeAttribute),
                (metadata, context, attribute) => new RangeAttributeAdapter(metadata, context, (RangeAttribute)attribute)
            },
            {
                typeof(RegularExpressionAttribute),
                (metadata, context, attribute) => new RegularExpressionAttributeAdapter(metadata, context, (RegularExpressionAttribute)attribute)
            },
            {
                typeof(RequiredAttribute),
                (metadata, context, attribute) => new RequiredAttributeAdapter(metadata, context, (RequiredAttribute)attribute)
            },
            {
                typeof(StringLengthAttribute),
                (metadata, context, attribute) => new StringLengthAttributeAdapter(metadata, context, (StringLengthAttribute)attribute)
            },
        };

        // Factories for IValidatableObject models

        internal static DataAnnotationsValidatableObjectAdapterFactory DefaultValidatableFactory =
            (metadata, context) => new ValidatableObjectAdapter(metadata, context);

        internal static Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory> ValidatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();

        public static bool AddImplicitRequiredAttributeForValueTypes {
            get {
                return _addImplicitRequiredAttributeForValueTypes;
            }
            set {
                _addImplicitRequiredAttributeForValueTypes = value;
            }
        }

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ModelBindingExecutionContext context, IEnumerable<Attribute> attributes) {
            _adaptersLock.EnterReadLock();

            try {
                List<ModelValidator> results = new List<ModelValidator>();

                // Add an implied [Required] attribute for any non-nullable value type,
                // unless they've configured us not to do that.
                if (AddImplicitRequiredAttributeForValueTypes &&
                        metadata.IsRequired &&
                        !attributes.Any(a => a is RequiredAttribute)) {
                    attributes = attributes.Concat(new[] { new RequiredAttribute() });
                }

                // Produce a validator for each validation attribute we find
                foreach (ValidationAttribute attribute in attributes.OfType<ValidationAttribute>()) {
                    DataAnnotationsModelValidationFactory factory;
                    if (!AttributeFactories.TryGetValue(attribute.GetType(), out factory)) {
                        factory = DefaultAttributeFactory;
                    }
                    results.Add(factory(metadata, context, attribute));
                }

                // Produce a validator if the type supports IValidatableObject
                if (typeof(IValidatableObject).IsAssignableFrom(metadata.ModelType)) {
                    DataAnnotationsValidatableObjectAdapterFactory factory;
                    if (!ValidatableFactories.TryGetValue(metadata.ModelType, out factory)) {
                        factory = DefaultValidatableFactory;
                    }
                    results.Add(factory(metadata, context));
                }

                return results;
            }
            finally {
                _adaptersLock.ExitReadLock();
            }
        }

        #region Validation attribute adapter registration

        public static void RegisterAdapter(Type attributeType, Type adapterType) {
            ValidateAttributeType(attributeType);
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(attributeType, adapterType);

            _adaptersLock.EnterWriteLock();

            try {
                AttributeFactories[attributeType] = (metadata, context, attribute) => (ModelValidator)constructor.Invoke(new object[] { metadata, context, attribute });
            }
            finally {
                _adaptersLock.ExitWriteLock();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "AttributeFactories", Justification = "The types that go into this dictionary are specifically ValidationAttribute derived types.")]
        public static void RegisterAdapterFactory(Type attributeType, DataAnnotationsModelValidationFactory factory)
        {
            ValidateAttributeType(attributeType);
            ValidateAttributeFactory(factory);

            _adaptersLock.EnterWriteLock();

            try {
                AttributeFactories[attributeType] = factory;
            }
            finally {
                _adaptersLock.ExitWriteLock();
            }
        }

        public static void RegisterDefaultAdapter(Type adapterType) {
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(typeof(ValidationAttribute), adapterType);

            DefaultAttributeFactory = (metadata, context, attribute) => (ModelValidator)constructor.Invoke(new object[] { metadata, context, attribute });
        }

        public static void RegisterDefaultAdapterFactory(DataAnnotationsModelValidationFactory factory) {
            ValidateAttributeFactory(factory);

            DefaultAttributeFactory = factory;
        }

        // Helpers 

        private static ConstructorInfo GetAttributeAdapterConstructor(Type attributeType, Type adapterType) {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(ModelMetadata), typeof(ModelBindingExecutionContext), attributeType });
            if (constructor == null) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.DataAnnotationsModelValidatorProvider_ConstructorRequirements),
                        adapterType.FullName,
                        typeof(ModelMetadata).FullName,
                        typeof(ModelBindingExecutionContext).FullName,
                        attributeType.FullName
                    ),
                    "adapterType"
                );
            }

            return constructor;
        }

        private static void ValidateAttributeAdapterType(Type adapterType) {
            if (adapterType == null) {
                throw new ArgumentNullException("adapterType");
            }
            if (!typeof(ModelValidator).IsAssignableFrom(adapterType)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.Common_TypeMustDriveFromType),
                        adapterType.FullName,
                        typeof(ModelValidator).FullName
                    ),
                    "adapterType"
                );
            }
        }

        private static void ValidateAttributeType(Type attributeType) {
            if (attributeType == null) {
                throw new ArgumentNullException("attributeType");
            }
            if (!typeof(ValidationAttribute).IsAssignableFrom(attributeType)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.Common_TypeMustDriveFromType),
                        attributeType.FullName,
                        typeof(ValidationAttribute).FullName
                    ),
                    "attributeType");
            }
        }

        private static void ValidateAttributeFactory(DataAnnotationsModelValidationFactory factory) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }
        }

        #endregion

        #region IValidatableObject adapter registration

        /// <summary>
        /// Registers an adapter type for the given <see cref="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="ModelBindingExecutionContext"/>.
        /// </summary>
        public static void RegisterValidatableObjectAdapter(Type modelType, Type adapterType) {
            ValidateValidatableModelType(modelType);
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

            _adaptersLock.EnterWriteLock();

            try {
                ValidatableFactories[modelType] = (metadata, context) => (ModelValidator)constructor.Invoke(new object[] { metadata, context });
            }
            finally {
                _adaptersLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers an adapter factory for the given <see cref="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "ValidatableFactories", Justification = "The types that go into this dictionary are specifically those which implement IValidatableObject.")]
        public static void RegisterValidatableObjectAdapterFactory(Type modelType, DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            ValidateValidatableModelType(modelType);
            ValidateValidatableFactory(factory);

            _adaptersLock.EnterWriteLock();

            try {
                ValidatableFactories[modelType] = factory;
            }
            finally {
                _adaptersLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers the default adapter type for objects which implement
        /// <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="ModelBindingExecutionContext"/>.
        /// </summary>
        public static void RegisterDefaultValidatableObjectAdapter(Type adapterType) {
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

            DefaultValidatableFactory = (metadata, context) => (ModelValidator)constructor.Invoke(new object[] { metadata, context });
        }

        /// <summary>
        /// Registers the default adapter factory for objects which implement
        /// <see cref="IValidatableObject"/>.
        /// </summary>
        public static void RegisterDefaultValidatableObjectAdapterFactory(DataAnnotationsValidatableObjectAdapterFactory factory) {
            ValidateValidatableFactory(factory);

            DefaultValidatableFactory = factory;
        }

        // Helpers 

        private static ConstructorInfo GetValidatableAdapterConstructor(Type adapterType) {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(ModelMetadata), typeof(ModelBindingExecutionContext) });
            if (constructor == null) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.DataAnnotationsModelValidatorProvider_ValidatableConstructorRequirements),
                        adapterType.FullName,
                        typeof(ModelMetadata).FullName,
                        typeof(ModelBindingExecutionContext).FullName
                    ),
                    "adapterType"
                );
            }

            return constructor;
        }

        private static void ValidateValidatableAdapterType(Type adapterType) {
            if (adapterType == null) {
                throw new ArgumentNullException("adapterType");
            }
            if (!typeof(ModelValidator).IsAssignableFrom(adapterType)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.Common_TypeMustDriveFromType),
                        adapterType.FullName,
                        typeof(ModelValidator).FullName
                    ),
                    "adapterType");
            }
        }

        private static void ValidateValidatableModelType(Type modelType) {
            if (modelType == null) {
                throw new ArgumentNullException("modelType");
            }
            if (!typeof(IValidatableObject).IsAssignableFrom(modelType)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.Common_TypeMustDriveFromType),
                        modelType.FullName,
                        typeof(IValidatableObject).FullName
                    ),
                    "modelType"
                );
            }
        }

        private static void ValidateValidatableFactory(DataAnnotationsValidatableObjectAdapterFactory factory) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }
        }

        #endregion
    }
}
