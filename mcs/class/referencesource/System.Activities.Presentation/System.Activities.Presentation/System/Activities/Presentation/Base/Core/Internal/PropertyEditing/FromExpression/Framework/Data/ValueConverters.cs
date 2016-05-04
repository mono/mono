// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------

//Cider comment:
//  - This file had many more converters but we are not using them at present
//  - And so I removed them. These are the classes I removed
//    OrientationToCheckStateConverter
//    DoubleToStringConverter
//    IntToStringConverter
//    IntToBoolConverter
//    BoolToCheckStateConverter
//    BoolToStringConverter
//    UIElementToStringConverter
//    NullToEmptyStringConverter
//    InverseVisibilityConverter
//    GridLengthConverter
//    CollapseIfOneConverter
//    StringFormatConverter

//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    internal static class ValueConverterUtilities
    {
        // handles the case of nullable bools
        public static bool AssureBool(object value, bool defaultIfNull)
        {
            if (value is bool?)
            {
                bool? nbValue = (bool?)value;

                if (nbValue.HasValue)
                {
                    return nbValue.Value;
                }
                else
                {
                    return defaultIfNull;
                }
            }

            return (bool)value;
        }
    }


    // <summary>
    // Transforms bool (or MixedProperty.Mixed) to a Visibility.
    // true -> CheckState.Checked
    // false -> CheckState.Unchecked
    // MixedProperty.Mixed -> CheckState.Intermediate
    // </summary>

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class BoolToVisibilityConverter : IValueConverter
    {
        // IValueConverter implementation
        // <summary>
        // Transform a CheckState into a bool.
        // </summary>
        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(o.GetType() == typeof(Visibility), "Object to inverse-transform isn't a CheckState.");
            Visibility value = (Visibility)o;
            return value == Visibility.Visible ? true : false;
        }

        // <summary>
        // Transform a boolean value (or mixed state) into a CheckState.
        // </summary>
        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            if (o == MixedProperty.Mixed)
            {
                return Visibility.Visible;
            }
            else
            {
                Fx.Assert(o.GetType() == typeof(bool), "Object to transform isn't a bool or mixed state.");
                bool value = (bool)o;
                return (object)(value ? Visibility.Visible : Visibility.Collapsed);
            }
        }
    }

}
