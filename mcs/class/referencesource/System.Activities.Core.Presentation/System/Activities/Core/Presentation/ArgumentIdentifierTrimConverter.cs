//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Converters;
    using System.Windows.Data;
    
    internal sealed class ArgumentIdentifierTrimConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return value;
            }
            
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // delegate to VBIdentifierTrimConverter.ConvertBack, because VBIdentifierTrimConverter is internal and cannot be directly referenced from XAML.
            VBIdentifierTrimConverter converter = new VBIdentifierTrimConverter();
            return converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
