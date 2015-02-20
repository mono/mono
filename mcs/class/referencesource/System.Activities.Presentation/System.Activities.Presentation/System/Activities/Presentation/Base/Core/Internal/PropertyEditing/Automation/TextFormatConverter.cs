//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation;

    // <summary>
    // Simple converter that uses a format string to convert a value into a display string
    // This class is referenced in XAML.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class TextFormatConverter : IValueConverter 
    {

        private string _format;

        // <summary>
        // Gets or sets the format string to apply
        // </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Format 
        {
            get {
                return _format;
            }
            set {
                _format = value;
            }
        }

        // IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            value = value ?? "null";
            return string.Format(culture ?? CultureInfo.CurrentCulture, _format ?? "{0}", value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }

    }
}
