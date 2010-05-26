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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public abstract class ModelValidator {
        protected ModelValidator(ModelMetadata metadata, ControllerContext controllerContext) {
            if (metadata == null) {
                throw new ArgumentNullException("metadata");
            }
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }

            Metadata = metadata;
            ControllerContext = controllerContext;
        }

        protected internal ControllerContext ControllerContext { get; private set; }

        public virtual bool IsRequired {
            get {
                return false;
            }
        }

        protected internal ModelMetadata Metadata { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return Enumerable.Empty<ModelClientValidationRule>();
        }

        public static ModelValidator GetModelValidator(ModelMetadata metadata, ControllerContext context) {
            return new CompositeModelValidator(metadata, context);
        }

        public abstract IEnumerable<ModelValidationResult> Validate(object container);

        private class CompositeModelValidator : ModelValidator {
            public CompositeModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
                : base(metadata, controllerContext) {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container) {
                bool propertiesValid = true;

                foreach (ModelMetadata propertyMetadata in Metadata.Properties) {
                    foreach (ModelValidator propertyValidator in propertyMetadata.GetValidators(ControllerContext)) {
                        foreach (ModelValidationResult propertyResult in propertyValidator.Validate(Metadata.Model)) {
                            propertiesValid = false;
                            yield return new ModelValidationResult {
                                MemberName = DefaultModelBinder.CreateSubPropertyName(propertyMetadata.PropertyName, propertyResult.MemberName),
                                Message = propertyResult.Message
                            };
                        }
                    }
                }

                if (propertiesValid) {
                    foreach (ModelValidator typeValidator in Metadata.GetValidators(ControllerContext)) {
                        foreach (ModelValidationResult typeResult in typeValidator.Validate(container)) {
                            yield return typeResult;
                        }
                    }
                }
            }
        }
    }
}
