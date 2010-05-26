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
    using System.ComponentModel;
    using System.Linq;

    public class DataErrorInfoModelValidatorProvider : ModelValidatorProvider {

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
            // If the metadata describes a model that implements IDataErrorInfo, we should call its
            // Error property at the appropriate time.
            if (TypeImplementsIDataErrorInfo(metadata.ModelType)) {
                yield return new DataErrorInfoClassModelValidator(metadata, context);
            }

            // If the metadata describes a property of a container that implements IDataErrorInfo,
            // we should call its Item indexer at the appropriate time.
            if (TypeImplementsIDataErrorInfo(metadata.ContainerType)) {
                yield return new DataErrorInfoPropertyModelValidator(metadata, context);
            }
        }

        private static bool TypeImplementsIDataErrorInfo(Type type) {
            return typeof(IDataErrorInfo).IsAssignableFrom(type);
        }

        internal sealed class DataErrorInfoClassModelValidator : ModelValidator {
            public DataErrorInfoClassModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
                : base(metadata, controllerContext) {
            }
            public override IEnumerable<ModelValidationResult> Validate(object container) {
                IDataErrorInfo castModel = Metadata.Model as IDataErrorInfo;
                if (castModel != null) {
                    string errorMessage = castModel.Error;
                    if (!String.IsNullOrEmpty(errorMessage)) {
                        return new ModelValidationResult[] {
                            new ModelValidationResult() { Message = errorMessage }
                        };
                    }
                }
                return Enumerable.Empty<ModelValidationResult>();
            }
        }

        internal sealed class DataErrorInfoPropertyModelValidator : ModelValidator {
            public DataErrorInfoPropertyModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
                : base(metadata, controllerContext) {
            }
            public override IEnumerable<ModelValidationResult> Validate(object container) {
                IDataErrorInfo castContainer = container as IDataErrorInfo;
                if (castContainer != null && !String.Equals(Metadata.PropertyName, "error", StringComparison.OrdinalIgnoreCase)) {
                    string errorMessage = castContainer[Metadata.PropertyName];
                    if (!String.IsNullOrEmpty(errorMessage)) {
                        return new ModelValidationResult[] {
                            new ModelValidationResult() { Message = errorMessage }
                        };
                    }
                }
                return Enumerable.Empty<ModelValidationResult>();
            }
        }

    }
}
