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
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    // <summary>
    // (object-to-string) Takes an object and returns a new string that is the object's ToString()
    // with the value of the suffix property appended to it.
    // </summary>

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class AppendSuffixConverter : IValueConverter
    {
        // Private Fields
        private string suffix;

        // Public Properties
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Suffix
        {
            get
            {
                return this.suffix;
            }
            set
            {
                this.suffix = value;
            }
        }

        // IValueConverter Implementation
        public object ConvertBack(object o, Type targetType, object value, CultureInfo culture)
        {
            Fx.Assert(false, "AppendSuffixConverter do not support inverse transform.");
            return null;
        }

        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            return o.ToString() + this.Suffix;
        }
    }

}
