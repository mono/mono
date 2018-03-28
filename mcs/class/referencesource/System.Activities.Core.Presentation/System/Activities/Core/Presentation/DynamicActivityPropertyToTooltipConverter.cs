//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Globalization;
    using System.Windows.Data;
    using Microsoft.Activities.Presentation;

    internal sealed class DynamicActivityPropertyToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DynamicActivityProperty property = value as DynamicActivityProperty;

            if (property == null)
            {
                return null;
            }

            if (property.Type == null)
            {
                return string.Format(CultureInfo.CurrentUICulture, SR.PropertyReferenceNotResolved, property.Name);
            }
            else
            {
                return TypeNameHelper.GetDisplayName(property.Type, false);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
