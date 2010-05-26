/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Web.Mvc.Resources;

    public delegate ModelValidator DataAnnotationsModelValidationFactory(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute);

    public class DataAnnotationsModelValidatorProvider : AssociatedValidatorProvider {
        private static bool _addImplicitRequiredAttributeForValueTypes = true;
        private static ReaderWriterLockSlim _adaptersLock = new ReaderWriterLockSlim();

        internal static DataAnnotationsModelValidationFactory DefaultAttributeFactory = DataAnnotationsModelValidator.Create;
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

        public static bool AddImplicitRequiredAttributeForValueTypes {
            get {
                return _addImplicitRequiredAttributeForValueTypes;
            }
            set {
                _addImplicitRequiredAttributeForValueTypes = value;
            }
        }

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes) {
            _adaptersLock.EnterReadLock();

            try {
                List<ModelValidator> results = new List<ModelValidator>();

                if (AddImplicitRequiredAttributeForValueTypes &&
                        metadata.IsRequired &&
                        !attributes.Any(a => a is RequiredAttribute)) {
                    attributes = attributes.Concat(new[] { new RequiredAttribute() });
                }

                foreach (ValidationAttribute attribute in attributes.OfType<ValidationAttribute>()) {
                    DataAnnotationsModelValidationFactory factory;
                    if (!AttributeFactories.TryGetValue(attribute.GetType(), out factory)) {
                        factory = DefaultAttributeFactory;
                    }
                    results.Add(factory(metadata, context, attribute));
                }

                return results;
            }
            finally {
                _adaptersLock.ExitReadLock();
            }
        }

        public static void RegisterAdapter(Type attributeType, Type adapterType) {
            ValidateAttributeType(attributeType);
            ValidateAdapterType(adapterType);
            ConstructorInfo constructor = GetAdapterConstructor(attributeType, adapterType);

            _adaptersLock.EnterWriteLock();

            try {
                AttributeFactories[attributeType] = (metadata, context, attribute) => (ModelValidator)constructor.Invoke(new object[] { metadata, context, attribute });
            }
            finally {
                _adaptersLock.ExitWriteLock();
            }
        }

        public static void RegisterAdapterFactory(Type attributeType, DataAnnotationsModelValidationFactory factory) {
            ValidateAttributeType(attributeType);
            ValidateFactory(factory);

            _adaptersLock.EnterWriteLock();

            try {
                AttributeFactories[attributeType] = factory;
            }
            finally {
                _adaptersLock.ExitWriteLock();
            }
        }

        public static void RegisterDefaultAdapter(Type adapterType) {
            ValidateAdapterType(adapterType);
            ConstructorInfo constructor = GetAdapterConstructor(typeof(ValidationAttribute), adapterType);

            DefaultAttributeFactory = (metadata, context, attribute) => (ModelValidator)constructor.Invoke(new object[] { metadata, context, attribute });
        }

        public static void RegisterDefaultAdapterFactory(DataAnnotationsModelValidationFactory factory) {
            ValidateFactory(factory);

            DefaultAttributeFactory = factory;
        }

        // Helpers

        private static ConstructorInfo GetAdapterConstructor(Type attributeType, Type adapterType) {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(ModelMetadata), typeof(ControllerContext), attributeType });
            if (constructor == null) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DataAnnotationsModelValidatorProvider_ConstructorRequirements,
                        adapterType.FullName,
                        typeof(ModelMetadata).FullName,
                        typeof(ControllerContext).FullName,
                        attributeType.FullName
                    ),
                    "adapterType"
                );
            }

            return constructor;
        }

        private static void ValidateAdapterType(Type adapterType) {
            if (adapterType == null) {
                throw new ArgumentNullException("adapterType");
            }
            if (!typeof(ModelValidator).IsAssignableFrom(adapterType)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.Common_TypeMustDriveFromType,
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
                        MvcResources.Common_TypeMustDriveFromType,
                        attributeType.FullName,
                        typeof(ValidationAttribute).FullName
                    ),
                    "attributeType");
            }
        }

        private static void ValidateFactory(DataAnnotationsModelValidationFactory factory) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }
        }
    }
}
