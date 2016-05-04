// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------

//Cider comment:
//  - This file also had a VisibilityOrConverter but we are not using it so I removed it

//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Windows.Data;
    using System.Windows;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class VisibilityAndConverter : IMultiValueConverter
    {
        // IMultiValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (!(value is Visibility) || ((Visibility)value) != Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException(ExceptionStringTable.MethodOrOperationIsNotImplemented));
        }
    }
}
