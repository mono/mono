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
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    public class DataAnnotationsModelMetadata : ModelMetadata {
        private DisplayColumnAttribute _displayColumnAttribute;

        public DataAnnotationsModelMetadata(DataAnnotationsModelMetadataProvider provider, Type containerType,
                                            Func<object> modelAccessor, Type modelType, string propertyName,
                                            DisplayColumnAttribute displayColumnAttribute)
            : base(provider, containerType, modelAccessor, modelType, propertyName) {
            _displayColumnAttribute = displayColumnAttribute;
        }

        protected override string GetSimpleDisplayText() {
            if (Model != null) {
                if (_displayColumnAttribute != null && !String.IsNullOrEmpty(_displayColumnAttribute.DisplayColumn)) {
                    PropertyInfo displayColumnProperty = ModelType.GetProperty(_displayColumnAttribute.DisplayColumn, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                    ValidateDisplayColumnAttribute(_displayColumnAttribute, displayColumnProperty, ModelType);

                    object simpleDisplayTextValue = displayColumnProperty.GetValue(Model, new object[0]);
                    if (simpleDisplayTextValue != null) {
                        return simpleDisplayTextValue.ToString();
                    }
                }
            }

            return base.GetSimpleDisplayText();
        }

        private static void ValidateDisplayColumnAttribute(DisplayColumnAttribute displayColumnAttribute, PropertyInfo displayColumnProperty, Type modelType) {
            if (displayColumnProperty == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DataAnnotationsModelMetadataProvider_UnknownProperty,
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }
            if (displayColumnProperty.GetGetMethod() == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DataAnnotationsModelMetadataProvider_UnreadableProperty,
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }
        }
    }
}
