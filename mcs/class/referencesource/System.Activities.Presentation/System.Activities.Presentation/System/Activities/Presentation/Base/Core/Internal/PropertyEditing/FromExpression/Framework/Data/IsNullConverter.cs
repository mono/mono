// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // (nullable-to-bool) Returns true if the value is null, false otherwise.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class IsNullConverter : IValueConverter
    {
        // IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Fail("IsNullConverter can only be used for BindType=OneWay");
            return null;
        }
    }
}
