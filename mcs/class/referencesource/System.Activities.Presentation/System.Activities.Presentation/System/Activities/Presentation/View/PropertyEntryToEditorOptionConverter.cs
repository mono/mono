//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Runtime;

    sealed class PropertyEntryToEditorOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PropertyEntry propertyEntry = value as PropertyEntry;
            if (null == propertyEntry)
            {
                PropertyValue propertyValue = value as PropertyValue;
                if (null != propertyValue)
                {
                    propertyEntry = propertyValue.ParentProperty;
                }
            }

            ModelPropertyEntry modelPropertyEntry = propertyEntry as ModelPropertyEntry;

            ModelProperty modelProperty = null;
            if (modelPropertyEntry != null)
            {
                modelProperty = modelPropertyEntry.FirstModelProperty;
            }

            if (modelProperty == null)
            {
                return Binding.DoNothing;
            }

            string optionName = parameter as string;
            if (optionName == null)
            {
                return Binding.DoNothing;
            }

            object optionValue;
            if (EditorOptionAttribute.TryGetOptionValue(modelProperty.Attributes, optionName, out optionValue))
            {
                return optionValue;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // It is simply impossible to convert back.
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
