//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    // Code borrowed from System.Activities.Presentation

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
            return !AssureBool(o, false);
        }

        static bool AssureBool(object value, bool defaultIfNull)
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
}
