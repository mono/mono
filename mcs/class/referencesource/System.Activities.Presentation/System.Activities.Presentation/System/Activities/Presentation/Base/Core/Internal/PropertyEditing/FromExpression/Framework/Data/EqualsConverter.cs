// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Activities.Presentation;

    internal class EqualsConverter : DependencyObject, IValueConverter
    {
        private object defaultValue = false, matchValue = true;

        // IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Object.Equals(value, parameter))
            {
                return this.matchValue;
            }
            else
            {
                return this.defaultValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }
    }
}
