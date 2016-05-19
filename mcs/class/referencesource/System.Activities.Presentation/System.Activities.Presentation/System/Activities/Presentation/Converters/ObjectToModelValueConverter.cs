//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;

    //This value converter is used in propertygrid scenarios to convert an object into a ModelItem.
    //It converts from ModelPropertyEntry to ModelItem that owns the property
    //The first binding is a two way binding with the PropertyValue object 
    //The second binding is a one way binding with the ModelPropertyEntry.
    public class ObjectToModelValueConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ModelItem convertedValue = null;
            if (values[1] != null)
            {
                ModelPropertyEntry modelPropertyEntry = values[1] as ModelPropertyEntry;
                if (modelPropertyEntry != null)
                {
                    ModelProperty property = modelPropertyEntry.FirstModelProperty;
                    if (property != null)
                    {
                        convertedValue = property.Value;
                    }
                }
            }
            return convertedValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] returnValue = new object[] { null };
            if (value != null)
            {
                returnValue[0] = ((ModelItem)value).GetCurrentValue();
            }
            return returnValue;
        }
    }
}
