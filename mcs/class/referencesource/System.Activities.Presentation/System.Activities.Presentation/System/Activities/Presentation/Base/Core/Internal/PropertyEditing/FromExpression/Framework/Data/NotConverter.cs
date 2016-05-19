// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Transform bool value using logical not.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class NotConverter : IValueConverter
    {
        // IValueConverter Members

        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)o;
        }

        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            return !ValueConverterUtilities.AssureBool(o, false);
        }

    }
}
