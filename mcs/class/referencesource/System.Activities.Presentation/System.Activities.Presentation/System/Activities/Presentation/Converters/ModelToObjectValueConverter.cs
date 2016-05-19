//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Windows.Data;
    using System.Globalization;
    using System.Activities.Presentation.Model;

    // This class is used to convert a ModelItem to the innerobject, when binding to whole objectin xaml.
    // reverse conversion is not required beacuse ModelProperty.SetValue accepts object as is.
    public class ModelToObjectValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedValue = null;
            if (value != null)
            {
                if (value is ModelItem)
                {
                    convertedValue = ((ModelItem)value).GetCurrentValue();
                }
                else
                {
                    convertedValue = value;
                }
            }
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
