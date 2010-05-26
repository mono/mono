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
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc.Resources;

    public class ClientDataTypeModelValidatorProvider : ModelValidatorProvider {

        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>(new Type[] {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        });

        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context) {
            if (metadata == null) {
                throw new ArgumentNullException("metadata");
            }
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            return GetValidatorsImpl(metadata, context);
        }

        private static IEnumerable<ModelValidator> GetValidatorsImpl(ModelMetadata metadata, ControllerContext context) {
            Type type = metadata.ModelType;
            if (IsNumericType(type)) {
                yield return new NumericModelValidator(metadata, context);
            }
        }

        private static bool IsNumericType(Type type) {
            Type underlyingType = Nullable.GetUnderlyingType(type); // strip off the Nullable<>
            return _numericTypes.Contains(underlyingType ?? type);
        }

        internal sealed class NumericModelValidator : ModelValidator {
            public NumericModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
                : base(metadata, controllerContext) {
            }

            public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
                ModelClientValidationRule rule = new ModelClientValidationRule() {
                    ValidationType = "number",
                    ErrorMessage = MakeErrorString(Metadata.GetDisplayName())
                };

                return new ModelClientValidationRule[] { rule };
            }

            private static string MakeErrorString(string displayName) {
                // use CurrentCulture since this message is intended for the site visitor
                return String.Format(CultureInfo.CurrentCulture, MvcResources.ClientDataTypeModelValidatorProvider_FieldMustBeNumeric, displayName);
            }

            public override IEnumerable<ModelValidationResult> Validate(object container) {
                // this is not a server-side validator
                return Enumerable.Empty<ModelValidationResult>();
            }
        }

    }
}
