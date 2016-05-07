//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows;

    sealed class CanExpandCollapseAllConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return false;
            }

            ActivityDesignerOptionsAttribute attr = WorkflowViewService.GetAttribute<ActivityDesignerOptionsAttribute>(value.GetType());
            return attr == null || !attr.AlwaysCollapseChildren;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
