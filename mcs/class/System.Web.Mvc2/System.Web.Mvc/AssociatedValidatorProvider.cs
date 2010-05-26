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
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc.Resources;

    public abstract class AssociatedValidatorProvider : ModelValidatorProvider {
        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type) {
            return TypeDescriptorHelper.Get(type);
        }

        public override sealed IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context) {
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

        protected abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes);

        private IEnumerable<ModelValidator> GetValidatorsForProperty(ModelMetadata metadata, ControllerContext context) {
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(metadata.ContainerType);
            PropertyDescriptor property = typeDescriptor.GetProperties().Find(metadata.PropertyName, true);
            if (property == null) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.Common_PropertyNotFound,
                        metadata.ContainerType.FullName, metadata.PropertyName),
                    "metadata");
            }

            return GetValidators(metadata, context, property.Attributes.OfType<Attribute>());
        }

        private IEnumerable<ModelValidator> GetValidatorsForType(ModelMetadata metadata, ControllerContext context) {
            return GetValidators(metadata, context, GetTypeDescriptor(metadata.ModelType).GetAttributes().Cast<Attribute>());
        }
    }
}
