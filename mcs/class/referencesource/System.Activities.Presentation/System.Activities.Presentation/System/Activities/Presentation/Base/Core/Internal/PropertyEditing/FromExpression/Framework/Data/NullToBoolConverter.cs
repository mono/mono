// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    // <summary>
    // Converts non-null to true, and null to false.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class NullToBoolConverter : IValueConverter
    {
        // IValueConverter Implementation

        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(false, "NullToBoolConverter can only be used for forward conversion.");
            return null;
        }

        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            return o != null;
        }

    }
}
