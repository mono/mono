﻿namespace System.Web.ModelBinding {
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Reflection;

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
                        SR.GetString(SR.DataAnnotationsModelMetadataProvider_UnknownProperty),
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }
            if (displayColumnProperty.GetGetMethod() == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.DataAnnotationsModelMetadataProvider_UnreadableProperty),
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }
        }
    }
}
