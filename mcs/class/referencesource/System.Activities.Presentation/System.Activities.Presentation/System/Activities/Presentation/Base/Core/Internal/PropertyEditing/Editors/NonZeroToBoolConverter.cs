//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation;

    // <summary>
    // Converts ints >0 to true, everything else to false.  This class is instantiated from XAML.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class NonZeroToBoolConverter : IValueConverter 
    {

        private bool _invert;

        // <summary>
        // If set to false, NonZeroToBoolConverter.Convert() converts 0 to false and !0 to true.
        // If set to true, the result is inverted.
        // </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool Invert 
        {
            get { return _invert; }
            set { _invert = value; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            if (targetType == typeof(bool) && value is int) 
            {
                return (((int)value) > 0) ^ _invert;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
