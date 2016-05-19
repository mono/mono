//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Model;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;

    sealed class ModelPropertyPathExpanderConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value != null)
            {
                ModelPropertyEntry modelPropertyEntry = value as ModelPropertyEntry;
                if (modelPropertyEntry != null)
                {
                    StringBuilder propertyPath = new StringBuilder(modelPropertyEntry.PropertyName);
                    propertyPath.Insert(0, '.');

                    ModelProperty property = modelPropertyEntry.FirstModelProperty;
                    if (property != null)
                    {
                        ModelItem convertedValue = property.Parent;
                        while (convertedValue != null && !typeof(Activity).IsAssignableFrom(convertedValue.ItemType))
                        {
                            if (null != convertedValue.Source)
                            {
                                propertyPath.Insert(0, convertedValue.Source.Name);
                                propertyPath.Insert(0, '.');
                            }
                            convertedValue = convertedValue.Parent;
                        }
                    }
                    propertyPath.Remove(0, 1);
                    result = propertyPath.ToString();
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError( new NotSupportedException(SR.NonSupportedModelPropertyPathExpanderConverterConvertBack));
        }
    }
}
