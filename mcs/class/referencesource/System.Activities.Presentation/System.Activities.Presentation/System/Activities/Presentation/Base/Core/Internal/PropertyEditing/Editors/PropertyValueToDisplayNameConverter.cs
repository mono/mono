//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;

    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation;

    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.Properties;

    // <summary>
    // ValueConverter that takes an instance of PropertyValue and returns a display name for
    // it.  The returned name consists of the value Type name as well as its x:Name property
    // if it is defined.
    // </summary>
    internal class PropertyValueToDisplayNameConverter : IValueConverter 
    {

        private static PropertyValueToDisplayNameConverter _instance;

        // <summary>
        // Static instance accessor for all non-XAML related conversion needs
        // </summary>
        public static PropertyValueToDisplayNameConverter Instance 
        {
            get {
                if (_instance == null)
                {
                    _instance = new PropertyValueToDisplayNameConverter();
                }

                return _instance;
            }
        }

        // Converts an instance of PropertyValue to its appropriate display name
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {

            if (typeof(string).IsAssignableFrom(targetType)) 
            {

                PropertyValue propertyValue = value as PropertyValue;
                if (propertyValue != null) 
                {

                    ModelPropertyEntryBase propertyEntry = propertyValue.ParentProperty as ModelPropertyEntryBase;

                    // Figure out the value type name
                    string valueTypeName = string.Empty;
                    if (propertyEntry != null)
                    {
                        valueTypeName = propertyEntry.CommonValueType == null ? string.Empty : propertyEntry.CommonValueType.Name;
                    }
                    else 
                    {
                        Debug.Fail("PropertyValueToDisplayNameConverter is being used for something other than ModelPropertyValues.  Re-evaluate the correctness of its logic.");

                        // Fallback mechanism
                        object rawPropertyValue = propertyValue.Value;
                        if (rawPropertyValue != null)
                        {
                            valueTypeName = rawPropertyValue.GetType().Name;
                        }
                    }

                    // See if there is a regular name
                    string propertyName = ModelUtilities.GetPropertyName(propertyValue);

                    if (string.IsNullOrEmpty(propertyName)) 
                    {
                        // Type only
                        return string.Format(
                            culture,
                            Resources.PropertyEditing_CollectionItemDisplayFormatType,
                            valueTypeName);
                    }
                    else 
                    {
                        // Type and name
                        return string.Format(
                            culture,
                            Resources.PropertyEditing_CollectionItemDisplayFormatTypeAndName,
                            valueTypeName,
                            propertyName);
                    }
                }
            }

            return string.Empty;
        }

        // This class is only a one-way converter
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
