//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows.Data;
    using System.Globalization;

    internal sealed class NotConverter : IValueConverter
    {
        static IValueConverter baseConverter =
            new System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data.NotConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return NotConverter.baseConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return NotConverter.baseConverter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
