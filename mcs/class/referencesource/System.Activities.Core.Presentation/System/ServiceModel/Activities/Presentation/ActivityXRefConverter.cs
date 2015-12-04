//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Activities;
    using System.Activities.Core.Presentation;

    sealed class ActivityXRefConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(targetType == typeof(string) || targetType == typeof(object)))
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }
            if (null == value)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("value"));
            }
            ModelItem activity = value as ModelItem;
            string displayName = value as string;
            
            string formatString = (parameter as string) ?? "{0}";

            if (null != activity && typeof(Activity).IsAssignableFrom(activity.ItemType))
            {
                displayName = ((string)activity.Properties["DisplayName"].ComputedValue);
            }

            if (null == displayName)
            {
                displayName = "<null>";
            }
            else if (displayName.Length == 0)
            {
                displayName = "...";
            }

            return string.Format(CultureInfo.CurrentUICulture, formatString, displayName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
